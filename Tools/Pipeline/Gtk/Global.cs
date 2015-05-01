using System;
using System.Diagnostics;

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
}

