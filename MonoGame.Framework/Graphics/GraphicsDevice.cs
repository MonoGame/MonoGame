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
using TextureTarget = OpenTK.Graphics.ES20.All;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
using DrawElementsType = OpenTK.Graphics.ES20.All;
using GetPName = OpenTK.Graphics.ES20.All;
using FramebufferErrorCode = OpenTK.Graphics.ES20.All;
using FramebufferTarget = OpenTK.Graphics.ES20.All;
using FramebufferAttachment = OpenTK.Graphics.ES20.All;
using RenderbufferTarget = OpenTK.Graphics.ES20.All;
using RenderbufferStorage = OpenTK.Graphics.ES20.All;
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

        private RenderTargetBinding[] currentRenderTargetBindings;
		
		// OpenGL ES2.0 attribute locations
		internal static int attributePosition = 0; //there can be a couple positions binded
		internal static int attributeColor = 3;
		internal static int attributeNormal = 4;
		internal static int attributeBlendIndicies = 5;
		internal static int attributeBlendWeight = 6;
		internal static int attributeTexCoord = 7; //must be the last one, texture index locations are added to it

		//OpenGL ES 1.1 extension consts
#if IPHONE && ES11
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
#elif IPHONE
		const FramebufferTarget GLFramebuffer = FramebufferTarget.Framebuffer;
		const RenderbufferTarget GLRenderbuffer = RenderbufferTarget.Renderbuffer;
		const FramebufferAttachment GLDepthAttachment = FramebufferAttachment.DepthAttachment;
		const FramebufferAttachment GLStencilAttachment = FramebufferAttachment.StencilAttachment;
		const FramebufferAttachment GLColorAttachment0 = FramebufferAttachment.ColorAttachment0;
		const GetPName GLFramebufferBinding = GetPName.FramebufferBinding;
		const RenderbufferStorage GLDepthComponent16 = RenderbufferStorage.DepthComponent16;
		const RenderbufferStorage GLDepthComponent24 = RenderbufferStorage.DepthComponent24Oes;
		const RenderbufferStorage GLDepth24Stencil8 = RenderbufferStorage.Depth24Stencil8Oes;
		const FramebufferErrorCode GLFramebufferComplete = FramebufferErrorCode.FramebufferComplete;
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
		
		// TODO Graphics Device events need implementing
		public event EventHandler<EventArgs> DeviceLost;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
		//public event EventHandler<ResourceCreatedEventArgs> ResourceCreated;
		//public event EventHandler<ResourceDestroyedEventArgs> ResourceDestroyed;

        internal int glFramebuffer;

		public RasterizerState RasterizerState {
			get {
				return _rasterizerState;
			}
			set {
				_rasterizerState = value;
				GLStateManager.SetRasterizerStates(value, GetRenderTargets().Length > 0);
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
            _viewport = new Viewport(0, 0,
			                         DisplayMode.Width, DisplayMode.Height);
            _viewport.MinDepth = 0.0f;
            _viewport.MaxDepth = 1.0f;
            Textures = new TextureCollection();

			Initialize();
        }

        internal void Initialize()
        {

#if ES11
			//Is this needed?
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.BlendSrc);
#endif

			BlendState = BlendState.Opaque;
			DepthStencilState = DepthStencilState.Default;
			RasterizerState = RasterizerState.CullCounterClockwise;

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
			ClearOptions options = ClearOptions.Target;
			if (true) { //TODO: Clear only if current backbuffer has a depth component
				options |= ClearOptions.DepthBuffer;
			}
			if (true) { //TODO: Clear only if current backbuffer has a stencil component
				options |= ClearOptions.Stencil;
			}
            Clear (options, color.ToVector4(), Viewport.MaxDepth, 0);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Clear (options, color.ToVector4 (), depth, stencil);
        }

		public void Clear (ClearOptions options, Vector4 color, float depth, int stencil)
		{
			GL.ClearColor (color.X, color.Y, color.Z, color.W);

			ClearBufferMask bufferMask = 0;
			if (options.HasFlag(ClearOptions.Target)) {
				bufferMask = bufferMask | ClearBufferMask.ColorBufferBit;
			}
			if (options.HasFlag(ClearOptions.Stencil)) {
				GL.ClearStencil (stencil);
				bufferMask = bufferMask | ClearBufferMask.StencilBufferBit;
			}
			if (options.HasFlag(ClearOptions.DepthBuffer)) {
				GL.ClearDepth (depth);
				bufferMask = bufferMask | ClearBufferMask.DepthBufferBit;
			}

#if IPHONE
			GL.Clear ((uint)bufferMask);
#else
			GL.Clear (bufferMask);
#endif
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
				GL.DepthRange(value.MinDepth, value.MaxDepth);
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
				if (RasterizerState.ScissorTestEnable)
                	return _scissorRectangle;
				return _viewport.Bounds;
            }
            set
            {
                _scissorRectangle = value;
				
				GLStateManager.SetScissor(_scissorRectangle);
				
				_scissorRectangle.Y = _viewport.Height - _scissorRectangle.Y - _scissorRectangle.Height;
            }
        }

		public void SetRenderTarget (RenderTarget2D renderTarget)
		{
			if (renderTarget == null)
				this.SetRenderTargets(null);
			else
				this.SetRenderTargets(new RenderTargetBinding(renderTarget));
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
			var previousRenderTargetBindings = this.currentRenderTargetBindings;
			this.currentRenderTargetBindings = renderTargets;
			
			//GLExt.DiscardFramebuffer(All.Framebuffer, 2, discards);
			
			if (this.currentRenderTargetBindings == null || this.currentRenderTargetBindings.Length == 0)
			{
				GL.BindFramebuffer(GLFramebuffer, 0);
				this.Viewport = new Viewport(0, 0,
					this.PresentationParameters.BackBufferWidth, 
					this.PresentationParameters.BackBufferHeight);
			}
			else
			{
				if (this.glFramebuffer == 0)
				{
#if IPHONE
					GL.GenFramebuffers(1, ref this.glFramebuffer);
#else
					GL.GenFramebuffers(1, out this.glFramebuffer);
#endif
				}
				
				var renderTarget = this.currentRenderTargetBindings[0].RenderTarget as RenderTarget2D;
				GL.BindFramebuffer(GLFramebuffer, this.glFramebuffer);
				GL.FramebufferTexture2D(GLFramebuffer, GLColorAttachment0, TextureTarget.Texture2D, renderTarget.glTexture, 0);
				if (renderTarget.DepthStencilFormat != DepthFormat.None)
				{
					GL.FramebufferRenderbuffer(GLFramebuffer, GLDepthAttachment, GLRenderbuffer, renderTarget.glDepthStencilBuffer);
					if (renderTarget.DepthStencilFormat == DepthFormat.Depth24Stencil8)
					{
						GL.FramebufferRenderbuffer(GLFramebuffer, GLStencilAttachment, GLRenderbuffer, renderTarget.glDepthStencilBuffer);
					}
				}

				var status = GL.CheckFramebufferStatus(GLFramebuffer);
				if (status != GLFramebufferComplete)
				{
					string message = "Framebuffer Incomplete.";
					switch (status)
					{
					case FramebufferErrorCode.FramebufferIncompleteAttachment: message = "Not all framebuffer attachment points are framebuffer attachment complete."; break;
					case FramebufferErrorCode.FramebufferIncompleteMissingAttachment : message = "No images are attached to the framebuffer."; break;
					case FramebufferErrorCode.FramebufferUnsupported : message = "The combination of internal formats of the attached images violates an implementation-dependent set of restrictions."; break;
					//case FramebufferErrorCode.FramebufferIncompleteDimensions : message = "Not all attached images have the same width and height."; break;
					}
					throw new InvalidOperationException(message);
				}
				this.Viewport = new Viewport(0, 0, renderTarget.Width, renderTarget.Height);
			}
			
			if (previousRenderTargetBindings != null)
			{
				for (var i = 0; i < previousRenderTargetBindings.Length; ++i)
				{
					var renderTarget = previousRenderTargetBindings[i].RenderTarget;
					if (renderTarget.LevelCount > 1)
					{
						throw new NotImplementedException();
						/*
						GL.ActiveTexture(TextureUnit.Texture0);
						GL.BindTexture(TextureTarget.Texture2D, renderTarget.ID);
						GL.GenerateMipmap(TextureTarget.Texture2D);
						GL.BindTexture(TextureTarget.Texture2D, 0);*/
					}
				}
			}

			//Reset the cull mode, because we flip verticies when rendering offscreen
			//and thus flip the cull direction
			GLStateManager.Cull (RasterizerState, GetRenderTargets().Length > 0);
		}

		static RenderTargetBinding[] emptyRenderTargetBinding = new RenderTargetBinding[0];
		public RenderTargetBinding[] GetRenderTargets ()
		{
			if (this.currentRenderTargetBindings == null)
				return emptyRenderTargetBinding;
			return currentRenderTargetBindings;
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
			_vertexBuffer.Apply();
        }

        private void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            _indexBuffer = indexBuffer;
			_indexBuffer.Apply ();
        }

        public IndexBuffer Indices { set { SetIndexBuffer(value); } }

        public bool ResourcesLost { get; set; }

        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numbVertices, int startIndex, int primitiveCount)
        {
			if (minVertexIndex > 0)
				throw new NotImplementedException ("minVertexIndex > 0 is supported");


			var indexElementType = DrawElementsType.UnsignedShort;
			var indexElementSize = 2;
			var indexOffsetInBytes = (IntPtr)(startIndex * indexElementSize);
			var indexElementCount = GetElementCountArray(primitiveType, primitiveCount);
			var target = PrimitiveTypeGL(primitiveType);
			var vertexOffset = (IntPtr)(_vertexBuffer.VertexDeclaration.VertexStride * baseVertex);


			_vertexBuffer.VertexDeclaration.Apply (vertexOffset);

			GL.DrawElements (target,
			                 indexElementCount,
			                 indexElementType,
			                 indexOffsetInBytes);

        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {

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
            vd.Apply ();

            //Draw
            GL.DrawArrays(PrimitiveTypeGL(primitiveType),
			              vertexOffset,
			              GetElementCountArray(primitiveType, primitiveCount));

            // Free resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            handle.Free();

		}

        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
			_vertexBuffer.VertexDeclaration.Apply ();

			GL.DrawArrays(PrimitiveTypeGL(primitiveType),
			              vertexStart,
			              GetElementCountArray(primitiveType,primitiveCount));
		}

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, short[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {

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
            vd.Apply ();

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
        }
		
		public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, int[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {

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
            vd.Apply ();

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
