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

        public PipelineProject Project
        {
            get { return _project; }
        }

        public PipelineController(IView view, PipelineProject project)
        {
            _view = view;
            _view.Attach(this);
            _project = project;
            _project.Attach(_view as IProjectObserver);            
        }

        public void NewProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            // Clear the existing model data.
            _project.NewProject();
            PipelineTypes.Load(_project);

            string projectFilePath = Environment.CurrentDirectory;
            if (!_view.AskSaveName(ref projectFilePath ))
                return;

            _project.FilePath = projectFilePath;            

            // Setup a default project.
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
                PipelineTypes.Load(_project);

                foreach (var i in _project.ContentItems)
                {
                    i.View = _view;
                    i.ResolveTypes();                    
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
        }

        public void CloseProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

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
            _project.IsDirty = false;
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

        private bool AskSaveProject()
        {
            // If the project is not dirty 
            // then we can simply skip it.
            if (!_project.IsDirty)
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

                _project.IsDirty = true;
            }                      
        }

        public void Exclude(ContentItem item)
        {
            _project.RemoveItem(item);
            _view.RemoveTreeItem(item);

            _project.IsDirty = true;
        }

        public void ProjectModified()
        {
            _project.IsDirty = true;
        }
    }
}