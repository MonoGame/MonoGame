// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using TOutput = System.DateTime;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the DateTime value to the output.
    /// </summary>
    [ContentTypeWriter]
    class DateTimeWriter : BuiltInContentWriter<TOutput>
    {
        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, TOutput value)
        {
            UInt64 ticks = (UInt64)value.Ticks & ~((UInt64)0xC << 62);
            UInt64 kind = (UInt64)value.Kind << 62;
            output.Write((UInt64)(ticks | kind));
        }
    }
}
