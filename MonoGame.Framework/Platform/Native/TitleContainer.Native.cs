// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.IO;
using MonoGame.Interop;


namespace Microsoft.Xna.Framework;

partial class TitleContainer
{

    static partial void PlatformInit()
    {
    }

    private static Stream PlatformOpenStream(string safeName)
    {
        var absolutePath = MGP.Platform_MakePath(Location, safeName);
        return MG.OpenRead(absolutePath);
    }
}
