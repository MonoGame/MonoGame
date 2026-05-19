// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Type of the media source.
    /// </summary>
    /// <remarks>
    /// Indicates the type of the current media source.
    /// The type can be either a local device, or a device connected through Windows Media Connect.
    /// </remarks>
 	public enum MediaSourceType
    {
        /// <summary>
        /// A local device.
        /// </summary>
        LocalDevice = 0,
        /// <summary>
        /// A Windows Media Connect device.
        /// </summary>
        WindowsMediaConnect = 4
    }
}
