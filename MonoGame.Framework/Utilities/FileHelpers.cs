// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
    }
}
