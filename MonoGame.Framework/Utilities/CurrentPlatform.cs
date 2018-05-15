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
        private static bool init = false;
        private static OS os;

        [DllImport ("libc")]
        static extern int uname (IntPtr buf);

        private static void Init()
        {
            if (!init)
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

                init = true;
            }
        }

        public static OS OS
        {
            get
            {
                Init();
                return os;
            }
        }

    }

    public static class PlatformParameters
    {
        /// <summary>
        /// If true, MonoGame will detect the CPU architecture (x86 or x86-64) and add the "./x86" or "./x64" folder to the
        /// native DLL resolution paths. This allows MonoGame to work with Any CPU by loading the correct dependencies at runtime.
        /// If false, MonoGame will look for native DLLs in the executing folder, which typically is the .exe home.
        /// 
        /// This parameter only works on Windows and doesn't affect other platforms.
        /// </summary>
        public static bool DetectWindowsArchitecture = (CurrentPlatform.OS == OS.Windows ? true : false);
    }

    internal static class NativeHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
        
        private static bool _dllDirectorySet = false;
        
        public static void InitDllDirectory()
        {
            if (CurrentPlatform.OS == OS.Windows && !_dllDirectorySet)
            {
                string executingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (Environment.Is64BitProcess)
                {
                    NativeHelper.SetDllDirectory(System.IO.Path.Combine(executingDirectory, "x64"));
                }
                else
                {
                    NativeHelper.SetDllDirectory(System.IO.Path.Combine(executingDirectory, "x86"));
                }
                
                _dllDirectorySet = true;
            }
        }
    }
}

