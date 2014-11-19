﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MGCB;

namespace MonoGame.Tools.Pipeline
{
    internal partial class PipelineController : IController
    {
        private readonly IView _view;
        private PipelineProject _project;

        private Task _buildTask;
        private Process _buildProcess;

        private readonly List<ContentItemTemplate> _templateItems;

        private static readonly string [] _mgcbSearchPaths = new []       
        {
            "",
#if DEBUG
            "../../../../../MGCB/bin/Windows/AnyCPU/Debug",
#else
            "../../../../../MGCB/bin/Windows/AnyCPU/Release",
#endif
            "../MGCB",
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        };

        public IEnumerable<ContentItemTemplate> Templates
        {
            get { return _templateItems; }
        }

        public Selection Selection { get; private set; }

        public bool LaunchDebugger { get; set; }

        public bool ProjectOpen { get; private set; }

        public bool ProjectDirty { get; set; }

        public bool ProjectBuilding 
        {
            get
            {
                return _buildTask != null && !_buildTask.IsCompleted;
            }
        }

        public IView View { get; set; }

        public event Action OnProjectLoading;

        public event Action OnProjectLoaded;

        public event Action OnBuildStarted;

        public event Action OnBuildFinished;

        public PipelineController(IView view, PipelineProject project)
        {
            _actionStack = new ActionStack();
            Selection = new Selection();

            _view = view;
            _view.Attach(this);
            _project = project;
            ProjectOpen = false;

            _templateItems = new List<ContentItemTemplate>();
            LoadTemplates(Environment.CurrentDirectory + "\\Templates");
        }

        public void OnProjectModified()
        {            
            Debug.Assert(ProjectOpen, "OnProjectModified called with no project open?");
            ProjectDirty = true;
        }

        public void OnReferencesModified()
        {
            Debug.Assert(ProjectOpen, "OnReferencesModified called with no project open?");
            ProjectDirty = true;
            ResolveTypes();
        }

        public void OnItemModified(ContentItem contentItem)
        {
            Debug.Assert(ProjectOpen, "OnItemModified called with no project open?");
            ProjectDirty = true;
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

            // A project needs a root directory or it is impossible to resolve relative paths.
            // So we need the user to choose that location even though the project has not
            // yet actually been saved to disk.
            var projectFilePath = Environment.CurrentDirectory;
            if (!_view.AskSaveName(ref projectFilePath, "New Project"))
                return;

            CloseProject();

            if (OnProjectLoading != null)
                OnProjectLoading();

            // Clear existing project data, initialize to a new blank project.
            _actionStack.Clear();
            _project = new PipelineProject();            
            PipelineTypes.Load(_project);

            // Save the new project.
            _project.OriginalPath = projectFilePath;
            ProjectOpen = true;
            ProjectDirty = true;

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

            CloseProject();

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
                ProjectDirty = true;                
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
            
            OpenProject(projectFilePath);
        }

        public void OpenProject(string projectFilePath)
        {
            CloseProject();

            if (OnProjectLoading != null)
                OnProjectLoading();

#if SHIPPING
            try
#endif
            {
                _actionStack.Clear();
                _project = new PipelineProject();
                
                var parser = new PipelineProjectParser(this, _project);
                var errorCallback = new MGBuildParser.ErrorCallback((msg, args) => View.OutputAppend(string.Format(Path.GetFileName(projectFilePath) + ": " + msg, args)));
                parser.OpenProject(projectFilePath, errorCallback);

                ResolveTypes();

                ProjectOpen = true;
                ProjectDirty = false;

                History.Default.AddProjectHistory(projectFilePath);
                History.Default.StartupProject = projectFilePath;
                History.Default.Save();
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
            if (!ProjectOpen)
                return;

            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            ProjectOpen = false;
            ProjectDirty = false;
            _project = null;
            _actionStack.Clear();
            _view.OutputClear();

            History.Default.StartupProject = null;
            History.Default.Save();

            Selection.Clear(this);
            UpdateTree();
        }

        public bool SaveProject(bool saveAs)
        {
            // Do we need file name?
            if (saveAs || string.IsNullOrEmpty(_project.OriginalPath))
            {
                string newFilePath = _project.OriginalPath;
                if (!_view.AskSaveName(ref newFilePath, null))
                    return false;

                _project.OriginalPath = newFilePath;
            }

            // Do the save.
            ProjectDirty = false;
            var parser = new PipelineProjectParser(this, _project);
            parser.SaveProject();

            // Note: This is where a project loaded via 'new project' or 'import project' 
            //       get recorded into history because up until this point they did not
            //       exist as files on disk.
            History.Default.AddProjectHistory(_project.OriginalPath);
            History.Default.StartupProject = _project.OriginalPath;
            History.Default.Save();

            return true;
        }

        public void Build(bool rebuild)
        {
            var commands = string.Format("/@:\"{0}\" {1}", _project.OriginalPath, rebuild ? "/rebuild" : string.Empty);
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
            var tempPath = Path.Combine(Path.GetDirectoryName(_project.OriginalPath), uniqueName);

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

            _buildTask = Task.Factory.StartNew(() => DoBuild(commands));
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

            _buildTask = Task.Factory.StartNew(() => DoBuild(commands));
            if (OnBuildFinished != null)
                _buildTask.ContinueWith((e) => OnBuildFinished());          
        }

        private string FindMGCB()
        {
            foreach (var root in _mgcbSearchPaths)
            {
                var mgcbPath = Path.Combine(root, "MGCB.exe");
                if (File.Exists(mgcbPath))
                    return Path.GetFullPath(mgcbPath);
            }

            throw new FileNotFoundException("MGCB.exe is not in the search path!");
        }

        private void DoBuild(string commands)
        {
            try
            {
                // Prepare the process.
                _buildProcess = _view.CreateProcess(FindMGCB(), commands);
                _buildProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_project.OriginalPath);
                _buildProcess.StartInfo.CreateNoWindow = true;
                _buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _buildProcess.StartInfo.UseShellExecute = false;
                _buildProcess.StartInfo.RedirectStandardOutput = true;
                _buildProcess.OutputDataReceived += (sender, args) => _view.OutputAppend(args.Data);

                // Fire off the process.
                _buildProcess.Start();
                _buildProcess.BeginOutputReadLine();
                _buildProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                // If we got a message assume it has everything the user needs to know.
                if (!string.IsNullOrEmpty(ex.Message))
                    _view.OutputAppend("Build failed:  " + ex.Message);
                else
                {
                    // Else we need to get verbose.
                    _view.OutputAppend("Build failed:" + Environment.NewLine);
                    _view.OutputAppend(ex.ToString());
                }
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
            if (!ProjectDirty)
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

            if (_project == null || string.IsNullOrEmpty(_project.OriginalPath))
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
            action.Do();
            _actionStack.Add(action);  
        }

        public void Exclude(IEnumerable<ContentItem> items)
        {
            var action = new ExcludeAction(this, items);
            action.Do();
            _actionStack.Add(action);
        }

        public void NewItem(string name, string location, ContentItemTemplate template)
        {
            var action = new NewAction(this, name, location, template);
            action.Do();
            _actionStack.Add(action);
        }

        public void AddAction(IProjectAction action)
        {
            _actionStack.Add(action);
        }

        public IProjectItem GetItem(string originalPath)
        {
            if (_project.OriginalPath.Equals(originalPath, StringComparison.OrdinalIgnoreCase))
                return _project;

            foreach (var i in _project.ContentItems)
            {
                if (string.Equals(i.OriginalPath, originalPath, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return null;
        }

        #region Undo, Redo

        private readonly ActionStack _actionStack;

        public event CanUndoRedoChanged OnCanUndoRedoChanged
        {
            add { _actionStack.OnCanUndoRedoChanged += value; }
            remove { _actionStack.OnCanUndoRedoChanged -= value; } 
        }

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
                i.Observer = this;
                i.ResolveTypes();
                _view.UpdateProperties(i);
            }

            LoadTemplates(_project.Location);
        }

        private void LoadTemplates(string path)
        {
            if (!Directory.Exists(path))
                return;

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

                var fpath = Path.GetDirectoryName(f);
                item.TemplateFile = Path.GetFullPath(Path.Combine(fpath, item.TemplateFile));

                _view.OnTemplateDefined(item);

                _templateItems.Add(item);
            }
        }

        public string GetFullPath(string filePath)
        {
            if (_project == null)
                return filePath;

            filePath = filePath.Replace("/", "\\");
            if (filePath.StartsWith("\\"))
                filePath = filePath.Substring(2);

            if (Path.IsPathRooted(filePath))
                return filePath;

            return _project.Location + "\\" + filePath;
        }
    }
}