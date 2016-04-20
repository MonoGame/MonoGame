// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;
using Eto.WinForms.Drawing;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconExW(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        private static void PlatformInit()
        {

        }

        public static System.Drawing.Icon ExtractIcon(int number)
        {
            IntPtr large;
            IntPtr small;
            ExtractIconExW("shell32.dll", number, out large, out small, 1);

            return System.Drawing.Icon.FromHandle(large);
        }

        private static Image PlatformGetDirectoryIcon(bool exists)
        {
            System.Drawing.Bitmap icon;

            if(exists)
                icon = ExtractIcon(4).ToBitmap();
            else
                icon = ExtractIcon(234).ToBitmap();

            return new Bitmap(new BitmapHandler(icon));
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

            return new Bitmap(new BitmapHandler(icon));
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

