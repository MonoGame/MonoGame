// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
#if IOS
using Foundation;
using UIKit;
#elif MONOMAC
using Foundation;
#endif

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        private static string CacheLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CACHE_PATH);

        static partial void PlatformInit()
        {
            Location = NSBundle.MainBundle.ResourcePath;
            SupportRetina = UIScreen.MainScreen.Scale >= 2.0f;
            RetinaScale = (int)Math.Round(UIScreen.MainScreen.Scale);
        }

        static internal bool SupportRetina { get; private set; }
        static internal int RetinaScale { get; private set; }

        private static Stream PlatformOpenStream(string safeName)
        {
            var cachePath = Path.Combine(CacheLocation, safeName);
            if (File.Exists(cachePath))
            {
                return File.OpenRead(cachePath);
            }

            var absolutePath = Path.Combine(Location, safeName);
            if (SupportRetina)
            {
                for (var scale = RetinaScale; scale >= 2; scale--)
                {
                    // Insert the @#x immediately prior to the extension. If this file exists
                    // and we are on a Retina device, return this file instead.
                    var absolutePathX = Path.Combine(Path.GetDirectoryName(absolutePath),
                                                      Path.GetFileNameWithoutExtension(absolutePath)
                                                      + "@" + scale + "x" + Path.GetExtension(absolutePath));
                    if (File.Exists(absolutePathX))
                        return File.OpenRead(absolutePathX);
                }
            }
            if (File.Exists(absolutePath))
            {
                return File.OpenRead(absolutePath);
            }

            return null;
        }

        private static Stream PlatformOpenWriteStream(string safeName)
        {
            var absolutePath = Path.Combine(CacheLocation, safeName);
            var dirPath = Path.GetDirectoryName(absolutePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            return File.OpenWrite(absolutePath);
        }
    }
}

