// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using TOutput = Microsoft.Xna.Framework.Curve;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the Curve value to the output.
    /// </summary>
    [ContentTypeWriter]
    class CurveWriter : BuiltInContentWriter<TOutput>
    {
        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, TOutput value)
        {
            output.Write((Int32)value.PreLoop);
            output.Write((Int32)value.PostLoop);
            output.Write(value.Keys.Count);
            foreach (var key in value.Keys)
            {
                output.Write(key.Position);
                output.Write(key.Value);
                output.Write(key.TangentIn);
                output.Write(key.TangentOut);
                output.Write((Int32)key.Continuity);
            }
        }
    }
}
