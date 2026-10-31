// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Interop;


namespace Microsoft.Xna.Framework;

partial class TitleContainer
{

    static partial void PlatformInit()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Location = Path.Combine(AppContext.BaseDirectory, "..", "Resources");
            if (!Directory.Exists(Location))
            {
                Location = Path.Combine(AppContext.BaseDirectory, "..", "..", "Resources");
            }
        }
        if (string.IsNullOrEmpty(Location) || !Directory.Exists(Location))
        {
            Location = AppContext.BaseDirectory;
        }
    }

    private static Stream PlatformOpenStream(string safeName)
    {
        try
        {
            var absolutePath = MGP.Platform_MakePath(Location, safeName);
            return MG.OpenRead(absolutePath);
        }
        catch
        {
            return null;
        }
    }

    private static Stream PlatformOpenWriteStream(string safeName)
    {
        throw new NotImplementedException();
    }
}
