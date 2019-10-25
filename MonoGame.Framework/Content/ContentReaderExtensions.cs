// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    public static class ContentReaderExtensions
    {
        /// <summary>
        /// Gets the GraphicsDevice from the ContentManager.ServiceProvider.
        /// </summary>
        /// <returns>The <see cref="GraphicsDevice"/>.</returns>
        public static GraphicsDevice GetGraphicsDevice(this ContentReader contentReader)
        {
            var serviceProvider = contentReader.ContentManager.ServiceProvider;
            var graphicsDeviceService = serviceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
            if (graphicsDeviceService == null)
                throw new InvalidOperationException("No Graphics Device Service");

            return graphicsDeviceService.GraphicsDevice;
        }
    }
}
