// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
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
        }

        public void NewProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            // Clear the existing model data.
            _project.NewProject();

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

            try
            {
                _project.OpenProject(projectFilePath);
            }
            catch (Exception)
            {
                _view.ShowError("Open Project", "Failed to open project!");
                return;
            }

            UpdateTree();
        }

        public void CloseProject()
        {
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

            string stdError = null;
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception e)
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
            _view.SetTreeRoot(_project);
            foreach (var item in _project.ContentItems)
                _view.AddTreeItem(item);
        }

        public bool Exit()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            return AskSaveProject();
        }
    }
}