// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Content.Pipeline.Builder.Server;

/// <summary>
/// EventArgs for the <see cref="ContentServer.ContentRequested"/> event that gets raised when
/// content is being requested by an <see cref="IContentProvider"/>.
/// </summary>
/// <param name="contentPath">Represents a relative output path for the content.</param>
public class ContentRequestedArgs(string contentPath) : EventArgs
{
    /// <summary>
    /// A relative output path for the content.
    /// </summary>
    public string ContentPath { get; } = contentPath;

    // TODO: Split bellow variables into separate event once we get to hot reload.

    /// <summary>
    /// An absolute path on the system to the content file passed by <see cref="ContentPath"/>.
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// Determines if <see cref="ContentServer"/> should wait for the content to be compiled.
    /// </summary>
    public bool CompilationStarted { get; set; }
}
