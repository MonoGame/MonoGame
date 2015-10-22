// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
#if WINDOWS_PHONE
using MonoGame.Framework.WindowsPhone;
#endif

#if WINDOWS_STOREAPP || WINDOWS_UAP
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Display;
using Windows.UI.Core;
using SharpDX.DXGI;
#endif

#if WINDOWS_UAP
using SharpDX.Mathematics.Interop;
#endif

#if WINDOWS
using SharpDX.DXGI;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        // Core Direct3D Objects
        internal SharpDX.Direct3D11.Device _d3dDevice;
        internal SharpDX.Direct3D11.DeviceContext _d3dContext;
        internal SharpDX.Direct3D11.RenderTargetView _renderTargetView;
        internal SharpDX.Direct3D11.DepthStencilView _depthStencilView;
        private int _vertexBufferSlotsUsed;

#if WINDOWS_STOREAPP || WINDOWS_UAP

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

#if WINDOWS_UAP
		SwapChainPanel _swapChainPanel;
#else
		SwapChainBackgroundPanel _swapChainBackgroundPanel;
#endif

		float _dpi; 
#endif
#if WINDOWS

        SwapChain _swapChain;

#endif

        // The active render targets.
        readonly SharpDX.Direct3D11.RenderTargetView[] _currentRenderTargets = new SharpDX.Direct3D11.RenderTargetView[4];

        // The active depth view.
        SharpDX.Direct3D11.DepthStencilView _currentDepthStencilView;

        private readonly Dictionary<VertexDeclaration, DynamicVertexBuffer> _userVertexBuffers = new Dictionary<VertexDeclaration, DynamicVertexBuffer>();

        private readonly Dictionary<IndexElementSize, DynamicIndexBuffer> _userIndexBuffers = new Dictionary<IndexElementSize, DynamicIndexBuffer>();

#if WINDOWS_STOREAPP || WINDOWS_UAP

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

	/// <summary>
        /// Returns a handle to internal device object. Valid only on DirectX platforms.
        /// For usage, convert this to SharpDX.Direct3D11.Device.
        /// </summary>
        public object Handle
        {
            get
            {
                return _d3dDevice;
            }
        }

        private void PlatformSetup()
        {
            MaxTextureSlots = 16;
            MaxVertexTextureSlots = 16;
        }

        private void PlatformInitialize()
        {
#if WINDOWS_PHONE

            UpdateDevice(DrawingSurfaceState.Device, DrawingSurfaceState.Context);
            UpdateTarget(DrawingSurfaceState.RenderTargetView);

            DrawingSurfaceState.Device = null;
            DrawingSurfaceState.Context = null;
            DrawingSurfaceState.RenderTargetView = null;
#endif
#if WINDOWS_UAP
			CreateDeviceIndependentResources();
			CreateDeviceResources();
			Dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
			CreateSizeDependentResources();
#endif
#if WINDOWS_STOREAPP

            CreateDeviceIndependentResources();
            CreateDeviceResources();
            Dpi = DisplayProperties.LogicalDpi;
            CreateSizeDependentResources();
#endif
#if WINDOWS

            CreateDeviceResources();
            CreateSizeDependentResources();
#endif

            _maxVertexBufferSlots = _d3dDevice.FeatureLevel >= FeatureLevel.Level_11_0 ? SharpDX.Direct3D11.InputAssemblerStage.VertexInputResourceSlotCount : 16;
        }

#if WINDOWS_PHONE

        private void UpdateDevice(Device device, DeviceContext context)
        {
            // TODO: Lost device logic!
            SharpDX.Utilities.Dispose(ref _d3dDevice);
            _d3dDevice = device;

            SharpDX.Utilities.Dispose(ref _d3dContext);
            _d3dContext = context;

            SharpDX.Utilities.Dispose(ref _depthStencilView);

            using (var dxgiDevice2 = device.QueryInterface<SharpDX.DXGI.Device2>())
            {
                // Ensure that DXGI does not queue more than one frame at a time. This both reduces 
                // latency and ensures that the application will only render after each VSync, minimizing 
                // power consumption.
                dxgiDevice2.MaximumFrameLatency = 1;
            }
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

                if (_depthStencilView == null || 
                    currentWidth  != texture2D.Description.Width ||
                    currentHeight != texture2D.Description.Height)
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
#endif
#if WINDOWS_STOREAPP || WINDOWS_UAP

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

            var driverType = GraphicsAdapter.UseReferenceDevice ? DriverType.Reference : DriverType.Hardware;

#if DEBUG
            try 
            {
#endif
                // Create the Direct3D device.
                using (var defaultDevice = new SharpDX.Direct3D11.Device(driverType, creationFlags, featureLevels.ToArray()))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();

                // Necessary to enable video playback
                var multithread = _d3dDevice.QueryInterface<SharpDX.Direct3D.DeviceMultithread>();
                multithread.SetMultithreadProtected(true);
#if DEBUG
            }
            catch(SharpDXException)
            {
                // Try again without the debug flag.  This allows debug builds to run
                // on machines that don't have the debug runtime installed.
                creationFlags &= ~SharpDX.Direct3D11.DeviceCreationFlags.Debug;
                using (var defaultDevice = new SharpDX.Direct3D11.Device(driverType, creationFlags, featureLevels.ToArray()))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
            }
#endif

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
#if WINDOWS_UAP
					PresentationParameters.SwapChainPanel == null)
#else
					(PresentationParameters.DeviceWindowHandle == IntPtr.Zero && PresentationParameters.SwapChainBackgroundPanel == null))
#endif
			{
                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }

                return;
            }

			// Did we change swap panels?
#if WINDOWS_UAP
			if (PresentationParameters.SwapChainPanel != _swapChainPanel)
			{
				_swapChainPanel = null;
#else
			if (PresentationParameters.SwapChainBackgroundPanel != _swapChainBackgroundPanel)
            {
                _swapChainBackgroundPanel = null;
#endif

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
#if WINDOWS_PHONE81 || WINDOWS_UAP || WINRT
                           _swapChain = new SwapChain1(dxgiFactory2, dxgiDevice2, comWindow, ref desc);
#else
                           _swapChain = dxgiFactory2.CreateSwapChainForCoreWindow(_d3dDevice, comWindow, ref desc, null);
#endif
                    }
                    else
                    {
#if WINDOWS_UAP
						_swapChainPanel = PresentationParameters.SwapChainPanel;
						using (var nativePanel = ComObject.As<SharpDX.DXGI.ISwapChainPanelNative>(PresentationParameters.SwapChainPanel))
						{
							_swapChain = new SwapChain1(dxgiFactory2, dxgiDevice2, ref desc, null);
							nativePanel.SwapChain = _swapChain;
						}
#else
						_swapChainBackgroundPanel = PresentationParameters.SwapChainBackgroundPanel;
                        using (var nativePanel = ComObject.As<SharpDX.DXGI.ISwapChainBackgroundPanelNative>(PresentationParameters.SwapChainBackgroundPanel))
                        {
#if WINDOWS_PHONE81 || WINRT
                            _swapChain = new SwapChain1(dxgiFactory2, dxgiDevice2, ref desc, null);
#else
                            _swapChain = dxgiFactory2.CreateSwapChainForComposition(_d3dDevice, ref desc, null);
#endif

                            nativePanel.SwapChain = _swapChain;
                        }
#endif
                    }

                    // Ensure that DXGI does not queue more than one frame at a time. This both reduces 
                    // latency and ensures that the application will only render after each VSync, minimizing 
                    // power consumption.
                    dxgiDevice2.MaximumFrameLatency = 1;
                }
            }

            _swapChain.Rotation = SharpDX.DXGI.DisplayModeRotation.Identity;

#if WINDOWS_UAP
            // Counter act the composition scale of the render target as 
            // we already handle this in the platform window code. 
            using (var swapChain2 = _swapChain.QueryInterface<SwapChain2>())
            {
                var inverseScale = new RawMatrix3x2();
                inverseScale.M11 = 1.0f / PresentationParameters.SwapChainPanel.CompositionScaleX;
                inverseScale.M22 = 1.0f / PresentationParameters.SwapChainPanel.CompositionScaleY;
                swapChain2.MatrixTransform = inverseScale;
            }
#endif

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

#endif
#if WINDOWS

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

            // We can not give featureLevels for granted in GraphicsProfile.Reach
            FeatureLevel supportedFeatureLevel = 0;
            try
            {
                supportedFeatureLevel = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel();
            }
            catch (SharpDX.SharpDXException)
            {   
                // if GetSupportedFeatureLevel() fails, do not crash the initialization. Program can run without this.
            }

            if (supportedFeatureLevel >= FeatureLevel.Level_9_3)
                featureLevels.Add(FeatureLevel.Level_9_3);
            if (supportedFeatureLevel >= FeatureLevel.Level_9_2)
                featureLevels.Add(FeatureLevel.Level_9_2);
            if (supportedFeatureLevel >= FeatureLevel.Level_9_1)
                featureLevels.Add(FeatureLevel.Level_9_1);

            var driverType = DriverType.Hardware;   //Default value
            switch (GraphicsAdapter.UseDriverType)
            {
                case GraphicsAdapter.DriverType.Reference:
                    driverType = DriverType.Reference;
                    break;

                case GraphicsAdapter.DriverType.FastSoftware:
                    driverType = DriverType.Warp;
                    break;
            }

#if DEBUG
            try 
            {
#endif
                // Create the Direct3D device.
                using (var defaultDevice = new SharpDX.Direct3D11.Device(driverType, creationFlags, featureLevels.ToArray()))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device>();
#if DEBUG
            }
            catch(SharpDXException)
            {
                // Try again without the debug flag.  This allows debug builds to run
                // on machines that don't have the debug runtime installed.
                creationFlags &= ~SharpDX.Direct3D11.DeviceCreationFlags.Debug;
                using (var defaultDevice = new SharpDX.Direct3D11.Device(driverType, creationFlags, featureLevels.ToArray()))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device>();
            }
#endif

            // Get Direct3D 11.1 context
            _d3dContext = _d3dDevice.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext>();
        }

        internal void CreateSizeDependentResources(bool useFullscreenParameter = false)
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
                //Find the maximum supported level coming down from 32, 16, 8, 4, 2, 1, 0
                var maxLevel = 32;
                while (maxLevel > 0)
                {
                    if (_d3dDevice.CheckMultisampleQualityLevels(Format.R32G32B32A32_Typeless, maxLevel) > 0)
                        break;
                    maxLevel /= 2;
                }

                var targetLevel = PresentationParameters.MultiSampleCount;
                if (PresentationParameters.MultiSampleCount > maxLevel)
                {
                    targetLevel = maxLevel;
                }

                multisampleDesc.Count = targetLevel;
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

                // This force to switch to fullscreen mode when hardware mode enabled(working in WindowsDX mode).
                if (useFullscreenParameter)
                {
                    _swapChain.SetFullscreenState(PresentationParameters.IsFullScreen, null);
                }

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
#if WINRT
                        Scaling = DisplayModeScaling.Stretched,
#else
                        Scaling = DisplayModeScaling.Unspecified,
#endif
                        Width = PresentationParameters.BackBufferWidth,
                        Height = PresentationParameters.BackBufferHeight,                        
                    },

                    OutputHandle = PresentationParameters.DeviceWindowHandle,
                    SampleDescription = multisampleDesc,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = SharpDXHelper.ToSwapEffect(PresentationParameters.PresentationInterval),
                    IsWindowed = useFullscreenParameter ? PresentationParameters.IsFullScreen : true
                };

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device
                
                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (var dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device1>())
                using (var dxgiAdapter = dxgiDevice.Adapter)
                using (var dxgiFactory = dxgiAdapter.GetParent<SharpDX.DXGI.Factory1>())
                {
                    _swapChain = new SwapChain(dxgiFactory, dxgiDevice, desc);
                    dxgiFactory.MakeWindowAssociation(PresentationParameters.DeviceWindowHandle, WindowAssociationFlags.IgnoreAll);
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

        public void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // Clear options for depth/stencil buffer if not attached.
            if (_currentDepthStencilView != null)
            {
                if (_currentDepthStencilView.Description.Format != SharpDX.DXGI.Format.D24_UNorm_S8_UInt)
                    options &= ~ClearOptions.Stencil;
            }
            else
            {
                options &= ~ClearOptions.DepthBuffer;
                options &= ~ClearOptions.Stencil;
            }

            lock (_d3dContext)
            {
                // Clear the diffuse render buffer.
                if ((options & ClearOptions.Target) == ClearOptions.Target)
                {
                    foreach (var view in _currentRenderTargets)
                    {
                        if (view != null)
#if WINDOWS_UAP
							_d3dContext.ClearRenderTargetView(view, new RawColor4(color.X, color.Y, color.Z, color.W));
#else
							_d3dContext.ClearRenderTargetView(view, new Color4(color.X, color.Y, color.Z, color.W));
#endif
					}
				}

                // Clear the depth/stencil render buffer.
                SharpDX.Direct3D11.DepthStencilClearFlags flags = 0;
                if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer)
                    flags |= SharpDX.Direct3D11.DepthStencilClearFlags.Depth;
                if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
                    flags |= SharpDX.Direct3D11.DepthStencilClearFlags.Stencil;

                if (flags != 0)
                    _d3dContext.ClearDepthStencilView(_currentDepthStencilView, flags, depth, (byte)stencil);
            }
        }

        private void PlatformDispose()
        {
            SharpDX.Utilities.Dispose(ref _renderTargetView);
            SharpDX.Utilities.Dispose(ref _depthStencilView);
            SharpDX.Utilities.Dispose(ref _d3dDevice);
            SharpDX.Utilities.Dispose(ref _d3dContext);

#if WINDOWS_STOREAPP || WINDOWS_UAP

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
        }

        public void PlatformPresent()
        {
#if WINDOWS_STOREAPP || WINDOWS_UAP
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

#endif
#if WINDOWS

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
#endif
        }

        private void PlatformSetViewport(ref Viewport value)
        {
            if (_d3dContext != null)
            {
#if WINDOWS_UAP
				var viewport = new RawViewportF
				{
					X = _viewport.X,
					Y = _viewport.Y,
					Width = (float)_viewport.Width,
					Height = (float)_viewport.Height,
					MinDepth = _viewport.MinDepth,
					MaxDepth = _viewport.MaxDepth
				};
#else
				var viewport = new SharpDX.ViewportF(_viewport.X, _viewport.Y, (float)_viewport.Width, (float)_viewport.Height, _viewport.MinDepth, _viewport.MaxDepth);
#endif
				lock (_d3dContext)
                    _d3dContext.Rasterizer.SetViewport(viewport);
            }
        }

        // Only implemented for DirectX right now, so not in GraphicsDevice.cs
        public void SetRenderTarget(RenderTarget2D renderTarget, int arraySlice)
        {
            if (!GraphicsCapabilities.SupportsTextureArrays)
                throw new InvalidOperationException("Texture arrays are not supported on this graphics device");

            if (renderTarget == null)
                SetRenderTarget(null);
            else
                SetRenderTargets(new RenderTargetBinding(renderTarget, arraySlice));
        }

        // Only implemented for DirectX right now, so not in GraphicsDevice.cs
        public void SetRenderTarget(RenderTarget3D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
                SetRenderTarget(null);
            else
                SetRenderTargets(new RenderTargetBinding(renderTarget, arraySlice));
        }

        private void PlatformApplyDefaultRenderTarget()
        {
            // Set the default swap chain.
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            _currentRenderTargets[0] = _renderTargetView;
            _currentDepthStencilView = _depthStencilView;

            lock (_d3dContext)
                _d3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
        }

        internal void PlatformResolveRenderTargets()
        {
            // Resolving MSAA render targets should be done here.
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            // Clear the current render targets.
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            _currentDepthStencilView = null;

            // Make sure none of the new targets are bound
            // to the device as a texture resource.
            lock (_d3dContext)
            {
                VertexTextures.ClearTargets(this, _currentRenderTargetBindings);
                Textures.ClearTargets(this, _currentRenderTargetBindings);
            }

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

            return renderTarget;
        }

#if WINRT
        internal void ResetRenderTargets()
        {
            if (_d3dContext != null)
            {
                lock (_d3dContext)
                {
#if WINDOWS_UAP
					var viewport = new RawViewportF
					{
						X = _viewport.X,
						Y = _viewport.Y,
						Width = _viewport.Width,
						Height = _viewport.Height,
						MinDepth = _viewport.MinDepth,
						MaxDepth = _viewport.MaxDepth
					};
#else
                    var viewport = new SharpDX.ViewportF( _viewport.X, _viewport.Y, 
                                                          _viewport.Width, _viewport.Height, 
                                                          _viewport.MinDepth, _viewport.MaxDepth);
#endif
                    _d3dContext.Rasterizer.SetViewport(viewport);
                    _d3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
                }
            }

            Textures.Dirty();
            SamplerStates.Dirty();
            _depthStencilStateDirty = true;
            _blendStateDirty = true;
            _indexBufferDirty = true;
            _vertexBuffersDirty = true;
            _pixelShaderDirty = true;
            _vertexShaderDirty = true;
            _rasterizerStateDirty = true;
            _scissorRectangleDirty = true;            
        }
#endif

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

        internal void PlatformBeginApplyState()
        {
            Debug.Assert(_d3dContext != null, "The d3d context is null!");
        }

        internal void PlatformApplyState(bool applyShaders)
        {
            // NOTE: This code assumes _d3dContext has been locked by the caller.

            if ( _scissorRectangleDirty )
	        {
	            _d3dContext.Rasterizer.SetScissorRectangle(
                    _scissorRectangle.X, 
                    _scissorRectangle.Y, 
                    _scissorRectangle.Right, 
                    _scissorRectangle.Bottom);
	            _scissorRectangleDirty = false;
	        }
            
            // If we're not applying shaders then early out now.
            if (!applyShaders)
                return;

            if (_indexBufferDirty)
            {
                if (_indexBuffer != null)
                {
                    _d3dContext.InputAssembler.SetIndexBuffer(
                        _indexBuffer.Buffer,
                        _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 
                            SharpDX.DXGI.Format.R16_UInt : SharpDX.DXGI.Format.R32_UInt,
                        0);
                }
                _indexBufferDirty = false;
            }

            if (_vertexBuffersDirty)
            {
                if (_vertexBuffers.Count > 0)
                {
                    for (int slot = 0; slot < _vertexBuffers.Count; slot++)
                    {
                        var vertexBufferBinding = _vertexBuffers.Get(slot);
                        var vertexBuffer = vertexBufferBinding.VertexBuffer;
                        var vertexDeclaration = vertexBuffer.VertexDeclaration;
                        int vertexStride = vertexDeclaration.VertexStride;
                        int vertexOffsetInBytes = vertexBufferBinding.VertexOffset * vertexStride;
                        _d3dContext.InputAssembler.SetVertexBuffers(
                            slot, new SharpDX.Direct3D11.VertexBufferBinding(vertexBuffer.Buffer, vertexStride, vertexOffsetInBytes));
                    }
                    _vertexBufferSlotsUsed = _vertexBuffers.Count;
                }
                else
                {
                    for (int slot = 0; slot < _vertexBufferSlotsUsed; slot++)
                        _d3dContext.InputAssembler.SetVertexBuffers(slot, new SharpDX.Direct3D11.VertexBufferBinding());

                    _vertexBufferSlotsUsed = 0;
                }
            }

            if (_vertexShader == null)
                throw new InvalidOperationException("A vertex shader must be set!");
            if (_pixelShader == null)
                throw new InvalidOperationException("A pixel shader must be set!");

            if (_vertexShaderDirty)
            {
                _d3dContext.VertexShader.Set(_vertexShader.VertexShader);

                unchecked
                {
                    _graphicsMetrics._vertexShaderCount++;
                }
            }
            if (_vertexShaderDirty || _vertexBuffersDirty)
            {
                _d3dContext.InputAssembler.InputLayout = _vertexShader.InputLayouts.GetOrCreate(_vertexBuffers);
                _vertexShaderDirty = _vertexBuffersDirty = false;
            }

            if (_pixelShaderDirty)
            {
                _d3dContext.PixelShader.Set(_pixelShader.PixelShader);
                _pixelShaderDirty = false;

                unchecked
                {
                    _graphicsMetrics._pixelShaderCount++;
                }
            }

            _vertexConstantBuffers.SetConstantBuffers(this);
            _pixelConstantBuffers.SetConstantBuffers(this);

            VertexTextures.SetTextures(this);
            VertexSamplerStates.PlatformSetSamplers(this);
            Textures.SetTextures(this);
            SamplerStates.PlatformSetSamplers(this);
        }

        private int SetUserVertexBuffer<T>(T[] vertexData, int vertexOffset, int vertexCount, VertexDeclaration vertexDecl) 
            where T : struct
        {
            DynamicVertexBuffer buffer;

            if (!_userVertexBuffers.TryGetValue(vertexDecl, out buffer) || buffer.VertexCount < vertexCount)
            {
                // Dispose the previous buffer if we have one.
                if (buffer != null)
                    buffer.Dispose();

                buffer = new DynamicVertexBuffer(this, vertexDecl, Math.Max(vertexCount, 2000), BufferUsage.WriteOnly);
                _userVertexBuffers[vertexDecl] = buffer;
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

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);

                var indexCount = GetElementCountArray(primitiveType, primitiveCount);
                _d3dContext.DrawIndexed(indexCount, startIndex, baseVertex);
            }
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {
            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, vertexCount, vertexDeclaration);

            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.Draw(vertexCount, startVertex);
            }
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.Draw(vertexCount, vertexStart);
            }
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            var indexCount = GetElementCountArray(primitiveType, primitiveCount);
            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            var startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);

            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.DrawIndexed(indexCount, startIndex, startVertex);
            }
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            var indexCount = GetElementCountArray(primitiveType, primitiveCount);
            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            var startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);

            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                _d3dContext.DrawIndexed(indexCount, startIndex, startVertex);
            }
        }

        private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int instanceCount)
        {
            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                int indexCount = GetElementCountArray(primitiveType, primitiveCount);
                _d3dContext.DrawIndexedInstanced(indexCount, instanceCount, startIndex, baseVertex, 0);
            }
        }

        /// <summary>
        /// Sends queued-up commands in the command buffer to the graphics processing unit (GPU).
        /// </summary>
        public void Flush()
        {
            _d3dContext.Flush();
        }
        
        private static GraphicsProfile PlatformGetHighestSupportedGraphicsProfile(GraphicsDevice graphicsDevice)
        {
            FeatureLevel featureLevel;
			try
            {
				if (graphicsDevice == null || graphicsDevice._d3dDevice == null || graphicsDevice._d3dDevice.NativePointer == null) 
					featureLevel = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel();
               	else
                	featureLevel = graphicsDevice._d3dDevice.FeatureLevel;
            }
            catch (SharpDXException)
            { 
            	featureLevel = FeatureLevel.Level_9_1; //Minimum defined level
            }

            GraphicsProfile graphicsProfile;

            if (featureLevel >= FeatureLevel.Level_10_0 || GraphicsAdapter.UseReferenceDevice)
                graphicsProfile = GraphicsProfile.HiDef;
            else 
                graphicsProfile = GraphicsProfile.Reach;

            return graphicsProfile;
        }

#if WINDOWS_STOREAPP || WINDOWS_UAP
        internal void Trim()
        {
            using (var dxgiDevice3 = _d3dDevice.QueryInterface<SharpDX.DXGI.Device3>())
                dxgiDevice3.Trim();
        }
#endif
    }
}
