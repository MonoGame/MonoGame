// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using System;

namespace MonoGame.Utilities
{
    /// <summary>
    /// Contains information on the current platform the app is running from.
    /// </summary>
    public static class CurrentPlatform
    {
        [DllImport ("libc")]
        static extern int uname (IntPtr buf);

        /// <summary>
        /// Gets the operating system that the app is running on.
        /// </summary>
        public static OS OS { get; private set; }

        static CurrentPlatform ()
        {
#if DESKTOPGL
            var pid = Environment.OSVersion.Platform;

            switch (pid) {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    OS = OS.Windows;
                    break;
                case PlatformID.MacOSX:
                    OS = OS.MacOSX;
                    break;
                case PlatformID.Unix:

                    // Mac can return a value of Unix sometimes, We need to double check it.
                    var buf = IntPtr.Zero;
                    try {
                        buf = Marshal.AllocHGlobal (8192);

                        if (uname (buf) == 0) {
                            var sos = Marshal.PtrToStringAnsi (buf);

                            if (sos == "Darwin") {
                                OS = OS.MacOSX;
                                return;
                            }
                        }
                    } catch {
                    } finally {
                        if (buf != IntPtr.Zero)
                            Marshal.FreeHGlobal (buf);
                    }

                    OS = OS.Linux;
                    break;
                default:
                    OS = OS.Unknown;
                    break;
            }
#elif (WINDOWS && DIRECTX) || WINRT
            OS = OS.Windows;
#elif TVOS
            OS = OS.TvOS;
#elif ANDROID
            OS = OS.Android;
#elif IOS
            OS = OS.iOS;
#elif WINDOWS_PHONE && WINDOWS_PHONE81
            OS = OS.WindowsPhone
#else
            OS = OS.Unknown;
#endif
        }
    }
}

