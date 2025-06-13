// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// Describes the mode in which the content builder will run in.
/// </summary>
public enum ContentBuilderMode
{
    /// <summary>
    /// Indicates that no builder mode has been specified, so the builder will show the help menu.
    /// </summary>
    None,

    /// <summary>
    /// Indicates that the content builder should build the passed content files.
    /// </summary>
    Builder,

    /// <summary>
    /// Indicates that the content builder should run in server mode and wait for content load requests.
    /// </summary>
    Server
}
