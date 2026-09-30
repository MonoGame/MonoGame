// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

/// <summary>
/// Writes the signed short value to the output.
/// </summary>
[ContentTypeWriter]
class Int16Writer : BuiltInContentWriter<short>
{
    /// <summary>
    /// Writes the value to the output.
    /// </summary>
    /// <param name="output">The output writer object.</param>
    /// <param name="value">The value to write to the output.</param>
    protected override void Write(ContentWriter output, short value) => output.Write(value);
}
