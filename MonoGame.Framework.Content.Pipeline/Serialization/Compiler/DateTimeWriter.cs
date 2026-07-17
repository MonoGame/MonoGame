// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

/// <summary>
/// Writes the DateTime value to the output.
/// </summary>
[ContentTypeWriter]
class DateTimeWriter : BuiltInContentWriter<DateTime>
{
    /// <summary>
    /// Writes the value to the output.
    /// </summary>
    /// <param name="output">The output writer object.</param>
    /// <param name="value">The value to write to the output.</param>
    protected override void Write(ContentWriter output, DateTime value)
    {
        var ticks = (ulong)value.Ticks & ~(3ul << 62);
        var kind = (ulong)value.Kind << 62;
        output.Write(ticks | kind);
    }
}
