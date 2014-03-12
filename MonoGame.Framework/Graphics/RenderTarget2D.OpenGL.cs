// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
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

    internal uint glDepthBuffer;
    internal uint glStencilBuffer;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;

			var glDepthFormat = GLDepthComponent16;
			var glStencilFormat = GLStencilIndex8;
			switch (preferredDepthFormat)
			{
				case DepthFormat.Depth16: 
					glDepthFormat = GLDepthComponent16; 
				break;
#if GLES
				case DepthFormat.Depth24: 
					glDepthFormat = GraphicsCapabilities.SupportsDepth24 ? GLDepthComponent24 : GraphicsCapabilities.SupportsDepthNonLinear ? GLDepthComponent16NonLinear : GLDepthComponent16; 
				break;
				case DepthFormat.Depth24Stencil8:
					glDepthFormat = GraphicsCapabilities.SupportsDepth24 ? GLDepthComponent24 : GraphicsCapabilities.SupportsDepthNonLinear ? GLDepthComponent16NonLinear : GLDepthComponent16;
					glStencilFormat = GLStencilIndex8; 
				break;
#else
				case DepthFormat.Depth24: 
				  	glDepthFormat = GLDepthComponent24;
				break;
				case DepthFormat.Depth24Stencil8:
					glDepthFormat = GLDepthComponent24;
					glStencilFormat = GLStencilIndex8; 
				break;
#endif
			}

            Threading.BlockOnUIThread(() =>
            {

#if GLES
			GL.GenRenderbuffers(1, ref glDepthBuffer);
#else
			GL.GenRenderbuffers(1, out glDepthBuffer);
#endif
			GraphicsExtensions.CheckGLError();
			if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
			{
				if (GraphicsCapabilities.SupportsPackedDepthStencil)
				{
					  this.glStencilBuffer = this.glDepthBuffer;
					  GL.BindRenderbuffer(GLRenderbuffer, this.glDepthBuffer);
					  GraphicsExtensions.CheckGLError();
					  GL.RenderbufferStorage(GLRenderbuffer, GLDepth24Stencil8, this.width, this.height);
					  GraphicsExtensions.CheckGLError();
				}
				else
				{
					GL.BindRenderbuffer(GLRenderbuffer, this.glDepthBuffer);
					GraphicsExtensions.CheckGLError();
					GL.RenderbufferStorage(GLRenderbuffer, glDepthFormat, this.width, this.height);
					GraphicsExtensions.CheckGLError();
#if GLES
					GL.GenRenderbuffers(1, ref glStencilBuffer);
#else
					GL.GenRenderbuffers(1, out glStencilBuffer);
#endif
					GraphicsExtensions.CheckGLError();
					GL.BindRenderbuffer(GLRenderbuffer, this.glStencilBuffer);
					GraphicsExtensions.CheckGLError();
					GL.RenderbufferStorage(GLRenderbuffer, glStencilFormat, this.width, this.height);
					GraphicsExtensions.CheckGLError();
				}
			}
			else
			{
				GL.BindRenderbuffer(GLRenderbuffer, this.glDepthBuffer);
				GraphicsExtensions.CheckGLError();
				GL.RenderbufferStorage(GLRenderbuffer, glDepthFormat, this.width, this.height);
				GraphicsExtensions.CheckGLError();
			}

            });
        }

        private void PlatformGraphicsDeviceResetting()
        {
        }

        private void PlatformDispose(bool disposing)
        {
				GraphicsDevice.AddDisposeAction(() =>
				{
					if (this.glStencilBuffer != 0 && this.glStencilBuffer != this.glDepthBuffer)
				    	GL.DeleteRenderbuffers(1, ref this.glStencilBuffer);
					GL.DeleteRenderbuffers(1, ref this.glDepthBuffer);
					GraphicsExtensions.CheckGLError();
				});
        }
    }
}
