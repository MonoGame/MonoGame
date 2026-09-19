// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        private static Action<RenderTarget2D> DisposeAction =
            (t) => t.GraphicsDevice.PlatformDeleteRenderTarget(t);

        private bool _isExternal;

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
            return glTarget;
        }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            Threading.BlockOnUIThread(() =>
            {
                graphicsDevice.PlatformCreateRenderTarget(
                    this, width, height, mipMap, this.Format, preferredDepthFormat, preferredMultiSampleCount, usage);
            });
        }

        private void PlatformGraphicsDeviceResetting()
        {
            if (_isExternal)
            {
                glTexture = -1;
            }
        }

        /// <summary/>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                {
                    Threading.BlockOnUIThread(DisposeAction, this);
                }

                if (_isExternal)
                {
                    // Disassociate glTexture.
                    // This is to prevent base.Dispose() from invoking GraphicsDevice.DisposeTexture() on the external texture.
                    glTexture = -1;
                }
            }

            base.Dispose(disposing);
        }

        private static RenderTarget2D PlatformFromNativeHandle(
            GraphicsDevice graphicsDevice,
            nint handle,
            int width,
            int height,
            SurfaceFormat format = SurfaceFormat.Color,
            DepthFormat preferredDepthFormat = DepthFormat.None,
            int preferredMultiSampleCount = 0)
        {
            var renderTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                format,
                preferredDepthFormat,
                preferredMultiSampleCount,
                RenderTargetUsage.DiscardContents,
                SurfaceType.SwapChainRenderTarget)
            {
                _isExternal = true,
                glTexture = (int)handle,
                glTarget = TextureTarget.Texture2D,
            };

            format.GetGLFormat(graphicsDevice, out renderTarget.glInternalFormat, out renderTarget.glFormat, out renderTarget.glType);

            if (preferredDepthFormat != DepthFormat.None || renderTarget.MultiSampleCount > 0)
            {
                Threading.BlockOnUIThread(() =>
                {
                    graphicsDevice.PlatformCreateRenderTarget(
                        renderTarget,
                        width,
                        height,
                        false,
                        format,
                        preferredDepthFormat,
                        preferredMultiSampleCount,
                        RenderTargetUsage.DiscardContents);
                });
            }

            return renderTarget;
        }
    }
}
