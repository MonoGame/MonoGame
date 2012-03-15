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

#if IPHONE
using MonoTouch.OpenGLES;
#endif
#if ANDROID
using Android.Opengl;
using Android.Views;
using OpenTK.Graphics;
#endif

using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using ALL11 = OpenTK.Graphics.ES11.All;
using ALL20 = OpenTK.Graphics.ES20.All;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsDevice : IDisposable
    {
        private GamePlatform _platform;
        private ALL11 _preferedFilter;
        private int _activeTexture = -1;
        private Viewport _viewport;

        private bool _isDisposed = false;
        public TextureCollection Textures { get; set; }
        private BlendState _blendState = BlendState.Opaque;
        private DepthStencilState _depthStencilState = DepthStencilState.Default;
        private SamplerStateCollection _samplerStates = new SamplerStateCollection();

        internal List<IntPtr> _pointerCache = new List<IntPtr>();
        private VertexBuffer _vertexBuffer = null;
        private IndexBuffer _indexBuffer = null;
        private uint VboIdArray;
        private uint[] VboIdArrays;
        private uint VboIdElement;
        private int _currentArray;
        private int _vbosAllocated = 1;

        public RasterizerState RasterizerState { get; set; }

        private RenderTargetBinding[] currentRenderTargets;
		
		// TODO Graphics Device events need implementing
		public event EventHandler<EventArgs> DeviceLost;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
		//public event EventHandler<ResourceCreatedEventArgs> ResourceCreated;
		//public event EventHandler<ResourceDestroyedEventArgs> ResourceDestroyed;

        //OpenGL Rendering API

#if ANDROID
		[Obsolete(
			"GraphicsDevice should be responsible for the version someday.  " +
			"It shouldn't need to be told, it should be doing the telling")]
		public static GLContextVersion OpenGLESVersion;
#else
		[Obsolete(
			"GraphicsDevice should be responsible for the version someday.  " +
			"It shouldn't need to be told, it should be doing the telling")]
		public static EAGLRenderingAPI OpenGLESVersion;
#endif

		[Obsolete(
			"GraphicsDevice should be responsible for the framebuffer someday.  " +
			"It shouldn't need to be told, it should be doing the telling")]
        public static int FrameBufferScreen;
		[Obsolete(
			"GraphicsDevice should be responsible for the DeafultFramebuffer someday.  " +
			"It shouldn't need to be told, it should be doing the telling")]
        public static bool DefaultFrameBuffer = true;

        internal ALL11 PreferedFilter
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

        /// <summary>
        /// This value indicates the # of VBO's the DrawUserPrimitive calls can cylce between.
        /// Increase this to improve performance per frame.  Though, the more you have allocated
        /// the more video memory permanently consumed.
        /// </summary>
        public int VBOsAllocated
        {
            get { return _vbosAllocated; }

            set
            {
                var generate = false;
                if (VboIdArrays != null)
                {
                    generate = true;
                    if(_vbosAllocated > 0)
                    {
#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
                        if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
                        {
                            GL20.DeleteBuffers(_vbosAllocated, VboIdArrays);
                        }
                        else
                        {
                            GL11.DeleteBuffers(_vbosAllocated, VboIdArrays);
                        }
                    }
                }

                _vbosAllocated = value;

                if(generate)
                    GenerateVBOs();
            }
        }

        private void GenerateVBOs()
        {
#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
            {
                OpenTK.Graphics.ES20.GL.GetError();

                VboIdArrays = new uint[_vbosAllocated];
                GL20.GenBuffers(_vbosAllocated, VboIdArrays);

                var er = OpenTK.Graphics.ES20.GL.GetError();
                if (er != OpenTK.Graphics.ES20.All.NoError)
                {
                    VboIdArrays = null;
                }
            }
            else
            {
                OpenTK.Graphics.ES11.GL.GetError();

                VboIdArrays = new uint[_vbosAllocated];
                GL11.GenBuffers(_vbosAllocated, VboIdArrays);

                var er = OpenTK.Graphics.ES11.GL.GetError();
                if (er != OpenTK.Graphics.ES11.All.NoError)
                {
                    VboIdArrays = null;
                }
            }
            _currentArray = 0;
        }

        private void MoveToNextVBO()
        {
            _currentArray++;

            if (_currentArray >= _vbosAllocated)
            {
                _currentArray = 0;
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
            Textures = new TextureCollection();

            // Init RasterizerState
            RasterizerState = new RasterizerState();
        }

        internal void Initialize(GamePlatform platform)
        {
            _platform = platform;
            
#if IPHONE
            if (OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
            {
                //Initialize OpenGl states
                GL20.Disable(ALL20.DepthTest);
            }else{
                // Initialize OpenGL states
                GL11.Disable(ALL11.DepthTest);
                GL11.TexEnv(ALL11.TextureEnv, ALL11.TextureEnvMode, (int)ALL11.Replace);
            }
            VboIdArray = 0;
            VboIdElement = 0;
        }

        public BlendState BlendState
        {
            get { return _blendState; }
            set
            {
                // ToDo check for invalid state
                _blendState = value;

                // Disable Blending by default = BlendState.Opaque
#if IPHONE
                if (OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
                if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
                if (false)
#endif
                {
                    GL20.Disable(ALL20.Blend);

                    // set the blend mode
                    if (_blendState == BlendState.NonPremultiplied)
                    {
                        GL20.BlendFunc(ALL20.SrcAlpha, ALL20.OneMinusSrcAlpha);
                        GL20.Enable(ALL20.Blend);
                    }

                    if (_blendState == BlendState.AlphaBlend)
                    {
                        GL20.BlendFunc(ALL20.One, ALL20.OneMinusSrcAlpha);
                        GL20.Enable(ALL20.Blend);
                    }

                    if (_blendState == BlendState.Additive)
                    {
                        GL20.BlendFunc(ALL20.SrcAlpha, ALL20.One);
                        GL20.Enable(ALL20.Blend);
                    }
                }else
                {
                    GL11.Disable(ALL11.Blend);

                    // set the blend mode
                    if (_blendState == BlendState.NonPremultiplied)
                    {
                        GL11.BlendFunc(ALL11.SrcAlpha, ALL11.OneMinusSrcAlpha);
                        GL11.Enable(ALL11.Blend);
                    }

                    if (_blendState == BlendState.AlphaBlend)
                    {
                        GL11.BlendFunc(ALL11.One, ALL11.OneMinusSrcAlpha);
                        GL11.Enable(ALL11.Blend);
                    }

                    if (_blendState == BlendState.Additive)
                    {
                        GL11.BlendFunc(ALL11.SrcAlpha, ALL11.One);
                        GL11.Enable(ALL11.Blend);
                    }
                }
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
                //var temp = _samplerStates;
                return _samplerStates;
            }
        }
        public void Clear(Color color)
        {
            Vector4 vector = color.ToVector4();

#if IPHONE
            if (OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
            {
                GL20.ClearColor(vector.X, vector.Y, vector.Z, vector.W);
                GL20.Clear((uint)ALL20.ColorBufferBit);
            }
            else
            {
                GL11.ClearColor(vector.X, vector.Y, vector.Z, vector.W);
                GL11.Clear((uint)ALL11.ColorBufferBit);
            }
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Clear(options, color.ToVector4(), depth, stencil);
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            uint mask = 0;

#if IPHONE
            if (OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
            {
                if (color.X != 0f || color.Y != 0f || color.Z != 0f || color.W != 0f)
                {
                    GL20.ClearColor(color.X, color.Y, color.Z, color.W);
                    mask = (uint)ALL20.ColorBufferBit | mask;
                }

                GL20.ClearDepth(depth);
                mask = (uint)ALL20.DepthBufferBit | mask;

                GL20.ClearStencil(stencil);
                mask = (uint)ALL20.StencilBufferBit | mask;

                GL20.Clear(mask);
            }
            else
            {
                if (color.X != 0f || color.Y != 0f || color.Z != 0f || color.W != 0f)
                {
                    GL11.ClearColor(color.X, color.Y, color.Z, color.W);
                    mask = (uint)ALL11.ColorBufferBit | mask;
                }

                GL11.ClearDepth(depth);
                mask = (uint)ALL11.DepthBufferBit | mask;

                GL11.ClearStencil(stencil);
                mask = (uint)ALL11.StencilBufferBit | mask;

                GL11.Clear(mask);
            }
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
#if ANDROID
            _platform.Present();
#else
#if IPHONE
            if (OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
                GL20.Flush();
            else
                GL11.Flush();
#endif
        }

        public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {

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
				
				_scissorRectangle.Y = _viewport.Height - _scissorRectangle.Y - _scissorRectangle.Height;
            }
        }

        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
                SetRenderTargetGL20(renderTarget);
			else
				SetRenderTargetGL11(renderTarget);
        }

        public void SetRenderTargetGL20(RenderTarget2D rendertarget)
        {
            if (rendertarget == null)
            {
                GL20.BindFramebuffer(ALL20.Framebuffer, FrameBufferScreen);
				
				// restore the saved Viewport
                Viewport = savedViewport;
				
                DefaultFrameBuffer = true;
            }
            else
            {
                GL20.BindFramebuffer(ALL20.Framebuffer, rendertarget.frameBuffer);
                GL20.FramebufferTexture2D(ALL20.Framebuffer, ALL20.ColorAttachment0, ALL20.Texture2D, rendertarget.ID, 0);

                ALL20 status = GL20.CheckFramebufferStatus(ALL20.Framebuffer);
                if (status != ALL20.FramebufferComplete)
                    throw new Exception("GL20: Error creating framebuffer: " + status);
				
				// Save off the current viewport to be reset later
                    savedViewport = Viewport;

                    // Create a new Viewport
                    Viewport renderTargetViewPort = new Viewport();

                    // Set the new viewport to the width and height of the render target
                    Texture2D target2 = (Texture2D) rendertarget;
                    renderTargetViewPort.Width = target2.Width;
                    renderTargetViewPort.Height = target2.Height;

                    // now we set our viewport to the new rendertarget viewport just created.
                    Viewport = renderTargetViewPort;
				
                DefaultFrameBuffer = false;
            }
        }

        public void SetRenderTargetGL11(RenderTarget2D renderTarget)
        {
            // We check if the rendertarget being passed is null or if we already have a rendertarget
            // NetRumble sample does not set the the renderTarget to null before setting another
            // rendertarget.  We handle that by checking first if we have a current render target set
            // if we do then we unbind the current rendertarget, reset the viewport and set the
            // rendertarget to the new one being passed if it is not null
            if (renderTarget == null || currentRenderTargets != null)
            {
#if ANDROID					
				try
				{
					// This is a work around for android gl1.1
                  byte[] imageInfo = new byte[4];
                  GL11.ReadPixels(0, 0, 1, 1, ALL11.Rgba, ALL11.UnsignedByte, imageInfo);
				}
				catch
				{ 
				}
#endif
                // Detach the render buffers.
                GL11.Oes.FramebufferRenderbuffer(ALL11.FramebufferOes, ALL11.DepthAttachmentOes,
                        ALL11.RenderbufferOes, 0);

                // delete the RBO's
                GL11.Oes.DeleteRenderbuffers(renderBufferIDs.Length, renderBufferIDs);

                // delete the FBO
                GL11.Oes.DeleteFramebuffers(frameBufferIDs.Length, frameBufferIDs);

                // Set the frame buffer back to the system window buffer
                GL11.Oes.BindFramebuffer(ALL11.FramebufferOes, originalFbo);

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
#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
            {
                throw new NotImplementedException();
            }
            else
            {
                currentRenderTargets = renderTargets;

                if (currentRenderTargets != null)
                {
                    // TODO: For speed we need to consider using FBO switching instead
                    // of multiple FBO's if they are the same size.

                    // http://www.songho.ca/opengl/gl_fbo.html

                    // Get the currently bound frame buffer object. On most platforms this just gives 0.				
                    GL11.GetInteger(ALL11.FramebufferBindingOes, ref originalFbo);

                    frameBufferIDs = new int[currentRenderTargets.Length];

                    renderBufferIDs = new int[currentRenderTargets.Length];
                    GL11.Oes.GenRenderbuffers(currentRenderTargets.Length, renderBufferIDs);

                    for (int i = 0; i < currentRenderTargets.Length; i++)
                    {
                        RenderTarget2D target = (RenderTarget2D) currentRenderTargets[i].RenderTarget;

                        // create a renderbuffer object to store depth info
                        GL11.Oes.BindRenderbuffer(ALL11.RenderbufferOes, renderBufferIDs[i]);

                        ClearOptions clearOptions = ClearOptions.Target | ClearOptions.DepthBuffer;

                        switch (target.DepthStencilFormat)
                        {
                            case DepthFormat.Depth16:
                                GL11.Oes.RenderbufferStorage(ALL11.RenderbufferOes, ALL11.DepthComponent16Oes,
                                                             target.Width, target.Height);
                                break;
                            case DepthFormat.Depth24:
                                GL11.Oes.RenderbufferStorage(ALL11.RenderbufferOes, ALL11.DepthComponent24Oes,
                                                             target.Width, target.Height);
                                break;
                            case DepthFormat.Depth24Stencil8:
                                GL11.Oes.RenderbufferStorage(ALL11.RenderbufferOes, ALL11.Depth24Stencil8Oes,
                                                             target.Width, target.Height);
                                GL11.Oes.FramebufferRenderbuffer(ALL11.FramebufferOes, ALL11.StencilAttachmentOes,
                                                                 ALL11.RenderbufferOes, renderBufferIDs[i]);
                                clearOptions = clearOptions | ClearOptions.Stencil;
                                break;
                            default:
                                GL11.Oes.RenderbufferStorage(ALL11.RenderbufferOes, ALL11.DepthComponent24Oes,
                                                             target.Width, target.Height);
                                break;
                        }

                        // create framebuffer
                        GL11.Oes.GenFramebuffers(1, ref frameBufferIDs[i]);
                        GL11.Oes.BindFramebuffer(ALL11.FramebufferOes, frameBufferIDs[i]);

                        // attach the texture to FBO color attachment point
                        GL11.Oes.FramebufferTexture2D(ALL11.FramebufferOes, ALL11.ColorAttachment0Oes, ALL11.Texture2D,
                                                      target.ID, 0);

                        // attach the renderbuffer to depth attachment point
                        GL11.Oes.FramebufferRenderbuffer(ALL11.FramebufferOes, ALL11.DepthAttachmentOes,
                                                         ALL11.RenderbufferOes, renderBufferIDs[i]);

                        if (target.RenderTargetUsage == RenderTargetUsage.DiscardContents)
                            Clear(clearOptions, Color.Transparent, 0, 0);

                       // GL11.Oes.BindRenderbuffer(ALL11.FramebufferOes, originalFbo);

                    }

                    ALL11 status = GL11.Oes.CheckFramebufferStatus(ALL11.FramebufferOes);

                    if (status != ALL11.FramebufferCompleteOes)
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
                    Texture2D target2 = (Texture2D) currentRenderTargets[0].RenderTarget;
                    renderTargetViewPort.Width = target2.Width;
                    renderTargetViewPort.Height = target2.Height;

                    // now we set our viewport to the new rendertarget viewport just created.
                    Viewport = renderTargetViewPort;
                }
            }
        }

		public RenderTargetBinding[] GetRenderTargets ()
		{
			return currentRenderTargets;
		}
		
        public void ResolveBackBuffer(ResolveTexture2D resolveTexture)
        {
        }

        internal ALL11 PrimitiveTypeGL11(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return ALL11.Lines;
                case PrimitiveType.LineStrip:
                    return ALL11.LineStrip;
                case PrimitiveType.TriangleList:
                    return ALL11.Triangles;
                case PrimitiveType.TriangleStrip:
                    return ALL11.TriangleStrip;
            }

            throw new NotImplementedException();
        }

        internal ALL20 PrimitiveTypeGL20(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return ALL20.Lines;
                case PrimitiveType.LineStrip:
                    return ALL20.LineStrip;
                case PrimitiveType.TriangleList:
                    return ALL20.Triangles;
                case PrimitiveType.TriangleStrip:
                    return ALL20.TriangleStrip;
            }

            throw new NotImplementedException();
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            _vertexBuffer = vertexBuffer;
#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
                GL20.BindBuffer(ALL20.ArrayBuffer, vertexBuffer._bufferStore);
            else
                GL11.BindBuffer(ALL11.ArrayBuffer, vertexBuffer._bufferStore);
        }

        private void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            _indexBuffer = indexBuffer;
#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
                GL20.BindBuffer(ALL20.ElementArrayBuffer, indexBuffer._bufferStore);
            else
                GL11.BindBuffer(ALL11.ElementArrayBuffer, indexBuffer._bufferStore);
        }

        public IndexBuffer Indices { set { SetIndexBuffer(value); } }

        public bool ResourcesLost { get; set; }

        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numbVertices, int startIndex, int primitiveCount)
        {
            if (minVertexIndex > 0 || baseVertex > 0)
                throw new NotImplementedException("baseVertex > 0 and minVertexIndex > 0 are not supported");

            var vd = VertexDeclaration.FromType(_vertexBuffer._type);
            // Hmm, can the pointer here be changed with baseVertex?
            VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
                GL20.DrawElements(PrimitiveTypeGL20(primitiveType), _indexBuffer._count, ALL20.UnsignedShort, new IntPtr(startIndex));
            else
                GL11.DrawElements(PrimitiveTypeGL11(primitiveType), _indexBuffer._count, ALL11.UnsignedShort, new IntPtr(startIndex));
        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {
#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
            {
                // Unbind the VBOs
                GL20.BindBuffer(ALL20.ArrayBuffer, 0);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, 0);

                //Create VBO if not created already
                if (VboIdArray == 0)
                    GL20.GenBuffers(1, ref VboIdArray);

                // Bind the VBO
                GL20.BindBuffer(ALL20.ArrayBuffer, VboIdArray);
                ////Clear previous data
                GL20.BufferData(ALL20.ArrayBuffer, (IntPtr) 0, (IntPtr) null, ALL20.DynamicDraw);

                //Get VertexDeclaration
                var vd = VertexDeclaration.FromType(typeof (T));

                //Pin data
                var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

                //Buffer data to VBO; This should use stream when we move to ES2.0
                GL20.BufferData(ALL20.ArrayBuffer,
                                (IntPtr)(vd.VertexStride * GetElementCountArray(primitiveType, primitiveCount) + vertexOffset * vd.VertexStride),
                                vertexData, ALL20.DynamicDraw);

                //Setup VertexDeclaration
                VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

                //Draw
                GL20.DrawArrays(PrimitiveTypeGL20(primitiveType), vertexOffset,
                                GetElementCountArray(primitiveType, primitiveCount));


                // Free resources
                GL20.BindBuffer(ALL20.ArrayBuffer, 0);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, 0);
                handle.Free();
            }else
            {
                // Unbind the VBOs
                GL11.BindBuffer(ALL11.ArrayBuffer, 0);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, 0);

                //Create VBO if not created already
                //if (VboIdArrays == null)
                //{
                //    GenerateVBOs();
                //}

                // Bind the VBO
                //GL11.BindBuffer(ALL11.ArrayBuffer, VboIdArrays[_currentArray]);

                //Get VertexDeclaration
                var vd = VertexDeclaration.FromType(typeof(T));

                //Pin data
                var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

                //Buffer data to VBO; This should use stream when we move to ES2.0
                //GL11.BufferData(ALL11.ArrayBuffer,
                //                (IntPtr)(vd.VertexStride * GetElementCountArray(primitiveType, primitiveCount) + vertexOffset * vd.VertexStride),
                //                vertexData, ALL11.DynamicDraw);
                

                //Setup VertexDeclaration
                VertexDeclaration.PrepareForUse(vd, handle.AddrOfPinnedObject());

                //Draw
                GL11.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexOffset,
                                GetElementCountArray(primitiveType, primitiveCount));

                GL11.Flush();

                handle.Free();

                // Free resources
                //GL11.BindBuffer(ALL11.ArrayBuffer, 0);

                //MoveToNextVBO();
            }
        }

        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            var vd = VertexDeclaration.FromType(_vertexBuffer._type);
            VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
                GL20.DrawArrays(PrimitiveTypeGL20(primitiveType), vertexStart, GetElementCountArray(primitiveType, primitiveCount));
            else
                GL11.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexStart, GetElementCountArray(primitiveType, primitiveCount));
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, short[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            ////////////////////////////
            //This has not been tested//
            ////////////////////////////
#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
            {
                // Unbind the VBOs
                GL20.BindBuffer(ALL20.ArrayBuffer, 0);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, 0);

                //Create VBO if not created already
                if (VboIdArray == 0)
                    GL20.GenBuffers(1, ref VboIdArray);
                if (VboIdElement == 0)
                    GL20.GenBuffers(1, ref VboIdElement);

                // Bind the VBO
                GL20.BindBuffer(ALL20.ArrayBuffer, VboIdArray);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, VboIdElement);
                ////Clear previous data
                GL20.BufferData(ALL20.ArrayBuffer, (IntPtr) 0, (IntPtr) null, ALL20.DynamicDraw);
                GL20.BufferData(ALL20.ElementArrayBuffer, (IntPtr) 0, (IntPtr) null, ALL20.DynamicDraw);

                //Get VertexDeclaration
                var vd = VertexDeclaration.FromType(typeof (T));

                //Pin data
                var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
                var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

                //Buffer data to VBO; This should use stream when we move to ES2.0
                GL20.BufferData(ALL20.ArrayBuffer,
                                (IntPtr) (vd.VertexStride*GetElementCountArray(primitiveType, primitiveCount)),
                                new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (vertexOffset*vd.VertexStride)),
                                ALL20.DynamicDraw);
                GL20.BufferData(ALL20.ElementArrayBuffer,
                                (IntPtr) (sizeof (ushort)*GetElementCountArray(primitiveType, primitiveCount)),
                                indexData, ALL20.DynamicDraw);

                //Setup VertexDeclaration
                VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

                //Draw
                GL20.DrawElements(PrimitiveTypeGL20(primitiveType), GetElementCountArray(primitiveType, primitiveCount),
                                  ALL20.UnsignedShort, (IntPtr) (indexOffset*sizeof (ushort)));


                // Free resources
                GL20.BindBuffer(ALL20.ArrayBuffer, 0);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, 0);
                handle.Free();
                handle2.Free();
            }else
            {
                // Unbind the VBOs
                GL11.BindBuffer(ALL11.ArrayBuffer, 0);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, 0);

                //Create VBO if not created already
                if (VboIdArray == 0)
                    GL11.GenBuffers(1, ref VboIdArray);
                if (VboIdElement == 0)
                    GL11.GenBuffers(1, ref VboIdElement);

                // Bind the VBO
                GL11.BindBuffer(ALL11.ArrayBuffer, VboIdArray);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, VboIdElement);
                ////Clear previous data
                GL11.BufferData(ALL11.ArrayBuffer, (IntPtr)0, (IntPtr)null, ALL11.DynamicDraw);
                GL11.BufferData(ALL11.ElementArrayBuffer, (IntPtr)0, (IntPtr)null, ALL11.DynamicDraw);

                //Get VertexDeclaration
                var vd = VertexDeclaration.FromType(typeof(T));

                //Pin data
                var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
                var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

                //Buffer data to VBO; This should use stream when we move to ES2.0
                GL11.BufferData(ALL11.ArrayBuffer,
                                (IntPtr)(vd.VertexStride * GetElementCountArray(primitiveType, primitiveCount)),
                                new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (vertexOffset * vd.VertexStride)),
                                ALL11.DynamicDraw);
                GL11.BufferData(ALL11.ElementArrayBuffer,
                                (IntPtr)(sizeof(ushort) * GetElementCountArray(primitiveType, primitiveCount)),
                                indexData, ALL11.DynamicDraw);

                //Setup VertexDeclaration
                VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

                //Draw
                GL11.DrawElements(PrimitiveTypeGL11(primitiveType), GetElementCountArray(primitiveType, primitiveCount),
                                  ALL11.UnsignedShort, (IntPtr)(indexOffset * sizeof(ushort)));


                // Free resources
                GL11.BindBuffer(ALL11.ArrayBuffer, 0);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, 0);
                handle.Free();
                handle2.Free();
            }
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, uint[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            ////////////////////////////
            //This has not been tested//
            ////////////////////////////

#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
            {
                // Unbind the VBOs
                GL20.BindBuffer(ALL20.ArrayBuffer, 0);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, 0);

                //Create VBO if not created already
                if (VboIdArray == 0)
                    GL20.GenBuffers(1, ref VboIdArray);
                if (VboIdElement == 0)
                    GL20.GenBuffers(1, ref VboIdElement);

                // Bind the VBO
                GL20.BindBuffer(ALL20.ArrayBuffer, VboIdArray);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, VboIdElement);
                ////Clear previous data
                GL20.BufferData(ALL20.ArrayBuffer, (IntPtr) 0, (IntPtr) null, ALL20.DynamicDraw);
                GL20.BufferData(ALL20.ElementArrayBuffer, (IntPtr) 0, (IntPtr) null, ALL20.DynamicDraw);

                //Get VertexDeclaration
                var vd = VertexDeclaration.FromType(typeof (T));

                //Pin data
                var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
                var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

                //Buffer data to VBO; This should use stream when we move to ES2.0
                GL20.BufferData(ALL20.ArrayBuffer,
                                (IntPtr) (vd.VertexStride*GetElementCountArray(primitiveType, primitiveCount)),
                                new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (vertexOffset*vd.VertexStride)),
                                ALL20.DynamicDraw);
                GL20.BufferData(ALL20.ElementArrayBuffer,
                                (IntPtr) (sizeof (uint)*GetElementCountArray(primitiveType, primitiveCount)), indexData,
                                ALL20.DynamicDraw);

                //Setup VertexDeclaration
                VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

                //Draw
                GL20.DrawElements(PrimitiveTypeGL20(primitiveType), GetElementCountArray(primitiveType, primitiveCount),
                                  ALL20.UnsignedInt248Oes, (IntPtr) (indexOffset*sizeof (uint)));


                // Free resources
                GL20.BindBuffer(ALL20.ArrayBuffer, 0);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, 0);
                handle.Free();
                handle2.Free();
            }else
            {
                // Unbind the VBOs
                GL11.BindBuffer(ALL11.ArrayBuffer, 0);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, 0);

                //Create VBO if not created already
                if (VboIdArray == 0)
                    GL11.GenBuffers(1, ref VboIdArray);
                if (VboIdElement == 0)
                    GL11.GenBuffers(1, ref VboIdElement);

                // Bind the VBO
                GL11.BindBuffer(ALL11.ArrayBuffer, VboIdArray);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, VboIdElement);
                ////Clear previous data
                GL11.BufferData(ALL11.ArrayBuffer, (IntPtr)0, (IntPtr)null, ALL11.DynamicDraw);
                GL11.BufferData(ALL11.ElementArrayBuffer, (IntPtr)0, (IntPtr)null, ALL11.DynamicDraw);

                //Get VertexDeclaration
                var vd = VertexDeclaration.FromType(typeof(T));

                //Pin data
                var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
                var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

                //Buffer data to VBO; This should use stream when we move to ES2.0
                GL11.BufferData(ALL11.ArrayBuffer,
                                (IntPtr)(vd.VertexStride * GetElementCountArray(primitiveType, primitiveCount)),
                                new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (vertexOffset * vd.VertexStride)),
                                ALL11.DynamicDraw);
                GL11.BufferData(ALL11.ElementArrayBuffer,
                                (IntPtr)(sizeof(uint) * GetElementCountArray(primitiveType, primitiveCount)), indexData,
                                ALL11.DynamicDraw);

                //Setup VertexDeclaration
                VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

                //Draw
                GL11.DrawElements(PrimitiveTypeGL11(primitiveType), GetElementCountArray(primitiveType, primitiveCount),
                                  ALL11.UnsignedInt248Oes, (IntPtr)(indexOffset * sizeof(uint)));


                // Free resources
                GL11.BindBuffer(ALL11.ArrayBuffer, 0);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, 0);
                handle.Free();
                handle2.Free();
            }
        }
		
		public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, int[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            ////////////////////////////
            //This has not been tested//
            ////////////////////////////

#if IPHONE
			if(OpenGLESVersion == EAGLRenderingAPI.OpenGLES2)
#elif ANDROID
            if (OpenGLESVersion == GLContextVersion.Gles2_0)
#else
            if (false)
#endif
            {
                // Unbind the VBOs
                GL20.BindBuffer(ALL20.ArrayBuffer, 0);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, 0);

                //Create VBO if not created already
                if (VboIdArray == 0)
                    GL20.GenBuffers(1, ref VboIdArray);
                if (VboIdElement == 0)
                    GL20.GenBuffers(1, ref VboIdElement);

                // Bind the VBO
                GL20.BindBuffer(ALL20.ArrayBuffer, VboIdArray);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, VboIdElement);
                ////Clear previous data
                GL20.BufferData(ALL20.ArrayBuffer, (IntPtr) 0, (IntPtr) null, ALL20.DynamicDraw);
                GL20.BufferData(ALL20.ElementArrayBuffer, (IntPtr) 0, (IntPtr) null, ALL20.DynamicDraw);

                //Get VertexDeclaration
                var vd = VertexDeclaration.FromType(typeof (T));

                //Pin data
                var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
                var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

                //Buffer data to VBO; This should use stream when we move to ES2.0
                GL20.BufferData(ALL20.ArrayBuffer,
                                (IntPtr) (vd.VertexStride*GetElementCountArray(primitiveType, primitiveCount)),
                                new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (vertexOffset*vd.VertexStride)),
                                ALL20.DynamicDraw);
                GL20.BufferData(ALL20.ElementArrayBuffer,
                                (IntPtr) (sizeof (int)*GetElementCountArray(primitiveType, primitiveCount)), indexData,
                                ALL20.DynamicDraw);

                //Setup VertexDeclaration
                VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

                //Draw
                GL20.DrawElements(PrimitiveTypeGL20(primitiveType), GetElementCountArray(primitiveType, primitiveCount),
                                  ALL20.UnsignedInt248Oes, (IntPtr) (indexOffset*sizeof (uint)));


                // Free resources
                GL20.BindBuffer(ALL20.ArrayBuffer, 0);
                GL20.BindBuffer(ALL20.ElementArrayBuffer, 0);
                handle.Free();
                handle2.Free();
            }else
            {
                // Unbind the VBOs
                GL11.BindBuffer(ALL11.ArrayBuffer, 0);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, 0);

                //Create VBO if not created already
                if (VboIdArray == 0)
                    GL11.GenBuffers(1, ref VboIdArray);
                if (VboIdElement == 0)
                    GL11.GenBuffers(1, ref VboIdElement);

                // Bind the VBO
                GL11.BindBuffer(ALL11.ArrayBuffer, VboIdArray);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, VboIdElement);
                ////Clear previous data
                GL11.BufferData(ALL11.ArrayBuffer, (IntPtr)0, (IntPtr)null, ALL11.DynamicDraw);
                GL11.BufferData(ALL11.ElementArrayBuffer, (IntPtr)0, (IntPtr)null, ALL11.DynamicDraw);

                //Get VertexDeclaration
                var vd = VertexDeclaration.FromType(typeof(T));

                //Pin data
                var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
                var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

                //Buffer data to VBO; This should use stream when we move to ES2.0
                GL11.BufferData(ALL11.ArrayBuffer,
                                (IntPtr)(vd.VertexStride * GetElementCountArray(primitiveType, primitiveCount)),
                                new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (vertexOffset * vd.VertexStride)),
                                ALL11.DynamicDraw);
                GL11.BufferData(ALL11.ElementArrayBuffer,
                                (IntPtr)(sizeof(int) * GetElementCountArray(primitiveType, primitiveCount)), indexData,
                                ALL11.DynamicDraw);

                //Setup VertexDeclaration
                VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

                //Draw
                GL11.DrawElements(PrimitiveTypeGL11(primitiveType), GetElementCountArray(primitiveType, primitiveCount),
                                  ALL11.UnsignedInt248Oes, (IntPtr)(indexOffset * sizeof(uint)));


                // Free resources
                GL11.BindBuffer(ALL11.ArrayBuffer, 0);
                GL11.BindBuffer(ALL11.ElementArrayBuffer, 0);
                handle.Free();
                handle2.Free();
            }
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
