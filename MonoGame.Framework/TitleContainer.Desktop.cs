using System;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    public static partial class TitleContainer
    {
        static TitleContainer() 
        {
            Location = AppDomain.CurrentDomain.BaseDirectory;
        }

        internal static Stream PlatformOpenStream (string safeName)
        {
            var absolutePath = Path.Combine(Location, safeName);
            return File.OpenRead(absolutePath);
        }

        internal static List<string> PlatformGetFiles (string directory, string filter)
        {
            return new List<string> (Directory.GetFiles(directory, filter));
        }
    }
}

