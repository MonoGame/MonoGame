// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework
{
    public static partial class TitleContainer
    {
        static partial void PlatformInit();

        static TitleContainer() 
        {
            Location = string.Empty;
            PlatformInit();
        }

        static internal string Location { get; private set; }

        /// <summary>
        /// Returns an open stream to an exsiting file in the title storage area.
        /// </summary>
        /// <param name="name">The filepath relative to the title storage area.</param>
        /// <returns>A open stream or null if the file is not found.</returns>
        public static Stream OpenStream(string name)
        {
            // Normalize the file path.
            var safeName = GetFilename(name);

            // We do not accept absolute paths here.
            if (Path.IsPathRooted(safeName))
                throw new ArgumentException("Invalid filename. TitleContainer.OpenStream requires a relative path.");

            // Call the platform code to open the stream.
            return PlatformOpenStream(name, safeName);
        }

        // TODO: This is just path normalization.  Remove this
        // and replace it with a proper utility function.  I'm sure
        // this same logic is duplicated all over the code base.
        internal static string GetFilename(string name)
        {
            return FileHelpers.NormalizeFilePathSeparators(new Uri("file:///" + name).LocalPath.Substring(1));
        }
    }
}

