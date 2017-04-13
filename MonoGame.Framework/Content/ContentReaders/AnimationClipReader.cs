// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics.SkinnedAnimation;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content
{
    class AnimationClipReader : ContentTypeReader<AnimationClip>
    {
        protected internal override AnimationClip Read(ContentReader input, AnimationClip existingInstance)
        {
            AnimationClip animationClip = existingInstance;
            
            if (existingInstance == null)
            {
                // Read Duration
                var duration = ReadDuration(input);
                // Read Keyframes
                var keyframes = ReadKeyframes(input, null);
                animationClip = new AnimationClip(duration, keyframes);
            }
            else
            {
                // Read Duration
                animationClip.Duration = ReadDuration(input);
                // Read Keyframes
                ReadKeyframes(input, animationClip.Keyframes);
            }

            return animationClip;
        }

        private TimeSpan ReadDuration(ContentReader input)
        {
            return new TimeSpan(input.ReadInt64());
        }

        private List<Keyframe> ReadKeyframes(ContentReader input, List<Keyframe> existingInstance)
        {
            var keyframes = existingInstance;

            int count = input.ReadInt32();
            if (keyframes == null)
                keyframes = new List<Keyframe>(count);

            for (int i = 0; i < count; i++)
            {
                Keyframe keyframe = new Keyframe(
                        input.ReadInt32(),
                        new TimeSpan(input.ReadInt64()),
                        input.ReadMatrix()
                    );
                if (existingInstance == null)
                    keyframes.Add(keyframe);
                else
                    keyframes[i] = keyframe;
            }

            return keyframes;
        }
    }
}
