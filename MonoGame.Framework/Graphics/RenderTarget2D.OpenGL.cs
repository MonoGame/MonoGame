// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

    internal int glDepthBuffer;
    internal int glStencilBuffer;

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

			glDepthBuffer = GraphicsDevice.Renderbuffer.Generate();

			if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
			{
				if (GraphicsCapabilities.SupportsPackedDepthStencil)
				{
					this.glStencilBuffer = this.glDepthBuffer;
					GraphicsDevice.Renderbuffer.Bind(GLRenderbuffer, this.glDepthBuffer);
					GraphicsDevice.Renderbuffer.Storage(GLRenderbuffer, GLDepth24Stencil8, this.width, this.height);
				}
				else
				{
					GraphicsDevice.Renderbuffer.Bind(GLRenderbuffer, this.glDepthBuffer);
					GraphicsDevice.Renderbuffer.Storage(GLRenderbuffer, glDepthFormat, this.width, this.height);

					glStencilBuffer = GraphicsDevice.Renderbuffer.Generate();
					GraphicsDevice.Renderbuffer.Bind(GLRenderbuffer, this.glStencilBuffer);
					GraphicsDevice.Renderbuffer.Storage(GLRenderbuffer, glStencilFormat, this.width, this.height);
				}
			}
			else
			{
				GraphicsDevice.Renderbuffer.Bind(GLRenderbuffer, this.glDepthBuffer);
				GraphicsDevice.Renderbuffer.Storage(GLRenderbuffer, glDepthFormat, this.width, this.height);
			}

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
                    if (this.glStencilBuffer != 0 && this.glStencilBuffer != this.glDepthBuffer)
                    {
                        GraphicsDevice.Renderbuffer.Delete(this.glStencilBuffer);
                    }
                    if (this.glDepthBuffer != 0)
                    {
                        GraphicsDevice.Renderbuffer.Delete(this.glDepthBuffer);
                    }
                });
            }

            base.Dispose(disposing);
        }
    }
}
