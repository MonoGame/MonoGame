// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
namespace Microsoft.Xna.Framework.Graphics;

public partial class DepthStencilState
{
    internal unsafe MGG_DepthStencilState* Handle;

    internal unsafe void PlatformApplyState(GraphicsDevice device)
    {
        if (Handle == null)
        {
            MGG_DepthStencilState_Info info;
            info.depthBufferEnable = DepthBufferEnable;
            info.depthBufferWriteEnable = DepthBufferWriteEnable;
            info.depthBufferFunction = DepthBufferFunction;
            info.referenceStencil = ReferenceStencil;
            info.stencilEnable = StencilEnable;
            info.stencilMask = StencilMask;
            info.stencilWriteMask = StencilWriteMask;
            info.stencilFunction = StencilFunction;
            info.stencilDepthBufferFail = StencilDepthBufferFail;
            info.stencilFail = StencilFail;
            info.stencilPass = StencilPass;

            Handle = MGG.DepthStencilState_Create(device.Handle, &info);
        }

        MGG.GraphicsDevice_SetDepthStencilState(device.Handle, Handle);
    }

    partial void PlatformDispose()
    {
        unsafe
        {
            if (Handle != null)
            {
                MGG.DepthStencilState_Destroy(GraphicsDevice.Handle, Handle);
                Handle = null;
            }
        }
    }
}
