// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class SoundEffectWriter : BuiltInContentWriter<SoundEffectContent>
    {
        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, SoundEffectContent value)
        {
            output.Write(value.format.Length);
            output.Write(value.format);

            output.Write(value.data.Length);
            output.Write(value.data);

            output.Write(value.loopStart);
            output.Write(value.loopLength);
            output.Write(value.duration);
        }
    }
}
