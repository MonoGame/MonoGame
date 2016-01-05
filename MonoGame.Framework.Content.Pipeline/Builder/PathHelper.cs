// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    public static class PathHelper
    {
        /// <summary>
        /// The/universal/standard/directory/seperator.
        /// </summary>
        public const char DirectorySeparator = '/';

        /// <summary>
        /// Returns a path string normalized to the/universal/standard.
        /// </summary>
        public static string Normalize(string path)
        {
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Returns a directory path string normalized to the/universal/standard
        /// with a trailing seperator.
        /// </summary>
        public static string NormalizeDirectory(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/') + '/';
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
        /// Returns a path string normalized to the current platform standard.
        /// </summary>
        /// <param name="path">Path to normalize</param>
        /// <param name="targetPlatform">The platform to normalize for</param>
        /// <returns>The normalized path</returns>
        public static string NormalizeOS(string path, TargetPlatform targetPlatform)
        {
            switch (targetPlatform)
            {
                case TargetPlatform.Windows:
                case TargetPlatform.DesktopGL:
                case TargetPlatform.WindowsPhone8:
                case TargetPlatform.WindowsStoreApp:
                    return NormalizeWindows(path);

                default:
                    return Normalize(path);
            }
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

            return Normalize(str);
        }

        /// <summary>
        /// Returns a path relative to the base path.
        /// </summary>
        /// <param name="basePath">The path to make relative to.  Must end with directory seperator.</param>
        /// <param name="path">The path to be made relative to the basePath.</param>
        /// <param name="targetPlatform">The platform to normalize the path for.</param>
        /// <returns>The relative path or the original string if it is not absolute or cannot be made relative.</returns>
        public static string GetRelativePath(string basePath, string path, TargetPlatform targetPlatform)
        {
            Uri uri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out uri))
                return path;

            uri = new Uri(basePath).MakeRelativeUri(uri);
            var str = Uri.UnescapeDataString(uri.ToString());

            return NormalizeOS(str, targetPlatform);
        }
    }
}
