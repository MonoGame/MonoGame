// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if PSM
using Sce.PlayStation.Core.Graphics;
#endif

#if DIRECTX
using SharpDX.Direct3D11;
#endif

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using RenderbufferTarget = OpenTK.Graphics.ES20.All;
using RenderbufferStorage = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class RenderTarget2D : Texture2D, IRenderTarget
	{
#if GLES
    const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.Renderbuffer;
    const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
    const RenderbufferStorage GLDepthComponent16NonLinear = (RenderbufferStorage)0x8E2C;
    const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24Oes;
    const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8Oes;
    const RenderbufferStorage GLStencilIndex8 = RenderbufferStorage.StencilIndex8;
#elif OPENGL
    const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.RenderbufferExt;
    const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
    const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24;
    const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8;
    const RenderbufferStorage GLStencilIndex8 = RenderbufferStorage.StencilIndex8;
#endif

#if DIRECTX
        internal RenderTargetView _renderTargetView;
        internal DepthStencilView _depthStencilView;
#endif
#if OPENGL
    internal uint glDepthBuffer;
    internal uint glStencilBuffer;
#endif
#if PSM
        internal FrameBuffer _frameBuffer;
#endif

		public DepthFormat DepthStencilFormat { get; private set; }
		
		public int MultiSampleCount { get; private set; }
		
		public RenderTargetUsage RenderTargetUsage { get; private set; }
		
		public bool IsContentLost { get { return false; } }
		
		public event EventHandler<EventArgs> ContentLost;
		
        private bool SuppressEventHandlerWarningsUntilEventsAreProperlyImplemented()
        {
            return ContentLost != null;
        }

        public RenderTarget2D (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
			:base (graphicsDevice, width, height, mipMap, preferredFormat, SurfaceType.RenderTarget, shared)
		{
			DepthStencilFormat = preferredDepthFormat;
			MultiSampleCount = preferredMultiSampleCount;
			RenderTargetUsage = usage;

            PlatformConstruct(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared);
        }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {

#if DIRECTX
            GenerateIfRequired();
#endif
#if PSM
            _frameBuffer = new FrameBuffer();     
            _frameBuffer.SetColorTarget(_texture2D,0);
#endif

            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;

#if PSM
            throw new NotImplementedException();
#endif
#if OPENGL
      
      
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
#endif
        }
		
		public RenderTarget2D (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
			:this (graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, false)
        {}

		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
			:this (graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) 
		{}
		
		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
			: this(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents) 
		{}

        /// <summary>
        /// Allows child class to specify the surface type, eg: a swap chain.
        /// </summary>        
        protected RenderTarget2D(GraphicsDevice graphicsDevice,
                        int width,
                        int height,
                        bool mipMap,
                        SurfaceFormat format,
                        DepthFormat depthFormat,
                        int preferredMultiSampleCount,
                        RenderTargetUsage usage,
                        SurfaceType surfaceType)
            : base(graphicsDevice, width, height, mipMap, format, surfaceType)
        {
            DepthStencilFormat = depthFormat;
            MultiSampleCount = preferredMultiSampleCount;
            RenderTargetUsage = usage;
		}
#if DIRECTX
        private void GenerateIfRequired()
        {
            if (_renderTargetView != null)
                return;

            // Create a view interface on the rendertarget to use on bind.
            _renderTargetView = new RenderTargetView(GraphicsDevice._d3dDevice, GetTexture());

            // If we don't need a depth buffer then we're done.
            if (DepthStencilFormat == DepthFormat.None)
                return;

            // Setup the multisampling description.
            var multisampleDesc = new SharpDX.DXGI.SampleDescription(1, 0);
            if (MultiSampleCount > 1)
            {
                multisampleDesc.Count = MultiSampleCount;
                multisampleDesc.Quality = (int)StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            // Create a descriptor for the depth/stencil buffer.
            // Allocate a 2-D surface as the depth/stencil buffer.
            // Create a DepthStencil view on this surface to use on bind.
            using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, new Texture2DDescription
            {
                Format = SharpDXHelper.ToFormat(DepthStencilFormat),
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = multisampleDesc,
                BindFlags = BindFlags.DepthStencil,
            }))
            {
                // Create the view for binding to the device.
                _depthStencilView = new DepthStencilView(GraphicsDevice._d3dDevice, depthBuffer,
                    new DepthStencilViewDescription()
                    {
                        Format = SharpDXHelper.ToFormat(DepthStencilFormat),
                        Dimension = DepthStencilViewDimension.Texture2D
                    });
            }
        }

#endif

        protected internal override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
            base.GraphicsDeviceResetting();
        }

        private void PlatformGraphicsDeviceResetting()
        {
#if DIRECTX
            SharpDX.Utilities.Dispose(ref _renderTargetView);
            SharpDX.Utilities.Dispose(ref _depthStencilView);
#endif
        }

		protected override void Dispose(bool disposing)
		{
            if (!IsDisposed)
            {
                PlatformDispose(disposing);
            }
            base.Dispose(disposing);
		}

        private void PlatformDispose(bool disposing)
        {
#if DIRECTX
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _renderTargetView);
                SharpDX.Utilities.Dispose(ref _depthStencilView);
            }
#endif
#if PSM
                _frameBuffer.Dispose();
#endif
#if OPENGL
				GraphicsDevice.AddDisposeAction(() =>
				{
					if (this.glStencilBuffer != 0 && this.glStencilBuffer != this.glDepthBuffer)
				    	GL.DeleteRenderbuffers(1, ref this.glStencilBuffer);
					GL.DeleteRenderbuffers(1, ref this.glDepthBuffer);
					GraphicsExtensions.CheckGLError();
				});
#endif
        }

#if DIRECTX
	    RenderTargetView IRenderTarget.GetRenderTargetView(int arraySlice)
	    {
            GenerateIfRequired();
	        return _renderTargetView;
	    }

	    DepthStencilView IRenderTarget.GetDepthStencilView()
	    {
            GenerateIfRequired();
            return _depthStencilView;
	    }
#endif
	}
}
