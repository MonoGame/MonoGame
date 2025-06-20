#pragma warning disable 1591

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder.Server;

public abstract class ContentServer
{
    public ContentBuildLogger Logger { get; set; } = new ContentBuildLogger();
    
    public event EventHandler<ContentRequestedArgs>? ContentRequested;

    public void OnContentRequested(ContentRequestedArgs args) => ContentRequested?.Invoke(this, args);

    public abstract void StartListening();

    public abstract void StopListening();

    public abstract void NotifyContentRequestCompiled();
}
