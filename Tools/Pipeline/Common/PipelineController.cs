// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonoGame.Tools.Pipeline
{
    internal class PipelineController : IController
    {
        private readonly IView _view;
        private readonly PipelineProject _project;

        private Task _buildProcess;

        public PipelineController(IView view, PipelineProject project)
        {
            _view = view;
            _view.Attach(this);
            _project = project;
            _project.Attach(_view as IProjectObserver);
            ProjectOpen = false;
        }

        public bool ProjectOpen { get; private set; }

        public bool ProjectDiry { get; set; }

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

        public void NewProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            ProjectDiry = false;

            // Clear existing project data, initialize to a new blank project.
            _project.NewProject();
            PipelineTypes.Load(_project);

            // Ask user to choose a location on disk for the new project.
            // Note: It is impossible to have a project without a project root directory, hence it has to be saved immediately.
            var projectFilePath = Environment.CurrentDirectory;
            if (!_view.AskSaveName(ref projectFilePath))
            {
                // User canceled the save operation, so we cannot create the new project, unload it.
                _project.CloseProject();
                PipelineTypes.Unload();                
                ProjectOpen = false;                
            }
            else
            {
                // User saved the new project.
                _project.FilePath = projectFilePath;
                ProjectOpen = true;
            }            
            
            UpdateTree();
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

#if SHIPPING
            try
#endif
            {
                _project.ImportProject(projectFilePath);
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

#if SHIPPING
            try
#endif
            {
                _project.OpenProject(projectFilePath);
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
        }

        public void CloseProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            ProjectOpen = false;
            ProjectDiry = false;
            _project.CloseProject();
            UpdateTree();
        }

        public bool SaveProject(bool saveAs)
        {
            // Do we need file name?
            if (saveAs || string.IsNullOrEmpty(_project.FilePath))
            {
                string newFilePath = _project.FilePath;
                if (!_view.AskSaveName(ref newFilePath))
                    return false;

                _project.FilePath = newFilePath;
            }

            // Do the save.
            ProjectDiry = false;            
            _project.SaveProject();

            return true;
        }

        public void OnTreeSelect(IProjectItem item)
        {
            _view.ShowProperties(item);
        }

        public void Build(bool rebuild)
        {
            Debug.Assert(_buildProcess == null || _buildProcess.IsCompleted, "The previous build wasn't completed!");

            // Make sure we save first!
            if (!AskSaveProject())
                return;

            _view.OutputClear();
            _buildProcess = Task.Run(() => DoBuild(rebuild ? "/rebuild" : string.Empty));            
        }

        public void Clean()
        {
            Debug.Assert(_buildProcess == null || _buildProcess.IsCompleted, "The previous build wasn't completed!");

            // Make sure we save first!
            if (!AskSaveProject())
                return;

            _view.OutputClear();
            _buildProcess = Task.Run(() => DoBuild("/clean"));            
        }

        private void DoBuild(string command)
        {
            var arguments = string.Format("/@:{0} {1}", _project.FilePath, command);

            var process = new Process();
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(_project.FilePath);
            process.StartInfo.FileName = "MGCB.exe";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += (sender, args) => _view.OutputAppend(args.Data);
            process.ErrorDataReceived += (sender, args) => _view.OutputAppend(args.Data);

            //string stdError = null;
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception)
            {
                // TODO: What if we fail here?
            }

            if (process.ExitCode != 0)
            {
                // TODO: Build failed!
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
            if (string.IsNullOrEmpty(_project.FilePath))
                _view.SetTreeRoot(null);
            else
            {
                _view.SetTreeRoot(_project);

                foreach (var item in _project.ContentItems)
                    _view.AddTreeItem(item);
            }
        }

        public bool Exit()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            return AskSaveProject();
        }

        public void Include(string initialDirectory)
        {            
            var projectRoot = _project.Location;
            var path = projectRoot + "\\" + initialDirectory;

            string file;
            if (_view.ChooseContentFile(initialDirectory, out file))
            {
                _project.OnBuild(file);
                var item = _project.ContentItems.Last();
                item.View = _view;
                item.ResolveTypes();
                _view.AddTreeItem(item);
                _view.SelectTreeItem(item);

                ProjectDiry = true;
            }                      
        }

        public void Exclude(ContentItem item)
        {
            _project.RemoveItem(item);
            _view.RemoveTreeItem(item);

            ProjectDiry = true;
        }            
    
        private void ResolveTypes()
        {
            PipelineTypes.Load(_project);
            foreach (var i in _project.ContentItems)
            {
                i.View = _view;
                i.ResolveTypes();
                _view.UpdateProperties(i);
            }                        
        }
    }
}