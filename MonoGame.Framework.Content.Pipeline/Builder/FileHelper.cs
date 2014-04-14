// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    public static class FileHelper
    {
        /// <summary>
        /// Checks  deletes a file from disk without throwing exceptions.
        /// </summary>
        public static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
