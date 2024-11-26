// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

public partial class RasterizerState
{
    internal unsafe MGG_RasterizerState* Handle;

    internal unsafe void PlatformApplyState(GraphicsDevice device)
    {
        if (Handle == null)
        {
            MGG_RasterizerState_Info info;
            info.fillMode = FillMode;
            info.cullMode = CullMode;
            info.scissorTestEnable = ScissorTestEnable;
            info.depthClipEnable = DepthClipEnable;
            info.depthBias = DepthBias;
            info.slopeScaleDepthBias = SlopeScaleDepthBias;
            info.multiSampleAntiAlias = MultiSampleAntiAlias;

            Handle = MGG.RasterizerState_Create(device.Handle, &info);
        }

        MGG.GraphicsDevice_SetRasterizerState(device.Handle, Handle);
    }

    partial void PlatformDispose()
    {
        unsafe
        {
            if (Handle != null)
            {
                MGG.RasterizerState_Destroy(GraphicsDevice.Handle, Handle);
                Handle = null;
            }
        }
    }
}
