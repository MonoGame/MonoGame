﻿// MonoGame - Copyright (C) The MonoGame Team
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

            var file = ExtractIcon(0).ToBitmap();
            var fileMissing = ExtractIcon(271).ToBitmap();
            var folder = ExtractIcon(4).ToBitmap();
            var folderMissing = ExtractIcon(234).ToBitmap();

            _files["."] = ToEtoImage(file);
            _fileMissing = ToEtoImage(fileMissing);
            _folder = ToEtoImage(folder);
            _folderMissing = ToEtoImage(folderMissing);

            _xwtFiles["."] = ToXwtImage(file);
            _xwtFileMissing = ToXwtImage(fileMissing);
            _xwtFolder = ToXwtImage(folder);
            _xwtFolderMissing = ToXwtImage(folderMissing);
        }

        public static System.Drawing.Icon ExtractIcon(int number)
        {
            IntPtr large;
            IntPtr small;
            ExtractIconExW("shell32.dll", number, out large, out small, 1);

            return System.Drawing.Icon.FromHandle(large);
        }

        private static System.Drawing.Bitmap PlatformGetFileIcon(string path)
        {
            return System.Drawing.Icon.ExtractAssociatedIcon(path).ToBitmap();
        }

        private static Bitmap ToEtoImage(System.Drawing.Bitmap bitmap)
        {
            var ret = new BitmapImage();

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                ret.BeginInit();
                ret.StreamSource = stream;
                ret.CacheOption = BitmapCacheOption.OnLoad;
                ret.EndInit();
            }

            return new Bitmap(new BitmapHandler(ret));
        }

        private static Xwt.Drawing.Image ToXwtImage(System.Drawing.Bitmap bitmap)
        {
            Xwt.Drawing.Image ret;

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                ret = Xwt.Drawing.Image.FromStream(stream);
            }
           
            return ret.Scale(0.5);
        }

        private static void PlatformShowOpenWithDialog(string filePath)
        {
            var args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
            Process.Start("rundll32.exe", args + ",OpenAs_RunDLL " + filePath);
        }
    }
}

