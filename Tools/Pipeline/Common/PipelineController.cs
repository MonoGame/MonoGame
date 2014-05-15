// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MonoGame.Tools.Pipeline.Common;

namespace MonoGame.Tools.Pipeline
{
    internal class PipelineController : IController
    {
        private readonly IView _view;
        private PipelineProject _project;

        private Task _buildTask;
        private Process _buildProcess;

        public PipelineController(IView view, PipelineProject project)
        {
            OutputWriter = new OutputWriter(view);
            Console.SetError(OutputWriter);
            Console.SetOut(OutputWriter);

            _view = view;
            _view.Attach(this);
            _project = project;
            _project.Controller = this;
            ProjectOpen = false;
        }

        public TextWriter OutputWriter { get; private set; }

        public bool ProjectOpen { get; private set; }

        public bool ProjectDiry { get; set; }

        public bool ProjectBuilding 
        {
            get
            {
                return _buildTask != null && !_buildTask.IsCompleted;
            }
        }

        public event ProjectEventCallback OnProjectLoading;

        public event ProjectEventCallback OnProjectLoaded;

        public event BuildEventCallback OnBuildStarted;

        public event BuildEventCallback OnBuildFinished;

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
            _view.OutputClear();

            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            // Ask user to choose a location on disk for the new project.            
            var projectFilePath = Environment.CurrentDirectory;
            if (!_view.AskSaveName(ref projectFilePath, "New Project"))
                return;

            if (OnProjectLoading != null)
                OnProjectLoading();            

            // Create blank project.
            _project = new PipelineProject();            
            PipelineTypes.Load(_project, _view);
            
            _project.FilePath = projectFilePath;
            ProjectOpen = true;
            ProjectDiry = true;
            
            UpdateTree();

            if (OnProjectLoaded != null)
                OnProjectLoaded();
        }

        public void ImportProject()
        {
            _view.OutputClear();

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
                var project = new PipelineProject();
                var parser = new PipelineProjectParser(this, project, _view);
                if (parser.ImportProject(projectFilePath))
                {                    
                    _project = project;
                    ResolveTypes();
                    ProjectOpen = true;
                    ProjectDiry = true;
                }                                
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
            _view.OutputClear();

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
                var project = new PipelineProject();
                var parser = new PipelineProjectParser(this, project, _view);
                if (parser.OpenProject(projectFilePath))
                {
                    _project = project;                    
                    ResolveTypes();
                    ProjectOpen = true;
                    ProjectDiry = false;                
                }
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
            _view.OutputClear();

            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            ProjectOpen = false;
            ProjectDiry = false;
            _project = null;

            UpdateTree();            
        }

        public bool SaveProject(bool saveAs)
        {
            _view.OutputClear();

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
            var parser = new PipelineProjectParser(this, _project, _view);
            parser.SaveProject();

            return true;
        }

        public void OnTreeSelect(IProjectItem item)
        {
            _view.ShowProperties(item);
        }

        public void Execute(BuildCommand cmd)
        {
            _view.OutputClear();

            Debug.Assert(_buildTask == null || _buildTask.IsCompleted, "The previous build wasn't completed!");

            // Make sure we save first!
            if (!AskSaveProject())
                return;

            if (OnBuildStarted != null)
                OnBuildStarted(cmd);           

            var parameters = new List<string>();
            if (cmd == BuildCommand.Clean)
            {
                parameters.Add("/clean");
                parameters.Add(string.Format("/intermediateDir:\"{0}\"", _project.IntermediateDir));
                parameters.Add(string.Format("/outputDir:\"{0}\"", _project.OutputDir));                
            }
            else
            {
                parameters.Add(string.Format("/@:\"{0}\"", _project.FilePath));

                if (cmd == BuildCommand.Rebuild)
                    parameters.Add("/rebuild");
            }
            
            _buildTask = Task.Run(() => DoBuild(parameters.ToArray()));
            if (OnBuildFinished != null)
                _buildTask.ContinueWith((e) => OnBuildFinished(cmd));
        }

        private void DoBuild(string[] parameters)
        {
            _buildProcess = new Process();
            _buildProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_project.FilePath);
            _buildProcess.StartInfo.FileName = "MGCB.exe";
            _buildProcess.StartInfo.Arguments = string.Join(" ", parameters);
            _buildProcess.StartInfo.CreateNoWindow = true;
            _buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _buildProcess.StartInfo.UseShellExecute = false;
            _buildProcess.StartInfo.RedirectStandardError = true;
            _buildProcess.StartInfo.RedirectStandardOutput = true;
            _buildProcess.OutputDataReceived += (sender, args) => _view.OutputAppend(args.Data);
            _buildProcess.ErrorDataReceived += (sender, args) => _view.OutputAppendLine(args.Data);

            //string stdError = null;
            try
            {
                _buildProcess.Start();
                _buildProcess.BeginOutputReadLine();
                _buildProcess.BeginErrorReadLine();
                _buildProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                _view.OutputAppendLine("Build process failed!");
                _view.OutputAppendLine(ex.Message);
                _view.OutputAppendLine(ex.StackTrace);                
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
            List<string> files;
            if (!_view.ChooseContentFile(initialDirectory, out files))
                return;

            var parser = new PipelineProjectParser(this, _project, _view);
            _view.BeginTreeUpdate();

            foreach (var file in files)
            {
                if (!parser.AddBuildItem(file, true))
                    continue;

                var item = _project.ContentItems.Last();
                item.Controller = this;
                item.ResolveTypes();
                _view.AddTreeItem(item);
                _view.SelectTreeItem(item);
            }

            _view.EndTreeUpdate();
            ProjectDiry = true;                  
        }

        public void Exclude(ContentItem item)
        {
            _project.ContentItems.Remove(item);

            _view.BeginTreeUpdate();
            _view.RemoveTreeItem(item);
            _view.EndTreeUpdate();

            ProjectDiry = true;
        }

        public string GetFullPath(string filePath)
        {
            filePath = filePath.Replace("/", "\\");
            if (filePath.StartsWith("\\"))
                filePath = filePath.Substring(2);

            if (Path.IsPathRooted(filePath))
                return filePath;
            
            return _project.Location + "\\" + filePath;
        }

        private void ResolveTypes()
        {
            PipelineTypes.Load(_project, _view);
            foreach (var i in _project.ContentItems)
            {
                i.Controller = this;
                i.ResolveTypes();
                _view.UpdateProperties(i);
            }        
        }
    }
}