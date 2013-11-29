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

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
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
#elif DIRECTX
using SharpDX;
using SharpDX.Direct3D;
#if WINDOWS_PHONE
using SharpDX.Direct3D11;
using Windows.Foundation;
using MonoGame.Framework.WindowsPhone;
#elif WINDOWS_STOREAPP
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Display;
using Windows.UI.Core;
using SharpDX.DXGI;
#elif WINDOWS
using SharpDX.DXGI;
#endif
#elif PSM
using Sce.PlayStation.Core.Graphics;
using PssVertexBuffer = Sce.PlayStation.Core.Graphics.VertexBuffer;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsDevice : IDisposable
    {
        private Viewport _viewport;

        private bool _isDisposed;

        private BlendState _blendState = BlendState.Opaque;
        private DepthStencilState _depthStencilState = DepthStencilState.Default;
		private RasterizerState _rasterizerState = RasterizerState.CullCounterClockwise;

        private bool _blendStateDirty;
        private bool _depthStencilStateDirty;
        private bool _rasterizerStateDirty;

        private Rectangle _scissorRectangle;
        private bool _scissorRectangleDirty;
  
        private VertexBuffer _vertexBuffer;
        private bool _vertexBufferDirty;

        private IndexBuffer _indexBuffer;
        private bool _indexBufferDirty;

        private readonly RenderTargetBinding[] _currentRenderTargetBindings = new RenderTargetBinding[4];
        private int _currentRenderTargetCount;

#if OPENGL && !GLES
		private DrawBuffersEnum[] _drawBuffers;
#endif

        public TextureCollection Textures { get; private set; }

        public SamplerStateCollection SamplerStates { get; private set; }

        private static readonly Color DiscardColor = new Color(68, 34, 136, 255);

        /// <summary>
        /// The active vertex shader.
        /// </summary>
        private Shader _vertexShader;
        private bool _vertexShaderDirty;

        /// <summary>
        /// The active pixel shader.
        /// </summary>
        private Shader _pixelShader;
        private bool _pixelShaderDirty;

#if OPENGL
        static List<Action> disposeActions = new List<Action>();
        static object disposeActionsLock = new object();
#endif

        private readonly ConstantBufferCollection _vertexConstantBuffers = new ConstantBufferCollection(ShaderStage.Vertex, 16);
        private readonly ConstantBufferCollection _pixelConstantBuffers = new ConstantBufferCollection(ShaderStage.Pixel, 16);

#if DIRECTX

        // Core Direct3D Objects
        internal SharpDX.Direct3D11.Device _d3dDevice;
        internal SharpDX.Direct3D11.DeviceContext _d3dContext;
        internal FeatureLevel _featureLevel;
        internal SharpDX.Direct3D11.RenderTargetView _renderTargetView;
        internal SharpDX.Direct3D11.DepthStencilView _depthStencilView;

#if WINDOWS_STOREAPP

        // Declare Direct2D Objects
        SharpDX.Direct2D1.Factory1 _d2dFactory;
        SharpDX.Direct2D1.Device _d2dDevice;
        SharpDX.Direct2D1.DeviceContext _d2dContext;

        // Declare DirectWrite & Windows Imaging Component Objects
        SharpDX.DirectWrite.Factory _dwriteFactory;
        SharpDX.WIC.ImagingFactory2 _wicFactory;

        // The swap chain resources.
        SharpDX.Direct2D1.Bitmap1 _bitmapTarget;
        SharpDX.DXGI.SwapChain1 _swapChain;
        SwapChainBackgroundPanel _swapChainPanel;

        float _dpi; 

#elif !WINDOWS_PHONE

        SwapChain _swapChain;

#endif

        // The active render targets.
        readonly SharpDX.Direct3D11.RenderTargetView[] _currentRenderTargets = new SharpDX.Direct3D11.RenderTargetView[4];

        // The active depth view.
        SharpDX.Direct3D11.DepthStencilView _currentDepthStencilView;

        private readonly Dictionary<ulong, SharpDX.Direct3D11.InputLayout> _inputLayouts = new Dictionary<ulong, SharpDX.Direct3D11.InputLayout>();

        private readonly Dictionary<int, DynamicVertexBuffer> _userVertexBuffers = new Dictionary<int, DynamicVertexBuffer>();

        private readonly Dictionary<IndexElementSize, DynamicIndexBuffer> _userIndexBuffers = new Dictionary<IndexElementSize, DynamicIndexBuffer>();


#endif // DIRECTX

#if OPENGL

        private readonly ShaderProgramCache _programCache = new ShaderProgramCache();

        private int _shaderProgram = -1;

        static readonly float[] _posFixup = new float[4];

        internal static readonly List<int> _enabledVertexAttributes = new List<int>();

#elif PSM

        internal GraphicsContext _graphics;
        internal List<PssVertexBuffer> _availableVertexBuffers = new List<PssVertexBuffer>();
        internal List<PssVertexBuffer> _usedVertexBuffers = new List<PssVertexBuffer>();
        internal GraphicsContext Context {
            get
            {
                return _graphics;
            }
            set
            {
                _graphics = value;
            }
        }
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
		public event EventHandler<ResourceCreatedEventArgs> ResourceCreated;
		public event EventHandler<ResourceDestroyedEventArgs> ResourceDestroyed;
        public event EventHandler<EventArgs> Disposing;


#if OPENGL
        internal int glFramebuffer;
        internal int glRenderTargetFrameBuffer;
        internal int MaxVertexAttributes;        
        internal List<string> _extensions = new List<string>();
        internal int _maxTextureSize = 0;
#endif
        
        internal int MaxTextureSlots;

#if WINDOWS_STOREAPP

        internal float Dpi
        {
            get { return _dpi; }
            set
            {
                if (_dpi == value)
                    return;

                _dpi = value;
                _d2dContext.DotsPerInch = new Size2F(_dpi, _dpi);

                //if (OnDpiChanged != null)
                    //OnDpiChanged(this);
            }
        }

#endif


#if OPENGL

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
	
	/// <summary>
        /// Returns a handle to internal device object. Valid only on DirectX platforms.
        /// For usage, convert this to SharpDX.Direct3D11.Device.
        /// </summary>
        public object Handle
        {
            get
            {
#if DIRECTX
                return _d3dDevice;
#else
                return null;
#endif
            }
        }
	
        internal bool IsRenderTargetBound
        {
            get
            {
                return _currentRenderTargetCount > 0;
            }
        }

        public GraphicsAdapter Adapter
        {
            get;
            private set;
        }

        internal GraphicsDevice(GraphicsDeviceInformation gdi)
        {
            SetupGL();
            if (gdi.PresentationParameters == null)
                throw new ArgumentNullException("presentationParameters");
            PresentationParameters = gdi.PresentationParameters;
            GraphicsProfile = gdi.GraphicsProfile;
            Initialize();
        }

        internal GraphicsDevice ()
		{
            SetupGL();
            PresentationParameters = new PresentationParameters();
            PresentationParameters.DepthStencilFormat = DepthFormat.Depth24;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <param name="presentationParameters">The presentation options.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="presentationParameters"/> is <see langword="null"/>.
        /// </exception>
        public GraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, PresentationParameters presentationParameters)
        {
            Adapter = adapter;
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");
            SetupGL();
            PresentationParameters = presentationParameters;
            GraphicsProfile = graphicsProfile;
            Initialize();
        }

        private void SetupGL() 
        {
			// Initialize the main viewport
			_viewport = new Viewport (0, 0,
			                         DisplayMode.Width, DisplayMode.Height);
			_viewport.MaxDepth = 1.0f;
   
#if PSM
            MaxTextureSlots = 8;
#else
            MaxTextureSlots = 16;
#endif
            
#if OPENGL
#if GLES
            GL.GetInteger(All.MaxTextureImageUnits, ref MaxTextureSlots);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(All.MaxVertexAttribs, ref MaxVertexAttributes);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(All.MaxTextureSize, ref _maxTextureSize);
            GraphicsExtensions.CheckGLError();
#elif OPENGL
            GL.GetInteger(GetPName.MaxTextureImageUnits, out MaxTextureSlots);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
            GraphicsExtensions.CheckGLError();
            
            GL.GetInteger(GetPName.MaxTextureSize, out _maxTextureSize);
            GraphicsExtensions.CheckGLError();

			// Initialize draw buffer attachment array
			int maxDrawBuffers;
			GL.GetInteger(GetPName.MaxDrawBuffers, out maxDrawBuffers);
			_drawBuffers = new DrawBuffersEnum[maxDrawBuffers];
			for (int i = 0; i < maxDrawBuffers; i++)
				_drawBuffers[i] = (DrawBuffersEnum)(FramebufferAttachment.ColorAttachment0Ext + i);
#endif
            _extensions = GetGLExtensions();
#endif // OPENGL

            Textures = new TextureCollection (MaxTextureSlots);
			SamplerStates = new SamplerStateCollection (MaxTextureSlots);

        }

        ~GraphicsDevice()
        {
            Dispose(false);
        }

#if OPENGL
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
#endif // OPENGL

        internal void Initialize()
        {
            GraphicsCapabilities.Initialize(this);

#if DIRECTX

#if WINDOWS_PHONE

            UpdateDevice(DrawingSurfaceState.Device, DrawingSurfaceState.Context);
            UpdateTarget(DrawingSurfaceState.RenderTargetView);

            DrawingSurfaceState.Device = null;
            DrawingSurfaceState.Context = null;
            DrawingSurfaceState.RenderTargetView = null;
			
#elif WINDOWS_STOREAPP

            CreateDeviceIndependentResources();
            CreateDeviceResources();
            Dpi = DisplayProperties.LogicalDpi;
            CreateSizeDependentResources();
			
#elif WINDOWS

            CreateDeviceResources();
            CreateSizeDependentResources();
#endif

#elif PSM
            _graphics = new GraphicsContext();
#elif OPENGL
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
#endif

            // Force set the default render states.
            _blendStateDirty = _depthStencilStateDirty = _rasterizerStateDirty = true;
            BlendState = BlendState.Opaque;
            DepthStencilState = DepthStencilState.Default;
            RasterizerState = RasterizerState.CullCounterClockwise;

            // Clear the texture and sampler collections forcing
            // the state to be reapplied.
            Textures.Clear();
            SamplerStates.Clear();

            ClearLayouts();

            // Clear constant buffers
            _vertexConstantBuffers.Clear();
            _pixelConstantBuffers.Clear();

#if OPENGL
            // Ensure the vertex attributes are reset
            _enabledVertexAttributes.Clear();
#endif

            // Force set the buffers and shaders on next ApplyState() call
            _indexBufferDirty = true;
            _vertexBufferDirty = true;
            _vertexShaderDirty = true;
            _pixelShaderDirty = true;

            // Set the default scissor rect.
            _scissorRectangleDirty = true;
            ScissorRectangle = _viewport.Bounds;

            // Set the default render target.
            ApplyRenderTargets(null);

#if OPENGL
            // Free all the cached shader programs. 
            _programCache.Clear();
            _shaderProgram = -1;
#endif
        }

#if DIRECTX 

#if WINDOWS_PHONE

        private void UpdateDevice(Device device, DeviceContext context)
        {
            // TODO: Lost device logic!
            SharpDX.Utilities.Dispose(ref _d3dDevice);
            _d3dDevice = device;

            SharpDX.Utilities.Dispose(ref _d3dContext);
            _d3dContext = context;

            SharpDX.Utilities.Dispose(ref _depthStencilView);
        }

        internal void UpdateTarget(RenderTargetView renderTargetView)
        {
            _renderTargetView = renderTargetView;
            _currentRenderTargets[0] = _renderTargetView;
            _currentDepthStencilView = _depthStencilView;
			
            var resource = _renderTargetView.Resource;
            using (var texture2D = new SharpDX.Direct3D11.Texture2D(resource.NativePointer))
            {
                var currentWidth = PresentationParameters.BackBufferWidth;
                var currentHeight = PresentationParameters.BackBufferHeight;

                if (_depthStencilView == null || (currentWidth != texture2D.Description.Width &&
                    currentHeight != texture2D.Description.Height))
                {
                    PresentationParameters.BackBufferWidth = texture2D.Description.Width;
                    PresentationParameters.BackBufferHeight = texture2D.Description.Height;

					SharpDX.Utilities.Dispose(ref _depthStencilView);
                    using (var depthTexture = new SharpDX.Direct3D11.Texture2D(
                        _d3dDevice,
                        new Texture2DDescription()
                        {
                            Width = PresentationParameters.BackBufferWidth,
                            Height = PresentationParameters.BackBufferHeight,
                            ArraySize = 1,
                            BindFlags = BindFlags.DepthStencil,
                            CpuAccessFlags = CpuAccessFlags.None,
                            Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
                            MipLevels = 1,
                            OptionFlags = ResourceOptionFlags.None,
                            SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                            Usage = ResourceUsage.Default
                        }))
                        _depthStencilView = new DepthStencilView(_d3dDevice, depthTexture);

                    Viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
                }
            }
        }

#elif WINDOWS_STOREAPP

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

            // Windows requires BGRA support out of DX.
            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport;
#if DEBUG
            creationFlags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
#endif

            // Pass the preferred feature levels based on the
            // target profile that may have been set by the user.
            var featureLevels = new List<FeatureLevel>();
            if (GraphicsProfile == GraphicsProfile.HiDef)
            {
                featureLevels.Add(FeatureLevel.Level_11_1);
                featureLevels.Add(FeatureLevel.Level_11_0);
                featureLevels.Add(FeatureLevel.Level_10_1);
                featureLevels.Add(FeatureLevel.Level_10_0);
            }
            featureLevels.Add(FeatureLevel.Level_9_3);
            featureLevels.Add(FeatureLevel.Level_9_2);
            featureLevels.Add(FeatureLevel.Level_9_1);

#if DEBUG
            try 
            {
#endif
                // Create the Direct3D device.
                using (var defaultDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags, featureLevels.ToArray()))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
#if DEBUG
            }
            catch(SharpDXException)
            {
                // Try again without the debug flag.  This allows debug builds to run
                // on machines that don't have the debug runtime installed.
                creationFlags &= ~SharpDX.Direct3D11.DeviceCreationFlags.Debug;
                using (var defaultDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags, featureLevels.ToArray()))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
            }
#endif

            // Set the correct profile based on the feature level.
            _featureLevel = _d3dDevice.FeatureLevel;
            GraphicsProfile = _featureLevel <= FeatureLevel.Level_9_3 ? GraphicsProfile.Reach : GraphicsProfile.HiDef;

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
            _d3dContext.OutputMerger.SetTargets((SharpDX.Direct3D11.DepthStencilView)null, 
                                                (SharpDX.Direct3D11.RenderTargetView)null);  

            _d2dContext.Target = null;
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
                _bitmapTarget = null;
            }

			// Clear the current render targets.
            _currentDepthStencilView = null;
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            Array.Clear(_currentRenderTargetBindings, 0, _currentRenderTargetBindings.Length);
            _currentRenderTargetCount = 0;

			// Make sure all pending rendering commands are flushed.
            _d3dContext.Flush();

            // We need presentation parameters to continue here.
            if (    PresentationParameters == null ||
                    (PresentationParameters.DeviceWindowHandle == IntPtr.Zero && PresentationParameters.SwapChainPanel == null))
            {
                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }

                return;
            }

            // Did we change swap panels?
            if (PresentationParameters.SwapChainPanel != _swapChainPanel)
            {
                _swapChainPanel = null;
                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }                
            }

            var multisampleDesc = new SharpDX.DXGI.SampleDescription(1, 0);
            if ( PresentationParameters.MultiSampleCount > 1 )
            {
                multisampleDesc.Count = PresentationParameters.MultiSampleCount;
                multisampleDesc.Quality = (int)SharpDX.Direct3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            // Use BGRA for the swap chain.
            var format = PresentationParameters.BackBufferFormat == SurfaceFormat.Color ? 
                            SharpDX.DXGI.Format.B8G8R8A8_UNorm : 
                            SharpDXHelper.ToFormat(PresentationParameters.BackBufferFormat);

            // If the swap chain already exists... update it.
            if (_swapChain != null)
            {
                _swapChain.ResizeBuffers(   2,
                                            PresentationParameters.BackBufferWidth,
                                            PresentationParameters.BackBufferHeight,
                                            format, 
                                            SwapChainFlags.None);
            }

            // Otherwise, create a new swap chain.
            else
            {
                // SwapChain description
                var desc = new SharpDX.DXGI.SwapChainDescription1()
                {
                    // Automatic sizing
                    Width = PresentationParameters.BackBufferWidth,
                    Height = PresentationParameters.BackBufferHeight,
                    Format = format,
                    Stereo = false,
                    SampleDescription = multisampleDesc,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = SharpDXHelper.ToSwapEffect(PresentationParameters.PresentationInterval),

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
                    if (PresentationParameters.DeviceWindowHandle != IntPtr.Zero)
                    {
                        // Creates a SwapChain from a CoreWindow pointer.
                        var coreWindow = Marshal.GetObjectForIUnknown(PresentationParameters.DeviceWindowHandle) as CoreWindow;
                        using (var comWindow = new ComObject(coreWindow))
                            _swapChain = dxgiFactory2.CreateSwapChainForCoreWindow(_d3dDevice, comWindow, ref desc, null);
                    }
                    else
                    {
                        _swapChainPanel = PresentationParameters.SwapChainPanel;

                        using (var nativePanel = ComObject.As<SharpDX.DXGI.ISwapChainBackgroundPanelNative>(PresentationParameters.SwapChainPanel))
                        {
                            _swapChain = dxgiFactory2.CreateSwapChainForComposition(_d3dDevice, ref desc, null);
                            nativePanel.SwapChain = _swapChain;
                        }
                    }

                    // Ensure that DXGI does not queue more than one frame at a time. This both reduces 
                    // latency and ensures that the application will only render after each VSync, minimizing 
                    // power consumption.
                    dxgiDevice2.MaximumFrameLatency = 1;
                }
            }

            _swapChain.Rotation = SharpDX.DXGI.DisplayModeRotation.Identity;

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
                var depthFormat = SharpDXHelper.ToFormat(PresentationParameters.DepthStencilFormat);

                // Allocate a 2-D surface as the depth/stencil buffer.
                using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(_d3dDevice, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Format = depthFormat,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = targetSize.X,
                    Height = targetSize.Y,
                    SampleDescription = multisampleDesc,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
                }))

                // Create a DepthStencil view on this surface to use on bind.
                _depthStencilView = new SharpDX.Direct3D11.DepthStencilView(_d3dDevice, depthBuffer);
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

#elif WINDOWS

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

            // Windows requires BGRA support out of DX.
            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport;
#if DEBUG
            creationFlags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
#endif

            // Pass the preferred feature levels based on the
            // target profile that may have been set by the user.
            var featureLevels = new List<FeatureLevel>();
            if (GraphicsProfile == GraphicsProfile.HiDef)
            {
                featureLevels.Add(FeatureLevel.Level_11_0);
                featureLevels.Add(FeatureLevel.Level_10_1);
                featureLevels.Add(FeatureLevel.Level_10_0);
            }
            featureLevels.Add(FeatureLevel.Level_9_3);
            featureLevels.Add(FeatureLevel.Level_9_2);
            featureLevels.Add(FeatureLevel.Level_9_1);

#if DEBUG
            try 
            {
#endif
                // Create the Direct3D device.
                using (var defaultDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags, featureLevels.ToArray()))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device>();
#if DEBUG
            }
            catch(SharpDXException)
            {
                // Try again without the debug flag.  This allows debug builds to run
                // on machines that don't have the debug runtime installed.
                creationFlags &= ~SharpDX.Direct3D11.DeviceCreationFlags.Debug;
                using (var defaultDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags, featureLevels.ToArray()))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device>();
            }
#endif

            // Set the correct profile based on the feature level.
            _featureLevel = _d3dDevice.FeatureLevel;
            GraphicsProfile = _featureLevel <= FeatureLevel.Level_9_3 ? GraphicsProfile.Reach : GraphicsProfile.HiDef;

            // Get Direct3D 11.1 context
            _d3dContext = _d3dDevice.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext>();
        }

        internal void CreateSizeDependentResources()
        {
            _d3dContext.OutputMerger.SetTargets((SharpDX.Direct3D11.DepthStencilView)null,
                                                (SharpDX.Direct3D11.RenderTargetView)null);

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

            // Clear the current render targets.
            _currentDepthStencilView = null;
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            Array.Clear(_currentRenderTargetBindings, 0, _currentRenderTargetBindings.Length);
            _currentRenderTargetCount = 0;

            // Make sure all pending rendering commands are flushed.
            _d3dContext.Flush();

            // We need presentation parameters to continue here.
            if (PresentationParameters == null || PresentationParameters.DeviceWindowHandle == IntPtr.Zero)
            {
                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }

                return;
            }

            var multisampleDesc = new SharpDX.DXGI.SampleDescription(1, 0);
            if (PresentationParameters.MultiSampleCount > 1)
            {
                multisampleDesc.Count = PresentationParameters.MultiSampleCount;
                multisampleDesc.Quality = (int)SharpDX.Direct3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            // Use BGRA for the swap chain.
            var format = PresentationParameters.BackBufferFormat == SurfaceFormat.Color ?
                            SharpDX.DXGI.Format.B8G8R8A8_UNorm :
                            SharpDXHelper.ToFormat(PresentationParameters.BackBufferFormat);

            int vSyncFrameLatency = PresentationParameters.PresentationInterval.GetFrameLatency();

            // If the swap chain already exists... update it.
            if (_swapChain != null)
            {
                _swapChain.ResizeBuffers(2,
                                            PresentationParameters.BackBufferWidth,
                                            PresentationParameters.BackBufferHeight,
                                            format,
                                            SwapChainFlags.None);

                // Update Vsync setting.
                using (var dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device1>())
                {
                    // If VSync is disabled, Ensure that DXGI does not queue more than one frame at a time. This 
                    // both reduces latency and ensures that the application will only render 
                    // after each VSync, minimizing power consumption.
                    // Setting latency to 0 (PresentInterval.Immediate) will result in the hardware default.
                    // (normally 3) 
                    dxgiDevice.MaximumFrameLatency = vSyncFrameLatency;
                }
            }

            // Otherwise, create a new swap chain.
            else
            {
                // SwapChain description
                var desc = new SharpDX.DXGI.SwapChainDescription()
                {
                    ModeDescription =
                    {
                        Format = format,
                        Scaling = DisplayModeScaling.Stretched,
                        Width = PresentationParameters.BackBufferWidth,
                        Height = PresentationParameters.BackBufferHeight,                        
                    },

                    OutputHandle = PresentationParameters.DeviceWindowHandle,
                    SampleDescription = multisampleDesc,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = SharpDXHelper.ToSwapEffect(PresentationParameters.PresentationInterval),
                    IsWindowed = true,
                };

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device
                
                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (var dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device1>())
                using (var dxgiAdapter = dxgiDevice.Adapter)
                using (var dxgiFactory = dxgiAdapter.GetParent<SharpDX.DXGI.Factory1>())
                {
                    _swapChain = new SwapChain(dxgiFactory, dxgiDevice, desc);

                    // If VSync is disabled, Ensure that DXGI does not queue more than one frame at a time. This 
                    // both reduces latency and ensures that the application will only render 
                    // after each VSync, minimizing power consumption.
                    // Setting latency to 0 (PresentInterval.Immediate) will result in the hardware default.
                    // (normally 3) 
                    dxgiDevice.MaximumFrameLatency = vSyncFrameLatency;
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
                var depthFormat = SharpDXHelper.ToFormat(PresentationParameters.DepthStencilFormat);

                // Allocate a 2-D surface as the depth/stencil buffer.
                using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(_d3dDevice, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Format = depthFormat,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = targetSize.X,
                    Height = targetSize.Y,
                    SampleDescription = multisampleDesc,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
                }))

                    // Create a DepthStencil view on this surface to use on bind.
                    _depthStencilView = new SharpDX.Direct3D11.DepthStencilView(_d3dDevice, depthBuffer);
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
        }
		
#endif // WINDOWS

#endif // DIRECTX

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

				_blendState = value;
                _blendStateDirty = true;
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
            }
        }

        public void Clear(Color color)
        {
			var options = ClearOptions.Target;

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

            Clear(options, color.ToVector4(), _viewport.MaxDepth, 0);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Clear (options, color.ToVector4 (), depth, stencil);
        }

		public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
		{
#if DIRECTX
            lock (_d3dContext)
            {
                // Clear the diffuse render buffer.
                if ((options & ClearOptions.Target) == ClearOptions.Target)
                {
                    foreach (var view in _currentRenderTargets)
                    {
                        if (view != null)
                            _d3dContext.ClearRenderTargetView(view, new Color4(color.X, color.Y, color.Z, color.W));
                    }
                }

                // Clear the depth/stencil render buffer.
                SharpDX.Direct3D11.DepthStencilClearFlags flags = 0;
                if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer)
                    flags |= SharpDX.Direct3D11.DepthStencilClearFlags.Depth;
                if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
                    flags |= SharpDX.Direct3D11.DepthStencilClearFlags.Stencil;

                if (flags != 0 && _currentDepthStencilView != null)
                    _d3dContext.ClearDepthStencilView(_currentDepthStencilView, flags, depth, (byte)stencil);
            }

#elif PSM

            _graphics.SetClearColor(color.ToPssVector4());
            _graphics.Clear();
#elif OPENGL

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
            DepthStencilState = DepthStencilState.Default;
		    BlendState = BlendState.Opaque;
            ApplyState(false);

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

#if GLES
			GL.Clear((uint)bufferMask);
            GraphicsExtensions.CheckGLError();
#else
			GL.Clear(bufferMask);
#endif
           		
            // Restore the previous render state.
		    ScissorRectangle = prevScissorRect;
		    DepthStencilState = prevDepthStencilState;
		    BlendState = prevBlendState;

#endif // OPENGL
        }
		
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private void ClearLayouts()
        {
#if DIRECTX
            foreach (var layout in _inputLayouts)
                layout.Value.Dispose();
            _inputLayouts.Clear();
#endif
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose of all remaining graphics resources before disposing of the graphics device
                    GraphicsResource.DisposeAll();

#if DIRECTX

                    ClearLayouts();
                    SharpDX.Utilities.Dispose(ref _renderTargetView);
                    SharpDX.Utilities.Dispose(ref _depthStencilView);
                    SharpDX.Utilities.Dispose(ref _d3dDevice);
                    SharpDX.Utilities.Dispose(ref _d3dContext);

#if WINDOWS_STOREAPP

                    if (_swapChain != null)
                    {
                        _swapChain.Dispose();
                        _swapChain = null;
                    }
                    if (_bitmapTarget != null)
                    {
                        _bitmapTarget.Dispose();
                        _depthStencilView = null;
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

#endif // WINDOWS_STOREAPP

#endif // DIRECTX

#if OPENGL
                    // Free all the cached shader programs.
                    _programCache.Dispose();

                    GraphicsDevice.AddDisposeAction(() =>
                                                    {
                        if (this.glRenderTargetFrameBuffer > 0)
                        {
                            GL.DeleteFramebuffers(1, ref this.glRenderTargetFrameBuffer);
                            GraphicsExtensions.CheckGLError();
                        }
                    });
#endif

#if PSM
                    if (_graphics != null)
                    {
                        _graphics.Dispose();
                        _graphics = null;
                    }
#endif
                }

                _isDisposed = true;
            }
        }

#if OPENGL
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
#endif

        public void Present()
        {
#if WINDOWS_STOREAPP
            // The application may optionally specify "dirty" or "scroll" rects to improve efficiency
            // in certain scenarios.  In this sample, however, we do not utilize those features.
            var parameters = new SharpDX.DXGI.PresentParameters();
            
            try
            {
                // TODO: Hook in PresentationParameters here!

                // The first argument instructs DXGI to block until VSync, putting the application
                // to sleep until the next VSync. This ensures we don't waste any cycles rendering
                // frames that will never be displayed to the screen.
                lock (_d3dContext)
                    _swapChain.Present(1, PresentFlags.None, parameters);
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

#elif DIRECTX && !WINDOWS_PHONE

            try
            {
                var syncInterval = PresentationParameters.PresentationInterval.GetFrameLatency();

                // The first argument instructs DXGI to block n VSyncs before presenting.
                lock (_d3dContext)
                    _swapChain.Present(syncInterval, PresentFlags.None);
            }
            catch (SharpDX.SharpDXException)
            {
                // TODO: How should we deal with a device lost case here?
            }
	
#elif PSM
            _graphics.SwapBuffers();
            _availableVertexBuffers.AddRange(_usedVertexBuffers);
            _usedVertexBuffers.Clear();
#elif OPENGL
			GL.Flush();
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
#endif
        }

        /*
        public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            // Manually resetting the device is not currently supported.
            throw new NotImplementedException();
        }

        public void Reset(PresentationParameters presentationParameters)
        {
            throw new NotImplementedException();
        }

        public void Reset(PresentationParameters presentationParameters, GraphicsAdapter graphicsAdapter)
        {
            throw new NotImplementedException();
        }
        */

        /// <summary>
        /// Trigger the DeviceResetting event
        /// Currently internal to allow the various platforms to send the event at the appropriate time.
        /// </summary>
        internal void OnDeviceResetting()
        {
            if (DeviceResetting != null)
                DeviceResetting(this, EventArgs.Empty);

            GraphicsResource.DoGraphicsDeviceResetting();
        }

        /// <summary>
        /// Trigger the DeviceReset event to allow games to be notified of a device reset.
        /// Currently internal to allow the various platforms to send the event at the appropriate time.
        /// </summary>
        internal void OnDeviceReset()
        {
            if (DeviceReset != null)
                DeviceReset(this, EventArgs.Empty);
        }

        public DisplayMode DisplayMode
        {
            get
            {
                return Adapter.CurrentDisplayMode;
            }
        }

        public GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get
            {
                return GraphicsDeviceStatus.Normal;
            }
        }

        public PresentationParameters PresentationParameters
        {
            get;
            private set;
        }

        public Viewport Viewport
        {
            get
            {
                return _viewport;
            }

            set
            {
                _viewport = value;
#if DIRECTX
                if (_d3dContext != null)
                {
                    var viewport = new SharpDX.ViewportF(_viewport.X, _viewport.Y, (float)_viewport.Width, (float)_viewport.Height, _viewport.MinDepth, _viewport.MaxDepth);
                    lock (_d3dContext)
                        _d3dContext.Rasterizer.SetViewport(viewport);
                }
#elif OPENGL
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
#elif PSM
                _graphics.SetViewport(
                    value.X, value.Y, value.Width, value.Height
                );
#endif
            }
        }

        public GraphicsProfile GraphicsProfile { get; set; }

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
            }
        }

		public void SetRenderTarget(RenderTarget2D renderTarget)
		{
			if (renderTarget == null)
                SetRenderTargets(null);
			else
				SetRenderTargets(new RenderTargetBinding(renderTarget));
		}
		
        public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
        {
            if (renderTarget == null)
                SetRenderTarget(null);
            else
                SetRenderTargets(new RenderTargetBinding(renderTarget, cubeMapFace));
        }

#if DIRECTX

        public void SetRenderTarget(RenderTarget3D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
                SetRenderTarget(null);
            else
                SetRenderTargets(new RenderTargetBinding(renderTarget, arraySlice));
        }

#endif

		public void SetRenderTargets(params RenderTargetBinding[] renderTargets) 
		{
            // Avoid having to check for null and zero length.
            var renderTargetCount = 0;
            if (renderTargets != null)
            {
                renderTargetCount = renderTargets.Length;
                if (renderTargetCount == 0)
                    renderTargets = null;
            }

            // Try to early out if the current and new bindings are equal.
            if (_currentRenderTargetCount == renderTargetCount)
            {
                var isEqual = true;
                for (var i = 0; i < _currentRenderTargetCount; i++)
                {
                    if (_currentRenderTargetBindings[i].RenderTarget != renderTargets[i].RenderTarget ||
                        _currentRenderTargetBindings[i].ArraySlice != renderTargets[i].ArraySlice)
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                    return;
            }

            ApplyRenderTargets(renderTargets);
        }

        internal void ApplyRenderTargets(RenderTargetBinding[] renderTargets)
        {
            var clearTarget = false;

            // Clear the current bindings.
            Array.Clear(_currentRenderTargetBindings, 0, _currentRenderTargetBindings.Length);

            int renderTargetWidth;
            int renderTargetHeight;
            if (renderTargets == null)
            {
                _currentRenderTargetCount = 0;

#if DIRECTX
                // Set the default swap chain.
                Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
                _currentRenderTargets[0] = _renderTargetView;
                _currentDepthStencilView = _depthStencilView;

                lock (_d3dContext) 
                    _d3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);                
#elif OPENGL
				GL.BindFramebuffer(GLFramebuffer, this.glFramebuffer);
                GraphicsExtensions.CheckGLError();
#elif PSM
                _graphics.SetFrameBuffer(_graphics.Screen);
#endif
                clearTarget = PresentationParameters.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = PresentationParameters.BackBufferWidth;
                renderTargetHeight = PresentationParameters.BackBufferHeight;
            }
			else
			{
                // Copy the new bindings.
                Array.Copy(renderTargets, _currentRenderTargetBindings, renderTargets.Length);
                _currentRenderTargetCount = renderTargets.Length;

#if DIRECTX
                // Clear the current render targets.
                Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
                _currentDepthStencilView = null;

                // Make sure none of the new targets are bound
                // to the device as a texture resource.
                lock (_d3dContext)
                    Textures.ClearTargets(this, _currentRenderTargetBindings);

                for (var i = 0; i < _currentRenderTargetCount; i++)
                {
                    var binding = _currentRenderTargetBindings[i];
                    var target = (IRenderTarget)binding.RenderTarget;
                    _currentRenderTargets[i] = target.GetRenderTargetView(binding.ArraySlice);
                }

                // Use the depth from the first target.
                var renderTarget = (IRenderTarget)_currentRenderTargetBindings[0].RenderTarget;
                _currentDepthStencilView = renderTarget.GetDepthStencilView();

                // Set the targets.
                lock (_d3dContext)
                    _d3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
#elif OPENGL
                if (_currentRenderTargetBindings[0].RenderTarget is RenderTargetCube)
                    throw new NotImplementedException("RenderTargetCube not yet implemented.");

                var renderTarget = _currentRenderTargetBindings[0].RenderTarget as RenderTarget2D;
				if (this.glRenderTargetFrameBuffer == 0)
				{
#if GLES
                    GL.GenFramebuffers(1, ref this.glRenderTargetFrameBuffer);
#else
                    GL.GenFramebuffers(1, out this.glRenderTargetFrameBuffer);
#endif
                    GraphicsExtensions.CheckGLError();
                }

                GL.BindFramebuffer(GLFramebuffer, this.glRenderTargetFrameBuffer);
                GraphicsExtensions.CheckGLError();
                GL.FramebufferTexture2D(GLFramebuffer, GLColorAttachment0, TextureTarget.Texture2D, renderTarget.glTexture, 0);
                GraphicsExtensions.CheckGLError();

				// Reverted this change, as per @prollin's suggestion
				GL.FramebufferRenderbuffer(GLFramebuffer, GLDepthAttachment, GLRenderbuffer, renderTarget.glDepthBuffer);
				GL.FramebufferRenderbuffer(GLFramebuffer, GLStencilAttachment, GLRenderbuffer, renderTarget.glStencilBuffer);

#if !GLES
				for (var i = 0; i < _currentRenderTargetCount; i++)
				{
					GL.BindTexture(TextureTarget.Texture2D, _currentRenderTargetBindings[i].RenderTarget.glTexture);
					GraphicsExtensions.CheckGLError();
					GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext + i, TextureTarget.Texture2D, _currentRenderTargetBindings[i].RenderTarget.glTexture, 0);
					GraphicsExtensions.CheckGLError();
				}

				GL.DrawBuffers(_currentRenderTargetCount, _drawBuffers);
				GraphicsExtensions.CheckGLError();
#endif

                // Test that the FBOs are attached and correct.
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
#elif PSM
                var renderTarget = (RenderTarget2D)_currentRenderTargetBindings[0].RenderTarget;
                _graphics.SetFrameBuffer(renderTarget._frameBuffer);
#endif

                // We clear the render target if asked.
                clearTarget = renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = renderTarget.Width;
                renderTargetHeight = renderTarget.Height;
            }

            // Set the viewport to the size of the first render target.
            Viewport = new Viewport(0, 0, renderTargetWidth, renderTargetHeight);

            // Set the scissor rectangle to the size of the first render target.
            ScissorRectangle = new Rectangle(0, 0, renderTargetWidth, renderTargetHeight);

            // In XNA 4, because of hardware limitations on Xbox, when
            // a render target doesn't have PreserveContents as its usage
            // it is cleared before being rendered to.
            if (clearTarget)
                Clear(DiscardColor);
            
#if OPENGL
			// Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.Dirty();
#endif
        }

#if WINRT
        internal void ResetRenderTargets()
        {
            if (_d3dContext != null)
            {
                lock (_d3dContext)
                {
                    var viewport = new SharpDX.ViewportF( _viewport.X, _viewport.Y, 
                                                          _viewport.Width, _viewport.Height, 
                                                          _viewport.MinDepth, _viewport.MaxDepth);
                    _d3dContext.Rasterizer.SetViewport(viewport);
                    _d3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
                }
            }

            Textures.Dirty();
            SamplerStates.Dirty();
            _depthStencilStateDirty = true;
            _blendStateDirty = true;
            _indexBufferDirty = true;
            _vertexBufferDirty = true;
            _pixelShaderDirty = true;
            _vertexShaderDirty = true;
            _rasterizerStateDirty = true;
            _scissorRectangleDirty = true;            
        }
#endif

		public RenderTargetBinding[] GetRenderTargets()
		{
            // Return a correctly sized copy our internal array.
            var bindings = new RenderTargetBinding[_currentRenderTargetCount];
            Array.Copy(_currentRenderTargetBindings, bindings, _currentRenderTargetCount);
            return bindings;
		}

#if OPENGL

        private static BeginMode PrimitiveTypeGL(PrimitiveType primitiveType)
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

            throw new ArgumentException();
        }
		
#elif DIRECTX

        private static PrimitiveTopology ToPrimitiveTopology(PrimitiveType primitiveType)
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

            throw new ArgumentException();
        }
		
#endif


        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            if (_vertexBuffer == vertexBuffer)
                return;

            _vertexBuffer = vertexBuffer;
            _vertexBufferDirty = true;
        }

        private void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            if (_indexBuffer == indexBuffer)
                return;
            
            _indexBuffer = indexBuffer;
            _indexBufferDirty = true;
        }

        public IndexBuffer Indices { set { SetIndexBuffer(value); } get { return _indexBuffer; } }

        internal Shader VertexShader
        {
            get { return _vertexShader; }

            set
            {
                if (_vertexShader == value)
                    return;

                _vertexShader = value;
                _vertexShaderDirty = true;
            }
        }

        internal Shader PixelShader
        {
            get { return _pixelShader; }

            set
            {
                if (_pixelShader == value)
                    return;

                _pixelShader = value;
                _pixelShaderDirty = true;
            }
        }

        internal void SetConstantBuffer(ShaderStage stage, int slot, ConstantBuffer buffer)
        {
            if (stage == ShaderStage.Vertex)
                _vertexConstantBuffers[slot] = buffer;
            else
                _pixelConstantBuffers[slot] = buffer;
        }

#if OPENGL

        /// <summary>
        /// Activates the Current Vertex/Pixel shader pair into a program.         
        /// </summary>
        private void ActivateShaderProgram()
        {
            // Lookup the shader program.
            var info = _programCache.GetProgramInfo(VertexShader, PixelShader);
            if (info.program == -1)
                return;
            // Set the new program if it has changed.
            if (_shaderProgram != info.program)
            {
                GL.UseProgram(info.program);
                GraphicsExtensions.CheckGLError();
                _shaderProgram = info.program;
            }

            if (info.posFixupLoc == -1)
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

            GL.Uniform4(info.posFixupLoc, 1, _posFixup);
            GraphicsExtensions.CheckGLError();
        }
#endif

        public bool ResourcesLost { get; set; }

        internal void ApplyState(bool applyShaders)
        {
#if DIRECTX
            // NOTE: This code assumes _d3dContext has been locked by the caller.
            Debug.Assert(_d3dContext != null, "The d3d context is null!");
#endif
            if ( _scissorRectangleDirty )
	        {
#if DIRECTX
	            _d3dContext.Rasterizer.SetScissorRectangle(
                    _scissorRectangle.X, 
                    _scissorRectangle.Y, 
                    _scissorRectangle.Right, 
                    _scissorRectangle.Bottom);
#elif OPENGL
                var scissorRect = _scissorRectangle;
                if (!IsRenderTargetBound)
                    scissorRect.Y = _viewport.Height - scissorRect.Y - scissorRect.Height;
                GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
                GraphicsExtensions.CheckGLError();
#endif
	            _scissorRectangleDirty = false;
	        }

            if (_blendStateDirty)
            {
                _blendState.ApplyState(this);
                _blendStateDirty = false;
            }
	        if ( _depthStencilStateDirty )
            {
	            _depthStencilState.ApplyState(this);
                _depthStencilStateDirty = false;
            }
	        if ( _rasterizerStateDirty )
            {
	            _rasterizerState.ApplyState(this);
	            _rasterizerStateDirty = false;
            }

            // If we're not applying shaders then early out now.
            if (!applyShaders)
                return;

            if (_indexBufferDirty)
            {
                if (_indexBuffer != null)
                {
#if DIRECTX
                    _d3dContext.InputAssembler.SetIndexBuffer(
                        _indexBuffer.Buffer,
                        _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 
                            SharpDX.DXGI.Format.R16_UInt : SharpDX.DXGI.Format.R32_UInt,
                        0);
#elif OPENGL

                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer.ibo);
                    GraphicsExtensions.CheckGLError();
#endif
                }
                _indexBufferDirty = false;
            }

            if (_vertexBufferDirty)
            {
#if DIRECTX
                if (_vertexBuffer != null)
                    _d3dContext.InputAssembler.SetVertexBuffers(0, _vertexBuffer.Binding);
                else
                    _d3dContext.InputAssembler.SetVertexBuffers(0);
#elif OPENGL
                if (_vertexBuffer != null)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.vbo);
                    GraphicsExtensions.CheckGLError();
                }
#endif
            }

            if (_vertexShader == null)
                throw new InvalidOperationException("A vertex shader must be set!");
            if (_pixelShader == null)
                throw new InvalidOperationException("A pixel shader must not set!");

#if DIRECTX 

            if (_vertexShaderDirty)
                _d3dContext.VertexShader.Set(_vertexShader.VertexShader);

            if (_vertexShaderDirty || _vertexBufferDirty)
            {
                _d3dContext.InputAssembler.InputLayout = GetInputLayout(_vertexShader, _vertexBuffer.VertexDeclaration);
                _vertexShaderDirty = _vertexBufferDirty = false;
            }

            if (_pixelShaderDirty)
            {
                _d3dContext.PixelShader.Set(_pixelShader.PixelShader);
                _pixelShaderDirty = false;
            }

#elif OPENGL

            if (_vertexShaderDirty || _pixelShaderDirty)
            {
                ActivateShaderProgram();
                _vertexShaderDirty = _pixelShaderDirty = false;
            }

#endif

#if DIRECTX
            _vertexConstantBuffers.SetConstantBuffers(this);
            _pixelConstantBuffers.SetConstantBuffers(this);
#elif OPENGL
            _vertexConstantBuffers.SetConstantBuffers(this, _shaderProgram);
            _pixelConstantBuffers.SetConstantBuffers(this, _shaderProgram);
#endif

            Textures.SetTextures(this);
            SamplerStates.SetSamplers(this);
        }

#if DIRECTX

        private SharpDX.Direct3D11.InputLayout GetInputLayout(Shader shader, VertexDeclaration decl)
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
            where T : struct
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


            if ((vertexCount + buffer.UserOffset) < buffer.VertexCount)
            {
                buffer.UserOffset += vertexCount;
                buffer.SetData(startVertex * vertexDecl.VertexStride, vertexData, vertexOffset, vertexCount, vertexDecl.VertexStride, SetDataOptions.NoOverwrite);
            }
            else
            {
                buffer.UserOffset = vertexCount;
                buffer.SetData(vertexData, vertexOffset, vertexCount, SetDataOptions.Discard);
                startVertex = 0;
            }

            SetVertexBuffer(buffer);

            return startVertex;
        }

        private int SetUserIndexBuffer<T>(T[] indexData, int indexOffset, int indexCount)
            where T : struct
        {
            DynamicIndexBuffer buffer;

            var indexType = typeof(T);
            var indexSize = Marshal.SizeOf(indexType);
            var indexElementSize = indexSize == 2 ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits;

            if (!_userIndexBuffers.TryGetValue(indexElementSize, out buffer) || buffer.IndexCount < indexCount)
            {
                if (buffer != null)
                    buffer.Dispose();

                buffer = new DynamicIndexBuffer(this, indexElementSize, Math.Max(indexCount, 6000), BufferUsage.WriteOnly);
                _userIndexBuffers[indexElementSize] = buffer;
            }

            var startIndex = buffer.UserOffset;

            if ((indexCount + buffer.UserOffset) < buffer.IndexCount)
            {
                buffer.UserOffset += indexCount;
                buffer.SetData(startIndex * indexSize, indexData, indexOffset, indexCount, SetDataOptions.NoOverwrite);
            }
            else
            {
                startIndex = 0;
                buffer.UserOffset = indexCount;
                buffer.SetData(indexData, indexOffset, indexCount, SetDataOptions.Discard);
            }

            Indices = buffer;

            return startIndex;
        }
#endif

        /// <summary>
        /// Draw geometry by indexing into the vertex buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="minVertexIndex">A hint of the lowest vertex indexed relative to baseVertex.</param>
        /// <param name="numVertices">An hint of the maximum vertex indexed.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives to render from the index buffer.</param>
        /// <remarks>Note that minVertexIndex and numVertices are unused in MonoGame and will be ignored.</remarks>
        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
        {
            Debug.Assert(_vertexBuffer != null, "The vertex buffer is null!");
            Debug.Assert(_indexBuffer != null, "The index buffer is null!");

            // NOTE: minVertexIndex and numVertices are only hints of the
            // range of vertex data which will be indexed.
            //
            // They will only be used if the graphics API can use
            // this range hint to optimize rendering.

#if DIRECTX

            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);

                var vertexCount = GetElementCountArray(primitiveType, primitiveCount);
                _d3dContext.DrawIndexed(vertexCount, startIndex, baseVertex);
            }
			
#elif OPENGL

            ApplyState(true);

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
#elif PSM
            BindVertexBuffer(true);
            _graphics.DrawArrays(PSSHelper.ToDrawMode(primitiveType), startIndex, GetElementCountArray(primitiveType, primitiveCount));
#endif
        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {
            DrawUserPrimitives(primitiveType, vertexData, vertexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {            
            Debug.Assert(vertexData != null && vertexData.Length > 0, "The vertexData must not be null or zero length!");

            var vertexCount = GetElementCountArray(primitiveType, primitiveCount);

#if DIRECTX

            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, vertexCount, vertexDeclaration);

            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.Draw(vertexCount, startVertex);
            }

#elif OPENGL

            ApplyState(true);

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
#endif
        }

        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            Debug.Assert(_vertexBuffer != null, "The vertex buffer is null!");

            var vertexCount = GetElementCountArray(primitiveType, primitiveCount);

#if DIRECTX

            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.Draw(vertexCount, vertexStart);
            }

#elif OPENGL

            ApplyState(true);

            _vertexBuffer.VertexDeclaration.Apply(_vertexShader, IntPtr.Zero);

			GL.DrawArrays(PrimitiveTypeGL(primitiveType),
			              vertexStart,
			              vertexCount);
            GraphicsExtensions.CheckGLError();
#elif PSM
            BindVertexBuffer(false);
            _graphics.DrawArrays(PSSHelper.ToDrawMode(primitiveType), vertexStart, vertexCount);
#endif
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            Debug.Assert(vertexData != null && vertexData.Length > 0, "The vertexData must not be null or zero length!");
            Debug.Assert(indexData != null && indexData.Length > 0, "The indexData must not be null or zero length!");

#if DIRECTX

            var indexCount = GetElementCountArray(primitiveType, primitiveCount);
            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            var startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);

            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.DrawIndexed(indexCount, startIndex, startVertex);
            }

#elif OPENGL

            ApplyState(true);

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
#else
            throw new NotImplementedException("Not implemented");
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
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.DrawIndexed(indexCount, startIndex, startVertex);
            }

#elif OPENGL

            ApplyState(true);

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

#else
            throw new NotImplementedException("Not implemented");
#endif
        }

#if PSM
        internal PssVertexBuffer GetVertexBuffer(VertexFormat[] vertexFormat, int requiredVertexLength, int requiredIndexLength)
        {
            int bestMatchIndex = -1;
            PssVertexBuffer bestMatch = null;
            
            //Search for a good one
            for (int i = _availableVertexBuffers.Count - 1; i >= 0; i--)
            {
                var buf = _availableVertexBuffers[i];
                
#region Check there is enough space
                if (buf.VertexCount < requiredVertexLength)
                    continue;
                if (requiredIndexLength == 0 && buf.IndexCount != 0)
                    continue;
                if (requiredIndexLength > 0 && buf.IndexCount < requiredIndexLength)
                    continue;
#endregion
                
#region Check VertexFormat is the same
                var bufFormats = buf.Formats;
                if (vertexFormat.Length != bufFormats.Length)
                    continue;
                bool allEqual = true;
                for (int j = 0; j < bufFormats.Length; j++)
                {
                    if (vertexFormat[j] != bufFormats[j])
                    {
                        allEqual = false;
                        break;
                    }
                }
                if (!allEqual)
                    continue;
#endregion
                
                //this one is acceptable
                
                //No current best or this one is smaller than the current best
                if (bestMatch == null || (buf.IndexCount + buf.VertexCount) < (bestMatch.IndexCount + bestMatch.VertexCount))
                {
                    bestMatch = buf;
                    bestMatchIndex = i;
                }
            }
            
            if (bestMatch != null)
            {
                _availableVertexBuffers.RemoveAt(bestMatchIndex);
            }
            else
            {
                //Create one
                bestMatch = new PssVertexBuffer(requiredVertexLength, requiredIndexLength, vertexFormat);
            }
            _usedVertexBuffers.Add(bestMatch);
            
            return bestMatch;
        }
        
        /// <summary>
        /// Set the current _graphics VertexBuffer based on _vertexBuffer and _indexBuffer, reusing an existing VertexBuffer if possible
        /// </summary>
        private void BindVertexBuffer(bool bindIndexBuffer)
        {
            int requiredVertexLength = _vertexBuffer.VertexCount;
            int requiredIndexLength = (!bindIndexBuffer || _indexBuffer == null) ? 0 : _indexBuffer.IndexCount;
            
            var vertexFormat = _vertexBuffer.VertexDeclaration.GetVertexFormat();
            
            var vertexBuffer = GetVertexBuffer(vertexFormat, requiredVertexLength, requiredIndexLength);
            
            vertexBuffer.SetVertices(_vertexBuffer._vertexArray);
            if (requiredIndexLength > 0)
                vertexBuffer.SetIndices(_indexBuffer._buffer);
            _graphics.SetVertexBuffer(0, vertexBuffer);
        }
#endif

        private static int GetElementCountArray(PrimitiveType primitiveType, int primitiveCount)
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

#if DIRECTX		
        /// <summary>
        /// Sends queued-up commands in the command buffer to the graphics processing unit (GPU).
        /// </summary>
        public void Flush()
        {
            _d3dContext.Flush();
        }
#endif
    }
}
