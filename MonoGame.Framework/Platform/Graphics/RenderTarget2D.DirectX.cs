// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        internal RenderTargetView[] _renderTargetViews;
        internal DepthStencilView _depthStencilView;
        private SharpDX.Direct3D11.Texture2D _msTexture;

        private SampleDescription _sampleDescription;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            _sampleDescription = GraphicsDevice.GetSupportedSampleDescription(SharpDXHelper.ToFormat(preferredDepthFormat), preferredMultiSampleCount);
            // set the actual multisample count, not the preferred value
            MultiSampleCount = _sampleDescription.Count;

            GenerateIfRequired();
        }

        private void PlatformFromNativeHandle(GraphicsDevice graphicsDevice, IntPtr handle, SurfaceFormat surfaceFormat, DepthFormat depthFormat, int preferredMultiSampleCount)
        {
            var dxgiFormat = surfaceFormat == SurfaceFormat.Color ? SharpDX.DXGI.Format.B8G8R8A8_UNorm : SharpDXHelper.ToFormat(surfaceFormat);

            var multisampleDesc = new SampleDescription(1, 0);
            if (preferredMultiSampleCount > 1)
            {
                multisampleDesc.Count = preferredMultiSampleCount;
                multisampleDesc.Quality = (int)StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            var d3dDevice = graphicsDevice._d3dDevice;

            SharpDX.Direct3D11.Resource resource = new SharpDX.Direct3D11.Resource(handle);

            _texture = resource;

            RenderTargetViewDescription desc = new RenderTargetViewDescription()
            {
                Format = dxgiFormat,
                Dimension = RenderTargetViewDimension.Texture2D
            };

            _renderTargetViews = new[] { new RenderTargetView(d3dDevice, resource, desc) };

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
                                                                           Width = width,
                                                                           Height = height,
                                                                           SampleDescription = multisampleDesc,
                                                                           Usage = ResourceUsage.Default,
                                                                           BindFlags = BindFlags.DepthStencil,
                                                                       }))

                    // Create a DepthStencil view on this surface to use on bind.
                    _depthStencilView = new DepthStencilView(d3dDevice, depthBuffer);
            }
        }

        private void GenerateIfRequired()
        {
            if (_renderTargetViews != null)
                return;

            if (_texture == null)
                CreateTexture();

            var viewTex = MultiSampleCount > 1 ? _msTexture : _texture;

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
            var multisampleDesc = _sampleDescription;

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

        internal void ResolveSubresource()
        {
            lock (GraphicsDevice._d3dContext)
            {
                GraphicsDevice._d3dContext.ResolveSubresource(
                    _msTexture,
                    0,
                    _texture,
                    0,
                    SharpDXHelper.ToFormat(_format));
            }
        }

        internal override void CreateTexture()
        {
            var desc = GetTexture2DDescription();

            // override sample description with ours
            desc.SampleDescription = _sampleDescription;

            desc.BindFlags |= BindFlags.RenderTarget;
            if (Mipmap)
                desc.OptionFlags |= ResourceOptionFlags.GenerateMipMaps;

            _texture = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc);

            // MSAA RT needs a MSAA texture and a non-MSAA texture where it is resolved
            // we store the resolved texture in _texture and the multi sampled texture
            // in _msTexture when MSAA is enabled
            if (MultiSampleCount > 1)
            {
                desc = GetTexture2DDescription();
                desc.BindFlags |= BindFlags.RenderTarget;
                // the multi sampled texture can never be bound directly
                desc.BindFlags &= ~BindFlags.ShaderResource;
                desc.SampleDescription = _sampleDescription;
                // mip mapping is applied to the resolved texture, not the multisampled texture
                desc.MipLevels = 1;
                var descr = desc;
                _msTexture = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, descr);
            }
        }
    }
}
