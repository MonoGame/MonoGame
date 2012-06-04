using System;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGameContentProcessors
{
    static public class ContentHelper
    {
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
            switch (platform.ToUpper())
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
                case "PSS":
                    return MonoGamePlatform.PSS;

                default:
                    throw new PipelineException("Unexpected MonoGame platform '{0}'!", platform);
            }
        }
    }
}
