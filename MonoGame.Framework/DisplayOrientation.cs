// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines the orientation of the display.
    /// </summary>
    [Flags]
    public enum DisplayOrientation
    {
        /// <summary>
        /// The default orientation.
        /// </summary>
        Default = 0,
        /// <summary>
        /// The display is rotated counterclockwise into a landscape orientation. Width is greater than height.
        /// </summary>
        LandscapeLeft = 1,
        /// <summary>
        /// The display is rotated clockwise into a landscape orientation. Width is greater than height.
        /// </summary>
        LandscapeRight = 2,
        /// <summary>
        /// The display is rotated as portrait, where height is greater than width.
        /// </summary>
        Portrait = 4,
        /// <summary>
        /// The display is rotated as inverted portrait, where height is greater than width.
        /// </summary>
        PortraitDown = 8,
        /// <summary>
        /// Unknown display orientation.
        /// </summary>
        Unknown = 16
    }
}