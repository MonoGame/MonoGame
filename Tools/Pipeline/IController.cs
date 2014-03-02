namespace MonoGame.Tools.Pipeline
{
    public interface IController
    {
        void NewProject();

        void OpenProject(string filePath);

        void CloseProject();
    }
}
