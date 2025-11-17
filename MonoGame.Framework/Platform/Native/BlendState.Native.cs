// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System.Runtime.CompilerServices;
namespace Microsoft.Xna.Framework.Graphics;

public partial class BlendState
{
    internal unsafe MGG_BlendState* Handle;

    internal unsafe void PlatformApplyState(GraphicsDevice device)
    {
        // Check if uninitialized
        if (Handle == null)
        {
            // Convert state data to interop variant
            var info = new MGG_BlendState_Info[4];

            if (_independentBlendEnable)
            {
                info[0] = CreateInfo(_targetBlendState[0]);
                info[1] = CreateInfo(_targetBlendState[1]);
                info[2] = CreateInfo(_targetBlendState[2]);
                info[3] = CreateInfo(_targetBlendState[3]);
            }
            else
            {
                // All share the same
                info[0] = info[1] = info[2] = info[3] = CreateInfo(_targetBlendState[0]);
            }

            // Create native blend state
            fixed (MGG_BlendState_Info* ptr = info)
            {
                Handle = MGG.BlendState_Create(device.Handle, ptr);
            }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MGG_BlendState_Info CreateInfo(TargetBlendState state) => new()
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
