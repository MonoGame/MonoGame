#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

using System;
#if PSM
using Sce.PlayStation.Core.Graphics;
#elif DIRECTX
using SharpDX.Direct3D11;
#elif OPENGL
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
#elif OPENGL
    internal uint glDepthBuffer;
    internal uint glStencilBuffer;
#elif PSM
        internal FrameBuffer _frameBuffer;
#endif

		public DepthFormat DepthStencilFormat { get; private set; }
		
		public int MultiSampleCount { get; private set; }
		
		public RenderTargetUsage RenderTargetUsage { get; private set; }
		
		public bool IsContentLost { get { return false; } }
		
		public event EventHandler<EventArgs> ContentLost;
		
        public RenderTarget2D (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
			:base (graphicsDevice, width, height, mipMap, preferredFormat, SurfaceType.RenderTarget, shared)
		{
			DepthStencilFormat = preferredDepthFormat;
			MultiSampleCount = preferredMultiSampleCount;
			RenderTargetUsage = usage;

#if DIRECTX

            GenerateIfRequired();

#elif PSM
            _frameBuffer = new FrameBuffer();     
            _frameBuffer.SetColorTarget(_texture2D,0);
#endif

            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;

#if DIRECTX
#elif PSM
            throw new NotImplementedException();
#elif OPENGL
      
      
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
#if DIRECTX
            SharpDX.Utilities.Dispose(ref _renderTargetView);
            SharpDX.Utilities.Dispose(ref _depthStencilView);
#endif
            base.GraphicsDeviceResetting();
        }

		protected override void Dispose(bool disposing)
		{
            if (!IsDisposed)
            {
#if DIRECTX
                if (disposing)
                {
                    SharpDX.Utilities.Dispose(ref _renderTargetView);
                    SharpDX.Utilities.Dispose(ref _depthStencilView);
                }
#elif PSM
                _frameBuffer.Dispose();
#elif OPENGL
				GraphicsDevice.AddDisposeAction(() =>
				{
					if (this.glStencilBuffer != 0 && this.glStencilBuffer != this.glDepthBuffer)
				    	GL.DeleteRenderbuffers(1, ref this.glStencilBuffer);
					GL.DeleteRenderbuffers(1, ref this.glDepthBuffer);
					GraphicsExtensions.CheckGLError();
				});
#endif
            }
            base.Dispose(disposing);
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
