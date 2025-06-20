#pragma warning disable 1591

namespace MonoGame.Framework.Content.Pipeline.Builder.Server;

public class ContentRequestedArgs(string contentPath) : EventArgs
{
    public string ContentPath { get; set; } = contentPath;

    public string FilePath { get; set; } = "";

    public bool CompilationStarted { get; set; }
}
