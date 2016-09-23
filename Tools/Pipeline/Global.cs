// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        public static bool Linux { get; private set; }
        public static bool UseHeaderBar { get; private set; }
        public static bool Unix { get; private set; }

        static Global()
        {
            Unix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;

            PlatformInit();
        }

        public static string NotAllowedCharacters
        {
            get
            {
                if (Global.Unix)
                    return Global.Linux ? "/" : ":";

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

        public static void ShowOpenWithDialog(string filePath)
        {
            try
            {
                PlatformShowOpenWithDialog(filePath);
            }
            catch
            {
                MainWindow.Instance.ShowError("Error", "The current platform does not have this dialog implemented.");
            }
        }

        public static Image GetEtoDirectoryIcon(bool exists)
        {
#if WINDOWS || LINUX
            try
            {
                return ToEtoImage(PlatformGetDirectoryIcon(exists));
            }
            catch { }
#endif

            return exists ? Bitmap.FromResource("TreeView.Folder.png") : Bitmap.FromResource("TreeView.FolderMissing.png");
        }

        public static Image GetEtoFileIcon(string path, bool exists)
        {
#if WINDOWS || LINUX
            try
            {
                return ToEtoImage(PlatformGetFileIcon(path, exists));
            }
            catch { }
#endif

            return exists ? Bitmap.FromResource("TreeView.File.png") : Bitmap.FromResource("TreeView.FileMissing.png");
        }

        public static Xwt.Drawing.Image GetXwtDirectoryIcon(bool exists)
        {
#if WINDOWS || LINUX
            try
            {
                return ToXwtImage(PlatformGetDirectoryIcon(exists));
            }
            catch { }
#endif

            return exists ? Xwt.Drawing.Image.FromResource("TreeView.Folder.png") : Xwt.Drawing.Image.FromResource("TreeView.FolderMissing.png");
        }

        public static Xwt.Drawing.Image GetXwtFileIcon(string path, bool exists)
        {
#if WINDOWS || LINUX
            try
            {
                return ToXwtImage(PlatformGetFileIcon(path, exists));
            }
            catch { }
#endif

            return exists ? Xwt.Drawing.Image.FromResource("TreeView.File.png") : Xwt.Drawing.Image.FromResource("TreeView.FileMissing.png");
        }

        public static Image GetEtoIcon(string resource)
        {
#if LINUX
            var nativeicon = PlatformGetIcon(resource);

            if (nativeicon != null)
                return ToEtoImage(nativeicon);
#endif

            return Icon.FromResource(resource);
        }

        public static Xwt.Drawing.Image GetXwtIcon(string resource)
        {
#if LINUX
            var nativeicon = PlatformGetIcon(resource);

            if (nativeicon != null)
                return ToXwtImage(nativeicon);
#endif
            
            return Xwt.Drawing.Image.FromResource(resource);
        }
    }
}

