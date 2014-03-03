// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    class PipelineController : IController
    {
        private readonly IView _view;
        private readonly PipelineProject _project;

        public PipelineController(IView view, PipelineProject project)
        {
            _view = view;
            _view.Attach(this);
            _project = project;
            _project.Attach(_view as IProjectObserver);
        }

        public void NewProject()
        {
            // If the project is dirty then let the user
            // save it or cancel out of creating a new project.
            if (_project.IsDirty && !SaveProject(false))
                return;

            // Clear the existing model data.
            _project.NewProject();
            
            // Setup a default project.
        }

        public void OpenProject(string filePath)
        {
            _project.LoadProject(filePath);
        }

        public void CloseProject()
        {
            _project.CloseProject();
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

        public bool Exit()
        {            
            // If the project is not dirty 
            // then we can simply exit.
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

            // Perform the save.
            return SaveProject(false);
        }
    }
}