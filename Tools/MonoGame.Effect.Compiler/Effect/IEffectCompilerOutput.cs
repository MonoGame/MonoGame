namespace MonoGame.Effect
{
    public interface IEffectCompilerOutput
    {
        void WriteWarning(string file, int line, int column, string message);
        void WriteError(string file, int line, int column, string message);
    }
}