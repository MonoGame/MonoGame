// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Interop;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

internal partial class EffectResource
{
    const string AlphaTestEffectName = "AlphaTestEffect";
    const string BasicEffectName = "BasicEffect";
    const string DualTextureEffectName = "DualTextureEffect";
    const string EnvironmentMapEffectName = "EnvironmentMapEffect";
    const string SkinnedEffectName = "SkinnedEffect";
    const string SpriteEffectName = "SpriteEffect";

    private static unsafe byte[] PlatformGetBytecode(string name)
    {
        byte* data;
        int size;
        fixed (byte* n = System.Text.Encoding.UTF8.GetBytes(name + '\0'))
            MGG.EffectResource_GetBytecode(n, &data, &size);

        var bytecode = new byte[size];
        Marshal.Copy((IntPtr)data, bytecode, 0, size);
        return bytecode;
    }
}
