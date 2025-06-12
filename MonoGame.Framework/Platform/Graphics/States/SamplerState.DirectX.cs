// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class SamplerState
    {
        private SharpDX.Direct3D11.SamplerState _state;

        /// <inheritdoc />
        protected internal override void GraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _state);
            base.GraphicsDeviceResetting();
        }

        internal SharpDX.Direct3D11.SamplerState GetState(GraphicsDevice device)
        {
            if (_state == null)
            {
                // Build the description.
                var desc = new SharpDX.Direct3D11.SamplerStateDescription();

                desc.AddressU = GetAddressMode(AddressU);
                desc.AddressV = GetAddressMode(AddressV);
                desc.AddressW = GetAddressMode(AddressW);
				desc.BorderColor = BorderColor.ToColor4();
				desc.Filter = GetFilter(Filter, FilterMode);
                desc.MaximumAnisotropy = Math.Min(MaxAnisotropy, device.GraphicsCapabilities.MaxTextureAnisotropy);
                desc.MipLodBias = MipMapLevelOfDetailBias;
                desc.ComparisonFunction = ComparisonFunction.ToComparison();

                // TODO: How do i do this?
                desc.MinimumLod = 0.0f;

                // To support feature level 9.1 these must
                // be set to these exact values.
                desc.MaximumLod = float.MaxValue;

                // Create the state.
                _state = new SharpDX.Direct3D11.SamplerState(GraphicsDevice._d3dDevice, desc);
            }

            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            return _state;
        }

        private static SharpDX.Direct3D11.Filter GetFilter(TextureFilter filter, TextureFilterMode mode)
        {
            switch (mode)
            {
                case TextureFilterMode.Comparison:
                    switch (filter)
                    {
                        case TextureFilter.Anisotropic:
                            return SharpDX.Direct3D11.Filter.ComparisonAnisotropic;

                        case TextureFilter.Linear:
                            return SharpDX.Direct3D11.Filter.ComparisonMinMagMipLinear;

                        case TextureFilter.LinearMipPoint:
                            return SharpDX.Direct3D11.Filter.ComparisonMinMagLinearMipPoint;

                        case TextureFilter.MinLinearMagPointMipLinear:
                            return SharpDX.Direct3D11.Filter.ComparisonMinLinearMagPointMipLinear;

                        case TextureFilter.MinLinearMagPointMipPoint:
                            return SharpDX.Direct3D11.Filter.ComparisonMinLinearMagMipPoint;

                        case TextureFilter.MinPointMagLinearMipLinear:
                            return SharpDX.Direct3D11.Filter.ComparisonMinPointMagMipLinear;

                        case TextureFilter.MinPointMagLinearMipPoint:
                            return SharpDX.Direct3D11.Filter.ComparisonMinPointMagLinearMipPoint;

                        case TextureFilter.Point:
                            return SharpDX.Direct3D11.Filter.ComparisonMinMagMipPoint;

                        case TextureFilter.PointMipLinear:
                            return SharpDX.Direct3D11.Filter.ComparisonMinMagPointMipLinear;

                        default:
                            throw new ArgumentException("Invalid texture filter!");
                    }
                case TextureFilterMode.Default:
                    switch (filter)
                    {
                        case TextureFilter.Anisotropic:
                            return SharpDX.Direct3D11.Filter.Anisotropic;

                        case TextureFilter.Linear:
                            return SharpDX.Direct3D11.Filter.MinMagMipLinear;

                        case TextureFilter.LinearMipPoint:
                            return SharpDX.Direct3D11.Filter.MinMagLinearMipPoint;

                        case TextureFilter.MinLinearMagPointMipLinear:
                            return SharpDX.Direct3D11.Filter.MinLinearMagPointMipLinear;

                        case TextureFilter.MinLinearMagPointMipPoint:
                            return SharpDX.Direct3D11.Filter.MinLinearMagMipPoint;

                        case TextureFilter.MinPointMagLinearMipLinear:
                            return SharpDX.Direct3D11.Filter.MinPointMagMipLinear;

                        case TextureFilter.MinPointMagLinearMipPoint:
                            return SharpDX.Direct3D11.Filter.MinPointMagLinearMipPoint;

                        case TextureFilter.Point:
                            return SharpDX.Direct3D11.Filter.MinMagMipPoint;

                        case TextureFilter.PointMipLinear:
                            return SharpDX.Direct3D11.Filter.MinMagPointMipLinear;

                        default:
                            throw new ArgumentException("Invalid texture filter!");
                    }
                default:
                    throw new ArgumentException("Invalid texture filter mode!");
            }
        }

        private static SharpDX.Direct3D11.TextureAddressMode GetAddressMode(TextureAddressMode mode)
        {
            switch (mode)
            {
                case TextureAddressMode.Clamp:
                    return SharpDX.Direct3D11.TextureAddressMode.Clamp;

                case TextureAddressMode.Mirror:
                    return SharpDX.Direct3D11.TextureAddressMode.Mirror;

                case TextureAddressMode.Wrap:
                    return SharpDX.Direct3D11.TextureAddressMode.Wrap;

                case TextureAddressMode.Border:
                    return SharpDX.Direct3D11.TextureAddressMode.Border;

                default:
                    throw new ArgumentException("Invalid texture address mode!");
            }
        }

        partial void PlatformDispose()
        {
            SharpDX.Utilities.Dispose(ref _state);
        }
    }
}

