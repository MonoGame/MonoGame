// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

#if MONOMAC
using MonoMac.OpenGL;
using GLPrimitiveType = MonoMac.OpenGL.BeginMode;
#endif

#if WINDOWS || LINUX
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using GLPrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;
#endif

#if ANGLE
using OpenTK.Graphics;
#endif

#if GLES
using OpenTK.Graphics.ES20;
using BeginMode = OpenTK.Graphics.ES20.All;
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
using GLPrimitiveType = OpenTK.Graphics.ES20.All;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
#if WINDOWS || LINUX || ANGLE
        internal IGraphicsContext Context { get; private set; }
#endif

#if !GLES
        private DrawBuffersEnum[] _drawBuffers;
#endif

        static List<Action> disposeActions = new List<Action>();
        static object disposeActionsLock = new object();

        private readonly ShaderProgramCache _programCache = new ShaderProgramCache();

        private ShaderProgram _shaderProgram = null;

        static readonly float[] _posFixup = new float[4];

        internal static readonly List<int> _enabledVertexAttributes = new List<int>();

#if GLES
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
#endif
#if !GLES
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

        internal FramebufferHelper framebufferHelper;

        internal int glFramebuffer = 0;
        internal int MaxVertexAttributes;        
        internal List<string> _extensions = new List<string>();
        internal int _maxTextureSize = 0;

        internal void SetVertexAttributeArray(bool[] attrs)
        {
            for(int x = 0; x < attrs.Length; x++)
            {
                if (attrs[x] && !_enabledVertexAttributes.Contains(x))
                {
                    _enabledVertexAttributes.Add(x);
                    GL.EnableVertexAttribArray(x);
                    GraphicsExtensions.CheckGLError();
                }
                else if (!attrs[x] && _enabledVertexAttributes.Contains(x))
                {
                    _enabledVertexAttributes.Remove(x);
                    GL.DisableVertexAttribArray(x);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        private void PlatformSetup()
        {
#if WINDOWS || LINUX || ANGLE
            GraphicsMode mode = GraphicsMode.Default;
            var wnd = (Game.Instance.Window as OpenTKGameWindow).Window.WindowInfo;

            #if GLES
            // Create an OpenGL ES 2.0 context
            var flags = GraphicsContextFlags.Embedded;
            int major = 2;
            int minor = 0;
            #else
            // Create an OpenGL compatibility context
            var flags = GraphicsContextFlags.Default;
            int major = 1;
            int minor = 0;
            #endif

            if (Context == null || Context.IsDisposed)
            {
                var color = PresentationParameters.BackBufferFormat.GetColorFormat();
                var depth =
                    PresentationParameters.DepthStencilFormat == DepthFormat.None ? 0 :
                    PresentationParameters.DepthStencilFormat == DepthFormat.Depth16 ? 16 :
                    24;
                var stencil =
                    PresentationParameters.DepthStencilFormat == DepthFormat.Depth24Stencil8 ? 8 :
                    0;

                var samples = 0;
                if (Game.Instance.graphicsDeviceManager.PreferMultiSampling)
                {
                    // Use a default of 4x samples if PreferMultiSampling is enabled
                    // without explicitly setting the desired MultiSampleCount.
                    if (PresentationParameters.MultiSampleCount == 0)
                    {
                        PresentationParameters.MultiSampleCount = 4;
                    }

                    samples = PresentationParameters.MultiSampleCount;
                }

                mode = new GraphicsMode(color, depth, stencil, samples);
                try
                {
                    Context = new GraphicsContext(mode, wnd, major, minor, flags);
                }
                catch (Exception e)
                {
                    Game.Instance.Log("Failed to create OpenGL context, retrying. Error: " +
                        e.ToString());
                    major = 1;
                    minor = 0;
                    flags = GraphicsContextFlags.Default;
                    Context = new GraphicsContext(mode, wnd, major, minor, flags);
                }
            }
            Context.MakeCurrent(wnd);
            (Context as IGraphicsContextInternal).LoadAll();
            Context.SwapInterval = PresentationParameters.PresentationInterval.GetSwapInterval();

            // Provide the graphics context for background loading
            // Note: this context should use the same GraphicsMode,
            // major, minor version and flags parameters as the main
            // context. Otherwise, context sharing will very likely fail.
            if (Threading.BackgroundContext == null)
            {
                Threading.BackgroundContext = new GraphicsContext(mode, wnd, major, minor, flags);
                Threading.WindowInfo = wnd;
                Threading.BackgroundContext.MakeCurrent(null);
            }
            Context.MakeCurrent(wnd);
#endif

            MaxTextureSlots = 16;

#if  IOS
            GL.GetInteger(All.MaxTextureImageUnits, ref MaxTextureSlots);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(All.MaxVertexAttribs, ref MaxVertexAttributes);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(All.MaxTextureSize, ref _maxTextureSize);
            GraphicsExtensions.CheckGLError();
#else
            GL.GetInteger(GetPName.MaxTextureImageUnits, out MaxTextureSlots);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
            GraphicsExtensions.CheckGLError();
            
            GL.GetInteger(GetPName.MaxTextureSize, out _maxTextureSize);
            GraphicsExtensions.CheckGLError();

#if !GLES
			// Initialize draw buffer attachment array
			int maxDrawBuffers;
			GL.GetInteger(GetPName.MaxDrawBuffers, out maxDrawBuffers);
			_drawBuffers = new DrawBuffersEnum[maxDrawBuffers];
			for (int i = 0; i < maxDrawBuffers; i++)
				_drawBuffers[i] = (DrawBuffersEnum)(FramebufferAttachment.ColorAttachment0Ext + i);
#endif
#endif
            _extensions = GetGLExtensions();
        }

        List<string> GetGLExtensions()
        {
            // Setup extensions.
            List<string> extensions = new List<string>();
#if GLES
            var extstring = GL.GetString(RenderbufferStorage.Extensions);            			
#else
            var extstring = GL.GetString(StringName.Extensions);
#endif
            GraphicsExtensions.CheckGLError();
            if (!string.IsNullOrEmpty(extstring))
            {
                extensions.AddRange(extstring.Split(' '));
#if ANDROID
                Android.Util.Log.Debug("MonoGame", "Supported extensions:");
#else
                System.Diagnostics.Debug.WriteLine("Supported extensions:");
#endif
                foreach (string extension in extensions)
#if ANDROID
                    Android.Util.Log.Debug("MonoGame", extension);
#else
                    System.Diagnostics.Debug.WriteLine(extension);
#endif
            }

            return extensions;
        }

        private void PlatformInitialize()
        {
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            _enabledVertexAttributes.Clear();

            // Free all the cached shader programs. 
            _programCache.Clear();
            _shaderProgram = null;

            if (GraphicsCapabilities.SupportsFramebufferObjectARB)
            {
                this.framebufferHelper = new FramebufferHelper(this);
            }
            #if !(GLES || MONOMAC)
            else if (GraphicsCapabilities.SupportsFramebufferObjectEXT)
            {
                this.framebufferHelper = new FramebufferHelperEXT(this);
            }
            #endif
            else
            {
                throw new PlatformNotSupportedException(
                    "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                    "Try updating your graphics drivers.");
            }
        }
        
        private DepthStencilState clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        public void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // TODO: We need to figure out how to detect if we have a
            // depth stencil buffer or not, and clear options relating
            // to them if not attached.

            // Unlike with XNA and DirectX...  GL.Clear() obeys several
            // different render states:
            //
            //  - The color write flags.
            //  - The scissor rectangle.
            //  - The depth/stencil state.
            //
            // So overwrite these states with what is needed to perform
            // the clear correctly and restore it afterwards.
            //
		    var prevScissorRect = ScissorRectangle;
		    var prevDepthStencilState = DepthStencilState;
            var prevBlendState = BlendState;
            ScissorRectangle = _viewport.Bounds;
            // DepthStencilState.Default has the Stencil Test disabled; 
            // make sure stencil test is enabled before we clear since
            // some drivers won't clear with stencil test disabled
            DepthStencilState = this.clearDepthStencilState;
		    BlendState = BlendState.Opaque;
            PlatformApplyState(false);

            ClearBufferMask bufferMask = 0;
            if ((options & ClearOptions.Target) == ClearOptions.Target)
            {
                GL.ClearColor(color.X, color.Y, color.Z, color.W);
                GraphicsExtensions.CheckGLError();
                bufferMask = bufferMask | ClearBufferMask.ColorBufferBit;
            }
			if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
            {
				GL.ClearStencil(stencil);
                GraphicsExtensions.CheckGLError();
                bufferMask = bufferMask | ClearBufferMask.StencilBufferBit;
			}

			if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer) 
            {
#if GLES
                GL.ClearDepth (depth);
                GraphicsExtensions.CheckGLError();
#else
                GL.ClearDepth ((double)depth);
#endif
				bufferMask = bufferMask | ClearBufferMask.DepthBufferBit;
			}

#if GLES && !ANGLE && !ANDROID
			GL.Clear((uint)bufferMask);
#else
			GL.Clear(bufferMask);
#endif
            GraphicsExtensions.CheckGLError();
           		
            // Restore the previous render state.
		    ScissorRectangle = prevScissorRect;
		    DepthStencilState = prevDepthStencilState;
		    BlendState = prevBlendState;
        }

        private void PlatformDispose()
        {
            // Free all the cached shader programs.
            _programCache.Dispose();

            GraphicsDevice.AddDisposeAction(() =>
                                            {
#if WINDOWS || LINUX || ANGLE
                Context.Dispose();
                Context = null;

                if (Threading.BackgroundContext != null)
                {
                    Threading.BackgroundContext.Dispose();
                    Threading.BackgroundContext = null;
                    Threading.WindowInfo = null;
                }
#endif
            });
        }

        /// <summary>
        /// Adds a dispose action to the list of pending dispose actions. These are executed at the end of each call to Present().
        /// This allows GL resources to be disposed from other threads, such as the finalizer.
        /// </summary>
        /// <param name="disposeAction">The action to execute for the dispose.</param>
        static internal void AddDisposeAction(Action disposeAction)
        {
            if (disposeAction == null)
                throw new ArgumentNullException("disposeAction");
            if (Threading.IsOnUIThread())
            {
                disposeAction();
            }
            else
            {
                lock (disposeActionsLock)
                {
                    disposeActions.Add(disposeAction);
                }
            }
        }

        public void PlatformPresent()
        {
#if WINDOWS || LINUX || ANGLE
            Context.SwapBuffers();
#endif
            GraphicsExtensions.CheckGLError();

            // Dispose of any GL resources that were disposed in another thread
            lock (disposeActionsLock)
            {
                if (disposeActions.Count > 0)
                {
                    foreach (var action in disposeActions)
                        action();
                    disposeActions.Clear();
                }
            }
        }

        private void PlatformSetViewport(ref Viewport value)
        {
            if (IsRenderTargetBound)
                GL.Viewport(value.X, value.Y, value.Width, value.Height);
            else
                GL.Viewport(value.X, PresentationParameters.BackBufferHeight - value.Y - value.Height, value.Width, value.Height);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");
#if GLES
            GL.DepthRange(value.MinDepth, value.MaxDepth);
#else
            GL.DepthRange((double)value.MinDepth, (double)value.MaxDepth);
#endif
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");
                
            // In OpenGL we have to re-apply the special "posFixup"
            // vertex shader uniform if the viewport changes.
            _vertexShaderDirty = true;

        }

        private void PlatformApplyDefaultRenderTarget()
        {
            this.framebufferHelper.BindFramebuffer(this.glFramebuffer);

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.Dirty();
        }

        private class RenderTargetBindingArrayComparer : IEqualityComparer<RenderTargetBinding[]>
        {
            public bool Equals(RenderTargetBinding[] first, RenderTargetBinding[] second)
            {
                if (object.ReferenceEquals(first, second))
                    return true;

                if (first == null || second == null)
                    return false;

                if (first.Length != second.Length)
                    return false;

                for (var i = 0; i < first.Length; ++i)
                {
                    if ((first[i].RenderTarget != second[i].RenderTarget) || (first[i].ArraySlice != second[i].ArraySlice))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(RenderTargetBinding[] array)
            {
                if (array != null)
                {
                    unchecked
                    {
                        int hash = 17;
                        foreach (var item in array)
                        {
                            if (item.RenderTarget != null)
                                hash = hash * 23 + item.RenderTarget.GetHashCode();
                            hash = hash * 23 + item.ArraySlice.GetHashCode();
                        }
                        return hash;
                    }
                }
                return 0;
            }
        }

        // FBO cache, we create 1 FBO per RenderTargetBinding combination
        private Dictionary<RenderTargetBinding[], int> glFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());
        // FBO cache used to resolve MSAA rendertargets, we create 1 FBO per RenderTargetBinding combination
        private Dictionary<RenderTargetBinding[], int> glResolveFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());

        internal void PlatformCreateRenderTarget(Texture renderTarget, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            var color = 0;
            var depth = 0;
            var stencil = 0;
            
            if (preferredMultiSampleCount > 0 && this.framebufferHelper.SupportsBlitFramebuffer)
            {
                this.framebufferHelper.GenRenderbuffer(out color);
                this.framebufferHelper.BindRenderbuffer(color);
#if GLES
                this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)RenderbufferStorage.Rgba8Oes, width, height);
#else
                this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)RenderbufferStorage.Rgba8, width, height);
#endif
            }

            if (preferredDepthFormat != DepthFormat.None)
            {
                var depthInternalFormat = RenderbufferStorage.DepthComponent16;
                var stencilInternalFormat = (RenderbufferStorage)0;
                switch (preferredDepthFormat)
                {
                    case DepthFormat.Depth16: 
                        depthInternalFormat = RenderbufferStorage.DepthComponent16; break;
#if GLES
                    case DepthFormat.Depth24:
                        if (GraphicsCapabilities.SupportsDepth24)
                            depthInternalFormat = RenderbufferStorage.DepthComponent24Oes;
                        else if (GraphicsCapabilities.SupportsDepthNonLinear)
                            depthInternalFormat = (RenderbufferStorage)0x8E2C;
                        else
                            depthInternalFormat = RenderbufferStorage.DepthComponent16;
                        break;
                    case DepthFormat.Depth24Stencil8:
                        if (GraphicsCapabilities.SupportsPackedDepthStencil)
                            depthInternalFormat = RenderbufferStorage.Depth24Stencil8Oes;
                        else
                        {
                            if (GraphicsCapabilities.SupportsDepth24)
                                depthInternalFormat = RenderbufferStorage.DepthComponent24Oes;
                            else if (GraphicsCapabilities.SupportsDepthNonLinear)
                                depthInternalFormat = (RenderbufferStorage)0x8E2C;
                            else
                                depthInternalFormat = RenderbufferStorage.DepthComponent16;
                            stencilInternalFormat = RenderbufferStorage.StencilIndex8;
                            break;
                        }
                        break;
#else
                    case DepthFormat.Depth24: depthInternalFormat = RenderbufferStorage.DepthComponent24; break;
                    case DepthFormat.Depth24Stencil8: depthInternalFormat = RenderbufferStorage.Depth24Stencil8; break;
#endif
                }

                if (depthInternalFormat != 0)
                {
                    this.framebufferHelper.GenRenderbuffer(out depth);
                    this.framebufferHelper.BindRenderbuffer(depth);
                    this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)depthInternalFormat, width, height);
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                    {
                        stencil = depth;
                        if (stencilInternalFormat != 0)
                        {
                            this.framebufferHelper.GenRenderbuffer(out stencil);
                            this.framebufferHelper.BindRenderbuffer(stencil);
                            this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)stencilInternalFormat, width, height);
                        }
                    }
                }
            }

            var renderTarget2D = renderTarget as RenderTarget2D;
            if (renderTarget2D != null)
            {
                if (color != 0)
                    renderTarget2D.glColorBuffer = color;
                else
                    renderTarget2D.glColorBuffer = renderTarget2D.glTexture;
                renderTarget2D.glDepthBuffer = depth;
                renderTarget2D.glStencilBuffer = stencil;
            }
            else
            {
                throw new NotSupportedException(); 
            }
        }

        internal void PlatformDeleteRenderTarget(Texture renderTarget)
        {
            var color = 0;
            var depth = 0;
            var stencil = 0;
            var colorIsRenderbuffer = false;

            var renderTarget2D = renderTarget as RenderTarget2D;
            if (renderTarget2D != null)
            {
                color = renderTarget2D.glColorBuffer;
                depth = renderTarget2D.glDepthBuffer;
                stencil = renderTarget2D.glStencilBuffer;
                colorIsRenderbuffer = color != renderTarget2D.glTexture;
            }

            if (color != 0)
            {
                if (colorIsRenderbuffer)
                    this.framebufferHelper.DeleteRenderbuffer(color);
                if (stencil != 0 && stencil != depth)
                    this.framebufferHelper.DeleteRenderbuffer(stencil);
                if (depth != 0)
                    this.framebufferHelper.DeleteRenderbuffer(depth);

                var bindingsToDelete = new List<RenderTargetBinding[]>();
                foreach (var bindings in this.glFramebuffers.Keys)
                {
                    foreach (var binding in bindings)
                    {
                        if (binding.RenderTarget == renderTarget)
                        {
                            bindingsToDelete.Add(bindings);
                            break;
                        }
                    }
                }

                foreach (var bindings in bindingsToDelete)
                {
                    var fbo = 0;
                    if (this.glFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        this.framebufferHelper.DeleteFramebuffer(fbo);
                        this.glFramebuffers.Remove(bindings);
                    }
                    if (this.glResolveFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        this.framebufferHelper.DeleteFramebuffer(fbo);
                        this.glResolveFramebuffers.Remove(bindings);
                    }
                }
            }
        }

        private void PlatformResolveRenderTargets()
        {
            if (this._currentRenderTargetCount == 0)
                return;

            var renderTargetBinding = this._currentRenderTargetBindings[0];
            var renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
            if (renderTarget.MultiSampleCount > 0 && this.framebufferHelper.SupportsBlitFramebuffer)
            {
                var glResolveFramebuffer = 0;
                if (!this.glResolveFramebuffers.TryGetValue(this._currentRenderTargetBindings, out glResolveFramebuffer))
                {
                    this.framebufferHelper.GenFramebuffer(out glResolveFramebuffer);
                    this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);
                    for (var i = 0; i < this._currentRenderTargetCount; ++i)
                    {
                        this.framebufferHelper.FramebufferTexture2D((int)(FramebufferAttachment.ColorAttachment0 + i), (int)renderTarget.glTarget, renderTarget.glTexture);
                    }
                    this.glResolveFramebuffers.Add(this._currentRenderTargetBindings, glResolveFramebuffer);
                }
                else
                {
                    this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);
                }
                // The only fragment operations which affect the resolve are the pixel ownership test, the scissor test, and dithering.
                GL.Disable(EnableCap.ScissorTest);
                GraphicsExtensions.CheckGLError();
                var glFramebuffer = this.glFramebuffers[this._currentRenderTargetBindings];
                this.framebufferHelper.BindReadFramebuffer(glFramebuffer);
                for (var i = 0; i < this._currentRenderTargetCount; ++i)
                {
                    renderTargetBinding = this._currentRenderTargetBindings[i];
                    renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
                    this.framebufferHelper.BlitFramebuffer(i, renderTarget.Width, renderTarget.Height);
                }
                if (renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents && this.framebufferHelper.SupportsInvalidateFramebuffer)
                    this.framebufferHelper.InvalidateReadFramebuffer();
                this._depthStencilStateDirty = true;
            }
            for (var i = 0; i < this._currentRenderTargetCount; ++i)
            {
                renderTargetBinding = this._currentRenderTargetBindings[i];
                renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
                if (renderTarget.LevelCount > 1)
                {
                    GL.BindTexture(renderTarget.glTarget, renderTarget.glTexture);
                    GraphicsExtensions.CheckGLError();
                    this.framebufferHelper.GenerateMipmap((int)renderTarget.glTarget);
                }
            }
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            var glFramebuffer = 0;
            if (!this.glFramebuffers.TryGetValue(this._currentRenderTargetBindings, out glFramebuffer))
            {
                this.framebufferHelper.GenFramebuffer(out glFramebuffer);
                this.framebufferHelper.BindFramebuffer(glFramebuffer);
                var renderTargetBinding = this._currentRenderTargetBindings[0];
                var renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
                this.framebufferHelper.FramebufferRenderbuffer((int)FramebufferAttachment.DepthAttachment, renderTarget.glDepthBuffer, 0);
                this.framebufferHelper.FramebufferRenderbuffer((int)FramebufferAttachment.StencilAttachment, renderTarget.glStencilBuffer, 0);
                for (var i = 0; i < this._currentRenderTargetCount; ++i)
                {
                    renderTargetBinding = this._currentRenderTargetBindings[i];
                    renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
                    var attachement = (int)(FramebufferAttachment.ColorAttachment0 + i);
                    if (renderTarget.glColorBuffer != renderTarget.glTexture)
                        this.framebufferHelper.FramebufferRenderbuffer(attachement, renderTarget.glColorBuffer, 0);
                    else
                        this.framebufferHelper.FramebufferTexture2D(attachement, (int)renderTarget.glTarget, renderTarget.glTexture, 0, renderTarget.MultiSampleCount);
                }

#if DEBUG
                this.framebufferHelper.CheckFramebufferStatus();
#endif
                this.glFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                this.framebufferHelper.BindFramebuffer(glFramebuffer);
            }
#if !GLES
            GL.DrawBuffers(this._currentRenderTargetCount, this._drawBuffers);
#endif

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.Dirty();

            return _currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
        }

        private static GLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return GLPrimitiveType.Lines;
                case PrimitiveType.LineStrip:
                    return GLPrimitiveType.LineStrip;
                case PrimitiveType.TriangleList:
                    return GLPrimitiveType.Triangles;
                case PrimitiveType.TriangleStrip:
                    return GLPrimitiveType.TriangleStrip;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Activates the Current Vertex/Pixel shader pair into a program.         
        /// </summary>
        private void ActivateShaderProgram()
        {
            // Lookup the shader program.
            var shaderProgram = _programCache.GetProgram(VertexShader, PixelShader);
            if (shaderProgram.Program == -1)
                return;
            // Set the new program if it has changed.
            if (_shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GraphicsExtensions.CheckGLError();
                _shaderProgram = shaderProgram;
            }

            var posFixupLoc = shaderProgram.GetUniformLocation("posFixup");
            if (posFixupLoc == -1)
                return;

            // Apply vertex shader fix:
            // The following two lines are appended to the end of vertex shaders
            // to account for rendering differences between OpenGL and DirectX:
            //
            // gl_Position.y = gl_Position.y * posFixup.y;
            // gl_Position.xy += posFixup.zw * gl_Position.ww;
            //
            // (the following paraphrased from wine, wined3d/state.c and wined3d/glsl_shader.c)
            //
            // - We need to flip along the y-axis in case of offscreen rendering.
            // - D3D coordinates refer to pixel centers while GL coordinates refer
            //   to pixel corners.
            // - D3D has a top-left filling convention. We need to maintain this
            //   even after the y-flip mentioned above.
            // In order to handle the last two points, we translate by
            // (63.0 / 128.0) / VPw and (63.0 / 128.0) / VPh. This is equivalent to
            // translating slightly less than half a pixel. We want the difference to
            // be large enough that it doesn't get lost due to rounding inside the
            // driver, but small enough to prevent it from interfering with any
            // anti-aliasing.
            //
            // OpenGL coordinates specify the center of the pixel while d3d coords specify
            // the corner. The offsets are stored in z and w in posFixup. posFixup.y contains
            // 1.0 or -1.0 to turn the rendering upside down for offscreen rendering. PosFixup.x
            // contains 1.0 to allow a mad.

            _posFixup[0] = 1.0f;
            _posFixup[1] = 1.0f;
            _posFixup[2] = (63.0f/64.0f)/Viewport.Width;
            _posFixup[3] = -(63.0f/64.0f)/Viewport.Height;

            //If we have a render target bound (rendering offscreen)
            if (IsRenderTargetBound)
            {
                //flip vertically
                _posFixup[1] *= -1.0f;
                _posFixup[3] *= -1.0f;
            }

            GL.Uniform4(posFixupLoc, 1, _posFixup);
            GraphicsExtensions.CheckGLError();
        }

        internal void PlatformApplyState(bool applyShaders)
        {
            Threading.EnsureUIThread();

            if ( _scissorRectangleDirty )
	        {
                var scissorRect = _scissorRectangle;
                if (!IsRenderTargetBound)
                    scissorRect.Y = _viewport.Height - scissorRect.Y - scissorRect.Height;
                GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
                GraphicsExtensions.CheckGLError();
	            _scissorRectangleDirty = false;
	        }

            if (_blendStateDirty)
            {
                _blendState.PlatformApplyState(this);
                _blendStateDirty = false;
            }
	        if ( _depthStencilStateDirty )
            {
	            _depthStencilState.PlatformApplyState(this);
                _depthStencilStateDirty = false;
            }
	        if ( _rasterizerStateDirty )
            {
	            _rasterizerState.PlatformApplyState(this);
	            _rasterizerStateDirty = false;
            }

            // If we're not applying shaders then early out now.
            if (!applyShaders)
                return;

            if (_indexBufferDirty)
            {
                if (_indexBuffer != null)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer.ibo);
                    GraphicsExtensions.CheckGLError();
                }
                _indexBufferDirty = false;
            }

            if (_vertexBufferDirty)
            {
                if (_vertexBuffer != null)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.vbo);
                    GraphicsExtensions.CheckGLError();
                }
            }

            if (_vertexShader == null)
                throw new InvalidOperationException("A vertex shader must be set!");
            if (_pixelShader == null)
                throw new InvalidOperationException("A pixel shader must be set!");

            if (_vertexShaderDirty || _pixelShaderDirty)
            {
                ActivateShaderProgram();
                _vertexShaderDirty = _pixelShaderDirty = false;
            }

            _vertexConstantBuffers.SetConstantBuffers(this, _shaderProgram);
            _pixelConstantBuffers.SetConstantBuffers(this, _shaderProgram);

            Textures.SetTextures(this);
            SamplerStates.PlatformSetSamplers(this);
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            PlatformApplyState(true);

            var shortIndices = _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits;

			var indexElementType = shortIndices ? DrawElementsType.UnsignedShort : DrawElementsType.UnsignedInt;
            var indexElementSize = shortIndices ? 2 : 4;
			var indexOffsetInBytes = (IntPtr)(startIndex * indexElementSize);
			var indexElementCount = GetElementCountArray(primitiveType, primitiveCount);
			var target = PrimitiveTypeGL(primitiveType);
			var vertexOffset = (IntPtr)(_vertexBuffer.VertexDeclaration.VertexStride * baseVertex);

			_vertexBuffer.VertexDeclaration.Apply(_vertexShader, vertexOffset);

            GL.DrawElements(target,
                                     indexElementCount,
                                     indexElementType,
                                     indexOffsetInBytes);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {
            PlatformApplyState(true);

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBufferDirty = _indexBufferDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            vertexDeclaration.Apply(_vertexShader, vbHandle.AddrOfPinnedObject());

            //Draw
            GL.DrawArrays(PrimitiveTypeGL(primitiveType),
                          vertexOffset,
                          vertexCount);
            GraphicsExtensions.CheckGLError();

            // Release the handles.
            vbHandle.Free();
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            PlatformApplyState(true);

            _vertexBuffer.VertexDeclaration.Apply(_vertexShader, IntPtr.Zero);

			GL.DrawArrays(PrimitiveTypeGL(primitiveType),
			              vertexStart,
			              vertexCount);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            PlatformApplyState(true);

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBufferDirty = _indexBufferDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            vertexDeclaration.Apply(_vertexShader, vertexAddr);

            //Draw
            GL.DrawElements(    PrimitiveTypeGL(primitiveType),
                                GetElementCountArray(primitiveType, primitiveCount),
                                DrawElementsType.UnsignedShort,
                                (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(short))));
            GraphicsExtensions.CheckGLError();

            // Release the handles.
            ibHandle.Free();
            vbHandle.Free();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct, IVertexType
        {
            PlatformApplyState(true);

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBufferDirty = _indexBufferDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            vertexDeclaration.Apply(_vertexShader, vertexAddr);

            //Draw
            GL.DrawElements(    PrimitiveTypeGL(primitiveType),
                                GetElementCountArray(primitiveType, primitiveCount),
                                DrawElementsType.UnsignedInt,
                                (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(int))));
            GraphicsExtensions.CheckGLError();

            // Release the handles.
            ibHandle.Free();
            vbHandle.Free();
        }

        private static GraphicsProfile PlatformGetHighestSupportedGraphicsProfile(GraphicsDevice graphicsDevice)
        {
           return GraphicsProfile.HiDef;
        }
    }
}
