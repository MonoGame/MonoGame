// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// The settings used in creation of the graphics device.
    /// See <see cref="GraphicsDeviceManager.PreparingDeviceSettings"/>.
    /// </summary>
    public class GraphicsDeviceInformation
    {	
        /// <summary>
        /// The graphics adapter on which the graphics device will be created.
        /// </summary>
        /// <remarks>
        /// This is only valid on desktop systems where multiple graphics 
        /// adapters are possible.  Defaults to <see cref="GraphicsAdapter.DefaultAdapter"/>.
        /// </remarks>
        public GraphicsAdapter Adapter { get; set; }
        
        /// <summary>
        /// The requested graphics device feature set. 
        /// </summary>
        public GraphicsProfile GraphicsProfile { get; set; }
        
        /// <summary>
        /// The settings that define how graphics will be presented to the display.
        /// </summary>
        public PresentationParameters PresentationParameters { get; set; }
    }
}

