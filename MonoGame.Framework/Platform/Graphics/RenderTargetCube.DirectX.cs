// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube
    {
        private RenderTargetView[] _renderTargetViews;
        private DepthStencilView _depthStencilView;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            // Create one render target view per cube map face.
            _renderTargetViews = new RenderTargetView[6];
            for (int i = 0; i < _renderTargetViews.Length; i++)
            {
                var renderTargetViewDescription = new RenderTargetViewDescription
                {
                    Dimension = RenderTargetViewDimension.Texture2DArray,
                    Format = SharpDXHelper.ToFormat(this.Format),
                    Texture2DArray =
                    {
                        ArraySize = 1,
                        FirstArraySlice = i,
                        MipSlice = 0
                    }
                };

                _renderTargetViews[i] = new RenderTargetView(graphicsDevice._d3dDevice, GetTexture(), renderTargetViewDescription);
            }

            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;

            var sampleDescription = new SampleDescription(1, 0);
            if (preferredMultiSampleCount > 1)
            {
                sampleDescription.Count = preferredMultiSampleCount;
                sampleDescription.Quality = (int)StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            var depthStencilDescription = new Texture2DDescription
            {
                Format = SharpDXHelper.ToFormat(preferredDepthFormat),
                ArraySize = 1,
                MipLevels = 1,
                Width = size,
                Height = size,
                SampleDescription = sampleDescription,
                BindFlags = BindFlags.DepthStencil,
            };

            using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(graphicsDevice._d3dDevice, depthStencilDescription))
            {
                var depthStencilViewDescription = new DepthStencilViewDescription
                {
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Format = SharpDXHelper.ToFormat(preferredDepthFormat),
                };
                _depthStencilView = new DepthStencilView(graphicsDevice._d3dDevice, depthBuffer, depthStencilViewDescription);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_renderTargetViews != null)
                {
                    for (var i = 0; i < _renderTargetViews.Length; i++)
                        _renderTargetViews[i].Dispose();

                    _renderTargetViews = null;
                    SharpDX.Utilities.Dispose(ref _depthStencilView);
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public RenderTargetView GetRenderTargetView(int arraySlice)
        {
            return _renderTargetViews[arraySlice];
        }

        /// <inheritdoc/>
        public DepthStencilView GetDepthStencilView()
        {
            return _depthStencilView;
        }
    }
}
