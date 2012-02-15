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
#else
 #if ES11
using OpenTK.Graphics.ES11;
 #else
using OpenTK.Graphics.ES20;
using RenderbufferTarget = OpenTK.Graphics.ES20.All;
using RenderbufferStorage = OpenTK.Graphics.ES20.All;
 #endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class RenderTarget2D : Texture2D
	{
#if IPHONE && ES11
		const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.RenderbufferOes;
		const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16Oes;
		const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24Oes;
		const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8Oes;
#elif IPHONE || ANDROID
		const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.Renderbuffer;
		const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
		const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24Oes;
		const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8Oes;
#else
		const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.RenderbufferExt;
		const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
		const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24;
		const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8;
#endif


		
		internal uint glDepthStencilBuffer;
		
		public DepthFormat DepthStencilFormat { get; private set; }
		
		public int MultiSampleCount { get; private set; }
		
		public RenderTargetUsage RenderTargetUsage { get; private set; }
		
		public RenderTarget2D (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
			:base (graphicsDevice, width, height, mipMap, preferredFormat)
		{
			this.DepthStencilFormat = preferredDepthFormat;
			this.MultiSampleCount = preferredMultiSampleCount;
			this.RenderTargetUsage = usage;
			
			if (preferredDepthFormat != DepthFormat.None)
			{
#if IPHONE || ANDROID
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
			}
		}
		
		public RenderTarget2D (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
			:this (graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) 
		{}
		
		public RenderTarget2D (GraphicsDevice graphicsDevice, int width, int height)
			: this(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents) 
		{}

		public override void Dispose ()
		{
			GL.DeleteRenderbuffers(1, ref this.glDepthStencilBuffer);
			base.Dispose();
		}
	}
}
