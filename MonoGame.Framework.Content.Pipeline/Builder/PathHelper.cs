// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

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
        /// Returns null if 'fileName' does not exist.
        /// Returns false if exists and the full path is identical.
        /// Returns true if exists and does not match in case.
        /// </summary>
        public static bool? FileExistsWithDifferentCase(string fileName)
        {
            // jcf: if i use Normalize() instead, i'd have to also use it on the results from Directory.GetFiles...
            fileName = NormalizeWindows(fileName);

            bool? result = null;
            if (File.Exists(fileName))
            {
                result = false;
                string directory = Path.GetDirectoryName(fileName);
                string fileTitle = Path.GetFileName(fileName);
                string[] files = Directory.GetFiles(directory, fileTitle);
                if (String.Compare(files[0], fileName, false) != 0)
                    result = true;
            }
            return result;
        }

        /// <summary>
        /// Returns null if 'directoryName' does not exist.
        /// Returns false if exists and the full path is identical.
        /// Returns true if exists and does not match in case.
        /// </summary>
        public static bool? DirectoryExistsWithDifferentCase(string directoryName)
        {
			// jcf: if i use Normalize() instead, i'd have to also use it on the results from Directory.GetFiles...
            directoryName = NormalizeWindows(directoryName);

            bool? result = null;
            if (Directory.Exists(directoryName))
            {
                result = false;
                directoryName = directoryName.TrimEnd(Path.DirectorySeparatorChar);

                int lastPathSeparatorIndex = directoryName.LastIndexOf(Path.DirectorySeparatorChar);
                if (lastPathSeparatorIndex >= 0)
                {
                    string baseDirectory = directoryName.Substring(lastPathSeparatorIndex + 1);
                    string parentDirectory = directoryName.Substring(0, lastPathSeparatorIndex);

                    string[] directories = Directory.GetDirectories(parentDirectory, baseDirectory);
                    if (String.Compare(directories[0], directoryName, false) != 0)
                        result = true;
                }
                else
                {
                    //if directory is a drive
                    directoryName += Path.DirectorySeparatorChar.ToString();
                    DriveInfo[] drives = DriveInfo.GetDrives();
                    foreach (DriveInfo driveInfo in drives)
                    {
                        if (String.Compare(driveInfo.Name, directoryName, true) == 0)
                        {
                            if (String.Compare(driveInfo.Name, directoryName, false) != 0)
                                result = true;
                            break;
                        }
                    }

                }
            }
            return result;
        }
    }
}
