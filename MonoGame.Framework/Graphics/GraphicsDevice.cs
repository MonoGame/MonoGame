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

#if MONOMAC
using MonoMac.OpenGL;
using GL_Oes = MonoMac.OpenGL.GL;
#elif WINDOWS
using OpenTK.Graphics.OpenGL;
using GL_Oes = OpenTK.Graphics.OpenGL.GL;
#else

#if ES11
using OpenTK.Graphics.ES11;
using GL_Oes = OpenTK.Graphics.ES11.GL.Oes;
#if IPHONE
using EnableCap = OpenTK.Graphics.ES11.All;
using TextureTarget = OpenTK.Graphics.ES11.All;
using BufferTarget = OpenTK.Graphics.ES11.All;
using BufferUsageHint = OpenTK.Graphics.ES11.All;
using DrawElementsType = OpenTK.Graphics.ES11.All;
using TextureEnvTarget = OpenTK.Graphics.ES11.All;
using TextureEnvParameter = OpenTK.Graphics.ES11.All;
using GetPName = OpenTK.Graphics.ES11.All;
using FramebufferErrorCode = OpenTK.Graphics.ES11.All;
using FramebufferTarget = OpenTK.Graphics.ES11.All;
using FramebufferAttachment = OpenTK.Graphics.ES11.All;
using RenderbufferTarget = OpenTK.Graphics.ES11.All;
using RenderbufferStorage = OpenTK.Graphics.ES11.All;
#endif
#else
using OpenTK.Graphics.ES20;
#if IPHONE
using EnableCap = OpenTK.Graphics.ES20.All;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
using DrawElementsType = OpenTK.Graphics.ES20.All;
#else
using BufferUsageHint = OpenTK.Graphics.ES20.BufferUsage;
#endif

#endif

#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

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
		private RasterizerState _rasterizerState = RasterizerState.CullCounterClockwise;
        private SamplerStateCollection _samplerStates = new SamplerStateCollection();

        internal List<IntPtr> _pointerCache = new List<IntPtr>();
        private VertexBuffer _vertexBuffer = null;
        private IndexBuffer _indexBuffer = null;
        private uint VboIdArray;
        private uint VboIdElement;

        private RenderTargetBinding[] currentRenderTargets;
		
		// OpenGL ES2.0 attribute locations
		internal static int attributePosition = 0;
		internal static int attributeTexCoord = 1;
		internal static int attributeTint = 2;
		internal static int attributeColor = 3;
		internal static int attributeNormal = 4;
		
#if ES11
		//OpenGL ES1.1 consts
#if IPHONE
		const FramebufferTarget GLFramebuffer = FramebufferTarget.FramebufferOes;
		const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.RenderbufferOes;
		const FramebufferAttachment GLDepthAttachment = FramebufferAttachment.DepthAttachmentOes;
		const FramebufferAttachment GLStencilAttachment = FramebufferAttachment.StencilAttachmentOes;
		const FramebufferAttachment GLColorAttachment0 = FramebufferAttachment.ColorAttachment0Oes;
		const GetPName GLFramebufferBinding = GetPName.FramebufferBindingOes;
		const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16Oes;
		const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24Oes;
		const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8Oes;
		const FramebufferErrorCode GLFramebufferComplete = FramebufferErrorCode.FramebufferCompleteOes;
		
#else
		const FramebufferTarget GLFramebuffer = FramebufferTarget.FramebufferExt;
		const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.RenderbufferExt;
		const FramebufferAttachment GLDepthAttachment = FramebufferAttachment.DepthAttachmentExt;
		const FramebufferAttachment GLStencilAttachment = FramebufferAttachment.StencilAttachment;
		const FramebufferAttachment GLColorAttachment0 = FramebufferAttachment.ColorAttachment0;
		const GetPName GLFramebufferBinding = GetPName.FramebufferBinding;
		const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
		const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24;
		const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8;
		const FramebufferErrorCode GLFramebufferComplete = FramebufferErrorCode.FramebufferComplete;
#endif
#endif
		
		// TODO Graphics Device events need implementing
		public event EventHandler<EventArgs> DeviceLost;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
		//public event EventHandler<ResourceCreatedEventArgs> ResourceCreated;
		//public event EventHandler<ResourceDestroyedEventArgs> ResourceDestroyed;

        public static int FrameBufferScreen;
        public static bool DefaultFrameBuffer = true;

		public RasterizerState RasterizerState {
			get {
				return _rasterizerState;
			}
			set {
				_rasterizerState = value;
				GLStateManager.SetRasterizerStates(value);
			}
		}
		
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
		
		public bool IsContentLost { 
			get {
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

            // Init RasterizerState
            //RasterizerState = new RasterizerState();
        }

        internal void Initialize()
        {
            //Initialize OpenGl states
            GL.Disable(EnableCap.DepthTest);
#if ES11
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.BlendSrc);
#endif
            VboIdArray = 0;
            VboIdElement = 0;
        }

		public BlendState BlendState {
			get { return _blendState; }
			set { 
				// ToDo check for invalid state
				_blendState = value;
				GLStateManager.SetBlendStates(value);
			}
		}

        public DepthStencilState DepthStencilState
        {
            get { return _depthStencilState; }
            set
            {
                _depthStencilState = value;
				GLStateManager.SetDepthStencilState(value);
            }
        }

        public SamplerStateCollection SamplerStates
        {
            get
            {
                //var temp = _samplerStates;
                return _samplerStates;
            }
        }
        public void Clear(Color color)
        {
            Clear (ClearOptions.Target, color.ToVector4(), 0, 0);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Clear (options, color.ToVector4 (), depth, stencil);
        }

		public void Clear (ClearOptions options, Vector4 color, float depth, int stencil)
		{
			GL.ClearColor (color.X, color.Y, color.Z, color.W);
#if IPHONE
			GL.Clear ((uint)CreateClearOptions(options, depth, stencil));
#else
			GL.Clear (CreateClearOptions(options, depth, stencil));
#endif
		}

		private ClearBufferMask CreateClearOptions (ClearOptions clearOptions, float depth, int stencil)
		{
			ClearBufferMask bufferMask = 0;
			if (clearOptions.HasFlag(ClearOptions.Target)) {
				bufferMask = bufferMask | ClearBufferMask.ColorBufferBit;
			}
			if (clearOptions.HasFlag(ClearOptions.Stencil)) {
				GL.ClearStencil (stencil);
				bufferMask = bufferMask | ClearBufferMask.StencilBufferBit;
			}
			if (clearOptions.HasFlag(ClearOptions.DepthBuffer)) {
				GL.ClearDepth (depth);
				bufferMask = bufferMask | ClearBufferMask.DepthBufferBit;
			}

			return bufferMask;
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
			GL.Flush ();
        }

        public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            _viewport.Width = DisplayMode.Width;
            _viewport.Height = DisplayMode.Height;

            if (ResourcesLost)
            {
                ContentManager.ReloadAllContent();
                ResourcesLost = false;
            }

            if(DeviceReset != null)
                DeviceReset(null, new EventArgs());
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
				GL.Viewport (value.X, value.Y, value.Width, value.Height);
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
				
				GLStateManager.SetScissor(_scissorRectangle);
				
				_scissorRectangle.Y = _viewport.Height - _scissorRectangle.Y - _scissorRectangle.Height;
            }
        }

        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
#if ES11
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
                GL_Oes.FramebufferRenderbuffer(
				                           GLFramebuffer,
				                           GLDepthAttachment,
                                           GLRenderbuffer, 0);

                // delete the RBO's
                GL_Oes.DeleteRenderbuffers(renderBufferIDs.Length, renderBufferIDs);

                // delete the FBO
                GL_Oes.DeleteFramebuffers(frameBufferIDs.Length, frameBufferIDs);

                // Set the frame buffer back to the system window buffer
                GL_Oes.BindFramebuffer(GLFramebuffer, originalFbo);

                // We need to reset our GraphicsDevice viewport back to what it was
                // before rendering.
                Viewport = savedViewport;

                if (renderTarget == null)
                    currentRenderTargets = null;
                else
                {
                    SetRenderTargets(new RenderTargetBinding(renderTarget));
                }
            }
            else
            {
                SetRenderTargets(new RenderTargetBinding(renderTarget));
            }

#else
			/*
            if (renderTarget == null)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferScreen);
                DefaultFrameBuffer = true;
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, renderTarget.frameBuffer);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, rendertarget.ID, 0);

                FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (status != FramebufferErrorCode.FramebufferComplete)
                    throw new Exception("GL20: Error creating framebuffer: " + status);

                DefaultFrameBuffer = false;
            }*/
			throw new NotImplementedException();
#endif
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

        public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
        {
#if ES11
            currentRenderTargets = renderTargets;

            if (currentRenderTargets != null)
            {
                // TODO: For speed we need to consider using FBO switching instead
                // of multiple FBO's if they are the same size.

                // http://www.songho.ca/opengl/gl_fbo.html

                // Get the currently bound frame buffer object. On most platforms this just gives 0.
#if IPHONE
				GL.GetInteger(GLFramebufferBinding, ref originalFbo);
#else
				GL.GetInteger(GLFramebufferBinding, out originalFbo);
#endif
				
				frameBufferIDs = new int[currentRenderTargets.Length];
				
				renderBufferIDs = new int[currentRenderTargets.Length];
				
				GL_Oes.GenRenderbuffers(currentRenderTargets.Length, renderBufferIDs);
				
				for (int i = 0; i < currentRenderTargets.Length; i++) {
					RenderTarget2D target = (RenderTarget2D)currentRenderTargets[i].RenderTarget;

					// create a renderbuffer object to store depth info
					GL_Oes.BindRenderbuffer(GLRenderbuffer, renderBufferIDs[i]);

					ClearOptions clearOptions = ClearOptions.Target | ClearOptions.DepthBuffer;

					// create framebuffer
#if IPHONE
					GL_Oes.GenFramebuffers(1, ref frameBufferIDs[i]);
#else
					GL_Oes.GenFramebuffers(1, out frameBufferIDs[i]);
#endif

#if IPHONE
					GL_Oes.BindFramebuffer(GLFramebuffer, frameBufferIDs[i]);
#else
					GL_Oes.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBufferIDs[i]);
#endif
					
					//allocate depth buffer
					switch (target.DepthStencilFormat) {
					case DepthFormat.Depth16:
						GL_Oes.RenderbufferStorage(GLRenderbuffer, GLDepthComponent16,
							target.Width, target.Height);
						break;
					case DepthFormat.Depth24:
						GL_Oes.RenderbufferStorage(GLRenderbuffer, GLDepthComponent24,
							target.Width, target.Height);
						break;
					case DepthFormat.Depth24Stencil8:
						GL_Oes.RenderbufferStorage(GLRenderbuffer, GLDepth24Stencil8,
							target.Width, target.Height);
						// attach stencil buffer
						GL_Oes.FramebufferRenderbuffer(GLFramebuffer, GLStencilAttachment,
							GLRenderbuffer, renderBufferIDs[i]);
						clearOptions = clearOptions | ClearOptions.Stencil;
						break;
					default:
						GL_Oes.RenderbufferStorage(GLRenderbuffer, GLDepthComponent24,
							target.Width, target.Height);
						break;
					}
					
					// attach the texture to FBO color attachment point
					GL_Oes.FramebufferTexture2D(GLFramebuffer, GLColorAttachment0,
						TextureTarget.Texture2D, target.ID,0);
					
					// attach the renderbuffer to depth attachment point
					GL_Oes.FramebufferRenderbuffer(GLFramebuffer, GLDepthAttachment,
						GLRenderbuffer, renderBufferIDs[i]);					
					
					if (target.RenderTargetUsage == RenderTargetUsage.DiscardContents)
						Clear (clearOptions, Color.Transparent, 0, 0);
					
					GL_Oes.BindRenderbuffer(GLRenderbuffer, 0);
				}
				
				FramebufferErrorCode status = GL_Oes.CheckFramebufferStatus(GLFramebuffer);
				
				if (status != GLFramebufferComplete)
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
#else
			throw new NotImplementedException();
#endif
        }

		public RenderTargetBinding[] GetRenderTargets ()
		{
			return currentRenderTargets;
		}
		
        public void ResolveBackBuffer(ResolveTexture2D resolveTexture)
        {
        }
		
#if IPHONE
		internal All PrimitiveTypeGL(PrimitiveType primitiveType)
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
#else
        internal BeginMode PrimitiveTypeGL(PrimitiveType primitiveType)
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
#endif


        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            _vertexBuffer = vertexBuffer;
			//GL.BindBuffer (BufferTarget.ArrayBuffer, vertexBuffer._bufferStore);
        }

        private void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            _indexBuffer = indexBuffer;
			//GL.BindBuffer (BufferTarget.ElementArrayBuffer, indexBuffer._bufferStore);
        }

        public IndexBuffer Indices { set { SetIndexBuffer(value); } }

        public bool ResourcesLost { get; set; }
		
		internal void SetGraphicsStates ()
		{
			//GL.PushMatrix();
			// Set up our Rasterizer States
			GLStateManager.SetRasterizerStates(RasterizerState);
			GLStateManager.SetBlendStates(BlendState);
		}
		
		bool resetVertexStates = false;
		internal void UnsetGraphicsStates ()
		{
#if !ES11
			// Make sure we are not user any shaders
			GL.UseProgram(0);
#endif
			
			// if primitives were used then we need to reset them
			if (resetVertexStates) {
#if ES11
				GLStateManager.VertexArray(false);
				GLStateManager.ColorArray(false);
				GLStateManager.NormalArray(false);
				GLStateManager.TextureCoordArray(false);
#endif
				resetVertexStates = false;
			}
			//GL.PopMatrix();
		}
		

        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numbVertices, int startIndex, int primitiveCount)
        {
			if (minVertexIndex > 0 || baseVertex > 0)
				throw new NotImplementedException ("baseVertex > 0 and minVertexIndex > 0 are not supported");

			// we need to reset vertex states afterwards
			resetVertexStates = true;

			// Set up our Graphics States
			SetGraphicsStates();

            // Unbind the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //Create VBO if not created already
#if IPHONE
			if (VboIdArray == 0)
                GL.GenBuffers(1, ref VboIdArray);
            if (VboIdElement == 0)
                GL.GenBuffers(1, ref VboIdElement);
#else
            if (VboIdArray == 0)
                GL.GenBuffers(1, out VboIdArray);
            if (VboIdElement == 0)
                GL.GenBuffers(1, out VboIdElement);
#endif
			
            // Bind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboIdArray);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VboIdElement);
            ////Clear previous data
            GL.BufferData(BufferTarget.ArrayBuffer,
			              (IntPtr)0, (IntPtr)null,
			              BufferUsageHint.DynamicDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
			              (IntPtr)0, (IntPtr)null,
			              BufferUsageHint.DynamicDraw);

			//Get VertexDeclaration
			var vd = _vertexBuffer.VertexDeclaration;
			if (vd == null) {

				vd = VertexDeclaration.FromType(_vertexBuffer._type);
			}

            //Buffer data to VBO; This should use stream when we move to ES2.0
            GL.BufferData(BufferTarget.ArrayBuffer,
				(IntPtr)(vd.VertexStride * GetElementCountArray(primitiveType, primitiveCount)),
				_vertexBuffer._bufferPtr,
				BufferUsageHint.DynamicDraw);

            GL.BufferData(BufferTarget.ElementArrayBuffer,
				(IntPtr)(sizeof(ushort) * GetElementCountArray(primitiveType, primitiveCount)),
				_indexBuffer._bufferPtr,
				BufferUsageHint.DynamicDraw);

            //Setup VertexDeclaration
            VertexDeclaration.PrepareForUse(vd);

            //Draw
            GL.DrawElements(PrimitiveTypeGL(primitiveType),
				GetElementCountArray(primitiveType, primitiveCount),
				DrawElementsType.UnsignedShort,
				(IntPtr)(startIndex * sizeof(ushort)));


            // Free resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

			UnsetGraphicsStates();
        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {
			// we need to reset vertex states afterwards
			resetVertexStates = true;

			// Set up our Graphics States
			SetGraphicsStates();

           // Unbind the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //Create VBO if not created already
#if IPHONE
            if (VboIdArray == 0)
                GL.GenBuffers(1, ref VboIdArray);
#else
            if (VboIdArray == 0)
                GL.GenBuffers(1, out VboIdArray);
#endif
			
            // Bind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboIdArray);
            ////Clear previous data
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)null, BufferUsageHint.DynamicDraw);

            //Get VertexDeclaration
            var vd = VertexDeclaration.FromType(typeof(T));

            //Pin data
            var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

            //Buffer data to VBO; This should use stream when we move to ES2.0
            GL.BufferData(BufferTarget.ArrayBuffer,
			              (IntPtr)(vd.VertexStride * GetElementCountArray(primitiveType, primitiveCount)),
			              vertexData,
			              BufferUsageHint.DynamicDraw);

            //Setup VertexDeclaration
            VertexDeclaration.PrepareForUse(vd);

            //Draw
            GL.DrawArrays(PrimitiveTypeGL(primitiveType),
			              vertexOffset,
			              GetElementCountArray(primitiveType,
			              primitiveCount));

            // Free resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            handle.Free();

			// Unset our Graphics States
			UnsetGraphicsStates();
		}

        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
			// we need to reset vertex states afterwards
			resetVertexStates = true;

			// Set up our Graphics States
			SetGraphicsStates();

           // Unbind the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //Create VBO if not created already
#if IPHONE
			if (VboIdArray == 0)
                GL.GenBuffers(1, ref VboIdArray);
#else
            if (VboIdArray == 0)
                GL.GenBuffers(1, out VboIdArray);
#endif
			
            // Bind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboIdArray);
            ////Clear previous data
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)null, BufferUsageHint.DynamicDraw);

			//Get VertexDeclaration
			var vd = _vertexBuffer.VertexDeclaration;
			if (vd == null) {

				vd = VertexDeclaration.FromType(_vertexBuffer._type);
			}

            //Buffer data to VBO; This should use stream when we move to ES2.0
            GL.BufferData(BufferTarget.ArrayBuffer,
				(IntPtr)(vd.VertexStride * GetElementCountArray(primitiveType, primitiveCount)),
				_vertexBuffer._bufferPtr,
				BufferUsageHint.DynamicDraw);

            //Setup VertexDeclaration
            VertexDeclaration.PrepareForUse(vd);

            //Draw
            GL.DrawArrays(PrimitiveTypeGL(primitiveType),
			              vertexStart,
			              GetElementCountArray(primitiveType, primitiveCount));

            // Free resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

			// Unset our Graphics States
			UnsetGraphicsStates();
		}

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, short[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
			// we need to reset vertex states afterwards
			resetVertexStates = true;

			// Set up our Graphics States
			SetGraphicsStates();

           // Unbind the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //Create VBO if not created already
#if IPHONE
			if (VboIdArray == 0)
                GL.GenBuffers(1, ref VboIdArray);
            if (VboIdElement == 0)
                GL.GenBuffers(1, ref VboIdElement);
#else
            if (VboIdArray == 0)
                GL.GenBuffers(1, out VboIdArray);
            if (VboIdElement == 0)
                GL.GenBuffers(1, out VboIdElement);
#endif
			
            // Bind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboIdArray);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VboIdElement);
            ////Clear previous data
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)null, BufferUsageHint.DynamicDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)0, (IntPtr)null, BufferUsageHint.DynamicDraw);

            //Get VertexDeclaration
            var vd = VertexDeclaration.FromType(typeof (T));

            //Pin data
            var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

            //Buffer data to VBO; This should use stream when we move to ES2.0
            GL.BufferData(BufferTarget.ArrayBuffer,
                            (IntPtr) (vd.VertexStride*GetElementCountArray(primitiveType, primitiveCount)),
                            new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (vertexOffset*vd.VertexStride)),
                            BufferUsageHint.DynamicDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                            (IntPtr) (sizeof (ushort)*GetElementCountArray(primitiveType, primitiveCount)),
                            indexData, BufferUsageHint.DynamicDraw);

            //Setup VertexDeclaration
            VertexDeclaration.PrepareForUse(vd);

            //Draw
            GL.DrawElements(PrimitiveTypeGL(primitiveType),
			                GetElementCountArray(primitiveType, primitiveCount),
                            DrawElementsType.UnsignedShort/* .UnsignedInt248Oes*/,
			                (IntPtr) (indexOffset*sizeof (ushort)));


            // Free resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            handle.Free();
            handle2.Free();

			// Unset our Graphics States
			UnsetGraphicsStates();
        }
		
		public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, int[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
			// we need to reset vertex states afterwards
			resetVertexStates = true;

			// Set up our Graphics States
			SetGraphicsStates();

			// Unbind the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //Create VBO if not created already
#if IPHONE
			if (VboIdArray == 0)
                GL.GenBuffers(1, ref VboIdArray);
            if (VboIdElement == 0)
                GL.GenBuffers(1, ref VboIdElement);
#else
            if (VboIdArray == 0)
                GL.GenBuffers(1, out VboIdArray);
            if (VboIdElement == 0)
                GL.GenBuffers(1, out VboIdElement);
#endif
            // Bind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboIdArray);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VboIdElement);
            ////Clear previous data
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)null, BufferUsageHint.DynamicDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)0, (IntPtr)null, BufferUsageHint.DynamicDraw);
			
			//Get VertexDeclaration
            var vd = VertexDeclaration.FromType(typeof (T));
			
            //Pin data
            var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

            //Buffer data to VBO; This should use stream when we move to ES2.0
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vd.VertexStride * GetElementCountArray(primitiveType, primitiveCount)), new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (vertexOffset * vd.VertexStride)), BufferUsageHint.DynamicDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * GetElementCountArray(primitiveType, primitiveCount)), indexData, BufferUsageHint.DynamicDraw);

            //Setup VertexDeclaration
            VertexDeclaration.PrepareForUse(vd);

            //Draw
            GL.DrawElements(PrimitiveTypeGL(primitiveType),
			                GetElementCountArray(primitiveType, primitiveCount),
#if WINDOWS
                            (DrawElementsType)All.UnsignedInt,
#elif IPHONE
			                DrawElementsType.UnsignedInt248Oes,
#else
			                DrawElementsType.UnsignedInt,
#endif
			                (IntPtr)(indexOffset * sizeof(uint)));


            // Free resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            handle.Free();
            handle2.Free();

			// Unset our Graphics States
			UnsetGraphicsStates();
        }

        internal int GetElementCountArray(PrimitiveType primitiveType, int primitiveCount)
        {
            //TODO: Overview the calculation
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return primitiveCount * 2;
                case PrimitiveType.LineStrip:
                    return primitiveCount + 1;
                case PrimitiveType.TriangleList:
                    return primitiveCount * 3;
                case PrimitiveType.TriangleStrip:
                    return 3 + (primitiveCount - 1); // ???
            }

            throw new NotSupportedException();
        }
		
		
		internal void SetViewPort(int Width, int Height)
		{
			this._viewport.Width = Width;
			this._viewport.Height = Height;
		}

    }
}
