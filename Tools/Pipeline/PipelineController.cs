namespace MonoGame.Tools.Pipeline
{
    class PipelineController : IController
    {
        private MainView _view;
        private PipelineModel _model;

        public PipelineController(MainView view, PipelineModel model)
        {
            _view = view;
            _model = model;
        }

        public void NewProject()
        {
            throw new System.NotImplementedException();
        }

        public void OpenProject(string filePath)
        {
            throw new System.NotImplementedException();
        }

        public void CloseProject()
        {
            throw new System.NotImplementedException();
        }
    }
}