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

        byte* _name = stackalloc byte[StringInterop.GetMaxSize(name)];
        StringInterop.CopyString(_name, name);
        MGG.EffectResource_GetBytecode(_name, out data, out size);

        var bytecode = new byte[size];
        Marshal.Copy((IntPtr)data, bytecode, 0, size);
        return bytecode;
    }
}
