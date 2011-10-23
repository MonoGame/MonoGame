#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK.Graphics.ES11;

using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{	
    public class GraphicsDevice : IDisposable
    {
		private All _preferedFilter;
		private int _activeTexture = -1;
		private Viewport _viewport;
		
		private bool _isDisposed = false;
		public TextureCollection Textures { get; set; }
		private BlendState _blendState = BlendState.Opaque;
		private DepthStencilState _depthStencilState = DepthStencilState.Default;
		private SamplerStateCollection _samplerStates = new SamplerStateCollection ();
        internal List<IntPtr> _pointerCache = new List<IntPtr>();
        private VertexBuffer _vertexBuffer = null;
        private IndexBuffer _indexBuffer = null;

        public RasterizerState RasterizerState { get; set; }
		
		private RenderTargetBinding[] currentRenderTargets;
		
		internal All PreferedFilter 
		{
			get 
			{
				return _preferedFilter;
			}
			set 
			{
				_preferedFilter = value;
			}
		
		}
		
		internal int ActiveTexture
		{
			get 
			{
				return _activeTexture;
			}
			set 
			{
				_activeTexture = value;
			}
		}
		
		public bool IsDisposed 
		{ 
			get
			{
				return _isDisposed;
			}
		}
				
		public GraphicsDevice()
        {	
			// Initialize the main viewport
			_viewport = new Viewport();
			_viewport.X = 0;
			_viewport.Y = 0;						
			_viewport.Width = DisplayMode.Width;
			_viewport.Height = DisplayMode.Height;
			_viewport.MinDepth = 0.0f;
			_viewport.MaxDepth = 1.0f;
			
			// Init RasterizerState
			RasterizerState = new RasterizerState();
        }

		public BlendState BlendState {
			get { return _blendState; }
			set { 
				// ToDo check for invalid state
				_blendState = value;
			}
		}

		public DepthStencilState DepthStencilState {
			get { return _depthStencilState; }
			set { 
				_depthStencilState = value;
			}
		}

		public SamplerStateCollection SamplerStates { 
			get {
				var temp = _samplerStates;
				return temp;
			} 
		}
        public void Clear(Color color)
        {
			Vector4 vector = color.ToEAGLColor();			
			GL.ClearColor (vector.X,vector.Y,vector.Z,vector.W);
			GL.Clear ((uint) All.ColorBufferBit);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
			Clear(options,color.ToEAGLColor(),depth,stencil);
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
			GL.ClearColor(color.X, color.Y, color.Z, color.W);
			GL.ClearDepth(depth);
			GL.ClearStencil(stencil);
			GL.Clear((uint) (ClearBufferMask.ColorBufferBit| ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil, Rectangle[] regions)
        {
			throw new NotImplementedException();
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil, Rectangle[] regions)
        {
			throw new NotImplementedException();
        }

		public void Dispose()
		{
			_isDisposed = true;
		}
		
		protected virtual void Dispose(bool aReleaseEverything)
		{
			if (aReleaseEverything)
			{
				
			}
			
			_isDisposed = true;
		}
		
        public void Present()
        {
			GL.Flush();
        }
		
        public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
  			throw new NotImplementedException();
		}
				
        public void Reset()
        {
			throw new NotImplementedException();
        }

        public void Reset(Microsoft.Xna.Framework.Graphics.PresentationParameters presentationParameters)
        {
			throw new NotImplementedException();
        }

        public void Reset(Microsoft.Xna.Framework.Graphics.PresentationParameters presentationParameters, GraphicsAdapter graphicsAdapter)
        {
			throw new NotImplementedException();
        }

        public Microsoft.Xna.Framework.Graphics.DisplayMode DisplayMode
        {
            get
            {
                return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            }
        }

        public Microsoft.Xna.Framework.Graphics.GraphicsDeviceCapabilities GraphicsDeviceCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Microsoft.Xna.Framework.Graphics.GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Microsoft.Xna.Framework.Graphics.PresentationParameters PresentationParameters
        {
            get;
			set;
        }

        public Microsoft.Xna.Framework.Graphics.Viewport Viewport
        {
            get
            {
				return _viewport;
			}
			set
			{
				_viewport = value;
			}
		}	
		
		public Microsoft.Xna.Framework.Graphics.GraphicsProfile GraphicsProfile 
		{ 
			get; 
			set;
		}	
		
		public VertexDeclaration VertexDeclaration 
		{ 
			get; 
			set; 
		}
			
		Rectangle _scissorRectangle;
		public Rectangle ScissorRectangle 
		{ 
			get
			{
				return _scissorRectangle;
			}
			set
			{
				_scissorRectangle = value;
				
				switch (this.PresentationParameters.DisplayOrientation )
				{
					case DisplayOrientation.Portrait :
					{	
						_scissorRectangle.Y = _viewport.Height - _scissorRectangle.Y - _scissorRectangle.Height;
						break;
					}
					
					case DisplayOrientation.LandscapeLeft :
					{								
						var x = _scissorRectangle.X;
						_scissorRectangle.X = _viewport.Width - _scissorRectangle.Height - _scissorRectangle.Y;
						_scissorRectangle.Y = _viewport.Height - _scissorRectangle.Width - x;
					
						// Swap Width and Height
						var w = _scissorRectangle.Width;
						_scissorRectangle.Width = _scissorRectangle.Height;
						_scissorRectangle.Height = w;	
						break;
					}
					
					case DisplayOrientation.LandscapeRight :
					{			
						// Swap X and Y
						var x = _scissorRectangle.X;
						_scissorRectangle.X = _scissorRectangle.Y;
						_scissorRectangle.Y = x;
						
						// Swap Width and Height
						var w = _scissorRectangle.Width;
						_scissorRectangle.Width = _scissorRectangle.Height;
						_scissorRectangle.Height = w;
						break;
					}					
					
					case DisplayOrientation.PortraitUpsideDown :
					{		
						_scissorRectangle.Y = _viewport.Height - _scissorRectangle.Height - _scissorRectangle.Y;
						_scissorRectangle.X = _viewport.Width - _scissorRectangle.Width - _scissorRectangle.X;
						break;
					}
					
					case DisplayOrientation.Default :
					{
						_scissorRectangle.Y = _viewport.Height - _scissorRectangle.Y - _scissorRectangle.Height;
						break;
					}
				}
			}
		}
		
		public void SetRenderTarget (RenderTarget2D renderTarget)
		{
			// We check if the rendertarget being passed is null or if we already have a rendertarget
			// NetRumble sample does not set the the renderTarget to null before setting another
			// rendertarget.  We handle that by checking first if we have a current render target set
			// if we do then we unbind the current rendertarget, reset the viewport and set the
			// rendertarget to the new one being passed if it is not null
			if (renderTarget == null || currentRenderTargets != null)
			{
#if ANDROID
                byte[] imageInfo = new byte[4];
                GL.ReadPixels(0, 0, 1, 1, All.Rgba, All.UnsignedByte, imageInfo);
#endif
				// Detach the render buffers.
				GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.DepthAttachmentOes,
						All.RenderbufferOes, 0);
				
				// delete the RBO's
				GL.Oes.DeleteRenderbuffers(renderBufferIDs.Length,renderBufferIDs);
				
				// delete the FBO
				GL.Oes.DeleteFramebuffers(frameBufferIDs.Length, frameBufferIDs);
				
				// Set the frame buffer back to the system window buffer
				GL.Oes.BindFramebuffer(All.FramebufferOes, originalFbo);

				// We need to reset our GraphicsDevice viewport back to what it was
				// before rendering.
				Viewport = savedViewport;

				if (renderTarget == null)
					currentRenderTargets = null;
				else {
					SetRenderTargets(new RenderTargetBinding(renderTarget));
				}
			}
			else {
				SetRenderTargets(new RenderTargetBinding(renderTarget));
			}
		}
		
		int[] frameBufferIDs;
		int[] renderBufferIDs;
		int originalFbo = -1;
		
		// TODO: We need to come up with a state save and restore of the GraphicsDevice
		//  This would probably work with a Stack that allows pushing and popping of the current
		//  Graphics device state.
		//  Right now here is the list of state values that should be implemented
		//  Viewport - Used for RenderTargets
		//  Depth and Stencil formats	- To be determined
		Viewport savedViewport;
		
		public void SetRenderTargets (params RenderTargetBinding[] renderTargets) 
		{
			
			currentRenderTargets = renderTargets;
			
			if (currentRenderTargets != null) 
			{				
				// TODO: For speed we need to consider using FBO switching instead
				// of multiple FBO's if they are the same size.
				
				// http://www.songho.ca/opengl/gl_fbo.html
				
				// Get the currently bound frame buffer object. On most platforms this just gives 0.				
				GL.GetInteger(All.FramebufferBindingOes, ref originalFbo);
				
				frameBufferIDs = new int[currentRenderTargets.Length];
				
				renderBufferIDs = new int[currentRenderTargets.Length];
				GL.Oes.GenRenderbuffers(currentRenderTargets.Length, renderBufferIDs);
				
				for (int i = 0; i < currentRenderTargets.Length; i++) 
				{
					RenderTarget2D target = (RenderTarget2D)currentRenderTargets[i].RenderTarget;
					
					// create a renderbuffer object to store depth info
					GL.Oes.BindRenderbuffer(All.RenderbufferOes, renderBufferIDs[i]);
					
					ClearOptions clearOptions = ClearOptions.Target | ClearOptions.DepthBuffer;
					
					switch (target.DepthStencilFormat) 
					{
						case DepthFormat.Depth16:
							GL.Oes.RenderbufferStorage(All.RenderbufferOes, All.DepthComponent16Oes,
							target.Width, target.Height);
							break;
						case DepthFormat.Depth24:
							GL.Oes.RenderbufferStorage(All.RenderbufferOes, All.DepthComponent24Oes,
							target.Width, target.Height);
							break;
						case DepthFormat.Depth24Stencil8:
							GL.Oes.RenderbufferStorage(All.RenderbufferOes, All.Depth24Stencil8Oes,
							target.Width, target.Height);
							GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.StencilAttachmentOes,
							All.RenderbufferOes, renderBufferIDs[i]);
							clearOptions = clearOptions | ClearOptions.Stencil;
							break;
						default :
							GL.Oes.RenderbufferStorage(All.RenderbufferOes, All.DepthComponent24Oes,
							target.Width, target.Height);
							break;
					}					
					
					// create framebuffer
					GL.Oes.GenFramebuffers(1, ref frameBufferIDs[i]);
					GL.Oes.BindFramebuffer(All.FramebufferOes, frameBufferIDs[i]);
					
					// attach the texture to FBO color attachment point
					GL.Oes.FramebufferTexture2D(All.FramebufferOes, All.ColorAttachment0Oes, All.Texture2D, target.ID, 0);
					
					// attach the renderbuffer to depth attachment point
					GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.DepthAttachmentOes,
							All.RenderbufferOes, renderBufferIDs[i]);							
					
					if (target.RenderTargetUsage == RenderTargetUsage.DiscardContents)
						Clear (clearOptions, Color.Transparent, 0, 0);
					
					GL.Oes.BindRenderbuffer(All.FramebufferOes, originalFbo);
						
				}
				
				All status = GL.Oes.CheckFramebufferStatus(All.FramebufferOes);
				
				if (status != All.FramebufferCompleteOes)
					throw new Exception("Error creating framebuffer: " + status);
				
				// We need to start saving off the ViewPort and setting the current ViewPort to
				// the width and height of the texture.  Then when we pop off the rendertarget
				// it needs to be reset.  This causes drawing problems if we do not set the viewport.
				// Makes sense once you follow the flow (hits head on desk)
				// For an example of this take a look at NetRumble's sample for the BloomPostprocess
               
				// Save off the current viewport to be reset later
				savedViewport = Viewport;

				// Create a new Viewport
				Viewport renderTargetViewPort = new Viewport();

				// Set the new viewport to the width and height of the render target
				Texture2D target2 = (Texture2D)currentRenderTargets[0].RenderTarget;
				renderTargetViewPort.Width = target2.Width;
				renderTargetViewPort.Height = target2.Height;

				// now we set our viewport to the new rendertarget viewport just created.
				Viewport = renderTargetViewPort;				
			}
		}
		
		public void ResolveBackBuffer( ResolveTexture2D resolveTexture )
		{
		}
		
        public All PrimitiveTypeGL11(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return All.Lines;
                case PrimitiveType.LineStrip:
                    return All.LineStrip;
                case PrimitiveType.TriangleList:
                    return All.Triangles;
                case PrimitiveType.TriangleStrip:
                    return All.TriangleStrip;
            }

            throw new NotImplementedException();
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            _vertexBuffer = vertexBuffer;
            GL.BindBuffer(All.ArrayBuffer, vertexBuffer._bufferStore);
        }

        private void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            _indexBuffer = indexBuffer;
            GL.BindBuffer(All.ElementArrayBuffer, indexBuffer._bufferStore);
        }

        public IndexBuffer Indices { set { SetIndexBuffer(value); } }

        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numbVertices, int startIndex, int primitiveCount)
        {
            if (minVertexIndex > 0 || baseVertex > 0)
                throw new NotImplementedException("baseVertex > 0 and minVertexIndex > 0 are not supported");

            var vd = VertexDeclaration.FromType(_vertexBuffer._type);
            // Hmm, can the pointer here be changed with baseVertex?
            VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

            GL.DrawElements(PrimitiveTypeGL11(primitiveType), _indexBuffer._count, All.UnsignedShort, new IntPtr(startIndex));
        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {
            // Unbind the VBOs
            GL.BindBuffer(All.ArrayBuffer, 0);
            GL.BindBuffer(All.ElementArrayBuffer, 0);
			
            var vd = VertexDeclaration.FromType(typeof(T));

            IntPtr arrayStart = GCHandle.Alloc(vertexData, GCHandleType.Pinned).AddrOfPinnedObject();

            if (vertexOffset > 0)
                arrayStart = new IntPtr(arrayStart.ToInt32() + (vertexOffset * vd.VertexStride));

            VertexDeclaration.PrepareForUse(vd, arrayStart);

            GL.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexOffset, getElementCountArray(primitiveType, primitiveCount));
        }

        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            var vd = VertexDeclaration.FromType(_vertexBuffer._type);
            VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

            GL.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexStart, getElementCountArray(primitiveType, primitiveCount));
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, int[] indexData, int indexOffset, int primitiveCount) where T : IVertexType
        {
            // Unbind the VBOs
            GL.BindBuffer(All.ArrayBuffer, 0);
            GL.BindBuffer(All.ElementArrayBuffer, 0);

            var vd = VertexDeclaration.FromType(typeof(T));

            IntPtr arrayStart = GCHandle.Alloc(vertexData, GCHandleType.Pinned).AddrOfPinnedObject();

            if (vertexOffset > 0)
                arrayStart = new IntPtr(arrayStart.ToInt32() + (vertexOffset * vd.VertexStride));

            VertexDeclaration.PrepareForUse(vd, arrayStart);

            GL.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexOffset, getElementCountArray(primitiveType, primitiveCount));
        }

        public int getElementCountArray(PrimitiveType primitiveType, int primitiveCount)
        {
            //TODO: Overview the calculation
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return primitiveCount * 2;
                case PrimitiveType.LineStrip:
                    return 3 + (primitiveCount - 1); // ???
                case PrimitiveType.TriangleList:
                    return primitiveCount * 2;
                case PrimitiveType.TriangleStrip:
                    return 3 + (primitiveCount - 1); // ???
            }

            throw new NotSupportedException();
        }

	}
}

