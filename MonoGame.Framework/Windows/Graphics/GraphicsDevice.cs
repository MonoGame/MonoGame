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

using OpenTK.Graphics.OpenGL;

using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{	
    public class GraphicsDevice : IDisposable
    {
		private All _preferedFilter;
		private int _activeTexture = -1;
		private Viewport _viewport;
		private bool _isDisposed = false;
		private DisplayMode _displayMode;
		private RenderState _renderState;
        internal GraphicsDeviceManager mngr;

        private BlendState _blendState = BlendState.Opaque;
        private DepthStencilState _depthStencilState = DepthStencilState.Default;
        private SamplerStateCollection _samplerStates = new SamplerStateCollection();
        internal List<IntPtr> _pointerCache = new List<IntPtr>();
        private VertexBuffer _vertexBuffer = null;
        private IndexBuffer _indexBuffer = null;
        public TextureCollection Textures { get; set; }

        private RenderTargetBinding[] currentRenderTargets;        
        
        public RasterizerState RasterizerState { get; set; }        

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

        public bool IsContentLost
        {
            get
            {
                // We will just return IsDisposed for now
                // as that is the only case I can see for now
                return IsDisposed;
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
            Textures = new TextureCollection();

            // Init RenderState
            RasterizerState = new RasterizerState();
        }

        internal GraphicsDevice(GraphicsDeviceManager mngr)
        {
            this.mngr = mngr;
            _displayMode = new DisplayMode(this.mngr.PreferredBackBufferWidth, this.mngr.PreferredBackBufferHeight);
            Textures = new TextureCollection();
            // Init RenderState
            _renderState = new RenderState();

            SizeChanged(this.mngr.PreferredBackBufferWidth, this.mngr.PreferredBackBufferHeight);
        }

        internal void SizeChanged(int width, int height)
        {
            _displayMode = new DisplayMode(width, height);

            // Initialize the main viewport
            _viewport = new Viewport();
            _viewport.X = 0;
            _viewport.Y = 0;
            _viewport.Width = DisplayMode.Width;
            _viewport.Height = DisplayMode.Height;            

            if (PresentationParameters != null) {
                PresentationParameters.BackBufferWidth = width;
                PresentationParameters.BackBufferHeight = height;
            }
        }

        public BlendState BlendState
        {
            get { return _blendState; }
            set
            {
                // ToDo check for invalid state
                _blendState = value;
            }
        }

        public DepthStencilState DepthStencilState
        {
            get { return _depthStencilState; }
            set
            {
                _depthStencilState = value;
            }
        }

        public SamplerStateCollection SamplerStates
        {
            get
            {
                var temp = _samplerStates;
                return temp;
            }
        }

        public void Clear(Color color)
        {
            Vector4 vector = color.ToEAGLColor();
            // The following was not working with Color.Transparent
            // Once we get some regression tests take the following out			
            //GL.ClearColor (vector.X, vector.Y, vector.Z, 1.0f);
            GL.ClearColor(vector.X, vector.Y, vector.Z, vector.W);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Clear(options, color.ToEAGLColor(), depth, stencil);
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            GL.ClearColor(color.X, color.Y, color.Z, color.W);
            GL.ClearDepth(depth);
            GL.ClearStencil(stencil);
            GL.Clear((ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));
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
                return _displayMode;
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
						var x = _scissorRectangle.X;
						_scissorRectangle.X = _scissorRectangle.Y;
						_scissorRectangle.Y = x;
						var w = _scissorRectangle.Width;
						_scissorRectangle.Width = _scissorRectangle.Height;
						_scissorRectangle.Height = w;
						break;
					}					
					
					case DisplayOrientation.PortraitUpsideDown :
					{		
						_scissorRectangle.Y = _scissorRectangle.X;
						_scissorRectangle.X = _viewport.Width - _scissorRectangle.X - _scissorRectangle.Width;
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
				
		public BeginMode PrimitiveTypeGL11(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return BeginMode.Lines;
                case PrimitiveType.LineStrip:
                    return BeginMode.LineStrip;
                case PrimitiveType.TriangleList:
                    return BeginMode.Triangles;
                case PrimitiveType.TriangleStrip:
                    return BeginMode.TriangleStrip;
            }

            throw new NotImplementedException();
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            _vertexBuffer = vertexBuffer;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer._bufferStore);
        }

        private void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            _indexBuffer = indexBuffer;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer._bufferStore);
        }

        public IndexBuffer Indices { set { SetIndexBuffer(value); } }

        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numbVertices, int startIndex, int primitiveCount)
        {
            if (minVertexIndex > 0 || baseVertex > 0)
                throw new NotImplementedException("baseVertex > 0 and minVertexIndex > 0 are not supported");

            var vd = VertexDeclaration.FromType(_vertexBuffer._type);
            // Hmm, can the pointer here be changed with baseVertex?
            VertexDeclaration.PrepareForUse(vd);

            GL.DrawElements(PrimitiveTypeGL11(primitiveType), _indexBuffer._count, DrawElementsType.UnsignedShort, startIndex);
        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {
            // Unbind the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            var vd = VertexDeclaration.FromType(typeof(T));

            IntPtr arrayStart = GCHandle.Alloc(vertexData, GCHandleType.Pinned).AddrOfPinnedObject();

            if (vertexOffset > 0)
                arrayStart = new IntPtr(arrayStart.ToInt32() + (vertexOffset * vd.VertexStride));

            VertexDeclaration.PrepareForUse(vd);

            GL.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexOffset, getElementCountArray(primitiveType, primitiveCount));
        }

        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            var vd = VertexDeclaration.FromType(_vertexBuffer._type);
            VertexDeclaration.PrepareForUse(vd);

            GL.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexStart, getElementCountArray(primitiveType, primitiveCount));
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, int[] indexData, int indexOffset, int primitiveCount) where T : IVertexType
        {
            // NOT TESTED

            if (indexOffset > 0 || vertexOffset > 0)
                throw new NotImplementedException("vertexOffset and indexOffset is not yet supported.");

            // Unload the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            var vd = VertexDeclaration.FromType(typeof(T));

            IntPtr arrayStart = GCHandle.Alloc(vertexData, GCHandleType.Pinned).AddrOfPinnedObject();
            if (vertexOffset > 0)
                arrayStart = new IntPtr(arrayStart.ToInt32() + vertexOffset);
            VertexDeclaration.PrepareForUse(vd);

            GL.DrawElements(PrimitiveTypeGL11(primitiveType), vertexCount, DrawElementsType.UnsignedShort, indexData);
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

        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget == null) 
			{
				// Detach the render buffers.
				GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, 0);
				// delete the RBO's
				GL.DeleteRenderbuffers(renderBufferIDs.Length,renderBufferIDs);
				// delete the FBO
				GL.DeleteFramebuffers(1, ref framebufferId);
				// Set the frame buffer back to the system window buffer
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			}
			else {
				SetRenderTargets(new RenderTargetBinding(renderTarget));
			}
        }
        
        private int framebufferId = -1;
		int[] renderBufferIDs;
		
		public void SetRenderTargets (params RenderTargetBinding[] renderTargets) 
		{
			
			currentRenderTargets = renderTargets;
			
			if (currentRenderTargets != null) {
				
				// http://www.songho.ca/opengl/gl_fbo.html
				
				// create framebuffer
				GL.GenFramebuffers(1, out framebufferId);
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
				
				renderBufferIDs = new int[currentRenderTargets.Length];
				GL.GenRenderbuffers(currentRenderTargets.Length, renderBufferIDs);
				
				for (int i = 0; i < currentRenderTargets.Length; i++) {
					RenderTarget2D target = (RenderTarget2D)currentRenderTargets[0].RenderTarget;
					
					// attach the texture to FBO color attachment point
					GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
					                        TextureTarget.Texture2D, (int)target.ID,0);
					
					// create a renderbuffer object to store depth info
					GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBufferIDs[i]);
					GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24,
						target.Width, target.Height);
					GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBufferIDs[i]);
					
					// attach the renderbuffer to depth attachment point
					GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment
					                           , RenderbufferTarget.Renderbuffer, renderBufferIDs[i]);
						
				}
				
				FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
				
				if (status != FramebufferErrorCode.FramebufferComplete)
					throw new Exception("Error creating framebuffer: " + status);				
				
			}
			
			
		}
    }
}

