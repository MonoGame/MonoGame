// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        static partial void PlatformInit()
        {
#if WINDOWS || DESKTOPGL
#if DESKTOPGL
            // Check for the package Resources Folder first. This is where the assets
            // will be bundled.
            if (CurrentPlatform.OS == OS.MacOSX)
                Location = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "..", "Resources");
            if (!Directory.Exists (Location))
#endif
            Location = AppDomain.CurrentDomain.BaseDirectory;
#endif
        }

        private static Stream PlatformOpenStream(string safeName)
        {
            var absolutePath = Path.Combine(Location, safeName);
            return File.OpenRead(absolutePath);
        }
    }
}

