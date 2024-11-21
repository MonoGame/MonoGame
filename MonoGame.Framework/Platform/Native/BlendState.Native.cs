// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
namespace Microsoft.Xna.Framework.Graphics;

public partial class BlendState
{
    internal unsafe MGG_BlendState* Handle;

    internal unsafe void PlatformApplyState(GraphicsDevice device)
    {
        if (Handle == null)
        {
            var info = new MGG_BlendState_Info[4];
            for (int i = 0; i < 4; i++)
            {
                var state = _targetBlendState[i];

                info[i] = new MGG_BlendState_Info()
                {
                    colorSourceBlend = state.ColorSourceBlend,
                    colorDestBlend = state.ColorDestinationBlend,
                    colorBlendFunc = state.ColorBlendFunction,
                    alphaSourceBlend = state.AlphaSourceBlend,
                    alphaDestBlend = state.AlphaDestinationBlend,
                    alphaBlendFunc = state.AlphaBlendFunction,
                    colorWriteChannels = state.ColorWriteChannels
                };
            }

            fixed (MGG_BlendState_Info* ptr = info)
                Handle = MGG.BlendState_Create(device.Handle, ptr);
        }

        MGG.GraphicsDevice_SetBlendState(device.Handle, Handle, _blendFactor.R / 255.0f, _blendFactor.G / 255.0f, _blendFactor.B / 255.0f, _blendFactor.A / 255.0f);
    }

    partial void PlatformDispose()
    {
        unsafe
        {
            if (Handle != null)
            {
                MGG.BlendState_Destroy(GraphicsDevice.Handle, Handle);
                Handle = null;
            }
        }
    }
}
