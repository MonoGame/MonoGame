using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DXGI;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Only currently implemented for for DIRECTX.
    /// This class is not part of stock XNA. Normal rendering does
    /// not make use of this class. It is only for games which need
    /// to manage rendering of multiple GameWindow(s).
    /// </summary>
#if WINDOWS && DIRECTX
    public class SwapChainRenderTarget : RenderTarget2D
    {
        private SharpDX.DXGI.SwapChain _swapChain;

        public SwapChainRenderTarget(GraphicsDevice graphicsDevice,
                                     IntPtr windowHandle,                                     
                                     int width,
                                     int height,
                                     bool mipMap,
                                     PresentInterval presentInterval,
                                     SurfaceFormat surfaceFormat,
                                     DepthFormat depthFormat,
                                     int preferredMultiSampleCount,
                                     RenderTargetUsage usage)
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

            var multisampleDesc = new SharpDX.DXGI.SampleDescription(1, 0);
            if (preferredMultiSampleCount > 1)
            {
                multisampleDesc.Count = preferredMultiSampleCount;
                multisampleDesc.Quality =
                    (int)SharpDX.Direct3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            var desc = new SharpDX.DXGI.SwapChainDescription()
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
                               Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                               BufferCount = 2,
                               SwapEffect = SharpDXHelper.ToSwapEffect(presentInterval),
                               IsWindowed = true,
                           };

            // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device
            var d3dDevice = graphicsDevice._d3dDevice;

            // First, retrieve the underlying DXGI Device from the D3D Device.
            // Creates the swap chain 
            using (var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device1>())
            using (var dxgiAdapter = dxgiDevice.Adapter)
            using (var dxgiFactory = dxgiAdapter.GetParent<SharpDX.DXGI.Factory1>())
            {
                _swapChain = new SwapChain(dxgiFactory, dxgiDevice, desc);

                // Ensure that DXGI does not queue more than one frame at a time. This 
                // both reduces latency and ensures that the application will only render 
                // after each VSync, minimizing power consumption.
                dxgiDevice.MaximumFrameLatency = 1;
            }

            // Obtain the backbuffer for this window which will be the final 3D rendertarget.
            Point targetSize;
            var backBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<SharpDX.Direct3D11.Texture2D>(_swapChain, 0);

            // Create a view interface on the rendertarget to use on bind.
            _renderTargetView = new SharpDX.Direct3D11.RenderTargetView(d3dDevice, backBuffer);

            // Get the rendertarget dimensions for later.
            var backBufferDesc = backBuffer.Description;
            targetSize = new Point(backBufferDesc.Width, backBufferDesc.Height);

            _texture = backBuffer;

            // Create the depth buffer if we need it.
            if (depthFormat != DepthFormat.None)
            {
                dxgiFormat = SharpDXHelper.ToFormat(depthFormat);

                // Allocate a 2-D surface as the depth/stencil buffer.
                using (
                    var depthBuffer = new SharpDX.Direct3D11.Texture2D(d3dDevice,
                                                                       new SharpDX.Direct3D11.Texture2DDescription()
                                                                           {
                                                                               Format = dxgiFormat,
                                                                               ArraySize = 1,
                                                                               MipLevels = 1,
                                                                               Width = targetSize.X,
                                                                               Height = targetSize.Y,
                                                                               SampleDescription = multisampleDesc,
                                                                               Usage =
                                                                                   SharpDX.Direct3D11.ResourceUsage
                                                                                          .Default,
                                                                               BindFlags =
                                                                                   SharpDX.Direct3D11.BindFlags
                                                                                          .DepthStencil,
                                                                           }))

                    // Create a DepthStencil view on this surface to use on bind.
                    _depthStencilView = new SharpDX.Direct3D11.DepthStencilView(d3dDevice, depthBuffer);
            }
        }

        public void Present()
        {
            lock (GraphicsDevice._d3dContext)
            {
                try
                {
                    _swapChain.Present(1, PresentFlags.None);
                }
                catch (SharpDX.SharpDXException)
                {
                }
            }
        }
    } 
#endif
}
