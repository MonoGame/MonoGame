// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors;

/// <summary>
/// Represents a compiled Effect.
/// </summary>
/// <param name="effectCode">The compiled effect code.</param>
public class CompiledEffectContent(byte[] effectCode) : ContentItem
{
    private readonly byte[] _effectCode = effectCode;

    /// <summary>
    /// Retrieves the compiled byte code for this shader.
    /// </summary>
    /// <returns>The compiled bytecode.</returns>
    public byte[] GetEffectCode() => _effectCode;
}
