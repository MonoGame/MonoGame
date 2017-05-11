﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.GtkSharp.Drawing;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    static partial class Gtk3Wrapper
    {
        public const string giolibpath = "libgio-2.0.so.0";

        [DllImport(giolibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_file_query_info(IntPtr gfile, string attributes, int flag, IntPtr cancelable, IntPtr error);

        [DllImport(giolibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_file_new_for_path(string path);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool gtk_application_prefers_app_menu(IntPtr application);
    }

    static partial class Global
    {
        private static IconTheme _theme;

        private static void PlatformInit()
        {
            Linux = true;
            _theme = IconTheme.Default;

            var iconMissing = _theme.LoadIcon("dialog-error", 16, 0);
            var file = _theme.LoadIcon("text-x-generic", 16, 0);
            var fileMissing = file.Copy();
            iconMissing.Composite(fileMissing, 8, 8, 8, 8, 8, 8, 0.5, 0.5, Gdk.InterpType.Tiles, 255);
            var folder = _theme.LoadIcon("folder", 16, 0);
            var folderMissing = folder.Copy();
            iconMissing.Composite(folderMissing, 8, 8, 8, 8, 8, 8, 0.5, 0.5, Gdk.InterpType.Tiles, 255);

            _files["."] = ToEtoImage(file);
            _fileMissing = ToEtoImage(fileMissing);
            _folder = ToEtoImage(folder);
            _folderMissing = ToEtoImage(folderMissing);

            _xwtFiles["."] = ToXwtImage(file);
            _xwtFileMissing = ToXwtImage(fileMissing);
            _xwtFolder = ToXwtImage(folder);
            _xwtFolderMissing = ToXwtImage(folderMissing);
        }

        private static Gdk.Pixbuf PlatformGetFileIcon(string path)
        {
            Gdk.Pixbuf icon = null;

            var info = new GLib.FileInfo(Gtk3Wrapper.g_file_query_info(Gtk3Wrapper.g_file_new_for_path(path), "standard::*", 0, new IntPtr(), new IntPtr()));
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

            if (icon == null)
                throw new Exception();

            return icon;
        }

        private static Eto.Drawing.Image ToEtoImage(Gdk.Pixbuf icon)
        {
            return new Bitmap(new BitmapHandler(icon));
        }

        private static Xwt.Drawing.Image ToXwtImage(Gdk.Pixbuf icon)
        {
            Xwt.Drawing.Image ret;

            var icon2 = new Gdk.Pixbuf(icon.Colorspace, true, icon.BitsPerSample, icon.Width + 1, icon.Height);
            icon2.Fill(0);
            icon.Composite(icon2, 0, 0, icon.Width, icon.Height, 0, 0, 1, 1, Gdk.InterpType.Tiles, 255);

            using (var stream = new MemoryStream(icon2.SaveToBuffer("png")))
            {
                stream.Position = 0;
                ret = Xwt.Drawing.Image.FromStream(stream);
            }

            return ret;
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

