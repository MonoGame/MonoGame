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
	public class RenderTarget2D : Texture2D
	{
#if GLES
		const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.Renderbuffer;
		const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
		const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24Oes;
		const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8Oes;
#elif OPENGL
		const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.RenderbufferExt;
		const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
		const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24;
		const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8;
#endif

#if DIRECTX
        internal SharpDX.Direct3D11.RenderTargetView _renderTargetView;
        internal SharpDX.Direct3D11.DepthStencilView _depthStencilView;
#elif OPENGL
		internal uint glDepthStencilBuffer;
#endif

		public DepthFormat DepthStencilFormat { get; private set; }
		
		public int MultiSampleCount { get; private set; }
		
		public RenderTargetUsage RenderTargetUsage { get; private set; }
		
		public RenderTarget2D (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
			:base (graphicsDevice, width, height, mipMap, preferredFormat, true)
		{
			DepthStencilFormat = preferredDepthFormat;
			MultiSampleCount = preferredMultiSampleCount;
			RenderTargetUsage = usage;

#if DIRECTX
            // Create a view interface on the rendertarget to use on bind.
            _renderTargetView = new SharpDX.Direct3D11.RenderTargetView(graphicsDevice._d3dDevice, _texture);
#endif

            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;

#if DIRECTX

            // Setup the multisampling description.
            var multisampleDesc = new SharpDX.DXGI.SampleDescription(1, 0);
            if ( preferredMultiSampleCount > 1 )
            {
                multisampleDesc.Count = preferredMultiSampleCount;
                multisampleDesc.Quality = (int)SharpDX.Direct3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            // Create a descriptor for the depth/stencil buffer.
            // Allocate a 2-D surface as the depth/stencil buffer.
            // Create a DepthStencil view on this surface to use on bind.
            using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(graphicsDevice._d3dDevice, new SharpDX.Direct3D11.Texture2DDescription()
            {
                Format = SharpDXHelper.ToFormat(preferredDepthFormat),
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = multisampleDesc,
                BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
            }))

            // Create the view for binding to the device.
            _depthStencilView = new SharpDX.Direct3D11.DepthStencilView(graphicsDevice._d3dDevice, depthBuffer,
                new SharpDX.Direct3D11.DepthStencilViewDescription() 
                { 
                    Format = SharpDXHelper.ToFormat(preferredDepthFormat),
                    Dimension = SharpDX.Direct3D11.DepthStencilViewDimension.Texture2D 
                });

#elif OPENGL

#if GLES
			GL.GenRenderbuffers(1, ref glDepthStencilBuffer);
#else
			GL.GenRenderbuffers(1, out glDepthStencilBuffer);
#endif
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, this.glDepthStencilBuffer);
			var glDepthStencilFormat = GLDepthComponent16;
			switch (preferredDepthFormat)
			{
			case DepthFormat.Depth16: glDepthStencilFormat = GLDepthComponent16; break;
			case DepthFormat.Depth24: glDepthStencilFormat = GLDepthComponent24; break;
			case DepthFormat.Depth24Stencil8: glDepthStencilFormat = GLDepth24Stencil8; break;
			}
			GL.RenderbufferStorage(GLRenderbuffer, glDepthStencilFormat, this.width, this.height);
#endif
        }
		
		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
			:this (graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) 
		{}
		
		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
			: this(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents) 
		{}

		public override void Dispose ()
		{
#if DIRECTX
            if (_renderTargetView != null)
            {
                _renderTargetView.Dispose();
                _renderTargetView = null;
            }
            if (_depthStencilView != null)
            {
                _depthStencilView.Dispose();
                _depthStencilView = null;
            }
#elif OPENGL
			GL.DeleteRenderbuffers(1, ref this.glDepthStencilBuffer);
#endif
            base.Dispose();
		}
	}
}
