// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube
    {
        private RenderTargetView[] _renderTargetViews;
        private DepthStencilView _depthStencilView;
        private SharpDX.Direct3D11.Texture2D _msTexture;
        private SampleDescription _msSampleDescription;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            _msSampleDescription = graphicsDevice.GetSupportedSampleDescription(SharpDXHelper.ToFormat(this.Format), this.MultiSampleCount);
        }

        private void GenerateIfRequired()
        {
            if (_renderTargetViews != null)
                return;

            // Create one render target view per cube map face.
            _renderTargetViews = new RenderTargetView[6];
            for (int i = 0; i < _renderTargetViews.Length; i++)
            {
                var renderTargetViewDescription = new RenderTargetViewDescription
                {
                    Format = SharpDXHelper.ToFormat(this.Format),
                };

                SharpDX.Direct3D11.Texture2D viewTexture;
                if (MultiSampleCount > 1)
                {
                    // MSAA cubes still resolve back into the actual cube texture face on unbind.
                    renderTargetViewDescription.Dimension = RenderTargetViewDimension.Texture2DMultisampled;
                    viewTexture = GetMSTexture();
                }
                else
                {
                    renderTargetViewDescription.Dimension = RenderTargetViewDimension.Texture2DArray;
                    renderTargetViewDescription.Texture2DArray.ArraySize = 1;
                    renderTargetViewDescription.Texture2DArray.FirstArraySlice = i;
                    renderTargetViewDescription.Texture2DArray.MipSlice = 0;
                    viewTexture = (SharpDX.Direct3D11.Texture2D)GetTexture();
                }

                _renderTargetViews[i] = new RenderTargetView(
                    GraphicsDevice._d3dDevice,
                    viewTexture,
                    renderTargetViewDescription);
            }

            // If we don't need a depth buffer then we're done.
            if (DepthStencilFormat == DepthFormat.None)
                return;

            var depthStencilDescription = new Texture2DDescription
            {
                Format = SharpDXHelper.ToFormat(DepthStencilFormat),
                ArraySize = 1,
                MipLevels = 1,
                Width = size,
                Height = size,
                SampleDescription = _msSampleDescription,
                BindFlags = BindFlags.DepthStencil,
            };

            using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, depthStencilDescription))
            {
                var depthStencilViewDescription = new DepthStencilViewDescription
                {
                    Dimension = MultiSampleCount > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                    Format = SharpDXHelper.ToFormat(DepthStencilFormat),
                };

                _depthStencilView = new DepthStencilView(GraphicsDevice._d3dDevice, depthBuffer, depthStencilViewDescription);
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
                }

                SharpDX.Utilities.Dispose(ref _depthStencilView);
                SharpDX.Utilities.Dispose(ref _msTexture);
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public RenderTargetView GetRenderTargetView(int arraySlice)
        {
            GenerateIfRequired();
            return _renderTargetViews[arraySlice];
        }

        /// <inheritdoc/>
        public DepthStencilView GetDepthStencilView()
        {
            GenerateIfRequired();
            return _depthStencilView;
        }

        internal void ResolveSubresource(int arraySlice)
        {
            lock (GraphicsDevice._d3dContext)
            {
                GraphicsDevice._d3dContext.ResolveSubresource(
                    GetMSTexture(),
                    0,
                    GetTexture(),
                    CalculateSubresourceIndex(arraySlice, 0),
                    SharpDXHelper.ToFormat(_format));
            }
        }

        private SharpDX.Direct3D11.Texture2D GetMSTexture()
        {
            if (_msTexture == null)
                _msTexture = CreateMSTexture();

            return _msTexture;
        }

        private SharpDX.Direct3D11.Texture2D CreateMSTexture()
        {
            Texture2DDescription description = new Texture2DDescription
            {
                Width = size,
                Height = size,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDXHelper.ToFormat(_format),
                BindFlags = BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                SampleDescription = _msSampleDescription,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None
            };

            return new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, description);
        }

        private int CalculateSubresourceIndex(int arraySlice, int level)
        {
            return arraySlice * _levelCount + level;
        }
    }
}
