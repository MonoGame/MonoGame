using System;

#if WINDOWS && DIRECTX
using SharpDX.DXGI;
using SharpDX.Direct3D11;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
#if WINDOWS && DIRECTX

    /// <summary>
    /// A swap chain used for rendering to a secondary GameWindow.
    /// </summary>
    /// <remarks>
    /// This is an extension and not part of stock XNA.
    /// It is currently implemented for Windows and DirectX only.
    /// </remarks>
    public class SwapChainRenderTarget : RenderTarget2D
    {
        private SwapChain _swapChain;

        public PresentInterval PresentInterval;

        public SwapChainRenderTarget(   GraphicsDevice graphicsDevice,
                                        IntPtr windowHandle,
                                        int width,
                                        int height)
            : this( 
                graphicsDevice, 
                windowHandle, 
                width, 
                height, 
                false, 
                SurfaceFormat.Color,
                DepthFormat.Depth24,
                0, 
                RenderTargetUsage.DiscardContents,
                PresentInterval.Default)
        {
        }

        public SwapChainRenderTarget(   GraphicsDevice graphicsDevice,
                                        IntPtr windowHandle,                                     
                                        int width,
                                        int height,
                                        bool mipMap,
                                        SurfaceFormat surfaceFormat,
                                        DepthFormat depthFormat,                                        
                                        int preferredMultiSampleCount,
                                        RenderTargetUsage usage,
                                        PresentInterval presentInterval)
            : base(
                graphicsDevice,
                width,
                height,
                mipMap,
                surfaceFormat,
                depthFormat,
                preferredMultiSampleCount,
                usage,
                SurfaceType.SwapChainRenderTarget)
        {
            var dxgiFormat = surfaceFormat == SurfaceFormat.Color
                             ? SharpDX.DXGI.Format.B8G8R8A8_UNorm
                             : SharpDXHelper.ToFormat(surfaceFormat);

            var multisampleDesc = new SampleDescription(1, 0);
            if (preferredMultiSampleCount > 1)
            {
                multisampleDesc.Count = preferredMultiSampleCount;
                multisampleDesc.Quality = (int)StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            var desc = new SwapChainDescription()
            {
                ModeDescription =
                {
                    Format = dxgiFormat,
                    Scaling = DisplayModeScaling.Stretched,
                    Width = width,
                    Height = height,
                },

                OutputHandle = windowHandle,
                SampleDescription = multisampleDesc,
                Usage = Usage.RenderTargetOutput,
                BufferCount = 2,
                SwapEffect = SharpDXHelper.ToSwapEffect(presentInterval),
                IsWindowed = true,
            };

            PresentInterval = presentInterval;

            // Once the desired swap chain description is configured, it must 
            // be created on the same adapter as our D3D Device
            var d3dDevice = graphicsDevice._d3dDevice;

            // First, retrieve the underlying DXGI Device from the D3D Device.
            // Creates the swap chain 
            using (var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device1>())
            using (var dxgiAdapter = dxgiDevice.Adapter)
            using (var dxgiFactory = dxgiAdapter.GetParent<Factory1>())
            {
                _swapChain = new SwapChain(dxgiFactory, dxgiDevice, desc);
            }

            // Obtain the backbuffer for this window which will be the final 3D rendertarget.
            var backBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<SharpDX.Direct3D11.Texture2D>(_swapChain, 0);

            // Create a view interface on the rendertarget to use on bind.
            _renderTargetViews = new[] { new RenderTargetView(d3dDevice, backBuffer) };

            // Get the rendertarget dimensions for later.
            var backBufferDesc = backBuffer.Description;
            var targetSize = new Point(backBufferDesc.Width, backBufferDesc.Height);

            _texture = backBuffer;

            // Create the depth buffer if we need it.
            if (depthFormat != DepthFormat.None)
            {
                dxgiFormat = SharpDXHelper.ToFormat(depthFormat);

                // Allocate a 2-D surface as the depth/stencil buffer.
                using (
                    var depthBuffer = new SharpDX.Direct3D11.Texture2D(d3dDevice,
                                                                       new Texture2DDescription()
                                                                           {
                                                                               Format = dxgiFormat,
                                                                               ArraySize = 1,
                                                                               MipLevels = 1,
                                                                               Width = targetSize.X,
                                                                               Height = targetSize.Y,
                                                                               SampleDescription = multisampleDesc,
                                                                               Usage = ResourceUsage.Default,
                                                                               BindFlags = BindFlags.DepthStencil,
                                                                           }))

                    // Create a DepthStencil view on this surface to use on bind.
                    _depthStencilView = new DepthStencilView(d3dDevice, depthBuffer);
            }
        }


        // TODO: We need to expose the other Present() overloads
        // for passing source/dest rectangles.

        /// <summary>
        /// Displays the contents of the active back buffer to the screen.
        /// </summary>
        public void Present()
        {
            lock (GraphicsDevice._d3dContext)
            {
                try
                {
                    _swapChain.Present(PresentInterval.GetFrameLatency(), PresentFlags.None);
                }
                catch (SharpDX.SharpDXException)
                {
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _swapChain);
            }

            base.Dispose(disposing);
        }

    }

#endif // WINDOWS && DIRECTX
}
