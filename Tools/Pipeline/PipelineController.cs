// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    class PipelineController : IController
    {
        private readonly IView _view;
        private readonly IModel _model;

        public PipelineController(IView view, IModel model)
        {
            _view = view;
            _view.Attach(this);
            _model = model;
            _model.Attach(_view as IModelObserver);
        }

        public void NewProject()
        {
            // If the project is dirty then confirm to
            // save or discard it.
            //if (IsDirty)

            // Clear the existing model data.
            _model.NewProject();
            
            // Setup a default project.
        }

        public void OpenProject(string filePath)
        {
            _model.LoadProject(filePath);
        }

        public void CloseProject()
        {
            _model.CloseProject();
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