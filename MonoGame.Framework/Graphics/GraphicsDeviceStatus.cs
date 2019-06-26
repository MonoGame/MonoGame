// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Describes the status of the <see cref="GraphicsDevice"/>.
    /// </summary>
    public enum GraphicsDeviceStatus
    {
        /// <summary>
        /// The device is normal.
        /// </summary>
        Normal,
        /// <summary>
        /// The device has been lost.
        /// </summary>
        Lost,
        /// <summary>
        /// The device has not been reset.
        /// </summary>
        NotReset
    }
}

