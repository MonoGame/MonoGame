// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// The arguments to the <see cref="GraphicsDeviceManager.PreparingDeviceSettings"/> event.
    /// </summary>
    public class PreparingDeviceSettingsEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance of the event.
        /// </summary>
        /// <param name="graphicsDeviceInformation">The default settings to be used in device creation.</param>
        public PreparingDeviceSettingsEventArgs(GraphicsDeviceInformation graphicsDeviceInformation)
        {
            GraphicsDeviceInformation = graphicsDeviceInformation;
        }

        /// <summary>
        /// The default settings that will be used in device creation.
        /// </summary>
        public GraphicsDeviceInformation GraphicsDeviceInformation { get; private set; }
    }
}

