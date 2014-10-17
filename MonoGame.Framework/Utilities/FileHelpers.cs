// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Utilities
{
    internal static class FileHelpers
    {
        public static char ForwardSlash = '/';
        public static char BackwardSlash = '\\';

#if WINRT
        public static char NotSeparator = ForwardSlash;
        public static char Separator = BackwardSlash;
#else
        public static char NotSeparator = Path.DirectorySeparatorChar == BackwardSlash ? ForwardSlash : BackwardSlash;
        public static char Separator = Path.DirectorySeparatorChar;
#endif

        public static string NormalizeFilePathSeparators(string name)
        {
            return name.Replace(NotSeparator, Separator);
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
            // The Uri class can produce incorrect results with 
            // forward slashes on some unix-like systems.  So make 
            // sure we always pass in backslashes for the paths.
            filePath = filePath.Replace(ForwardSlash, BackwardSlash);
            relativeFile = relativeFile.Replace(ForwardSlash, BackwardSlash);

            // Get a uri for filePath using the file:// schema and no host.
            var src = new Uri("file:///" + filePath);

            // Add the relative path to relativeFile.
            var dst = new Uri(src, relativeFile);
            
            // The uri now contains the path to the relativeFile with 
            // relative addresses resolved... get the local path.
            var localPath = dst.LocalPath;

            // We can get an unwanted extra leading slash on some 
            // platforms which we need to trim off.
            if (localPath.StartsWith(@"\\") || localPath.StartsWith("//"))
                localPath = localPath.Substring(1);

            // Convert the directory separator characters to the 
            // correct platform specific separator.
            return NormalizeFilePathSeparators(localPath);
        }
    }
}
