// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#else
using OpenGL;
#endif
#elif DESKTOPGL
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
#elif GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        internal int glColorBuffer;
        internal int glDepthBuffer;
        internal int glStencilBuffer;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            Threading.BlockOnUIThread(() =>
            {
                graphicsDevice.PlatformCreateRenderTarget(this, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage);
            });
            
            
        }

        private void PlatformGraphicsDeviceResetting()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Threading.BlockOnUIThread(() =>
                {
                    this.GraphicsDevice.PlatformDeleteRenderTarget(this);
                });
            }

            base.Dispose(disposing);
        }
    }
}
