// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics;


public partial class SamplerState
{
    private unsafe MGG_SamplerState* Handle;

    private unsafe void Destroy()
    {
        if (Handle != null)
        {
            MGG.SamplerState_Destroy(GraphicsDevice.Handle, Handle);
            Handle = null;
        }
    }

    /// <inheritdoc/>
    protected unsafe internal override void GraphicsDeviceResetting()
    {
        Destroy();

        base.GraphicsDeviceResetting();
    }

    internal unsafe MGG_SamplerState* GetHandle(GraphicsDevice device)
    {
        Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

        if (Handle != null)
            return Handle;

        var info = new MGG_SamplerState_Info();
        info.AddressU = AddressU;
        info.AddressV = AddressV;
        info.AddressW = AddressW;
        info.BorderColor = BorderColor.PackedValue;
        info.Filter = Filter;
        info.FilterMode = FilterMode;
        info.MaximumAnisotropy = Math.Min(MaxAnisotropy, device.GraphicsCapabilities.MaxTextureAnisotropy);
        info.MaxMipLevel = MaxMipLevel;
        info.MipMapLevelOfDetailBias = MipMapLevelOfDetailBias;
        info.ComparisonFunction = ComparisonFunction;

        Handle = MGG.SamplerState_Create(device.Handle, &info);

        return Handle;
    }

    partial void PlatformDispose()
    {
        Destroy();
    }
}

