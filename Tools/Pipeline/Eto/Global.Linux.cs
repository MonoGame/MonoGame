using System;
using Eto.Drawing;
using Eto.GtkSharp.Drawing;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        static IconTheme theme = IconTheme.Default;

        static Bitmap PlatformGetDirectoryIcon()
        {
            var icon = theme.LoadIcon("folder", 16, 0);
            return new Bitmap(new BitmapHandler(icon));
        }

        static Bitmap PlatformGetFileIcon(string path)
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
                        icon = theme.LoadIcon(sicon[i], 16, 0);
                        if (icon != null)
                            break;
                    }
                    catch { }
                }
            }
            catch { }

            if (icon == null)
                icon = theme.LoadIcon("text-x-generic", 16, 0);

            return new Bitmap(new BitmapHandler(icon));
        }
    }
}

