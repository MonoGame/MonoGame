// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors;

/// <summary>
/// Represents a processed sound effect.
/// </summary>
public sealed class SoundEffectContent
{
    /// <summary>
    /// Gets the byte metadata that describes the <see cref="Data"/> byte array.
    /// </summary>
    public required byte[] Format { get; init; }

    /// <summary>
    /// Gets the byte data of the sound effect.
    /// </summary>
    public required byte[] Data { get; init; }

    /// <summary>
    /// Get the start point at which the sound effect should loop.
    /// </summary>
    public required int LoopStart { get; init; }

    /// <summary>
    /// Gets the end point at which sound effect loops.
    /// </summary>
    public required int LoopLength { get; init; }

    /// <summary>
    /// Gets the duration of the sound effect.
    /// </summary>
    public required int Duration { get; init; }
}
