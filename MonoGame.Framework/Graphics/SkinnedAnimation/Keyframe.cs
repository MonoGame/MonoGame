// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Microsoft.Xna.Framework.Graphics.SkinnedAnimation
{
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    public class Keyframe
    {
        /// <summary>
        /// Constructs a new keyframe object.
        /// </summary>
        public Keyframe(int bone, TimeSpan time, Matrix transform)
        {
            Bone = bone;
            Time = time;
            Transform = transform;
        }
        
        /// <summary>
        /// Gets the index of the target bone that is animated by this keyframe.
        /// </summary>
        public int Bone { get; private set; }

        /// <summary>
        /// Gets the time offset from the start of the animation to this keyframe.
        /// </summary>
        public TimeSpan Time { get; private set; }

        /// <summary>
        /// Gets the bone transform for this keyframe.
        /// </summary>
        public Matrix Transform { get; private set; }
    }
}
