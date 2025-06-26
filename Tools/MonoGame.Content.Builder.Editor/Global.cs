// MonoGame - Copyright (C) MonoGame Foundation, Inc
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
                return "/?<>\\:*|\"";
            }
        }

        public static bool Linux { get; private set; }
        public static bool UseHeaderBar { get; set; }
        public static bool Unix { get; private set; }
        public static bool IsGtk { get; private set; }

        private static Dictionary<string, Bitmap> _files;
        private static Image _folder;
        private static Bitmap _link;

        static Global()
        {
            Unix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;

            _link = Bitmap.FromResource("TreeView.Link.png");
            _files = new Dictionary<string, Bitmap>();
            _files.Add("0.", Bitmap.FromResource("TreeView.File.png"));

            _folder = Bitmap.FromResource("TreeView.Folder.png");

            PlatformInit();

            // Generate default link file image
            var linkfile = new Bitmap(_files["0."]);
            var g = new Graphics(linkfile);
            g.DrawImage(_link, Point.Empty);
            g.Flush();
            _files.Add("1.", linkfile);
        }

        public static bool CheckString(string s)
        {
            var notAllowed = Path.GetInvalidFileNameChars();

            for (int i = 0; i < notAllowed.Length; i++)
                if (s.Contains(notAllowed[i].ToString()))
                    return false;

            return true;
        }

        public static Image GetEtoDirectoryIcon()
        {
            return _folder;
        }

        public static Image GetEtoFileIcon(string path, bool link)
        {
            var key = (link ? '1' : '0') + (File.Exists(path) ? Path.GetExtension(path) : ".");
            if (_files.ContainsKey(key))
                return _files[key];
            
            try
            {
                if (File.Exists(path))
                {
                    var platformicon = PlatformGetFileIcon(path);

                    if (platformicon != null)
                    {
                        var icon = ToEtoImage(platformicon);

                        if (icon != null)
                        {
                            if (link)
                            {
                                var g = new Graphics(icon);
                                g.DrawImage(_link, Point.Empty);
                                g.Flush();
                            }

                            _files.Add(key, icon);
                            return icon;
                        }
                    }
                }
            }
            catch { }

            return _files[(link) ? "1." : "0."];
        }

        public static Image GetEtoIcon(string resource)
        {
#if GTK
            var nativeicon = PlatformGetIcon(resource);

            if (nativeicon != null)
                return ToEtoImage(nativeicon);
#endif

            return Icon.FromResource(resource);
        }

        public static T Show<T>(this Eto.Forms.Dialog<T> dialog, Eto.Forms.Control parent)
        {
#if IDE
            return dialog.ShowModal(null);
#else
            return dialog.ShowModal(parent);
#endif
        }

        public static Eto.Forms.DialogResult Show(this Eto.Forms.CommonDialog dialog, Eto.Forms.Control parent)
        {
#if IDE
            return dialog.ShowDialog(null);
#else
            return dialog.ShowDialog(parent);
#endif
        }
    }
}

