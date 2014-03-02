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
            // If the project is dirty then confirm to
            // save or discard it.
            //if (IsDirty)

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

            // Do we need file name?
            if (string.IsNullOrEmpty(_project.FilePath))
            {
                string newFilePath;
                if (!_view.AskSaveName(out newFilePath))
                    return false;

                _project.FilePath = newFilePath;
            }

            // Perform the save.

            return true;
        }
    }
}