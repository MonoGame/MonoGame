// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics.SkinnedAnimation;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the AnimationClip to the output.
    /// </summary>
    [ContentTypeWriter]
    class AnimationClipWriter : BuiltInContentWriter<AnimationClip>
    {
        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, AnimationClip value)
        {
            // Write Duration
            output.Write(value.Duration.Ticks);
            // Write Keyframes
            WriteKeyframes(output, value.Keyframes);
        }

        private void WriteKeyframes(ContentWriter output, List<Keyframe> Keyframes)
        {
            output.Write(Keyframes.Count);
            foreach (var element in Keyframes)
            {
                output.Write(element.Bone);
                output.Write(element.Time.Ticks);
                output.Write(element.Transform);
            }
        }
    }
}
