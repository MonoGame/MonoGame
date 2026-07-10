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
    }
}
