// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Text;

namespace MonoGame.Utilities
{
    internal static class FileHelpers
    {
        private static readonly char[] UrlSafeChars = new[] { '.', '_', '-', ';', '/', '?', '\\', ':' };

        public static readonly char ForwardSlash = '/';
        public static readonly string ForwardSlashString = new string(ForwardSlash, 1);
        public static readonly char BackwardSlash = '\\';

#if WINDOWS_UAP
        public static readonly char NotSeparator = ForwardSlash;
        public static readonly char Separator = BackwardSlash;
#else
        public static readonly char NotSeparator = Path.DirectorySeparatorChar == BackwardSlash ? ForwardSlash : BackwardSlash;
        public static readonly char Separator = Path.DirectorySeparatorChar;
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
            // Uri accepts forward slashes
            filePath = filePath.Replace(BackwardSlash, ForwardSlash);

            bool hasForwardSlash = filePath.StartsWith(ForwardSlashString);
            if (!hasForwardSlash)
                filePath = ForwardSlashString + filePath;

            // Get a uri for filePath using the file:// schema and no host.
            var src = new Uri("file://" + UrlEncode(filePath));

            var dst = new Uri(src, UrlEncode(relativeFile));

            // The uri now contains the path to the relativeFile with 
            // relative addresses resolved... get the local path.
            var localPath = dst.LocalPath;

            if (!hasForwardSlash && localPath.StartsWith("/"))
                localPath = localPath.Substring(1);

            // Convert the directory separator characters to the 
            // correct platform specific separator.
            return NormalizeFilePathSeparators(localPath);
        }

        private static string UrlEncode(string url)
        {
            var encoder = new UTF8Encoding();
            var safeline = new StringBuilder(encoder.GetByteCount(url) * 3);

            foreach (var c in url)
            {
                if ((c >= 48 && c <= 57) || (c >= 65 && c <= 90) || (c >= 97 && c <= 122) || Array.IndexOf(UrlSafeChars, c) != -1)
                    safeline.Append(c);
                else
                {
                    var bytes = encoder.GetBytes(c.ToString());

                    foreach (var num in bytes)
                    {
                        safeline.Append("%");
                        safeline.Append(num.ToString("X"));
                    }
                }
            }

            return safeline.ToString();
        }
    }
}
