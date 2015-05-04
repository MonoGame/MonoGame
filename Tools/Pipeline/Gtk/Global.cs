using System;
using System.Diagnostics;
using Gdk;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public class Global
    {
        //by default this should be set to whatever Gtk libs we provide with Mac
        public static double GtkMajorVersion = 2;
        public static double GtkMinorVersion = 24;

        //indicates which desktop enviorment is currenlly in use
        public static string DesktopEnvironment = "OSX";

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
        }
    }

    public class IconCache
    {
        static IconTheme theme = IconTheme.Default;

        public static Pixbuf GetFolderIcon()
        {
            return theme.LoadIcon("folder", 16, (IconLookupFlags)0);
        }

        public static Pixbuf GetIcon(string fileName)
        {
            Pixbuf icon = new Pixbuf(null, "MonoGame.Tools.Pipeline.Icons.blueprint.png");

            #if GTK3
            GLib.FileInfo info = new GLib.FileInfo(Gtk3Wrapper.g_file_query_info(Gtk3Wrapper.g_file_new_for_path(fileName), "standard::*", 0, new IntPtr(), new IntPtr()));

            try
            {
                string[] sicon = info.Icon.ToString().Split(' ');

                for(int i = sicon.Length - 1;i >= 0;i--)
                {
                    icon = theme.LoadIcon(sicon[i], 16, (IconLookupFlags)0);

                    if(icon != null)
                        i = 2;
                }
            }
            catch { }
            #endif

            return icon;
        }
    }
}

