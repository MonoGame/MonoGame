using System;
using System.Diagnostics;
using Gdk;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public static class Global
    {
        //by default this should be set to whatever Gtk libs we provide with Mac
        public static double GtkMajorVersion = 2;
        public static double GtkMinorVersion = 24;

        //indicates which desktop enviorment is currenlly in use
        public static string DesktopEnvironment = "OSX";

        public static bool UseHeaderBar;

        public static void Initalize()
        {
            #if LINUX
            GtkMajorVersion = Gtk3Wrapper.gtk_get_major_version();
            GtkMinorVersion = Gtk3Wrapper.gtk_get_minor_version();

            Process proc = new Process ();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \"echo $XDG_CURRENT_DESKTOP\"";
            proc.StartInfo.UseShellExecute = false; 
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start ();

            while (!proc.StandardOutput.EndOfStream) {
                string line = proc.StandardOutput.ReadLine ();
                DesktopEnvironment = line;
            }
            #endif

            UseHeaderBar = Global.GtkMajorVersion >= 3 && Global.GtkMinorVersion >= 12 && Global.DesktopEnvironment == "GNOME";
        }

        public static IntPtr GetNewDialog(IntPtr parrent)
        {
            #if GTK3
            if(UseHeaderBar)
                return Gtk3Wrapper.gtk_dialog_new_with_buttons("", parrent, 4 + (int)DialogFlags.Modal);
            #endif

            return (new Dialog("", new Gtk.Window(parrent), DialogFlags.Modal)).Handle;
        }
    }

    public static class IconCache
    {
        static IconTheme theme = IconTheme.Default;

        public static Pixbuf GetFolderIcon()
        {
            #if GTK3
            return theme.LoadIcon("folder", 16, (IconLookupFlags)0);
            #endif
            return new Pixbuf(null, "MonoGame.Tools.Pipeline.Icons.folder_open.png");
        }

        public static Pixbuf GetIcon(string fileName)
        {
            #if GTK3
            GLib.FileInfo info = new GLib.FileInfo(Gtk3Wrapper.g_file_query_info(Gtk3Wrapper.g_file_new_for_path(fileName), "standard::*", 0, new IntPtr(), new IntPtr()));

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
            #endif

            return new Pixbuf(null, "MonoGame.Tools.Pipeline.Icons.blueprint.png");
        }
    }
}

