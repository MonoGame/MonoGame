// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        public static string DesktopEnvironment { get; private set; }
        public static bool UseHeaderBar { get; private set; }

        static Global()
        {
#if LINUX
            Global.DesktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
            UseHeaderBar = Gtk.Global.MajorVersion >= 3 && Gtk.Global.MinorVersion >= 16 && Global.DesktopEnvironment == "GNOME";
#else
            DesktopEnvironment = "OSX";
#endif
        }

        public static string NotAllowedCharacters
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    if (Global.DesktopEnvironment == "OSX")
                        return ":";
                    
                    return "/";
                }

                return "/?<>\\:*|\"";

            }
        }

        public static bool CheckString(string s)
        {
            var notAllowed = Path.GetInvalidFileNameChars();

            for (int i = 0; i < notAllowed.Length; i++)
                if (s.Contains(notAllowed[i].ToString()))
                    return false;

            return true;
        }
    }
}
