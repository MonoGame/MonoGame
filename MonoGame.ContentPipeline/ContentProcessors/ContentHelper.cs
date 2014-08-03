using System;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGameContentProcessors
{
    static public class ContentHelper
    {
        /// <summary>
        /// Returns the next Power of Two for the given value. If x = 3, then this returns 4.
        /// If x = 4 then 4 is returned. If the value is a power of two, then the same value
        /// is returned.
        /// </summary>
        /// <param name="x">The base of the POT test</param>
        /// <returns>The next power of 2 (1, 2, 4, 8, 16, 32, 64, 128, etc)</returns>
        static public int NextPOT(int x)
        {
            x = x - 1;
            x = x | (x >> 1);
            x = x | (x >> 2);
            x = x | (x >> 4);
            x = x | (x >> 8);
            x = x | (x >> 16);
            return x + 1;
        }

        static public MonoGamePlatform GetMonoGamePlatform()
        {
            // Stock XNA uses a hardcoded enum for platforms... this disallows 
            // us from extending it for MonoGame platforms.  
            //
            // To work around this we currently depend on an environment variable.
            //
            // In the future when MonoGame has the full content pipeline 
            // implemented this hack will not be nessasary.

            var platform = Environment.GetEnvironmentVariable("MONOGAME_PLATFORM", EnvironmentVariableTarget.User);

            // If the platform is unset then this isn't a MonoGame platform we're building for.
            if (string.IsNullOrEmpty(platform))
                return MonoGamePlatform.None;

            // Else return the right platform identifer.
            switch (platform.ToUpperInvariant())
            {
                case "WINDOWS":
                    return MonoGamePlatform.Windows;
                case "WINDOWS8":
                    return MonoGamePlatform.Windows8;
                case "IOS":
                    return MonoGamePlatform.iOS;
                case "ANDROID":
                    return MonoGamePlatform.Android;
                case "LINUX":
                    return MonoGamePlatform.Linux;
                case "OSX":
                    return MonoGamePlatform.OSX;
                case "PSM":
                    return MonoGamePlatform.PSM;

                default:
                    throw new PipelineException("Unexpected MonoGame platform '{0}'!", platform);
            }
        }
    }
}
