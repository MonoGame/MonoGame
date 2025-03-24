// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        internal RenderTargetView[] _renderTargetViews;
        internal DepthStencilView _depthStencilView;
        private SharpDX.Direct3D11.Texture2D _msTexture;

        private SampleDescription _msSampleDescription;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            _msSampleDescription = GraphicsDevice.GetSupportedSampleDescription(SharpDXHelper.ToFormat(this.Format), this.MultiSampleCount);

            GenerateIfRequired();
        }

        private void GenerateIfRequired()
        {
            if (_renderTargetViews != null)
                return;

            var viewTex = MultiSampleCount > 1 ? GetMSTexture() : GetTexture();

            // Create a view interface on the rendertarget to use on bind.
            if (ArraySize > 1)
            {
                _renderTargetViews = new RenderTargetView[ArraySize];
                for (var i = 0; i < ArraySize; i++)
                {
                    var renderTargetViewDescription = new RenderTargetViewDescription();
                    if (MultiSampleCount > 1)
                    {
                        renderTargetViewDescription.Dimension = RenderTargetViewDimension.Texture2DMultisampledArray;
                        renderTargetViewDescription.Texture2DMSArray.ArraySize = 1;
                        renderTargetViewDescription.Texture2DMSArray.FirstArraySlice = i;
                    }
                    else
                    {
                        renderTargetViewDescription.Dimension = RenderTargetViewDimension.Texture2DArray;
                        renderTargetViewDescription.Texture2DArray.ArraySize = 1;
                        renderTargetViewDescription.Texture2DArray.FirstArraySlice = i;
                        renderTargetViewDescription.Texture2DArray.MipSlice = 0;
                    }
                    _renderTargetViews[i] = new RenderTargetView(
                        GraphicsDevice._d3dDevice, viewTex, renderTargetViewDescription);
                }
            }
            else
            {
                _renderTargetViews = new[] { new RenderTargetView(GraphicsDevice._d3dDevice, viewTex) };
            }

            // If we don't need a depth buffer then we're done.
            if (DepthStencilFormat == DepthFormat.None)
                return;

            // The depth stencil view's multisampling configuration must strictly
            // match the texture's multisampling configuration.  Ignore whatever parameters
            // were provided and use the texture's configuration so that things are
            // guarenteed to work.
            var multisampleDesc = _msSampleDescription;

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
                        Dimension = MultiSampleCount > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D
                    });
            }
        }

        private void PlatformGraphicsDeviceResetting()
        {
            if (_renderTargetViews != null)
            {
                for (var i = 0; i < _renderTargetViews.Length; i++)
                    _renderTargetViews[i].Dispose();
                _renderTargetViews = null;
            }
            SharpDX.Utilities.Dispose(ref _depthStencilView);
        }

        /// <summary />
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

        RenderTargetView IRenderTarget.GetRenderTargetView(int arraySlice)
        {
            GenerateIfRequired();
            return _renderTargetViews[arraySlice];
        }

        DepthStencilView IRenderTarget.GetDepthStencilView()
        {
            GenerateIfRequired();
            return _depthStencilView;
        }

        internal virtual void ResolveSubresource()
        {
            lock (GraphicsDevice._d3dContext)
            {
                GraphicsDevice._d3dContext.ResolveSubresource(
                    GetMSTexture(),
                    0,
                    GetTexture(),
                    0,
                    SharpDXHelper.ToFormat(_format));
            }
        }

        /// <summary />
        protected internal override Texture2DDescription GetTexture2DDescription()
        {
            var desc = base.GetTexture2DDescription();

            if (MultiSampleCount == 0 || Shared)
                desc.BindFlags |= BindFlags.RenderTarget;

            if (Mipmap)
            {
                desc.OptionFlags |= ResourceOptionFlags.GenerateMipMaps;
            }

            return desc;
        }

        private SharpDX.Direct3D11.Texture2D GetMSTexture()
        {
            if (_msTexture == null)
                _msTexture = CreateMSTexture();

            return _msTexture;
        }

        internal virtual SharpDX.Direct3D11.Texture2D CreateMSTexture()
        {
            var desc = GetMSTexture2DDescription();

            return new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc);
        }

        internal virtual Texture2DDescription GetMSTexture2DDescription()
        {
            var desc = base.GetTexture2DDescription();

            desc.BindFlags |= BindFlags.RenderTarget;
            // the multi sampled texture can never be bound directly
            desc.BindFlags &= ~BindFlags.ShaderResource;
            desc.SampleDescription = _msSampleDescription;
            // mip mapping is applied to the resolved texture, not the multisampled texture
            desc.MipLevels = 1;
            desc.OptionFlags &= ~ResourceOptionFlags.GenerateMipMaps;

            return desc;
        }
    }
}
