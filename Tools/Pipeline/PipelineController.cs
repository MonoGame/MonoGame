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
            // If it's dirty then ask about saving first.
            if (true)
            {
                var result = _view.AskSave();

                // Did we cancel exit?
                if (result == AskResult.Cancel)
                    return false;

                // Did we want to save?
                if (result == AskResult.Yes)
                {
                    // TODO: Save project!
                }
            }

            return true;
        }
    }
}