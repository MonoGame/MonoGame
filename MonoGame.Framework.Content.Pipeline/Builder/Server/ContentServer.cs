// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder.Server;

/// <summary>
/// Describes a content server implementation to be used by the <see cref="ContentBuilder"/>.
/// </summary>
public abstract class ContentServer
{
    /// <summary>
    /// <see cref="ContentBuildLogger"/> to use for the server to log messages.
    /// 
    /// If run from <see cref="ContentBuilder"/> it will be set by it.
    /// </summary>
    public ContentBuildLogger Logger { get; set; } = new ContentBuildLogger();

    /// <summary>
    /// An event that gets called when the content server recieves a content request by an <see cref="IContentProvider"/>.
    /// </summary>
    public event EventHandler<ContentRequestedArgs>? ContentRequested;

    /// <summary>
    /// Requests the content server to start listening for the events.
    /// 
    /// This method is called by the main thread and should be a non blocking call.
    /// </summary>
    public abstract void StartListening();

    /// <summary>
    /// Requests the content server to stop listening for the events.
    /// 
    /// This method is called by the main thread and should be a non blocking call.
    /// </summary>
    public abstract void StopListening();

    /// <summary>
    /// The content server calls this event once it has finished compiling content.
    /// 
    /// This method is called by the main thread and should be a non blocking call.
    /// </summary>
    public abstract void NotifyContentRequestCompiled();

    /// <summary>
    /// A protected method used to invoke <see cref="ContentRequested"/> event.
    /// </summary>
    /// <param name="args"><see cref="ContentRequestedArgs"/> to use for invoking <see cref="ContentRequested"/> event.</param>
    protected void OnContentRequested(ContentRequestedArgs args) => ContentRequested?.Invoke(this, args);
}
