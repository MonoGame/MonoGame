// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors;

/// <summary>
/// Represents a processed Song object.
/// </summary>
public sealed class SongContent
{
    /// <summary>
    /// Relative file path to the song content.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Duration of the song.
    /// </summary>
    public required TimeSpan Duration { get; init; }
}
