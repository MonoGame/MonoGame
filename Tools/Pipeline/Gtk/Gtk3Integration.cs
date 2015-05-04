using System.Runtime.InteropServices;
using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public class Gtk3Wrapper
    {
        public const string gtklibpath = "libgtk-3.so.0";

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_color_chooser_dialog_new (string title, IntPtr parent);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_color_chooser_get_rgba (IntPtr chooser, out Gdk.RGBA color);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_color_chooser_set_rgba (IntPtr chooser, double[] color);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gtk_get_major_version ();

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gtk_get_minor_version ();
    }

    public class ColorChooserDialog : Dialog
    {
        public ColorChooser ColorChooser;

        public ColorChooserDialog (Window parrent, string title) : base(Gtk3Wrapper.gtk_color_chooser_dialog_new (title, parrent.Handle))
        {
            ColorChooser = new ColorChooser(this.Handle);
        }
    }

    public class ColorChooser : Widget
    {
        public Gdk.RGBA CurrentRgba
        {
            get
            {
                Gdk.RGBA rgba;
                Gtk3Wrapper.gtk_color_chooser_get_rgba(this.Handle, out rgba);
                return rgba;
            }
            set
            {
                Gtk3Wrapper.gtk_color_chooser_set_rgba(this.Handle, new double[] { value.Red, value.Green, value.Blue, value.Alpha });
            }
        }

        public ColorChooser(IntPtr native) : base(native) { }
    }
}

