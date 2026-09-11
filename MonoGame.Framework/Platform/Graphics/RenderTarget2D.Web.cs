// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        private void PlatformConstruct(
            GraphicsDevice graphicsDevice, 
            int width, 
            int height, 
            bool mipMap,
            DepthFormat preferredDepthFormat,
            int preferredMultiSampleCount,
            RenderTargetUsage usage, 
            bool shared)
        {
            throw new NotImplementedException();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates a <see cref="RenderTarget2D"/> that wraps an externally-owned native graphics handle.
        /// </summary>
        /// <param name="graphicsDevice">The MonoGame graphics device.</param>
        /// <param name="handle">The native image handle.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="format">The surface format of the native image.</param>
        /// <param name="preferredDepthFormat">The preferred depth format of the render target.<para><see cref="DepthFormat.None"/> by default.</para></param>
        /// <param name="preferredMultiSampleCount">The preferred number of samples per pixel when multisampling.<para><c>0</c> by default.</para></param>
        /// <returns>A non-owning <see cref="RenderTarget2D"/> backed by the native resource handle.</returns>
        public static RenderTarget2D FromNativeHandle(
            GraphicsDevice graphicsDevice,
            nint handle,
            int width,
            int height,
            SurfaceFormat format = SurfaceFormat.Color,
            DepthFormat preferredDepthFormat = DepthFormat.None,
            int preferredMultiSampleCount = 0)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
