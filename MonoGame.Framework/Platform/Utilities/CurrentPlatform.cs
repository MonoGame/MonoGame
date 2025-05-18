// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using System;

namespace MonoGame.Framework.Utilities
{
    internal enum OS
    {
        Windows,
        Linux,
        MacOSX,
        Unknown
    }

    internal static class CurrentPlatform
    {
        private static bool _init = false;
        private static OS _os;

        private static void Init()
        {
            if (_init)
                return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _os = OS.Windows;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _os = OS.MacOSX;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _os = OS.Linux;
            }
            _init = true;
        }

        public static OS OS
        {
            get
            {
                Init();
                return _os;
            }
        }

        public static string Rid
        {
            get
            {
                if (CurrentPlatform.OS == OS.Windows && Environment.Is64BitProcess)
                    return "win-x64";
                else if (CurrentPlatform.OS == OS.Windows && !Environment.Is64BitProcess)
                    return "win-x86";
                else if (CurrentPlatform.OS == OS.Linux)
                    return "linux-x64";
                else if (CurrentPlatform.OS == OS.MacOSX)
                    return "osx";
                else
                    return "unknown";
            }
        }
    }
}

