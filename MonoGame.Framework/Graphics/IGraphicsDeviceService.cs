// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{ 
    /// <summary>
    /// Interface for retrieving <see cref="GraphicsDevice"/> objects.
    /// </summary>
    public interface IGraphicsDeviceService
    {
        /// <summary>
        /// Returns a graphics device.
        /// </summary>
		GraphicsDevice GraphicsDevice { get; }
        
        /// <summary>
        /// The event which occurs when a graphics device is created.
        /// </summary>
		event EventHandler<EventArgs> DeviceCreated;

        /// <summary>
        /// The event which occurs when a graphics device is disposing.
        /// </summary>
        event EventHandler<EventArgs> DeviceDisposing;

        /// <summary>
        /// The event which occurs when a graphics device is reset.
        /// </summary>
        event EventHandler<EventArgs> DeviceReset;

        /// <summary>
        /// The event which occurs when a graphics device is in resetting process.
        /// </summary>
        event EventHandler<EventArgs> DeviceResetting;
    }
}