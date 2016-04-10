// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#else
using OpenGL;
using OpenTK.Graphics.OpenGL;
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
    public partial class RenderTargetCube
    {
        int IRenderTarget.GLTexture
        {
            get { return glTexture; }
        }

        TextureTarget IRenderTarget.GLTarget
        {
            get { return glTarget; }
        }

        int IRenderTarget.GLColorBuffer { get; set; }
        int IRenderTarget.GLDepthBuffer { get; set; }
        int IRenderTarget.GLStencilBuffer { get; set; }

        TextureTarget IRenderTarget.GetFramebufferTarget(RenderTargetBinding renderTargetBinding)
        {
            return TextureTarget.TextureCubeMapPositiveX + renderTargetBinding.ArraySlice;
        }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            Threading.BlockOnUIThread(() =>
            {
                graphicsDevice.PlatformCreateRenderTarget(this, size, size, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage);
            });
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
