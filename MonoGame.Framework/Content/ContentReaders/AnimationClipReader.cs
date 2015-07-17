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

        private Keyframe[] ReadKeyframes(ContentReader input, Keyframe[] existingInstance)
        {
            var keyframes = existingInstance;

            int count = input.ReadInt32();
            if (keyframes == null)
                keyframes = new Keyframe[count];

            for (int i = 0; i < count; i++)
            {
                keyframes[i].bone = input.ReadInt32();
                keyframes[i].time = new TimeSpan(input.ReadInt64());
                keyframes[i].transform.M11 = input.ReadSingle();
                keyframes[i].transform.M12 = input.ReadSingle();
                keyframes[i].transform.M13 = input.ReadSingle();
                keyframes[i].transform.M14 = 0;
                keyframes[i].transform.M21 = input.ReadSingle();
                keyframes[i].transform.M22 = input.ReadSingle();
                keyframes[i].transform.M23 = input.ReadSingle();
                keyframes[i].transform.M24 = 0;
                keyframes[i].transform.M31 = input.ReadSingle();
                keyframes[i].transform.M32 = input.ReadSingle();
                keyframes[i].transform.M33 = input.ReadSingle();
                keyframes[i].transform.M34 = 0;
                keyframes[i].transform.M41 = input.ReadSingle();
                keyframes[i].transform.M42 = input.ReadSingle();
                keyframes[i].transform.M43 = input.ReadSingle();
                keyframes[i].transform.M44 = 1;
            }

            return keyframes;
        }
    }
}
