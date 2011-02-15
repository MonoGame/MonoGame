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
using OpenTK.Graphics.ES20;

namespace Microsoft.Xna.Framework.Graphics
{
	public class RenderTarget2D : RenderTarget
	{
		private Texture2D texture = null;
		private uint textureFrameBuffer;
		
		public RenderTarget2D (
         GraphicsDevice graphicsDevice,
         int width,
         int height,
         int numberLevels,
         SurfaceFormat format
		                       )
		{
			// throw new NotImplementedException();
			
			texture = new Texture2D( graphicsDevice, width, height, numberLevels, TextureUsage.None, format );

			// create framebuffer
			GL.GenFramebuffers(1, ref textureFrameBuffer);
			GL.BindFramebuffer(All.Framebuffer, textureFrameBuffer);
			
			// attach renderbuffer
			GL.FramebufferTexture2D(All.Framebuffer, All.ColorAttachment0, All.Texture2D, textureFrameBuffer, 0);
			
			// attach depth buffer
			/* uint depthRenderbuffer;
			GL.GenRenderbuffers(1, ref depthRenderbuffer);
			GL.BindRenderbuffer(All.Renderbuffer, depthRenderbuffer);
			GL.RenderbufferStorage(All.Renderbuffer, All.DepthComponent16, width, height);
			GL.FramebufferRenderbuffer(All.Framebuffer, All.DepthAttachment, All.Renderbuffer, depthRenderbuffer);*/
			
			// unbind frame buffer
			GL.BindFramebuffer(All.Framebuffer, 0);

		}
		
		public RenderTarget2D (
         GraphicsDevice graphicsDevice,
         int width,
         int height,
         int numberLevels,
         SurfaceFormat format,
         MultiSampleType multiSampleType,
         int multiSampleQuality
		                       )
		{
			throw new NotImplementedException();
		}
		
		public RenderTarget2D (
         GraphicsDevice graphicsDevice,
         int width,
         int height,
         int numberLevels,
         SurfaceFormat format,
         MultiSampleType multiSampleType,
         int multiSampleQuality,
         RenderTargetUsage usage )
		{
			throw new NotImplementedException();
		}
		
		public RenderTarget2D (
         GraphicsDevice graphicsDevice,
         int width,
         int height,
         int numberLevels,
         SurfaceFormat format,
         RenderTargetUsage usage
		                       )
		{
			throw new NotImplementedException();
		}
		
		public Texture2D GetTexture()
		{
			return texture;
		}
		
		public int Width 
		{ 
			get; 
			set; 
		}
		
		public int Height 
		{ 
			get; 
			set; 
		}
	}
}
