// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    /// <summary>
    /// Common file and path helpers.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// A forward slash.
        /// </summary>
        public static readonly char ForwardSlash = '/';

        /// <summary>
        /// A backward slash.
        /// </summary>
        public static readonly char BackwardSlash = '\\';

        /// <summary>
        /// The directory separator that is not common for the current platform.
        /// </summary>
        public static readonly char NotSeparator = Path.DirectorySeparatorChar == BackwardSlash ? ForwardSlash : BackwardSlash;

        /// <summary>
        /// The required or default directory separator for this platform.
        /// </summary>
        public static readonly char Separator = Path.DirectorySeparatorChar;

        /// <summary>
        /// Fixes the incoming path to have the default directory separators for the current platform.
        /// </summary>
        public static string NormalizeDirectorySeparators(string path) => path.Replace(NotSeparator, Separator);

        /// <summary>
        /// Checks  deletes a file from disk without throwing exceptions.
        /// </summary>
        public static void DeleteIfExists(string? path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
