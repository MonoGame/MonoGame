// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties for maintaining an animation.
    /// </summary>
    public class AnimationContent : ContentItem
    {
        /// <summary>
        /// Gets the collection of animation data channels. Each channel describes the movement of a single bone or rigid object.
        /// </summary>
        public AnimationChannelDictionary Channels { get; } = [];

        /// <summary>
        /// Gets or sets the total length of the animation.
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}
