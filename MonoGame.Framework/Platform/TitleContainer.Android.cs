// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        private static string CacheLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CACHE_PATH);

        private static Stream PlatformOpenStream(string safeName)
        {
            var cachePath = Path.Combine(CacheLocation, safeName);
            if (File.Exists(cachePath))
            {
                return File.OpenRead(cachePath);
            }

            try
            {
                return Android.App.Application.Context.Assets.Open(safeName);
            }
            catch
            {
                return null;
            }
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

