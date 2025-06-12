// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MonoGame.Framework.Utilities;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using SharpDX.DXGI;

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

#if WINDOWS
        SwapChain _swapChain;
#endif

        // The active render targets.
        readonly SharpDX.Direct3D11.RenderTargetView[] _currentRenderTargets = new SharpDX.Direct3D11.RenderTargetView[8];

        // The active depth view.
        SharpDX.Direct3D11.DepthStencilView _currentDepthStencilView;

        private readonly Dictionary<VertexDeclaration, DynamicVertexBuffer> _userVertexBuffers = new Dictionary<VertexDeclaration, DynamicVertexBuffer>();
        private DynamicIndexBuffer _userIndexBuffer16;
        private DynamicIndexBuffer _userIndexBuffer32;

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

#if WINDOWS
            CreateDeviceResources();
#endif

            _maxVertexBufferSlots = _d3dDevice.FeatureLevel >= FeatureLevel.Level_11_0 ? SharpDX.Direct3D11.InputAssemblerStage.VertexInputResourceSlotCount : 16;
        }

        private void PlatformInitialize()
        {
#if WINDOWS
            CorrectBackBufferSize();
#endif
            CreateSizeDependentResources();
        }

        partial void PlatformReset()
        {
#if WINDOWS
            CorrectBackBufferSize();
#endif

            if (PresentationParameters.DeviceWindowHandle == IntPtr.Zero)
            {
                throw new ArgumentException("PresentationParameters.DeviceWindowHandle must not be null.");
            }
        }

#if WINDOWS

        private void CorrectBackBufferSize()
        {
            // Window size can be modified when we're going full screen, we need to take that into account
            // so the back buffer has the right size.
            if (PresentationParameters.IsFullScreen)
            {
                int newWidth, newHeight;
                if (PresentationParameters.HardwareModeSwitch)
                    GetModeSwitchedSize(out newWidth, out newHeight);
                else
                    GetDisplayResolution(out newWidth, out newHeight);

                PresentationParameters.BackBufferWidth = newWidth;
                PresentationParameters.BackBufferHeight = newHeight;
            }
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

            // Windows requires BGRA support out of DX.
            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport;

            if (GraphicsAdapter.UseDebugLayers)
            {
                creationFlags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
            }

            // Pass the preferred feature levels based on the
            // target profile that may have been set by the user.
            FeatureLevel[] featureLevels;
            if (GraphicsProfile == GraphicsProfile.HiDef)
            {
                featureLevels = new[]
                    {
                        FeatureLevel.Level_11_0,
                        FeatureLevel.Level_10_1,
                        FeatureLevel.Level_10_0,
                        // Feature levels below 10 are not supported for the HiDef profile
                    };
            }
            else // Reach profile
            {
                featureLevels = new[]
                    {
                        // For the Reach profile, first try use the highest supported 9_X feature level
                        FeatureLevel.Level_9_3,
                        FeatureLevel.Level_9_2,
                        FeatureLevel.Level_9_1,
                        // If level 9 is not supported, then just use the highest supported level
                        FeatureLevel.Level_11_0,
                        FeatureLevel.Level_10_1,
                        FeatureLevel.Level_10_0,
                    };
            }

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
            
            try
            {
                // Create the Direct3D device.
                using (var defaultDevice = new SharpDX.Direct3D11.Device(driverType, creationFlags, featureLevels))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device>();
            }
            catch (SharpDXException)
            {
                // Try again without the debug flag.  This allows debug builds to run
                // on machines that don't have the debug runtime installed.
                creationFlags &= ~SharpDX.Direct3D11.DeviceCreationFlags.Debug;
                using (var defaultDevice = new SharpDX.Direct3D11.Device(driverType, creationFlags, featureLevels))
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device>();
            }

            // Get Direct3D 11.1 context
            _d3dContext = _d3dDevice.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext>();
            
            // Create a new instance of GraphicsDebug because we support it on Windows platforms.
            _graphicsDebug = new GraphicsDebug(this);
        }

        internal void SetHardwareFullscreen()
        {
            _swapChain.SetFullscreenState(PresentationParameters.IsFullScreen && PresentationParameters.HardwareModeSwitch, null);
        }

        internal void ClearHardwareFullscreen()
        {
            _swapChain.SetFullscreenState(false, null);
        }

        internal void ResizeTargets()
        {
            var format = SharpDXHelper.ToFormat(PresentationParameters.BackBufferFormat);
            var descr = new ModeDescription
            {
                Format = format,
                Scaling = DisplayModeScaling.Unspecified,
                Width = PresentationParameters.BackBufferWidth,
                Height = PresentationParameters.BackBufferHeight,
            };

            _swapChain.ResizeTarget(ref descr);
        }

        internal void GetModeSwitchedSize(out int width, out int height)
        {
            Output output = null;
            if (_swapChain == null)
            {
                // get the primary output
                using (var factory = new SharpDX.DXGI.Factory1())
                using (var adapter = factory.GetAdapter1(0))
                    output = adapter.Outputs[0];
            }
            else
            {
                try
                {
                    output = _swapChain.ContainingOutput;
                }
                catch (SharpDXException) { /* ContainingOutput fails on a headless device */ }
            }

            var format = SharpDXHelper.ToFormat(PresentationParameters.BackBufferFormat);
            var target = new ModeDescription
            {
                Format = format,
                Scaling = DisplayModeScaling.Unspecified,
                Width = PresentationParameters.BackBufferWidth,
                Height = PresentationParameters.BackBufferHeight,
            };

            if (output == null)
            {
                width = PresentationParameters.BackBufferWidth;
                height = PresentationParameters.BackBufferHeight;
            }
            else
            {
                ModeDescription closest;
                output.GetClosestMatchingMode(_d3dDevice, target, out closest);
                width = closest.Width;
                height = closest.Height;
                output.Dispose();
            }
        }

        internal void GetDisplayResolution(out int width, out int height)
        {
            width = Adapter.CurrentDisplayMode.Width;
            height = Adapter.CurrentDisplayMode.Height;
        }

        internal void CreateSizeDependentResources()
        {
            // Clamp MultiSampleCount
            PresentationParameters.MultiSampleCount =
                GetClampedMultisampleCount(PresentationParameters.MultiSampleCount);

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

            var format = SharpDXHelper.ToFormat(PresentationParameters.BackBufferFormat);
            var multisampleDesc = GetSupportedSampleDescription(
                format, 
                PresentationParameters.MultiSampleCount);

            // If the swap chain already exists... update it.
            if (_swapChain != null
                // check if multisampling hasn't changed
                && _swapChain.Description.SampleDescription.Count == multisampleDesc.Count
                && _swapChain.Description.SampleDescription.Quality == multisampleDesc.Quality)
            {
                _swapChain.ResizeBuffers(2,
                                        PresentationParameters.BackBufferWidth,
                                        PresentationParameters.BackBufferHeight,
                                        format,
                                        SwapChainFlags.AllowModeSwitch);
            }

            // Otherwise, create a new swap chain.
            else
            {
                var wasFullScreen = false;
                // Dispose of old swap chain if exists
                if (_swapChain != null)
                {
                    wasFullScreen = _swapChain.IsFullScreen;
                    // Before releasing a swap chain, first switch to windowed mode
                    _swapChain.SetFullscreenState(false, null);
                    _swapChain.Dispose();
                }

                // SwapChain description
                var desc = new SharpDX.DXGI.SwapChainDescription()
                {
                    ModeDescription =
                    {
                        Format = format,
                        Scaling = DisplayModeScaling.Unspecified,
                        Width = PresentationParameters.BackBufferWidth,
                        Height = PresentationParameters.BackBufferHeight,
                    },

                    OutputHandle = PresentationParameters.DeviceWindowHandle,
                    SampleDescription = multisampleDesc,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = SharpDXHelper.ToSwapEffect(PresentationParameters.PresentationInterval),
                    IsWindowed = true,
                    Flags = SwapChainFlags.AllowModeSwitch
                };

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device

                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (var dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device1>())
                using (var dxgiAdapter = dxgiDevice.Adapter)
                using (var dxgiFactory = dxgiAdapter.GetParent<SharpDX.DXGI.Factory1>())
                {
                    _swapChain = new SwapChain(dxgiFactory, dxgiDevice, desc);
                    RefreshAdapter();
                    dxgiFactory.MakeWindowAssociation(PresentationParameters.DeviceWindowHandle, WindowAssociationFlags.IgnoreAll);
                    // To reduce latency, ensure that DXGI does not queue more than one frame at a time.
                    // Docs: https://msdn.microsoft.com/en-us/library/windows/desktop/ff471334(v=vs.85).aspx
                    dxgiDevice.MaximumFrameLatency = 1;
                }
                // Preserve full screen state, after swap chain is re-created 
                if (PresentationParameters.HardwareModeSwitch
                    && wasFullScreen)
                    SetHardwareFullscreen();
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

        internal void RefreshAdapter()
        {
            if (_swapChain == null)
                return;

            Output output = null;
            try
            {
                output = _swapChain.ContainingOutput;
            }
            catch (SharpDXException) { /* ContainingOutput fails on a headless device */ }

            if (output != null)
            {
                foreach (var adapter in GraphicsAdapter.Adapters)
                {
                    if (adapter.DeviceName == output.Description.DeviceName)
                    {
                        Adapter = adapter;
                        break;
                    }
                }

                output.Dispose();
            }
        }

        internal void OnPresentationChanged()
        {
            CreateSizeDependentResources();
            ApplyRenderTargets(null);
        }

#endif // WINDOWS


        /// <summary>
        /// Get highest multisample quality level for specified format and multisample count.
        /// Returns 0 if multisampling is not supported for input parameters.
        /// </summary>
        /// <param name="format">The texture format.</param>
        /// <param name="multiSampleCount">The number of samples during multisampling.</param>
        /// <returns>
        /// Higher than zero if multiSampleCount is supported. 
        /// Zero if multiSampleCount is not supported.
        /// </returns>
        private int GetMultiSamplingQuality(Format format, int multiSampleCount)
        {
            // The valid range is between zero and one less than the level returned by CheckMultisampleQualityLevels
            // https://msdn.microsoft.com/en-us/library/windows/desktop/bb173072(v=vs.85).aspx
            var quality = _d3dDevice.CheckMultisampleQualityLevels(format, multiSampleCount) - 1;
            // NOTE: should we always return highest quality?
            return Math.Max(quality, 0); // clamp minimum to 0 
        }

        internal SampleDescription GetSupportedSampleDescription(Format format, int multiSampleCount)
        {
            var multisampleDesc = new SharpDX.DXGI.SampleDescription(1, 0);

            if (multiSampleCount > 1)
            {
                var quality = GetMultiSamplingQuality(format, multiSampleCount);

                multisampleDesc.Count = multiSampleCount;
                multisampleDesc.Quality = quality;
            }

            return multisampleDesc;
        }

        private void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
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
                            _d3dContext.ClearRenderTargetView(view, new RawColor4(color.X, color.Y, color.Z, color.W));
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
            // make sure to release full screen or this might cause issues on exit
            if (_swapChain != null && _swapChain.IsFullScreen)
                _swapChain.SetFullscreenState(false, null);

            SharpDX.Utilities.Dispose(ref _renderTargetView);
            SharpDX.Utilities.Dispose(ref _depthStencilView);

            if (_userIndexBuffer16 != null)
                _userIndexBuffer16.Dispose();
            if (_userIndexBuffer32 != null)
                _userIndexBuffer32.Dispose();

            foreach (var vb in _userVertexBuffers.Values)
                vb.Dispose();

            SharpDX.Utilities.Dispose(ref _swapChain);
            SharpDX.Utilities.Dispose(ref _d3dContext);
            SharpDX.Utilities.Dispose(ref _d3dDevice);
        }

        private void PlatformPresent()
        {
#if WINDOWS

            try
            {
                var syncInterval = PresentationParameters.PresentationInterval.GetSyncInterval();

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
                var viewport = new RawViewportF
                {
                    X = _viewport.X,
                    Y = _viewport.Y,
                    Width = (float)_viewport.Width,
                    Height = (float)_viewport.Height,
                    MinDepth = _viewport.MinDepth,
                    MaxDepth = _viewport.MaxDepth
                };
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
            {
                _tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(_tempRenderTargetBinding);
            }
        }

        // Only implemented for DirectX right now, so not in GraphicsDevice.cs
        public void SetRenderTarget(RenderTarget3D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
                SetRenderTarget(null);
            else
            {
                _tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(_tempRenderTargetBinding);
            }
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
            for (var i = 0; i < _currentRenderTargetCount; i++)
            {
                var renderTargetBinding = _currentRenderTargetBindings[i];

                // Resolve MSAA render targets
                var renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
                if (renderTarget != null && renderTarget.MultiSampleCount > 1)
                    renderTarget.ResolveSubresource();

                // Generate mipmaps.
                if (renderTargetBinding.RenderTarget.LevelCount > 1)
                {
                    lock (_d3dContext)
                        _d3dContext.GenerateMips(renderTargetBinding.RenderTarget.GetShaderResourceView());
                }
            }
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
                case PrimitiveType.PointList:
                    return PrimitiveTopology.PointList;
            }

            throw new ArgumentException();
        }

        internal void PlatformBeginApplyState()
        {
            Debug.Assert(_d3dContext != null, "The d3d context is null!");
        }

        private void PlatformApplyBlend()
        {
            if (_blendFactorDirty || _blendStateDirty)
            {
                var state = _actualBlendState.GetDxState(this);
                var factor = GetBlendFactor();
                _d3dContext.OutputMerger.SetBlendState(state, factor);

                _blendFactorDirty = false;
                _blendStateDirty = false;
            }
        }

        private SharpDX.Mathematics.Interop.RawColor4 GetBlendFactor()
        {
            return new SharpDX.Mathematics.Interop.RawColor4(
                    BlendFactor.R / 255.0f,
                    BlendFactor.G / 255.0f,
                    BlendFactor.B / 255.0f,
                    BlendFactor.A / 255.0f);
        }

        internal void PlatformApplyState(bool applyShaders)
        {
            // NOTE: This code assumes _d3dContext has been locked by the caller.

            if (_scissorRectangleDirty)
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

            var indexSize = ReflectionHelpers.SizeOf<T>.Get();
            var indexElementSize = indexSize == 2 ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits;

            var requiredIndexCount = Math.Max(indexCount, 6000);
            if (indexElementSize == IndexElementSize.SixteenBits)
            {
                if (_userIndexBuffer16 == null || _userIndexBuffer16.IndexCount < requiredIndexCount)
                {
                    if (_userIndexBuffer16 != null)
                        _userIndexBuffer16.Dispose();

                    _userIndexBuffer16 = new DynamicIndexBuffer(this, indexElementSize, requiredIndexCount, BufferUsage.WriteOnly);
                }

                buffer = _userIndexBuffer16;
            }
            else
            {
                if (_userIndexBuffer32 == null || _userIndexBuffer32.IndexCount < requiredIndexCount)
                {
                    if (_userIndexBuffer32 != null)
                        _userIndexBuffer32.Dispose();

                    _userIndexBuffer32 = new DynamicIndexBuffer(this, indexElementSize, requiredIndexCount, BufferUsage.WriteOnly);
                }

                buffer = _userIndexBuffer32;
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

        private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex,
            int primitiveCount, int baseInstance, int instanceCount)
        {
            lock (_d3dContext)
            {
                ApplyState(true);

                _d3dContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
                int indexCount = GetElementCountArray(primitiveType, primitiveCount);
                _d3dContext.DrawIndexedInstanced(indexCount, instanceCount, startIndex, baseVertex, baseInstance);
            }
        }

        private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
        {
            // TODO share code with Texture2D.GetData and do pooling for staging textures
            // first set up a staging texture
            const SurfaceFormat format = SurfaceFormat.Color;
            //You can't Map the BackBuffer surface, so we copy to another texture
            using (var backBufferTexture = SharpDX.Direct3D11.Resource.FromSwapChain<SharpDX.Direct3D11.Texture2D>(_swapChain, 0))
            {
                var desc = backBufferTexture.Description;
                desc.SampleDescription = new SampleDescription(1, 0);
                desc.BindFlags = BindFlags.None;
                desc.CpuAccessFlags = CpuAccessFlags.Read;
                desc.Usage = ResourceUsage.Staging;
                desc.OptionFlags = ResourceOptionFlags.None;

                using (var stagingTex = new SharpDX.Direct3D11.Texture2D(_d3dDevice, desc))
                {
                    lock (_d3dContext)
                    {
                        // Copy the data from the GPU to the staging texture.
                        // if MSAA is enabled we need to first copy to a resource without MSAA
                        if (backBufferTexture.Description.SampleDescription.Count > 1)
                        {
                            desc.Usage = ResourceUsage.Default;
                            desc.CpuAccessFlags = CpuAccessFlags.None;
                            using (var noMsTex = new SharpDX.Direct3D11.Texture2D(_d3dDevice, desc))
                            {
                                _d3dContext.ResolveSubresource(backBufferTexture, 0, noMsTex, 0, desc.Format);
                                if (rect.HasValue)
                                {
                                    var r = rect.Value;
                                    _d3dContext.CopySubresourceRegion(noMsTex, 0,
                                        new ResourceRegion(r.Left, r.Top, 0, r.Right, r.Bottom, 1), stagingTex,
                                        0);
                                }
                                else
                                    _d3dContext.CopyResource(noMsTex, stagingTex);
                            }
                        }
                        else
                        {
                            if (rect.HasValue)
                            {
                                var r = rect.Value;
                                _d3dContext.CopySubresourceRegion(backBufferTexture, 0,
                                    new ResourceRegion(r.Left, r.Top, 0, r.Right, r.Bottom, 1), stagingTex, 0);
                            }
                            else
                                _d3dContext.CopyResource(backBufferTexture, stagingTex);
                        }

                        // Copy the data to the array.
                        DataStream stream = null;
                        try
                        {
                            var databox = _d3dContext.MapSubresource(stagingTex, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out stream);

                            int elementsInRow, rows;
                            if (rect.HasValue)
                            {
                                elementsInRow = rect.Value.Width;
                                rows = rect.Value.Height;
                            }
                            else
                            {
                                elementsInRow = stagingTex.Description.Width;
                                rows = stagingTex.Description.Height;
                            }
                            var elementSize = format.GetSize();
                            var rowSize = elementSize * elementsInRow;
                            if (rowSize == databox.RowPitch)
                                stream.ReadRange(data, startIndex, count);
                            else
                            {
                                // Some drivers may add pitch to rows.
                                // We need to copy each row separately and skip trailing zeroes.
                                stream.Seek(0, SeekOrigin.Begin);

                                var elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
                                for (var row = 0; row < rows; row++)
                                {
                                    int i;
                                    for (i = row * rowSize / elementSizeInByte; i < (row + 1) * rowSize / elementSizeInByte; i++)
                                        data[i + startIndex] = stream.Read<T>();

                                    if (i >= count)
                                        break;

                                    stream.Seek(databox.RowPitch - rowSize, SeekOrigin.Current);
                                }
                            }
                        }
                        finally
                        {
                            SharpDX.Utilities.Dispose(ref stream);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends queued-up commands in the command buffer to the graphics processing unit (GPU).
        /// </summary>
        public void Flush()
        {
            _d3dContext.Flush();
        }

        private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
        {
            return new Rectangle(x, y, width, height);
        }
    }
}
