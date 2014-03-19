// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties for maintaining an animation.
    /// </summary>
    public class AnimationContent : ContentItem
    {
        AnimationChannelDictionary channels;
        TimeSpan duration;

        /// <summary>
        /// Gets the collection of animation data channels. Each channel describes the movement of a single bone or rigid object.
        /// </summary>
        public AnimationChannelDictionary Channels
        {
            get
            {
                return channels;
            }
        }

        /// <summary>
        /// Gets or sets the total length of the animation.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return duration;
            }
            set
            {
                duration = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of AnimationContent.
        /// </summary>
        public AnimationContent()
        {
            channels = new AnimationChannelDictionary();
        }
    }
}
