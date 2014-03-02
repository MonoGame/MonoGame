namespace MonoGame.Tools.Pipeline
{
    public delegate void SelectionChanged();

    public interface IView
    {
        event SelectionChanged OnSelectionChanged;

        void Attach(IController controller);
    }
}
