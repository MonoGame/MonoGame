// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    public static class PathHelper
    {
        /// <summary>
        /// Returns a path string normalized to the/universal/standard.
        /// </summary>
        public static string Normalize(string path)
        {
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Returns a path string normalized to the\Windows\standard.
        /// </summary>
        public static string NormalizeWindows(string path)
        {
            return path.Replace('/', '\\');
        }

        /// <summary>
        /// Returns a path string normalized to the current platform standard.
        /// </summary>
        public static string NormalizeOS(string path)
        {
#if WINRT
            return NormalizeWindows(path);
#else
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            path = path.Replace('/', Path.DirectorySeparatorChar);
            return path;
#endif
        }

        /// <summary>
        /// Returns a path relative to the base path.
        /// </summary>
        /// <param name="basePath">The path to make relative to.  Must end with directory seperator.</param>
        /// <param name="path">The path to be made relative to the basePath.</param>
        /// <returns>The relative path or the original string if it is not absolute or cannot be made relative.</returns>
        public static string GetRelativePath(string basePath, string path)
        {
            Uri uri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out uri))
                return path;

            uri = new Uri(basePath).MakeRelativeUri(uri);
            var str = Uri.UnescapeDataString(uri.ToString());

            return NormalizeOS(str);
        }
    }
}
