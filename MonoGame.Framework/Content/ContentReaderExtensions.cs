// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// Helper extension methods for <see cref="ContentReader"/>.
    /// </summary>
    public static class ContentReaderExtensions
    {
        /// <summary>
        /// Returns the <see cref="GraphicsDevice"/> instance from the service provider of the
        /// <see cref="ContentManager"/> associated with this content reader.
        /// </summary>
        /// <returns>The <see cref="GraphicsDevice"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ContentManager.ServiceProvider">ContentManager.ServiceProvider</see> does not contain a
        /// <see cref="GraphicsDevice"/> instance.
        /// </exception>
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
