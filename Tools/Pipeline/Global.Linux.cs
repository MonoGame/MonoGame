// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
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
        public static extern IntPtr gtk_app_chooser_dialog_new(IntPtr parrent, int flags, IntPtr file);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool gtk_application_prefers_app_menu(IntPtr application);
    }

    static partial class Global
    {
        private static IconTheme _theme;
        private static Gdk.Pixbuf _iconMissing;
        private static Gtk.Application _app;

        private static void PlatformInit()
        {
            Linux = true;
            _theme = IconTheme.Default;

            try
            {
                _iconMissing = _theme.LoadIcon("dialog-error", 16, 0);
            }
            catch
            {
                _iconMissing = new Gdk.Pixbuf(null, "TreeView.Missing.png");
            }

            if (Gtk.Global.MajorVersion >= 3 && Gtk.Global.MinorVersion >= 16)
            {
                _app = new Gtk.Application(null, GLib.ApplicationFlags.None);
                _app.Register(GLib.Cancellable.Current);

                UseHeaderBar = Gtk3Wrapper.gtk_application_prefers_app_menu(_app.Handle);
            }
        }

        private static void PlatformShowOpenWithDialog(string filePath)
        {
            var adialoghandle = Gtk3Wrapper.gtk_app_chooser_dialog_new(((Gtk.Window)MainWindow.Instance.ControlObject).Handle, 
                                                                       4 + (int)DialogFlags.Modal, 
                                                                       Gtk3Wrapper.g_file_new_for_path(filePath));
            var adialog = new AppChooserDialog(adialoghandle);

            if (adialog.Run() == (int)ResponseType.Ok)
                Process.Start(adialog.AppInfo.Executable, "\"" + filePath + "\"");

            adialog.Destroy();
        }

        private static Eto.Drawing.Image PlatformGetDirectoryIcon(bool exists)
        {
            var icon = _theme.LoadIcon("folder", 16, 0);

            if (!exists)
            {
                icon = icon.Copy();
                _iconMissing.Composite(icon, 8, 8, 8, 8, 8, 8, 0.5, 0.5, Gdk.InterpType.Tiles, 255);
            }
            
            return new Bitmap(new BitmapHandler(icon));
        }

        private static Eto.Drawing.Image PlatformGetFileIcon(string path, bool exists)
        {
            Gdk.Pixbuf icon = null;

            try
            {
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
            }
            catch { }

            if (icon == null)
                icon = _theme.LoadIcon("text-x-generic", 16, 0);


            if (!exists)
            {
                icon = icon.Copy();
                _iconMissing.Composite(icon, 8, 8, 8, 8, 8, 8, 0.5, 0.5, Gdk.InterpType.Tiles, 255);
            }

            return new Bitmap(new BitmapHandler(icon));
        }

        private static bool PlatformSetIcon(Command cmd)
        {
            IconInfo iconInfo = null;
            Gdk.Pixbuf icon = null;

            try
            {
                switch (cmd.MenuText)
                {
                    case "New...":
                        iconInfo = _theme.LookupIcon("document-new-symbolic", 16, 0);
                        break;
                    case "Open...":
                        iconInfo = _theme.LookupIcon("document-open-symbolic", 16, 0);
                        break;
                    case "Save...":
                        iconInfo = _theme.LookupIcon("document-save-symbolic", 16, 0);
                        break;
                    case "Undo":
                        iconInfo = _theme.LookupIcon("edit-undo-symbolic", 16, 0);
                        break;
                    case "Redo":
                        iconInfo = _theme.LookupIcon("edit-redo-symbolic", 16, 0);
                        break;
                    case "New Item...":
                        iconInfo = _theme.LookupIcon("document-new-symbolic", 16, 0);
                        break;
                    case "New Folder...":
                        iconInfo = _theme.LookupIcon("folder-new-symbolic", 16, 0);
                        break;
                    case "Existing Item...":
                        iconInfo = _theme.LookupIcon("folder-documents-symbolic", 16, 0);
                        break;
                    case "Existing Folder...":
                        iconInfo = _theme.LookupIcon("folder-open-symbolic", 16, 0);
                        break;
                    case "Build":
                        iconInfo = _theme.LookupIcon("emblem-system-symbolic", 16, 0);
                        break;
                    case "Rebuild":
                        iconInfo = _theme.LookupIcon("system-run-symbolic", 16, 0);
                        break;
                    case "Cancel Build":
                        iconInfo = _theme.LookupIcon("media-playback-stop-symbolic", 16, 0);
                        break;
                    case "Clean":
                        iconInfo = _theme.LookupIcon("edit-clear-symbolic", 16, 0);
                        break;
                    case "Filter Output":
                        iconInfo = _theme.LookupIcon("format-indent-more-symbolic", 16, 0);
                        break;
                }

                if (iconInfo != null)
                {
                    var colText = SystemColors.ControlText;
                    bool ws;
                    var col = new Gdk.RGBA();
                    col.Red = colText.R;
                    col.Green = colText.G;
                    col.Blue = colText.B;
                    col.Alpha = colText.A;

                    icon = iconInfo.LoadSymbolic(col, col, col, col, out ws);
                }
            }
            catch { }

            if (icon != null)
            {
                cmd.Image = new Bitmap(new BitmapHandler(icon));
                return true;
            }

            return false;
        }
    }
}

