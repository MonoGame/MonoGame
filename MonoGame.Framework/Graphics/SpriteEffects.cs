// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines sprite visual options for rotation.
    /// </summary>
    [Flags]
    public enum SpriteEffects
    {
        /// <summary>
        /// No options specified.
        /// </summary>
		None = 0,
        /// <summary>
        /// Rotate 180 degrees around the Y axis before rendering.
        /// </summary>
        FlipHorizontally = 1,
        /// <summary>
        /// Rotate 180 degrees around the X axis before rendering.
        /// </summary>
        FlipVertically = 2        
    }
}