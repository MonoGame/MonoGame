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
    public struct Keyframe
    {
        internal int bone;
        internal TimeSpan time;
        internal Matrix transform;

        /// <summary>
        /// Constructs a new keyframe object.
        /// </summary>
        public Keyframe(int bone, TimeSpan time, Matrix transform)
        {
            this.bone = bone;
            this.time = time;
            this.transform = transform;
        }
        
        /// <summary>
        /// Gets the index of the target bone that is animated by this keyframe.
        /// </summary>
        public int Bone 
        { 
            get {return bone;}
            internal set { bone = value; }
        }

        /// <summary>
        /// Gets the time offset from the start of the animation to this keyframe.
        /// </summary>
        public TimeSpan Time
        {
            get { return time; }
            internal set { time = value; }
        }

        /// <summary>
        /// Gets the bone transform for this keyframe.
        /// </summary>
        public Matrix Transform
        {
            get { return transform; }
            internal set { transform = value; }
        }
        
		        
    }
}
