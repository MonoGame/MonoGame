// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Utilities
{
    internal static class FileHelpers
    {
#if WINRT
        public static char notSeparator = '/';
        public static char separator = '\\';
#else
        public static char notSeparator = '\\';
        public static char separator = System.IO.Path.DirectorySeparatorChar;
#endif

        public static string NormalizeFilePathSeparators(string name)
        {
            return name.Replace(notSeparator, separator);
        }

        /// <summary>
        /// Combines the filePath and relativeFile based on relativeFile being a file in the same location as filePath.
        /// Relative directory operators (..) are also resolved
        /// </summary>
        /// <example>"A\B\C.txt","D.txt" becomes "A\B\D.txt"</example>
        /// <example>"A\B\C.txt","..\D.txt" becomes "A\D.txt"</example>
        /// <param name="filePath">Path to the file we are starting from</param>
        /// <param name="relativeFile">Relative location of another file to resolve the path to</param>
        public static string ResolveRelativePath(string filePath, string relativeFile)
        {
            // Get a uri for filePath using the file:// schema and no host
            var src = new Uri("file:///" + filePath);

            // Add the relative path to relativeFile
            var dst = new Uri(src, relativeFile);

            // The uri now contains the path to the relativeFile with relative addresses resolved
            // Get the local path and skip the first character (the path separator)
            return NormalizeFilePathSeparators(dst.LocalPath.Substring(1));
        }
    }
}
