// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
#elif GLES
using OpenTK.Graphics.ES20;
using RenderbufferTarget = OpenTK.Graphics.ES20.All;
using RenderbufferStorage = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
#if GLES
    const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.Renderbuffer;
    const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
    const RenderbufferStorage GLDepthComponent16NonLinear = (RenderbufferStorage)0x8E2C;
    const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24Oes;
    const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8Oes;
    const RenderbufferStorage GLStencilIndex8 = RenderbufferStorage.StencilIndex8;
#else
    const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.RenderbufferExt;
    const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
    const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24;
    const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8;
    const RenderbufferStorage GLStencilIndex8 = RenderbufferStorage.StencilIndex8;
#endif

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
                GraphicsDevice.AddDisposeAction(() =>
                {
                    this.GraphicsDevice.PlatformDeleteRenderTarget(this);
                });
            }

            base.Dispose(disposing);
        }
    }
}
