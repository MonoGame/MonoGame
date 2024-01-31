// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.GtkSharp.Drawing;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        public static Application Application;
        
        private static IconTheme _theme;

        private static void PlatformInit()
        {
            IsGtk = true;
            Linux = true;
            UseHeaderBar = true;
            _theme = IconTheme.Default;

            var linkIcon = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 16, 16);
            linkIcon.Fill(0x00000000);
            _theme.LoadIcon("emblem-symbolic-link", 16, 0).Composite(linkIcon, 8, 8, 8, 8, 8, 8, 0.5, 0.5, Gdk.InterpType.Tiles, 255);

            _files["0."] = ToEtoImage(_theme.LoadIcon("text-x-generic", 16, 0));
            _folder = ToEtoImage(_theme.LoadIcon("folder", 16, 0));
            _link = ToEtoImage(linkIcon);
        }

        private static Gdk.Pixbuf PlatformGetFileIcon(string path)
        {
            Gdk.Pixbuf icon = null;

            var file = GLib.FileFactory.NewForPath(path);
            var info = file.QueryInfo("standard::*", GLib.FileQueryInfoFlags.None, null);
            var sicon = info.Icon.ToString().Split(' ');

            for (int i = sicon.Length - 1; i >= 1; i--)
            {
                try
                {
                    icon = _theme.LoadIcon(sicon[i], 16, 0);
                    if (icon != null)
                        break;
                }
                catch { }
            }

            return icon;
        }

        private static Bitmap ToEtoImage(Gdk.Pixbuf icon)
        {
            return new Bitmap(new BitmapHandler(icon));
        }

        private static Gdk.Pixbuf PlatformGetIcon(string resource)
        {
            IconInfo iconInfo = null;
            Gdk.Pixbuf icon = null;

            try
            {
                switch (resource)
                {
                    case "Commands.New.png":
                        iconInfo = _theme.LookupIcon("document-new", 16, 0);
                        break;
                    case "Commands.Open.png":
                        iconInfo = _theme.LookupIcon("document-open", 16, 0);
                        break;
                    case "Commands.Close.png":
                        iconInfo = _theme.LookupIcon("window-close", 16, 0);
                        break;
                    case "Commands.Save.png":
                        iconInfo = _theme.LookupIcon("document-save", 16, 0);
                        break;
                    case "Commands.SaveAs.png":
                        iconInfo = _theme.LookupIcon("document-save-as", 16, 0);
                        break;
                    case "Commands.Undo.png":
                        iconInfo = _theme.LookupIcon("edit-undo", 16, 0);
                        break;
                    case "Commands.Redo.png":
                        iconInfo = _theme.LookupIcon("edit-redo", 16, 0);
                        break;
                    case "Commands.Delete.png":
                        iconInfo = _theme.LookupIcon("edit-delete", 16, 0);
                        break;
                    case "Commands.NewItem.png":
                        iconInfo = _theme.LookupIcon("document-new", 16, 0);
                        break;
                    case "Commands.NewFolder.png":
                        iconInfo = _theme.LookupIcon("folder-new", 16, 0);
                        break;
                    case "Commands.ExistingItem.png":
                        iconInfo = _theme.LookupIcon("document", 16, 0);
                        break;
                    case "Commands.ExistingFolder.png":
                        iconInfo = _theme.LookupIcon("folder", 16, 0);
                        break;
                    case "Commands.Build.png":
                        iconInfo = _theme.LookupIcon("applications-system", 16, 0);
                        break;
                    case "Commands.Rebuild.png":
                        iconInfo = _theme.LookupIcon("system-run", 16, 0);
                        break;
                    case "Commands.Clean.png":
                        iconInfo = _theme.LookupIcon("edit-clear-all", 16, 0);
                        if (iconInfo == null)
                            iconInfo = _theme.LookupIcon("edit-clear", 16, 0);
                        break;
                    case "Commands.CancelBuild.png":
                        iconInfo = _theme.LookupIcon("process-stop", 16, 0);
                        break;
                    case "Commands.Help.png":
                        iconInfo = _theme.LookupIcon("system-help", 16, 0);
                        break;
                        
                    case "Build.Information.png":
                        iconInfo = _theme.LookupIcon("dialog-information", 16, 0);
                        break;
                    case "Build.Fail.png":
                        iconInfo = _theme.LookupIcon("dialog-error", 16, 0);
                        break;
                    case "Build.Processing.png":
                        iconInfo = _theme.LookupIcon("preferences-system-time", 16, 0);
                        break;
                    case "Build.Skip.png":
                        iconInfo = _theme.LookupIcon("emblem-default", 16, 0);
                        break;
                    case "Build.Start.png":
                        iconInfo = _theme.LookupIcon("system-run", 16, 0);
                        break;
                    case "Build.EndSucceed.png":
                        iconInfo = _theme.LookupIcon("system-run", 16, 0);
                        break;
                    case "Build.EndFailed.png":
                        iconInfo = _theme.LookupIcon("system-run", 16, 0);
                        break;
                    case "Build.Succeed.png":
                        iconInfo = _theme.LookupIcon("emblem-default", 16, 0);
                        break;
                }

                if (iconInfo != null)
                    icon = iconInfo.LoadIcon();
                
                if (resource == "Commands.Rename.png" || resource == "Commands.OpenItem.png")
                    icon = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 1, 1, 1);
            }
            catch { }

            return icon;
        }
    }
}

