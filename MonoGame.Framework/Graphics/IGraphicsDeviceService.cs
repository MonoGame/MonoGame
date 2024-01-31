// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Provider of a <see cref="Graphics.GraphicsDevice"/>.
    /// </summary>
    public interface IGraphicsDeviceService
    {
		/// <summary>
		/// The provided <see cref="Graphics.GraphicsDevice"/>.
		/// </summary>
		GraphicsDevice GraphicsDevice { get; }

        /// <summary>
        /// Raised when a new <see cref="Graphics.GraphicsDevice"/> has been created.
        /// </summary>
		event EventHandler<EventArgs> DeviceCreated;

        /// <summary>
        /// Raised when the <see cref="GraphicsDevice"/> is disposed.
        /// </summary>
        event EventHandler<EventArgs> DeviceDisposing;

        /// <summary>
        /// Raised when the <see cref="GraphicsDevice"/> has reset.
        /// </summary>
        /// <seealso cref="Microsoft.Xna.Framework.Graphics.GraphicsDevice.Reset"/>
        event EventHandler<EventArgs> DeviceReset;

        /// <summary>
        /// Raised before the <see cref="GraphicsDevice"/> is resetting.
        /// </summary>
        event EventHandler<EventArgs> DeviceResetting;
    }
}

