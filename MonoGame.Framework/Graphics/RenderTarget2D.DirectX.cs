// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D11.Resource;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        internal RenderTargetView[] _renderTargetViews;
        internal DepthStencilView _depthStencilView;
        private RenderTarget2D _resolvedTexture;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            GenerateIfRequired();
        }

        private void GenerateIfRequired()
        {
            if (_renderTargetViews != null)
                return;

            // Create a view interface on the rendertarget to use on bind.
            if (ArraySize > 1)
            {
                _renderTargetViews = new RenderTargetView[ArraySize];
                for (var i = 0; i < ArraySize; i++)
                {
                    var renderTargetViewDescription = new RenderTargetViewDescription();
                    if (GetTextureSampleDescription().Count > 1)
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
                        GraphicsDevice._d3dDevice, GetTexture(),
                        renderTargetViewDescription);
                }
            }
            else
            {
                _renderTargetViews = new[] { new RenderTargetView(GraphicsDevice._d3dDevice, GetTexture()) };
            }

            // If we don't need a depth buffer then we're done.
            if (DepthStencilFormat == DepthFormat.None)
                return;

            // The depth stencil view's multisampling configuration must strictly
            // match the texture's multisampling configuration.  Ignore whatever parameters
            // were provided and use the texture's configuration so that things are
            // guarenteed to work.
            var multisampleDesc = GetTextureSampleDescription();

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
                        Dimension = GetTextureSampleDescription().Count > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D
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
                if (_resolvedTexture != null)
                    SharpDX.Utilities.Dispose(ref _resolvedTexture);
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

        protected internal override SampleDescription CreateSampleDescription()
        {
            return this.GraphicsDevice.GetSupportedSampleDescription
                (SharpDXHelper.ToFormat(this._format), this.MultiSampleCount);
        }

        internal void ResolveSubresource()
        {
            lock (GraphicsDevice._d3dContext)
            {
                GraphicsDevice._d3dContext.ResolveSubresource(
                    this._texture,
                    0,
                    _resolvedTexture._texture,
                    0,
                    SharpDXHelper.ToFormat(_format));
            }
        }

        internal override Resource CreateTexture()
        {
            var rt = base.CreateTexture();

            // MSAA RT needs another non-MSAA texture where it is resolved
            if (SampleDescription.Count > 1)
            {
                _resolvedTexture = new RenderTarget2D(
                    GraphicsDevice,
                    Width,
                    Height,
                    Mipmap,
                    Format,
                    DepthStencilFormat,
                    1,
                    RenderTargetUsage,
                    Shared,
                    ArraySize);
            }

            return rt;
        }

        protected override ShaderResourceView CreateShaderResourceView()
        {
            if (MultiSampleCount > 1)
                return new SharpDX.Direct3D11.ShaderResourceView
                (GraphicsDevice._d3dDevice, _resolvedTexture.GetTexture());
            else
                return base.CreateShaderResourceView();
        }

        protected internal override Texture2DDescription GetTexture2DDescription()
        {
            var desc = base.GetTexture2DDescription();

            desc.BindFlags |= BindFlags.RenderTarget;
            if (desc.SampleDescription.Count > 1)
                desc.BindFlags &= ~BindFlags.ShaderResource;

            if (Mipmap)
            {
                // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                // because generation of mipmaps is not supported on the Xbox 360.
                // TODO: New method Texture.GenerateMipMaps() required.
                desc.OptionFlags |= ResourceOptionFlags.GenerateMipMaps;
            }

            return desc;
        }
    }
}
