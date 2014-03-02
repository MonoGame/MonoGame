namespace MonoGame.Tools.Pipeline
{
    class PipelineController : IController
    {
        private readonly MainView _view;
        private readonly PipelineModel _model;

        public PipelineController(MainView view, PipelineModel model)
        {
            _view = view;
            _view.Attach(this);
            _model = model;
            _model.Attach(_view);
        }

        public void NewProject()
        {
            _model.NewProject();
        }

        public void OpenProject(string filePath)
        {
            _model.OpenProject(filePath);
        }

        public void CloseProject()
        {
            _model.CloseProject();
        }
    }
}