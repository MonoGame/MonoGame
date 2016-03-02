using System;
using System.IO;
using System.Collections.Generic;
#if IOS
using Foundation;
using UIKit;
#endif

namespace Microsoft.Xna.Framework
{
    public static partial class TitleContainer
    {
        static internal bool SupportRetina { get; private set; }
        static internal int RetinaScale { get; private set; }

         static TitleContainer() 
        {
            Location = NSBundle.MainBundle.ResourcePath;
            SupportRetina = UIScreen.MainScreen.Scale >= 2.0f;
            RetinaScale = (int)Math.Round(UIScreen.MainScreen.Scale);
        }

        internal static Stream PlatformOpenStream (string safeName)
        {
            var absolutePath = Path.Combine(Location, safeName);
            if (SupportRetina)
            {
                for (var scale = RetinaScale; scale >= 2; scale--)
                {
                    // Insert the @#x immediately prior to the extension. If this file exists
                    // and we are on a Retina device, return this file instead.
                    var absolutePathX = Path.Combine(Path.GetDirectoryName(absolutePath),
                        Path.GetFileNameWithoutExtension(absolutePath)
                        + "@" + scale + "x" + Path.GetExtension(absolutePath));
                    if (File.Exists(absolutePathX))
                        return File.OpenRead(absolutePathX);
                }
            }
            return File.OpenRead(absolutePath);
        }

        internal static List<string> PlatformGetFiles (string directory, string filter)
        {
            return new List<string> (Directory.GetFiles(directory, filter));
        }
    }
}

