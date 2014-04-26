// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class SamplerState
    {
        private SharpDX.Direct3D11.SamplerState _state;

        protected internal override void GraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _state);
            base.GraphicsDeviceResetting();
        }

        internal SharpDX.Direct3D11.SamplerState GetState(GraphicsDevice device)
        {
            if (_state == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                GraphicsDevice = device;

                // Build the description.
                var desc = new SharpDX.Direct3D11.SamplerStateDescription();

                desc.AddressU = GetAddressMode(AddressU);
                desc.AddressV = GetAddressMode(AddressV);
                desc.AddressW = GetAddressMode(AddressW);

                desc.Filter = GetFilter(Filter);
                desc.MaximumAnisotropy = MaxAnisotropy;
                desc.MipLodBias = MipMapLevelOfDetailBias;

                // TODO: How do i do these?
                desc.MinimumLod = 0.0f;
                desc.BorderColor = new SharpDX.Color4(0, 0, 0, 0);

                // To support feature level 9.1 these must 
                // be set to these exact values.
                desc.MaximumLod = float.MaxValue;
                desc.ComparisonFunction = SharpDX.Direct3D11.Comparison.Never;

                // Create the state.
                _state = new SharpDX.Direct3D11.SamplerState(GraphicsDevice._d3dDevice, desc);
            }

            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            return _state;
        }

        private static SharpDX.Direct3D11.Filter GetFilter(TextureFilter filter)
        {
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

                default:
                    throw new ArgumentException("Invalid texture address mode!");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                SharpDX.Utilities.Dispose(ref _state);
            base.Dispose(disposing);
        }
    }
}

