// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        internal RenderTargetView _renderTargetView;
        internal DepthStencilView _depthStencilView;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            GenerateIfRequired();

            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;
        }

        private void GenerateIfRequired()
        {
            if (_renderTargetView != null)
                return;

            // Create a view interface on the rendertarget to use on bind.
            _renderTargetView = new RenderTargetView(GraphicsDevice._d3dDevice, GetTexture());

            // If we don't need a depth buffer then we're done.
            if (DepthStencilFormat == DepthFormat.None)
                return;

            // Setup the multisampling description.
            var multisampleDesc = new SharpDX.DXGI.SampleDescription(1, 0);
            if (MultiSampleCount > 1)
            {
                multisampleDesc.Count = MultiSampleCount;
                multisampleDesc.Quality = (int)StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            // Create a descriptor for the depth/stencil buffer.
            // Allocate a 2-D surface as the depth/stencil buffer.
            // Create a DepthStencil view on this surface to use on bind.
            using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, new Texture2DDescription
            {
                Format = SharpDXHelper.ToFormat(DepthStencilFormat),
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = multisampleDesc,
                BindFlags = BindFlags.DepthStencil,
            }))
            {
                // Create the view for binding to the device.
                _depthStencilView = new DepthStencilView(GraphicsDevice._d3dDevice, depthBuffer,
                    new DepthStencilViewDescription()
                    {
                        Format = SharpDXHelper.ToFormat(DepthStencilFormat),
                        Dimension = DepthStencilViewDimension.Texture2D
                    });
            }
        }

        private void PlatformGraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _renderTargetView);
            SharpDX.Utilities.Dispose(ref _depthStencilView);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _renderTargetView);
                SharpDX.Utilities.Dispose(ref _depthStencilView);
            }

            base.Dispose(disposing);
        }

        RenderTargetView IRenderTarget.GetRenderTargetView(int arraySlice)
        {
            GenerateIfRequired();
            return _renderTargetView;
        }

        DepthStencilView IRenderTarget.GetDepthStencilView()
        {
            GenerateIfRequired();
            return _depthStencilView;
        }
    }
}
