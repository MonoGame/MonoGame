// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines how <see cref="GraphicsDevice.Present"/> updates the game window.
    /// </summary>
    public enum PresentInterval
    {
        /// <summary>
        /// Equivalent to <see cref="PresentInterval.One"/>.
        /// </summary>
        Default,
        /// <summary>
        /// The driver waits for the vertical retrace period, before updating window client area. Present operations are not affected more frequently than the screen refresh rate.
        /// </summary>
        One,
        /// <summary>
        /// The driver waits for the vertical retrace period, before updating window client area. Present operations are not affected more frequently than every second screen refresh. 
        /// </summary>
        Two,
        /// <summary>
        /// The driver updates the window client area immediately. Present operations might be affected immediately. There is no limit for framerate.
        /// </summary>
        Immediate,
    }
}