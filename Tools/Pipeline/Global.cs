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

        static Global()
        {
            Unix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;

            _files = new Dictionary<string, Image>();
            _files.Add(".", Bitmap.FromResource("TreeView.File.png").WithSize(16, 16));
            _fileMissing = Bitmap.FromResource("TreeView.FileMissing.png").WithSize(16, 16);
            _folder = Bitmap.FromResource("TreeView.Folder.png").WithSize(16, 16);
            _folderMissing = Bitmap.FromResource("TreeView.FolderMissing.png").WithSize(16, 16);

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

        public static Image GetDirectoryIcon(bool exists)
        {
            return exists ? _folder : _folderMissing;
        }

        public static Image GetFileIcon(string path, bool exists)
        {
            if (!exists)
                return _fileMissing;
            
            var ext = Path.GetExtension(path);
            if (_files.ContainsKey(ext))
                return _files[ext];

            Image icon;

            try
            {
                icon = ToEtoImage(PlatformGetFileIcon(path)).WithSize(16, 16);
            }
            catch
            {
                icon = _files["."];
            }

            _files.Add(ext, icon);
            return icon;
        }

        public static Image GetIcon(string resource)
        {
#if LINUX
            var nativeicon = PlatformGetIcon(resource);

            if (nativeicon != null)
                return ToEtoImage(nativeicon);
#endif

            return Icon.FromResource(resource);
        }
    }
}

