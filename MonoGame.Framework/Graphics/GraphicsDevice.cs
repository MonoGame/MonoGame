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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif WINRT
using SharpDX;
using SharpDX.Direct3D;
using Windows.Graphics.Display;
using Windows.UI.Core;
#elif PSS
using Sce.Pss.Core.Graphics;
#elif GLES
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
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsDevice : IDisposable
    {
        private Viewport _viewport;

        private bool _isDisposed = false;

        private BlendState _blendState = BlendState.Opaque;
        private DepthStencilState _depthStencilState = DepthStencilState.Default;
		private RasterizerState _rasterizerState = RasterizerState.CullCounterClockwise;

        private bool _blendStateDirty;
        private bool _depthStencilStateDirty;
        private bool _rasterizerStateDirty;

        private Rectangle _scissorRectangle;
        private bool _scissorRectangleDirty;

        internal List<IntPtr> _pointerCache = new List<IntPtr>();
        private VertexBuffer _vertexBuffer = null;
        private IndexBuffer _indexBuffer = null;

        private RenderTargetBinding[] _currentRenderTargetBindings;

        private static RenderTargetBinding[] EmptyRenderTargetBinding = new RenderTargetBinding[0];

        public TextureCollection Textures { get; private set; }

        public SamplerStateCollection SamplerStates { get; private set; }

        private static Color DiscardColor = new Color(68, 34, 136, 255);

#if DIRECTX

        // Declare Direct2D Objects
        protected SharpDX.Direct2D1.Factory1 _d2dFactory;
        protected SharpDX.Direct2D1.Device _d2dDevice;
        protected SharpDX.Direct2D1.DeviceContext _d2dContext;

        // Declare DirectWrite & Windows Imaging Component Objects
        protected SharpDX.DirectWrite.Factory _dwriteFactory;
        protected SharpDX.WIC.ImagingFactory2 _wicFactory;

        // Direct3D Objects
        internal SharpDX.Direct3D11.Device1 _d3dDevice;
        internal SharpDX.Direct3D11.DeviceContext1 _d3dContext;
        protected FeatureLevel _featureLevel;

        // The backbuffer resources.
        protected SharpDX.Direct3D11.RenderTargetView _renderTargetView;
        protected SharpDX.Direct3D11.DepthStencilView _depthStencilView;
        protected SharpDX.Direct2D1.Bitmap1 _bitmapTarget;
        protected SharpDX.DXGI.SwapChain1 _swapChain;

        // The active render targets.
        protected SharpDX.Direct3D11.RenderTargetView[] _currentRenderTargets = new SharpDX.Direct3D11.RenderTargetView[4];

        // The active depth view.
        protected SharpDX.Direct3D11.DepthStencilView _currentDepthStencilView;

        protected float _dpi;

        /// <summary>
        /// The active vertex shader.
        /// </summary>
        internal DXShader _vertexShader;

        private readonly Dictionary<ulong, SharpDX.Direct3D11.InputLayout> _inputLayouts = new Dictionary<ulong, SharpDX.Direct3D11.InputLayout>();

        private readonly Dictionary<int, DynamicVertexBuffer> _userVertexBuffers = new Dictionary<int, DynamicVertexBuffer>();

        private readonly Dictionary<IndexElementSize, DynamicIndexBuffer> _userIndexBuffers = new Dictionary<IndexElementSize, DynamicIndexBuffer>();


#endif // DIRECTX

#if OPENGL

		// OpenGL ES2.0 attribute locations
		internal static int attributePosition = 0; //there can be a couple positions binded
		internal static int attributeColor = 3;
		internal static int attributeNormal = 4;
		internal static int attributeBlendIndicies = 5;
		internal static int attributeBlendWeight = 6;
		internal static int attributeTexCoord = 7; //must be the last one, texture index locations are added to it

        private uint VboIdArray;
        private uint VboIdElement;
        private All _preferedFilter;

        private int _activeTexture = -1;

#elif PSS

        internal GraphicsContext _graphics;

#endif

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
#elif OPENGL
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

		List<string> extensions = new List<string>();

        internal int glFramebuffer;

#if DIRECTX

        internal float Dpi
        {
            get { return _dpi; }
            set
            {
                if (_dpi == value)
                    return;

                _dpi = value;
                _d2dContext.DotsPerInch = new DrawingSizeF(_dpi, _dpi);

                //if (OnDpiChanged != null)
                    //OnDpiChanged(this);
            }
        }

#endif // DIRECTX


#if OPENGL

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

#endif

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
            _viewport.MaxDepth = 1.0f;

            Textures = new TextureCollection(16);
            SamplerStates = new SamplerStateCollection(16);

            PresentationParameters = new PresentationParameters();
        }

        internal void Initialize()
        {
            // Clear the effect cache since the
            // device context is going to be reset.
            Effect.FlushCache();

            // Setup extensions.
#if OPENGL
#if GLES
            string[] extstring = GL.GetString(RenderbufferStorage.Extensions).Split(' ');            			
#else
            string[] extstring = GL.GetString(StringName.Extensions).Split(' ');	
#endif
            if (extstring != null)
            {
                extensions.AddRange(extstring);
                System.Diagnostics.Debug.WriteLine("Supported extensions:");
                foreach (string extension in extensions)
                    System.Diagnostics.Debug.WriteLine(extension);
            }

#endif // OPENGL

            PresentationParameters.DisplayOrientation = TouchPanel.DisplayOrientation;

#if DIRECTX

            CreateDeviceIndependentResources();
            CreateDeviceResources();
            Dpi = DisplayProperties.LogicalDpi;
            CreateSizeDependentResources();

#elif PSS
            _graphics = new GraphicsContext();
#elif OPENGL

            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
            VboIdArray = 0;
            VboIdElement = 0;
#endif

            // Force set the default render states.
            _blendStateDirty = _depthStencilStateDirty = _rasterizerStateDirty = true;
            BlendState = BlendState.Opaque;
            DepthStencilState = DepthStencilState.Default;
            RasterizerState = RasterizerState.CullCounterClockwise;

#if OPENGL
            // Force the GLStateManager to set these render states
            GLStateManager.SetRasterizerStates(RasterizerState, GetRenderTargets().Length > 0);
            GLStateManager.SetBlendStates(BlendState);
            GLStateManager.SetDepthStencilState(DepthStencilState);
#endif

            // Set the default render target.
            ApplyRenderTargets(null);

            // Set the default scissor rect.
            _scissorRectangleDirty = true;
            ScissorRectangle = _viewport.Bounds;
        }

#if DIRECTX

        /// <summary>
        /// Creates resources not tied the active graphics device.
        /// </summary>
        protected void CreateDeviceIndependentResources()
        {
#if DEBUG
            var debugLevel = SharpDX.Direct2D1.DebugLevel.Information;
#else 
            var debugLevel = SharpDX.Direct2D1.DebugLevel.None; 
#endif
            // Dispose previous references.
            if (_d2dFactory != null)
                _d2dFactory.Dispose();
            if (_dwriteFactory != null)
                _dwriteFactory.Dispose();
            if (_wicFactory != null)
                _wicFactory.Dispose();

            // Allocate new references
            _d2dFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.SingleThreaded, debugLevel);
            _dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
            _wicFactory = new SharpDX.WIC.ImagingFactory2();
        }

        /// <summary>
        /// Create graphics device specific resources.
        /// </summary>
        protected virtual void CreateDeviceResources()
        {
            // Dispose previous references.
            if (_d3dDevice != null)
                _d3dDevice.Dispose();
            if (_d3dContext != null)
                _d3dContext.Dispose();
            if (_d2dDevice != null)
                _d2dDevice.Dispose();
            if (_d2dContext != null)
                _d2dContext.Dispose();

            // Retrieve the Direct3D 11.1 device amd device context
            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport;
#if DEBUG
            creationFlags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
#endif

            // Create the Direct3D device.
            using (var defaultDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags))
                _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
            _featureLevel = _d3dDevice.FeatureLevel;

            // Get Direct3D 11.1 context
            _d3dContext = _d3dDevice.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext1>();

            // Create the Direct2D device.
            using (var dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device>())
                _d2dDevice = new SharpDX.Direct2D1.Device(_d2dFactory, dxgiDevice);

            // Create Direct2D context
            _d2dContext = new SharpDX.Direct2D1.DeviceContext(_d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
        }

        internal void CreateSizeDependentResources()
        {
            if (PresentationParameters == null || PresentationParameters.DeviceWindowHandle == IntPtr.Zero)
                return;

            _d2dContext.Target = null;
            if (_renderTargetView != null)
                _renderTargetView.Dispose();
            if (_depthStencilView != null)
                _depthStencilView.Dispose();
            if (_bitmapTarget != null)
                _bitmapTarget.Dispose();

			// Clear the current render targets.
            _currentDepthStencilView = null;
            _currentRenderTargets[0] = null;
            _currentRenderTargets[1] = null;
            _currentRenderTargets[2] = null;
            _currentRenderTargets[3] = null;
            _currentDepthStencilView = null;
            _currentRenderTargetBindings = null;
            
            var multisampleDesc = new SharpDX.DXGI.SampleDescription(1, 0);
            if ( PresentationParameters.MultiSampleCount > 1 )
            {
                multisampleDesc.Count = PresentationParameters.MultiSampleCount;
                multisampleDesc.Quality = (int)SharpDX.Direct3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            var format = SharpDXHelper.ToFormat(PresentationParameters.BackBufferFormat);

            // If the swap chain already exists... update it.
            if (_swapChain != null)
                _swapChain.ResizeBuffers(   2,
                                            PresentationParameters.BackBufferWidth, 
                                            PresentationParameters.BackBufferHeight,
                                            format,
                                            0); // SharpDX.DXGI.SwapChainFlags

            // Otherwise, create a new swap chain.
            else
            {
                // SwapChain description
                var desc =  new SharpDX.DXGI.SwapChainDescription1()
                {
                    // Automatic sizing
                    Width = PresentationParameters.BackBufferWidth,
                    Height = PresentationParameters.BackBufferHeight,
                    Format = format,
                    Stereo = false,
                    SampleDescription = multisampleDesc,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,                    
                    SwapEffect = SharpDXHelper.ToSwapEffect(PresentationParameters),

                    // By default we scale the backbuffer to the window 
                    // rectangle to function more like a WP7 game.
                    Scaling = SharpDX.DXGI.Scaling.Stretch,
                };

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device

                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (var dxgiDevice2 = _d3dDevice.QueryInterface<SharpDX.DXGI.Device2>())
                using (var dxgiAdapter = dxgiDevice2.Adapter)
                using (var dxgiFactory2 = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>())
                {
                    // Creates a SwapChain from a CoreWindow pointer.
                    var coreWindow = Marshal.GetObjectForIUnknown(PresentationParameters.DeviceWindowHandle) as CoreWindow;
                    using (var comWindow = new ComObject(coreWindow))
                        _swapChain = dxgiFactory2.CreateSwapChainForCoreWindow(_d3dDevice, comWindow, ref desc, null);

                    // Ensure that DXGI does not queue more than one frame at a time. This both reduces 
                    // latency and ensures that the application will only render after each VSync, minimizing 
                    // power consumption.
                    dxgiDevice2.MaximumFrameLatency = 1;
                }
            }

            // Obtain the backbuffer for this window which will be the final 3D rendertarget.
            Point targetSize;
            using (var backBuffer = SharpDX.Direct3D11.Texture2D.FromSwapChain<SharpDX.Direct3D11.Texture2D>(_swapChain, 0))
            {
                // Create a view interface on the rendertarget to use on bind.
                _renderTargetView = new SharpDX.Direct3D11.RenderTargetView(_d3dDevice, backBuffer);

                // Get the rendertarget dimensions for later.
                var backBufferDesc = backBuffer.Description;
                targetSize = new Point(backBufferDesc.Width, backBufferDesc.Height);
            }

            // Create the depth buffer if we need it.
            if (PresentationParameters.DepthStencilFormat != DepthFormat.None)
            {
                // Allocate a 2-D surface as the depth/stencil buffer.
                using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(_d3dDevice, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Format = SharpDXHelper.ToFormat(PresentationParameters.DepthStencilFormat),
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = targetSize.X,
                    Height = targetSize.Y,
                    SampleDescription = multisampleDesc,
                    BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
                }))

                // Create a DepthStencil view on this surface to use on bind.
                _depthStencilView = new SharpDX.Direct3D11.DepthStencilView(_d3dDevice, depthBuffer,
                    new SharpDX.Direct3D11.DepthStencilViewDescription()
                    {
                        Format = SharpDXHelper.ToFormat(PresentationParameters.DepthStencilFormat),
                        Dimension = SharpDX.Direct3D11.DepthStencilViewDimension.Texture2D
                    });
            }

            // Set the current viewport.
            Viewport = new Viewport
            { 
                X = 0, 
                Y = 0,
                Width = targetSize.X, 
                Height = targetSize.Y, 
                MinDepth = 0.0f, 
                MaxDepth = 1.0f 
            };

            // Now we set up the Direct2D render target bitmap linked to the swapchain. 
            // Whenever we render to this bitmap, it will be directly rendered to the 
            // swapchain associated with the window.
            var bitmapProperties = new SharpDX.Direct2D1.BitmapProperties1(
                new SharpDX.Direct2D1.PixelFormat(format, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                _dpi, _dpi,
                SharpDX.Direct2D1.BitmapOptions.Target | SharpDX.Direct2D1.BitmapOptions.CannotDraw);
            
            // Direct2D needs the dxgi version of the backbuffer surface pointer.
            // Get a D2D surface from the DXGI back buffer to use as the D2D render target.
            using (var dxgiBackBuffer = _swapChain.GetBackBuffer<SharpDX.DXGI.Surface>(0))
                _bitmapTarget = new SharpDX.Direct2D1.Bitmap1(_d2dContext, dxgiBackBuffer, bitmapProperties);

            // So now we can set the Direct2D render target.
            _d2dContext.Target = _bitmapTarget;

            // Set D2D text anti-alias mode to Grayscale to 
            // ensure proper rendering of text on intermediate surfaces.
            _d2dContext.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
        }

#endif // WINRT

        public RasterizerState RasterizerState
        {
            get
            {
                return _rasterizerState;
            }

            set
            {
                // Don't set the same state twice!
                if (_rasterizerState == value)
                    return;

                _rasterizerState = value;
                _rasterizerStateDirty = true;

#if OPENGL
				GLStateManager.SetRasterizerStates(value, GetRenderTargets().Length > 0);
#endif
            }
        }

        public BlendState BlendState 
        {
			get { return _blendState; }
			set 
            {
                // Don't set the same state twice!
                if (_blendState == value)
                    return;

				// ToDo check for invalid state
				_blendState = value;
                _blendStateDirty = true;

#if OPENGL
				GLStateManager.SetBlendStates(value);
#endif
            }
		}

        public DepthStencilState DepthStencilState
        {
            get { return _depthStencilState; }
            set
            {
                // Don't set the same state twice!
                if (_depthStencilState == value)
                    return;

                _depthStencilState = value;
                _depthStencilStateDirty = true;
#if OPENGL
				GLStateManager.SetDepthStencilState(value);
#endif
            }
        }

        public void Clear(Color color)
        {
			ClearOptions options = ClearOptions.Target;

#if DIRECTX

            if (_currentDepthStencilView != null)
            {
                options |= ClearOptions.DepthBuffer;

                if (_currentDepthStencilView.Description.Format == SharpDX.DXGI.Format.D24_UNorm_S8_UInt)
                    options |= ClearOptions.Stencil;
            }

#else
            // TODO: We need to figure out how to detect if
            // we have a depth stencil buffer or not!
            options |= ClearOptions.DepthBuffer;
            options |= ClearOptions.Stencil;
#endif

            Clear (options, color.ToVector4(), _viewport.MaxDepth, 0);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Clear (options, color.ToVector4 (), depth, stencil);
        }

		public void Clear (ClearOptions options, Vector4 color, float depth, int stencil)
		{
#if DIRECTX
            lock (_d3dContext)
            {
                // Clear the diffuse render buffer.
                if ((options & ClearOptions.Target) != 0)
                {
                    foreach (var view in _currentRenderTargets)
                    {
                        if (view != null)
                            _d3dContext.ClearRenderTargetView(view, new Color4(color.X, color.Y, color.Z, color.W));
                    }
                }

                // Clear the depth/stencil render buffer.
                SharpDX.Direct3D11.DepthStencilClearFlags flags = 0;
                if ((options & ClearOptions.DepthBuffer) != 0)
                    flags |= SharpDX.Direct3D11.DepthStencilClearFlags.Depth;
                if ((options & ClearOptions.Stencil) != 0)
                    flags |= SharpDX.Direct3D11.DepthStencilClearFlags.Stencil;

                if (flags != 0 && _currentDepthStencilView != null)
                    _d3dContext.ClearDepthStencilView(_currentDepthStencilView, flags, depth, (byte)stencil);
            }

#elif PSS

            _graphics.SetClearColor(color.ToPssVector4());
            _graphics.Clear();

#elif OPENGL

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

#if GLES
			GL.Clear ((uint)bufferMask);
#else
			GL.Clear (bufferMask);
#endif
#endif // OPENGL
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
            Dispose(true);
        }

        protected virtual void Dispose(bool aReleaseEverything)
        {
            if (aReleaseEverything)
            {
#if DIRECTX
                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }
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
                if (_bitmapTarget != null)
                {
                    _bitmapTarget.Dispose();
                    _depthStencilView = null;
                }

                if (_d3dDevice != null)
                {
                    _d3dDevice.Dispose();
                    _d3dDevice = null;
                }
                if (_d3dContext != null)
                {
                    _d3dContext.Dispose();
                    _d3dContext = null;
                }
                if (_d2dDevice != null)
                {
                    _d2dDevice.Dispose();
                    _d2dDevice = null;
                }
                if (_d2dContext != null)
                {
                    _d2dContext.Target = null;
                    _d2dContext.Dispose();
                    _d2dContext = null;
                }
                if (_d2dFactory != null)
                {
                    _d2dFactory.Dispose();
                    _d2dFactory = null;
                }
                if (_dwriteFactory != null)
                {
                    _dwriteFactory.Dispose();
                    _dwriteFactory = null;
                }
                if (_wicFactory != null)
                {
                    _wicFactory.Dispose();
                    _wicFactory = null;
                }

#endif // DIRECTX

#if PSS
                if (_graphics != null)
                {
                    _graphics.Dispose();
                    _graphics = null;
                }
#endif
            }

            _isDisposed = true;
        }

        public void Present()
        {
#if DIRECTX
            // The application may optionally specify "dirty" or "scroll" rects to improve efficiency
            // in certain scenarios.  In this sample, however, we do not utilize those features.
            var parameters = new SharpDX.DXGI.PresentParameters();
            
            try
            {
                // TODO: Hook in PresentationParameters here!

                // The first argument instructs DXGI to block until VSync, putting the application
                // to sleep until the next VSync. This ensures we don't waste any cycles rendering
                // frames that will never be displayed to the screen.
                _swapChain.Present(1, SharpDX.DXGI.PresentFlags.None, parameters);
            }
            catch (SharpDX.SharpDXException)
            {
                // TODO: How should we deal with a device lost case here?

                /*               
                // If the device was removed either by a disconnect or a driver upgrade, we 
                // must completely reinitialize the renderer.
                if (    ex.ResultCode == SharpDX.DXGI.DXGIError.DeviceRemoved ||
                        ex.ResultCode == SharpDX.DXGI.DXGIError.DeviceReset)
                    this.Initialize();
                else
                    throw;
                */
            }
						
#elif PSS
            _graphics.SwapBuffers();
#elif ANDROID
			Game.Instance.Platform.Present();
#elif OPENGL
			GL.Flush ();
#endif
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
            private set;
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
#if DIRECTX
                var viewport = new SharpDX.Direct3D11.Viewport(_viewport.X, _viewport.Y, (float)_viewport.Width, (float)_viewport.Height, _viewport.MinDepth, _viewport.MaxDepth);
                lock (_d3dContext) 
                    _d3dContext.Rasterizer.SetViewports(viewport);
#elif OPENGL
				GL.Viewport (value.X, value.Y, value.Width, value.Height);
				GL.DepthRange(value.MinDepth, value.MaxDepth);
#endif
            }
        }

        public Microsoft.Xna.Framework.Graphics.GraphicsProfile GraphicsProfile
        {
            get;
            set;
        }

        public Rectangle ScissorRectangle
        {
            get
            {
                return _scissorRectangle;
            }

            set
            {
                if (_scissorRectangle == value)
                    return;

                _scissorRectangle = value;
                _scissorRectangleDirty = true;

#if OPENGL
                var glScissorRectangle = _scissorRectangle;
                glScissorRectangle.Y = _viewport.Height - glScissorRectangle.Y - glScissorRectangle.Height;
                GLStateManager.SetScissor(glScissorRectangle);				
#endif
            }
        }

		public void SetRenderTarget(RenderTarget2D renderTarget)
		{
			if (renderTarget == null)
                SetRenderTargets(null);
			else
				SetRenderTargets(new RenderTargetBinding(renderTarget));
		}
		
		public void SetRenderTargets(params RenderTargetBinding[] renderTargets) 
		{
            // If the default swap chain is already set then do nothing.
            if (_currentRenderTargetBindings == null && renderTargets == null)
                return;

            // If the bindings are the same then early out as well.
            if (    _currentRenderTargetBindings != null && renderTargets != null &&
                    _currentRenderTargetBindings.Length == renderTargets.Length )
            {
                var isEqual = true;

                for (var i = 0; i < _currentRenderTargetBindings.Length; i++)
                {
                    if (_currentRenderTargetBindings[i].RenderTarget != renderTargets[i].RenderTarget)
                    {
                        isEqual = false;
                        break;
                    }
                }

                if ( isEqual )
                    return;
            }

            ApplyRenderTargets(renderTargets);
        }

        internal void ApplyRenderTargets(RenderTargetBinding[] renderTargets)
        {
            // The render target is really changing now.
            var previousRenderTargetBindings = _currentRenderTargetBindings;
            _currentRenderTargetBindings = renderTargets;
		
            var clearTarget = false;

            if (_currentRenderTargetBindings == null || _currentRenderTargetBindings.Length == 0)
			{
#if DIRECTX
                // Set the default swap chain.
                _currentRenderTargets[0] = _renderTargetView;
                _currentRenderTargets[1] = null;
                _currentRenderTargets[2] = null;
                _currentRenderTargets[3] = null;
                _currentDepthStencilView = _depthStencilView;

                lock (_d3dContext) 
                    _d3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);                
#elif OPENGL
				GL.BindFramebuffer(GLFramebuffer, 0);
#endif

                clearTarget = true;

                Viewport = new Viewport(0, 0,
					PresentationParameters.BackBufferWidth, 
					PresentationParameters.BackBufferHeight);
			}
			else
			{
                var renderTarget = _currentRenderTargetBindings[0].RenderTarget as RenderTarget2D;

#if DIRECTX
                // Set the new render targets.
                _currentRenderTargets[0] = null;
                _currentRenderTargets[1] = null;
                _currentRenderTargets[2] = null;
                _currentRenderTargets[3] = null;
                _currentDepthStencilView = null;

                for (var b = 0; b < _currentRenderTargetBindings.Length; b++)
                {
                    var target = _currentRenderTargetBindings[b]._renderTarget as RenderTarget2D;
                    _currentRenderTargets[b] = target._renderTargetView;

                    // Use the depth from the first target.
                    if (b == 0)
                        _currentDepthStencilView = target._depthStencilView;
                }

                // Set the targets.
                lock (_d3dContext)
                    _d3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);

#elif OPENGL
				if (this.glFramebuffer == 0)
				{
#if GLES
					GL.GenFramebuffers(1, ref this.glFramebuffer);
#else
					GL.GenFramebuffers(1, out this.glFramebuffer);
#endif
				}

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
                                
#endif
                // Set the viewport to the size of the first render target.
                Viewport = new Viewport(0, 0, renderTarget.Width, renderTarget.Height);

                // We clear the render target if asked.
                clearTarget = renderTarget.RenderTargetUsage != RenderTargetUsage.DiscardContents;
            }

            // In XNA 4, because of hardware limitations on Xbox, when
            // a render target doesn't have PreserveContents as its usage
            // it is cleared before being rendered to.
            if (clearTarget)
                Clear(DiscardColor);

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

#if OPENGL
			//Reset the cull mode, because we flip verticies when rendering offscreen
			//and thus flip the cull direction
			GLStateManager.Cull (RasterizerState, GetRenderTargets().Length > 0);
#endif
        }

#if WINRT
        internal void ResetRenderTargets()
        {
            if ( _d3dContext != null )
                lock (_d3dContext)
                    _d3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);                
        }
#endif

		public RenderTargetBinding[] GetRenderTargets()
		{
            if (_currentRenderTargetBindings == null)
                return EmptyRenderTargetBinding;

            return _currentRenderTargetBindings;
		}

#if OPENGL
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
		
#elif DIRECTX

        internal PrimitiveTopology ToPrimitiveTopology(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return PrimitiveTopology.LineList;
                case PrimitiveType.LineStrip:
                    return PrimitiveTopology.LineStrip;
                case PrimitiveType.TriangleList:
                    return PrimitiveTopology.TriangleList;
                case PrimitiveType.TriangleStrip:
                    return PrimitiveTopology.TriangleStrip;
            }

            throw new NotImplementedException();
        }
		
#endif


        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            // TODO: Should be be protecting against setting the
            // same VB twice or is that the caller's job?

            _vertexBuffer = vertexBuffer;
			
#if DIRECTX
            lock (_d3dContext)
            {
                if (_vertexBuffer != null)
                    _d3dContext.InputAssembler.SetVertexBuffers(0, _vertexBuffer._binding);
                else
                    _d3dContext.InputAssembler.SetVertexBuffers(0, null);
            }
#elif PSS
            _graphics.SetVertexBuffer(0, vertexBuffer._buffer);
#elif OPENGL
            if (_vertexBuffer != null)
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.vbo);
#endif
        }

        private void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            _indexBuffer = indexBuffer;
            if (_indexBuffer == null)
                return;

#if DIRECTX
            lock(_d3dContext)
                _d3dContext.InputAssembler.SetIndexBuffer(
                    _indexBuffer._buffer, 
                    _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? SharpDX.DXGI.Format.R16_UInt : SharpDX.DXGI.Format.R32_UInt,
                    0 );
#elif OPENGL
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer.ibo);
#endif
        }

        public IndexBuffer Indices { set { SetIndexBuffer(value); } }

        public bool ResourcesLost { get; set; }

#if DIRECTX

        private void ApplyState()
        {
            Debug.Assert(_d3dContext != null, "The d3d context is null!");

            // NOTE: This code assumes _d3dContext has been locked by the caller.

            if ( _scissorRectangleDirty )
	        {
	            _d3dContext.Rasterizer.SetScissorRectangle(_scissorRectangle.X, _scissorRectangle.Y, _scissorRectangle.Right, _scissorRectangle.Bottom);
	            _scissorRectangleDirty = false;
	        }

	        if ( _blendStateDirty)
	            _blendState.ApplyState(this);
	        if ( _depthStencilStateDirty )
	            _depthStencilState.ApplyState(this);
	        if ( _rasterizerStateDirty )
	            _rasterizerState.ApplyState(this);

	        _blendStateDirty = _depthStencilStateDirty = _rasterizerStateDirty = false;

	        SamplerStates.SetSamplers(this);
	        Textures.SetTextures(this);

            _d3dContext.InputAssembler.InputLayout = GetInputLayout(_vertexShader, _vertexBuffer.VertexDeclaration);
        }

        private SharpDX.Direct3D11.InputLayout GetInputLayout(DXShader shader, VertexDeclaration decl)
        {
            SharpDX.Direct3D11.InputLayout layout;

            // Lookup the layout using the shader and declaration as the key.
            var key = (ulong)decl.HashKey << 32 | (uint)shader.HashKey;           
            if (!_inputLayouts.TryGetValue(key, out layout))
            {
                layout = new SharpDX.Direct3D11.InputLayout(_d3dDevice, shader.Bytecode, decl.GetInputLayout());
                _inputLayouts.Add(key, layout);
            }

            return layout;
        }

        private int SetUserVertexBuffer<T>(T[] vertexData, int vertexOffset, int vertexCount, VertexDeclaration vertexDecl) 
            where T : struct, IVertexType
        {
            DynamicVertexBuffer buffer;

            if (!_userVertexBuffers.TryGetValue(vertexDecl.HashKey, out buffer) || buffer.VertexCount < vertexCount)
            {
                // Dispose the previous buffer if we have one.
                if (buffer != null)
                    buffer.Dispose();

                buffer = new DynamicVertexBuffer(this, vertexDecl, Math.Max(vertexCount, 2000), BufferUsage.WriteOnly);
                _userVertexBuffers[vertexDecl.HashKey] = buffer;
            }

            var startVertex = buffer.UserOffset;

            var copyCount = vertexCount - vertexOffset;
            if ((copyCount + buffer.UserOffset) < buffer.VertexCount)
            {
                buffer.UserOffset += copyCount;
                buffer.SetData(startVertex * vertexDecl.VertexStride, vertexData, vertexOffset, copyCount, SetDataOptions.NoOverwrite);
            }
            else
            {
                buffer.UserOffset = copyCount;
                buffer.SetData(vertexData, vertexOffset, copyCount, SetDataOptions.Discard);
                startVertex = 0;
            }

            SetVertexBuffer(buffer);

            return startVertex;
        }

        private int SetUserIndexBuffer<T>(T[] indexData, int indexOffset, int indexCount)
            where T : struct
        {
            DynamicIndexBuffer buffer;

            var indexSize = typeof(T) == typeof(short) ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits;

            if (!_userIndexBuffers.TryGetValue(indexSize, out buffer) || buffer.IndexCount < indexCount)
            {
                if (buffer != null)
                    buffer.Dispose();

                buffer = new DynamicIndexBuffer(this, indexSize, Math.Max(indexCount, 6000), BufferUsage.WriteOnly);
                _userIndexBuffers[indexSize] = buffer;
            }

            var startIndex = buffer.UserOffset;

            var copyCount = indexCount - indexOffset;
            if ((copyCount + buffer.UserOffset) < buffer.IndexCount)
            {
                buffer.UserOffset += copyCount;
                buffer.SetData(startIndex * 2, indexData, indexOffset, copyCount, SetDataOptions.NoOverwrite);
            }
            else
            {
                startIndex = 0;
                buffer.UserOffset = copyCount;
                buffer.SetData(indexData, indexOffset, copyCount, SetDataOptions.Discard);
            }

            Indices = buffer;

            return startIndex;
        }
#endif

        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numbVertices, int startIndex, int primitiveCount)
        {
            Debug.Assert(_vertexBuffer != null, "The vertex buffer is null!");
            Debug.Assert(_indexBuffer != null, "The index buffer is null!");

			if (minVertexIndex > 0)
				throw new NotImplementedException ("minVertexIndex > 0 is supported");

#if DIRECTX

            lock (_d3dContext)
            {
                ApplyState();

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);

                var vertexCount = GetElementCountArray(primitiveType, primitiveCount);
                _d3dContext.DrawIndexed(vertexCount, startIndex, baseVertex);
            }
			
#elif OPENGL

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
#endif
        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {
            DrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct, IVertexType
        {            
            Debug.Assert(vertexData != null && vertexData.Length > 0, "The vertexData must not be null or zero length!");

            var vertexCount = GetElementCountArray(primitiveType, primitiveCount);

#if DIRECTX

            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, vertexCount, vertexDeclaration);

            lock (_d3dContext)
            {
                ApplyState();

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.Draw(vertexCount, startVertex);
            }

#elif OPENGL
            // Unbind the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //Create VBO if not created already
#if GLES
            if (VboIdArray == 0)
                GL.GenBuffers(1, ref VboIdArray);
#else
            if (VboIdArray == 0)
                GL.GenBuffers(1, out VboIdArray);
#endif

            // Bind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboIdArray);
            ////Clear previous data
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexDeclaration.VertexStride * vertexData.Length - vertexOffset * vertexDeclaration.VertexStride), IntPtr.Zero, BufferUsageHint.StreamDraw);

            //Pin data
            var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

            //Buffer data to VBO; This should use stream when we move to ES2.0
            GL.BufferData(BufferTarget.ArrayBuffer,
                            (IntPtr)(vertexDeclaration.VertexStride * vertexData.Length - vertexOffset * vertexDeclaration.VertexStride),
                            new IntPtr(handle.AddrOfPinnedObject().ToInt64() + vertexOffset * vertexDeclaration.VertexStride),
                            BufferUsageHint.StreamDraw);

            //Setup VertexDeclaration
            vertexDeclaration.Apply();

            //Draw
            GL.DrawArrays(PrimitiveTypeGL(primitiveType),
                          vertexOffset,
                          vertexCount);

            // Free resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            handle.Free();
#endif
        }

        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            Debug.Assert(_vertexBuffer != null, "The vertex buffer is null!");

            var vertexCount = GetElementCountArray(primitiveType, primitiveCount);

#if DIRECTX

            lock (_d3dContext)
            {
                ApplyState();

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.Draw(vertexCount, vertexStart);
            }

#elif OPENGL

			_vertexBuffer.VertexDeclaration.Apply();

			GL.DrawArrays(PrimitiveTypeGL(primitiveType),
			              vertexStart,
			              vertexCount);
#elif PSS
            _graphics.DrawArrays(PSSHelper.ToDrawMode(primitiveType), vertexStart, vertexCount);
#endif
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct, IVertexType
        {
            Debug.Assert(vertexData != null && vertexData.Length > 0, "The vertexData must not be null or zero length!");
            Debug.Assert(indexData != null && indexData.Length > 0, "The indexData must not be null or zero length!");

#if DIRECTX

            var indexCount = GetElementCountArray(primitiveType, primitiveCount);
            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            var startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);

            lock (_d3dContext)
            {
                ApplyState();

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.DrawIndexed(indexCount, startIndex, startVertex);
            }

#elif OPENGL
            // Unbind the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //Create VBO if not created already
#if GLES
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
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexDeclaration.VertexStride * vertexData.Length - vertexOffset * vertexDeclaration.VertexStride), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * indexData.Length), IntPtr.Zero, BufferUsageHint.StreamDraw);

            // TODO: Why two handles when we only need one?
            //
            //Pin data
            var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);


            //Buffer data to VBO; This should use stream when we move to ES2.0
            GL.BufferData(BufferTarget.ArrayBuffer,
                            (IntPtr)(vertexDeclaration.VertexStride * vertexData.Length - vertexOffset * vertexDeclaration.VertexStride),
                            new IntPtr(handle.AddrOfPinnedObject().ToInt64() + vertexOffset * vertexDeclaration.VertexStride),
                            BufferUsageHint.StreamDraw);

            GL.BufferData(BufferTarget.ElementArrayBuffer,
                            (IntPtr)(sizeof(ushort) * indexData.Length),
                            indexData, BufferUsageHint.DynamicDraw);

            //Setup VertexDeclaration
            vertexDeclaration.Apply();

            //Draw
            GL.DrawElements(PrimitiveTypeGL(primitiveType),
                            GetElementCountArray(primitiveType, primitiveCount),
                            DrawElementsType.UnsignedShort/* .UnsignedInt248Oes*/,
                            (IntPtr)(indexOffset * sizeof(ushort)));


            // Free resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            handle.Free();
            handle2.Free();
#endif
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct, IVertexType
        {
            Debug.Assert(vertexData != null && vertexData.Length > 0, "The vertexData must not be null or zero length!");
            Debug.Assert(indexData != null && indexData.Length > 0, "The indexData must not be null or zero length!");

#if DIRECTX

            var indexCount = GetElementCountArray(primitiveType, primitiveCount);
            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            var startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);

            lock (_d3dContext)
            {
                ApplyState();

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.DrawIndexed(indexCount, startIndex, startVertex);
            }

#elif OPENGL
            // Unbind the VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //Create VBO if not created already
#if GLES
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
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexDeclaration.VertexStride * vertexData.Length - vertexOffset * vertexDeclaration.VertexStride), IntPtr.Zero, BufferUsageHint.StreamDraw);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * indexData.Length), IntPtr.Zero, BufferUsageHint.StreamDraw);

            //Pin data
            var handle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var handle2 = GCHandle.Alloc(vertexData, GCHandleType.Pinned);


            //Buffer data to VBO; This should use stream when we move to ES2.0
            GL.BufferData(BufferTarget.ArrayBuffer,
                            (IntPtr)(vertexDeclaration.VertexStride * vertexData.Length - vertexOffset * vertexDeclaration.VertexStride),
                            new IntPtr(handle.AddrOfPinnedObject().ToInt64() + vertexOffset * vertexDeclaration.VertexStride),
                            BufferUsageHint.StreamDraw);

            GL.BufferData(BufferTarget.ElementArrayBuffer,
                            (IntPtr)(sizeof(uint) * indexData.Length),
                            indexData, BufferUsageHint.DynamicDraw);

            //Setup VertexDeclaration
            vertexDeclaration.Apply();

            //Draw
            GL.DrawElements(PrimitiveTypeGL(primitiveType),
                            GetElementCountArray(primitiveType, primitiveCount),
                            DrawElementsType.UnsignedInt/* .UnsignedInt248Oes*/,
                            (IntPtr)(indexOffset * sizeof(uint)));


            // Free resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            handle.Free();
            handle2.Free();
#endif
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
		
    }
}
