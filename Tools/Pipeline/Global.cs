// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        public static string NotAllowedCharacters
        {
            get
            {
                if (Unix)
                    return Linux ? "/" : ":";

                return "/?<>\\:*|\"";
            }
        }

        public static bool Linux { get; private set; }
        public static bool UseHeaderBar { get; set; }
        public static bool Unix { get; private set; }

        private static Dictionary<string, Image> _files;
        private static Image _fileMissing, _folder, _folderMissing;

#if WINDOWS || LINUX
        private static Dictionary<string, Xwt.Drawing.Image> _xwtFiles;
        private static Xwt.Drawing.Image _xwtFileMissing, _xwtFolder, _xwtFolderMissing;
#endif

        static Global()
        {
            Unix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;

            _files = new Dictionary<string, Image>();
            _files.Add(".", Bitmap.FromResource("TreeView.File.png"));
            _fileMissing = Bitmap.FromResource("TreeView.FileMissing.png");
            _folder = Bitmap.FromResource("TreeView.Folder.png");
            _folderMissing = Bitmap.FromResource("TreeView.FolderMissing.png");

#if WINDOWS || LINUX
            _xwtFiles = new Dictionary<string, Xwt.Drawing.Image>();
            _xwtFiles.Add(".", Xwt.Drawing.Image.FromResource("TreeView.File.png"));
            _xwtFileMissing = Xwt.Drawing.Image.FromResource("TreeView.FileMissing.png");
            _xwtFolder = Xwt.Drawing.Image.FromResource("TreeView.Folder.png");
            _xwtFolderMissing = Xwt.Drawing.Image.FromResource("TreeView.FolderMissing.png");
#endif

            PlatformInit();
        }

        public static bool CheckString(string s)
        {
            var notAllowed = Path.GetInvalidFileNameChars();

            for (int i = 0; i < notAllowed.Length; i++)
                if (s.Contains(notAllowed[i].ToString()))
                    return false;

            return true;
        }

        public static Image GetEtoDirectoryIcon(bool exists)
        {
            return exists ? _folder : _folderMissing;
        }

        public static Image GetEtoFileIcon(string path, bool exists)
        {
            if (!exists)
                return _fileMissing;
            
            var ext = Path.GetExtension(path);
            if (_files.ContainsKey(ext))
                return _files[ext];

            Image icon;

            try
            {
                icon = ToEtoImage(PlatformGetFileIcon(path));
            }
            catch
            {
                icon = _files["."];
            }

            _files.Add(ext, icon);
            return icon;
        }

#if WINDOWS || LINUX
        public static Xwt.Drawing.Image GetXwtDirectoryIcon(bool exists)
        {
            return exists ? _xwtFolder : _xwtFolderMissing;
        }

        public static Xwt.Drawing.Image GetXwtFileIcon(string path, bool exists)
        {
            if (!exists)
                return _xwtFileMissing;

            var ext = Path.GetExtension(path);
            if (_xwtFiles.ContainsKey(ext))
                return _xwtFiles[ext];

            Xwt.Drawing.Image icon;

            try
            {
                icon = ToXwtImage(PlatformGetFileIcon(path));
            }
            catch
            {
                icon = _xwtFiles["."];
            }

            _xwtFiles.Add(ext, icon);
            return icon;
        }
#endif

        public static Image GetEtoIcon(string resource)
        {
#if LINUX
            var nativeicon = PlatformGetIcon(resource);

            if (nativeicon != null)
                return ToEtoImage(nativeicon);
#endif

            return Icon.FromResource(resource);
        }

#if WINDOWS || LINUX
        public static Xwt.Drawing.Image GetXwtIcon(string resource)
        {
#if LINUX
            var nativeicon = PlatformGetIcon(resource);

            if (nativeicon != null)
                return ToXwtImage(nativeicon);
#endif
            
            return Xwt.Drawing.Image.FromResource(resource);
        }
#endif
    }
}

