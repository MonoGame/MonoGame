// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        const string WindowsNotAllowedCharacters = "/?<>\\:*|\"";
        const string LinuxNotAllowedCharacters = "/";
        const string MacNotAllowedCharacters = ":";

        public static string NotAllowedCharacters
        {
            get
            {
#if WINDOWS
                return WindowsNotAllowedCharacters;
#else
                if (Global.DesktopEnvironment == "OSX")
                    return MacNotAllowedCharacters;
                else
                    return LinuxNotAllowedCharacters;
#endif
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
