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
            _files.Add(".", Bitmap.FromResource("TreeView.File.png"));
            _fileMissing = Bitmap.FromResource("TreeView.FileMissing.png");
            _folder = Bitmap.FromResource("TreeView.Folder.png");
            _folderMissing = Bitmap.FromResource("TreeView.FolderMissing.png");

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

        public static Image GetEtoIcon(string resource)
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

