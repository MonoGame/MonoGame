// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;
using Eto.Wpf.Drawing;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        public static bool IsWindows10 { get; set; }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconExW(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        private static void PlatformInit()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            IsWindows10 = (reg.GetValue("ProductName") as string).StartsWith("Windows 10");
        }

        public static System.Drawing.Icon ExtractIcon(int number)
        {
            IntPtr large;
            IntPtr small;
            ExtractIconExW("shell32.dll", number, out large, out small, 1);

            return System.Drawing.Icon.FromHandle(large);
        }

        private static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var ret = new BitmapImage();

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                ret.BeginInit();
                ret.StreamSource = memory;
                ret.CacheOption = BitmapCacheOption.OnLoad;
                ret.EndInit();
            }

            return ret;
        }

        private static Image PlatformGetDirectoryIcon(bool exists)
        {
            System.Drawing.Bitmap icon;

            if (exists)
                icon = ExtractIcon(4).ToBitmap();
            else
                icon = ExtractIcon(234).ToBitmap();

            return new Bitmap(new BitmapHandler(Convert(icon)));
        }

        private static Image PlatformGetFileIcon(string path, bool exists)
        {
            System.Drawing.Bitmap icon;

            if (exists)
            {
                try
                {
                    icon = System.Drawing.Icon.ExtractAssociatedIcon(path).ToBitmap();
                }
                catch
                {
                    icon = ExtractIcon(0).ToBitmap();
                }
            }
            else
                icon = ExtractIcon(271).ToBitmap();

            return new Bitmap(new BitmapHandler(Convert(icon)));
        }

        private static void PlatformShowOpenWithDialog(string filePath)
        {
            var args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
            Process.Start("rundll32.exe", args + ",OpenAs_RunDLL " + filePath);
        }

        private static bool PlatformSetIcon(Command cmd)
        {
            return false;
        }
    }
}

