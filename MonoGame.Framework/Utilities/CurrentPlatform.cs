// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using System;

namespace MonoGame.Utilities
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
        private static OS os;
        private static int osversion;

        [DllImport ("libc")]
        static extern int uname (IntPtr buf);

        static CurrentPlatform()
        {
            PlatformID pid = Environment.OSVersion.Platform;

            switch (pid) 
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    os = OS.Windows;
                    break;
                case PlatformID.MacOSX:
                    os = OS.MacOSX;
                    break;
                case PlatformID.Unix:

                    // Mac can return a value of Unix sometimes, We need to double check it.
                    IntPtr buf = IntPtr.Zero;
                    try
                    {
                        buf = Marshal.AllocHGlobal (8192);

                        if (uname (buf) == 0) {
                            string sos = Marshal.PtrToStringAnsi (buf);
                            if (sos == "Darwin")
                            {
                                os = OS.MacOSX;
                                return;
                            }
                        }
                    } catch {
                    } finally {
                        if (buf != IntPtr.Zero)
                            Marshal.FreeHGlobal (buf);
                    }

                    os = OS.Linux;
                    break;
                default:
                    os = OS.Unknown;
                    break;
            }

            osversion = Environment.OSVersion.Version.Major * 10 + Environment.OSVersion.Version.Minor;
        }

        public static OS OS
        {
            get
            {
                return os;
            }
        }

        public static int OSVersion
        {
            get
            {
                return osversion;
            }
        }
    }
}

