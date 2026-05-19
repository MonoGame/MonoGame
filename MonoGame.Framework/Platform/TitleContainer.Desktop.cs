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
            // Check for the package Resources Folder first. This is where the assets will be bundled.
            if (CurrentPlatform.OS == OS.MacOSX)
            {
                Location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources");
                if (!Directory.Exists(Location))
                    Location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources");
            }
            if (string.IsNullOrEmpty(Location) || !Directory.Exists(Location))
            {
                Location = AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        private static Stream PlatformOpenStream(string safeName)
        {
            var absolutePath = Path.Combine(Location, safeName);
            if (File.Exists(absolutePath))
            {
                return File.OpenRead(absolutePath);
            }

            return null;
        }
    }
}
