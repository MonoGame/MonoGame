using System;
using System.IO;
using Microsoft.Xna.Framework.Utilities;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    public static partial class TitleContainer
    {
        static TitleContainer() 
        {
            Location = String.Empty;
        }

        internal static Stream PlatformOpenStream (string safeName)
        {
            return Android.App.Application.Context.Assets.Open(safeName);
        }

        internal static List<string> PlatformGetFiles (string directory, string filter)
        {
            var results = new List<string>();
            var files = Game.Activity.Assets.List (directory);
            for (int i=files.Length; i > 0; i--)
            {
                var ext = Path.GetExtension (files[i]);
                if (filter.EndsWith ( filter.EndsWith ("*") ? filter : ext))
                {
                    results.Add(files[i]);
                }
            }
            return results;
        }
    }
}

