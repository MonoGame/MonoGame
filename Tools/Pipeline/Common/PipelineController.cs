// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MonoGame.Tools.Pipeline
{
    internal partial class PipelineController : IController
    {
        private readonly IView _view;
        private PipelineProject _project;

        private Task _buildTask;
        private Process _buildProcess;

        private readonly List<ContentItemTemplate> _templateItems;

        public IEnumerable<ContentItemTemplate> Templates
        {
            get { return _templateItems; }
        }

        public PipelineController(IView view, PipelineProject project)
        {
            _view = view;
            _view.Attach(this);
            _project = project;
            _project.Controller = this;
            ProjectOpen = false;

            _templateItems = new List<ContentItemTemplate>();
            LoadTemplates(Environment.CurrentDirectory + "\\Templates");         
   
            _actionStack = new ActionStack();
        }

        public bool LaunchDebugger { get; set; }

        public bool ProjectOpen { get; private set; }

        public bool ProjectDiry { get; set; }

        public bool ProjectBuilding 
        {
            get
            {
                return _buildTask != null && !_buildTask.IsCompleted;
            }
        }

        public event Action OnProjectLoading;

        public event Action OnProjectLoaded;

        public event Action OnBuildStarted;

        public event Action OnBuildFinished;

        public void OnProjectModified()
        {            
            Debug.Assert(ProjectOpen, "OnProjectModified called with no project open?");
            ProjectDiry = true;
        }

        public void OnReferencesModified()
        {
            Debug.Assert(ProjectOpen, "OnReferencesModified called with no project open?");
            ProjectDiry = true;
            ResolveTypes();
        }

        public void OnItemModified(ContentItem contentItem)
        {
            Debug.Assert(ProjectOpen, "OnItemModified called with no project open?");
            ProjectDiry = true;
            _view.UpdateProperties(contentItem);

            _view.BeginTreeUpdate();
            _view.UpdateTreeItem(contentItem);
            _view.EndTreeUpdate();
        }

        public void NewProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

           // Ask user to choose a location on disk for the new project.
            // Note: It is impossible to have a project without a project root directory, hence it has to be saved immediately.
            var projectFilePath = Environment.CurrentDirectory;
            if (!_view.AskSaveName(ref projectFilePath, "New Project"))
                return;

            if (OnProjectLoading != null)
                OnProjectLoading();

            // Clear existing project data, initialize to a new blank project.
            _actionStack.Clear();
            _project = new PipelineProject();            
            PipelineTypes.Load(_project);

            // Save the new project.
            _project.FilePath = projectFilePath;
            ProjectOpen = true;
            
            UpdateTree();

            if (OnProjectLoaded != null)
                OnProjectLoaded();
        }

        public void ImportProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            string projectFilePath;
            if (!_view.AskImportProject(out projectFilePath))
                return;

            if (OnProjectLoading != null)
                OnProjectLoading();

#if SHIPPING
            try
#endif
            {
                _actionStack.Clear();
                _project = new PipelineProject();
                var parser = new PipelineProjectParser(this, _project);
                parser.ImportProject(projectFilePath);

                ResolveTypes();                
                
                ProjectOpen = true;
                ProjectDiry = true;
            }
#if SHIPPING
            catch (Exception e)
            {
                _view.ShowError("Open Project", "Failed to open project!");
                return;
            }
#endif

            UpdateTree();

            if (OnProjectLoaded != null)
                OnProjectLoaded();
        }

        public void OpenProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            string projectFilePath;
            if (!_view.AskOpenProject(out projectFilePath))
                return;

            if (OnProjectLoading != null)
                OnProjectLoading();

#if SHIPPING
            try
#endif
            {
                _actionStack.Clear();
                _project = new PipelineProject();
                var parser = new PipelineProjectParser(this, _project);
                parser.OpenProject(projectFilePath);
                ResolveTypes();

                ProjectOpen = true;
                ProjectDiry = false;
            }
#if SHIPPING
            catch (Exception e)
            {
                _view.ShowError("Open Project", "Failed to open project!");
                return;
            }
#endif

            UpdateTree();

            if (OnProjectLoaded != null)
                OnProjectLoaded();
        }

        public void CloseProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            ProjectOpen = false;
            ProjectDiry = false;
            _project = null;
            _actionStack.Clear();

            UpdateTree();
        }

        public bool SaveProject(bool saveAs)
        {
            // Do we need file name?
            if (saveAs || string.IsNullOrEmpty(_project.FilePath))
            {
                string newFilePath = _project.FilePath;
                if (!_view.AskSaveName(ref newFilePath, null))
                    return false;

                _project.FilePath = newFilePath;
            }

            // Do the save.
            ProjectDiry = false;
            var parser = new PipelineProjectParser(this, _project);
            parser.SaveProject();            

            return true;
        }

        public void OnTreeSelect(IProjectItem item)
        {
            _view.ShowProperties(item);
        }

        public void Build(bool rebuild)
        {
            var commands = string.Format("/@:\"{0}\" {1}", _project.FilePath, rebuild ? "/rebuild" : string.Empty);
            if (LaunchDebugger)
                commands += " /launchdebugger";
            BuildCommand(commands);
        }

        public void RebuildItems(IEnumerable<IProjectItem> items)
        {
            // Make sure we save first!
            if (!AskSaveProject())
                return;

            // Create a unique file within the same folder as
            // the normal project to store this incremental build.
            var uniqueName = Guid.NewGuid().ToString();
            var tempPath = Path.Combine(Path.GetDirectoryName(_project.FilePath), uniqueName);

            // Write the incremental project file limiting the
            // content to just the files we want to rebuild.
            using (var io = File.CreateText(tempPath))
            {
                var parser = new PipelineProjectParser(this, _project);
                parser.SaveProject(io, (i) => !items.Contains(i));
            }

            // Run the build the command.
            var commands = string.Format("/@:\"{0}\" /rebuild /incremental", tempPath);
            if (LaunchDebugger)
                commands += " /launchdebugger";

            BuildCommand(commands);

            // Cleanup the temp file once we're done.
            _buildTask.ContinueWith((e) => File.Delete(tempPath));
        }

        private void BuildCommand(string commands)
        {
            Debug.Assert(_buildTask == null || _buildTask.IsCompleted, "The previous build wasn't completed!");

            // Make sure we save first!
            if (!AskSaveProject())
                return;

            if (OnBuildStarted != null)
                OnBuildStarted();

            _view.OutputClear();

            _buildTask = Task.Run(() => DoBuild(commands));
            if (OnBuildFinished != null)
                _buildTask.ContinueWith((e) => OnBuildFinished());
        }

        public void Clean()
        {
            Debug.Assert(_buildTask == null || _buildTask.IsCompleted, "The previous build wasn't completed!");

            // Make sure we save first!
            if (!AskSaveProject())
                return;

            if (OnBuildStarted != null)
                OnBuildStarted();

            _view.OutputClear();

            var commands = string.Format("/clean /intermediateDir:\"{0}\" /outputDir:\"{1}\"", _project.IntermediateDir, _project.OutputDir);
            if (LaunchDebugger)
                commands += " /launchdebugger";

            _buildTask = Task.Run(() => DoBuild(commands));
            if (OnBuildFinished != null)
                _buildTask.ContinueWith((e) => OnBuildFinished());          
        }

        private void DoBuild(string commands)
        {
            _buildProcess = new Process();
            _buildProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_project.FilePath);
            _buildProcess.StartInfo.FileName = "MGCB.exe";
            _buildProcess.StartInfo.Arguments = commands;
            _buildProcess.StartInfo.CreateNoWindow = true;
            _buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _buildProcess.StartInfo.UseShellExecute = false;
            _buildProcess.StartInfo.RedirectStandardOutput = true;
            _buildProcess.OutputDataReceived += (sender, args) => _view.OutputAppend(args.Data);

            //string stdError = null;
            try
            {
                _buildProcess.Start();
                _buildProcess.BeginOutputReadLine();
                _buildProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                _view.OutputAppend("Build process failed!" + Environment.NewLine);
                _view.OutputAppend(ex.Message);
                _view.OutputAppend(ex.StackTrace);
            }

            // Clear the process pointer, so that cancel
            // can run after we've already finished.
            lock (_buildTask)
                _buildProcess = null;
        }

        public void CancelBuild()
        {
            if (_buildTask == null || _buildTask.IsCompleted)
                return;

            lock (_buildTask)
            {
                if (_buildProcess == null)
                    return;

                _buildProcess.Kill();
                _view.OutputAppend("Build terminated!" + Environment.NewLine);
            }
        }

        /// <summary>
        /// Prompt the user if they wish to save the project.
        /// Save it if yes is chosen.
        /// Return true if yes or no is chosen.
        /// Return false if cancel is chosen.
        /// </summary>
        private bool AskSaveProject()
        {
            // If the project is not dirty 
            // then we can simply skip it.
            if (!ProjectDiry)
                return true;

            // Ask the user if they want to save or cancel.
            var result = _view.AskSaveOrCancel();

            // Did we cancel the exit?
            if (result == AskResult.Cancel)
                return false;

            // Did we want to skip saving?
            if (result == AskResult.No)
                return true;

            return SaveProject(false);
        }

        private void UpdateTree()
        {
            _view.BeginTreeUpdate();

            if (_project == null || string.IsNullOrEmpty(_project.FilePath))
                _view.SetTreeRoot(null);
            else
            {
                _view.SetTreeRoot(_project);

                foreach (var item in _project.ContentItems)
                    _view.AddTreeItem(item);
            }

            _view.EndTreeUpdate();
        }

        public bool Exit()
        {
            // Can't exit if we're building!
            if (ProjectBuilding)
            {
                _view.ShowMessage("You cannot exit while the project is building!");
                return false;
            }

            // Make sure we give the user a chance to
            // save the project if they need too.
            return AskSaveProject();
        }

        public void Include(string initialDirectory)
        {       
            // Root the path to the project.
            if (!Path.IsPathRooted(initialDirectory))
                initialDirectory = Path.Combine(_project.Location, initialDirectory);

            List<string> files;
            if (!_view.ChooseContentFile(initialDirectory, out files))
                return;

            var action = new IncludeAction(this, files);
            _actionStack.Add(action);  
        }

        public void Exclude(IEnumerable<ContentItem> items)
        {
            var action = new ExcludeAction(this, items);
            _actionStack.Add(action);
        }

        public void NewItem(string name, string location, ContentItemTemplate template)
        {
            var action = new NewItemAction(this, name, location, template);
            _actionStack.Add(action);
        }

        #region Undo, Redo

        private readonly ActionStack _actionStack;

        public bool CanUndo { get { return _actionStack.CanUndo; } }

        public bool CanRedo { get { return _actionStack.CanRedo; } }

        public void Undo()
        {
            _actionStack.Undo();
        }

        public void Redo()
        {
            _actionStack.Redo();
        }

        #endregion

        private void ResolveTypes()
        {
            PipelineTypes.Load(_project);
            foreach (var i in _project.ContentItems)
            {
                i.Controller = this;
                i.ResolveTypes();
                _view.UpdateProperties(i);
            }

            LoadTemplates(_project.Location);
        }

        private void LoadTemplates(string path)
        {                
            var files = Directory.GetFiles(path, "*.template", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                var lines = File.ReadAllLines(f);
                if (lines.Length != 5)
                    throw new Exception("Invalid template");

                var item = new ContentItemTemplate()
                    {
                        Label = lines[0],
                        Icon = lines[1],
                        ImporterName = lines[2],
                        ProcessorName = lines[3],
                        TemplateFile = lines[4],
                    };
                
                if (_templateItems.Any(i => i.Label == item.Label))
                    continue;

                item.TemplateFile = Path.GetFullPath(Path.Combine(path, item.TemplateFile));
                _templateItems.Add(item);
            }
        }
    }
}