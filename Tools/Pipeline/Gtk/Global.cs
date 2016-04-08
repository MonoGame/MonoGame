using System;
using System.Diagnostics;
using Gdk;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        public static Application App;

        //by default this should be set to whatever Gtk libs we provide with Mac
        public static uint GtkMajorVersion = 2;
        public static uint GtkMinorVersion = 24;

        public static void Initalize()
        {
#if GTK3
            GtkMajorVersion = Gtk.Global.MajorVersion;
            GtkMinorVersion = Gtk.Global.MinorVersion;

            if(UseHeaderBar)
            {
                /* Load Global Menu, I have no idea how to connect events to it so it's disabled for now.
                 * 
                 * App = new Application("MonoGame.Pipeline", GLib.ApplicationFlags.None);
                 * App.Register(GLib.Cancellable.Current);
                 * 
                 * var builder = new Builder(null, "MonoGame.Tools.Pipeline.Gtk.MainWindow.HeaderBar.glade", null);
                 * Gtk3Wrapper.gtk_application_set_app_menu(Global.App.Handle, builder.GetObject("appmenu").Handle);
                */
            }
#endif
        }

        public static IntPtr GetNewDialog(IntPtr parrent)
        {
#if GTK3
            if(UseHeaderBar)
                return Gtk3Wrapper.gtk_dialog_new_with_buttons("", parrent, 4 + (int)DialogFlags.Modal);
#endif

            return (new Dialog("", new Gtk.Window(parrent), DialogFlags.Modal)).Handle;
        }

        public static ToolButton GetToolButton(Gtk.Action action, string resource)
        {
            var ret = (ToolButton)action.CreateToolItem();
            ret.IconWidget = new Gtk.Image(null, resource);
            ret.Label = ret.Label.TrimEnd(new [] { '.' });
            ret.TooltipText = ret.Label;

            return ret;
        }

        public static ToggleToolButton GetToggleToolButton(Gtk.Action action, string resource)
        {
            var ret = (ToggleToolButton)action.CreateToolItem();
            ret.IconWidget = new Gtk.Image(null, resource);
            ret.Label = ret.TooltipText = ret.Label;

            return ret;
        }
    }

    public static class IconCache
    {
        static IconTheme theme = IconTheme.Default;

        public static Pixbuf GetFolderIcon()
        {
#if GTK3
            return theme.LoadIcon("folder", 16, (IconLookupFlags)0);
#else
            return new Pixbuf(null, "MonoGame.Tools.Pipeline.Icons.folder_open.png");
#endif
        }

        public static Pixbuf GetIcon(string fileName)
        {
#if GTK3
            var info = new GLib.FileInfo(Gtk3Wrapper.g_file_query_info(Gtk3Wrapper.g_file_new_for_path(fileName), "standard::*", 0, new IntPtr(), new IntPtr()));

            try
            {
                string[] sicon = info.Icon.ToString().Split(' ');

                for(int i = sicon.Length - 1;i >= 1;i--)
                {
                    try
                    {
                        var icon = theme.LoadIcon(sicon[i], 16, (IconLookupFlags)0);

                        if(icon != null)
                            return icon;
                    }
                    catch { }
                }
            }
            catch { }

            return theme.LoadIcon("text-x-generic", 16, (IconLookupFlags)0);
#else
            return new Pixbuf(null, "MonoGame.Tools.Pipeline.Icons.blueprint.png");
#endif
        }
    }
}

