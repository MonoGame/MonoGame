using System.Runtime.InteropServices;
using System;
using GLib;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public class Gtk3Wrapper
    {
        public const string gtklibpath = "libgtk-3.so.0";
        public const string giolibpath = "libgio-2.0.so.0";

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_color_chooser_dialog_new (string title, IntPtr parent);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_color_chooser_get_rgba (IntPtr chooser, out Gdk.RGBA color);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_color_chooser_set_rgba (IntPtr chooser, double[] color);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_header_bar_new ();

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_header_bar_set_subtitle (IntPtr handle, string text);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern string gtk_header_bar_get_subtitle (IntPtr handle);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_header_bar_set_show_close_button (IntPtr handle, bool show);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool gtk_header_bar_get_show_close_button (IntPtr handle);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_window_set_titlebar (IntPtr window, IntPtr widget);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gtk_get_major_version ();

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gtk_get_minor_version ();

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_dialog_new_with_buttons (string title, IntPtr parent, int flags);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_widget_set_opacity (IntPtr widget, double opacity);

        [DllImport (giolibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_file_new_for_path (string path);

        [DllImport (giolibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_file_query_info (IntPtr gfile, string attributes, int flag, IntPtr cancelable, IntPtr error);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_popover_new (IntPtr relative_to_widget);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_menu_button_new ();

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_menu_button_get_popover (IntPtr menu_button);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_menu_button_set_popover (IntPtr menu_button, IntPtr popover);

        [DllImport (gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_tree_view_set_activate_on_single_click (IntPtr treeview, bool value);
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

    public class HeaderBar : Container
    {
        public string Subtitle
        {
            get
            {
                return Gtk3Wrapper.gtk_header_bar_get_subtitle(this.Handle);
            }
            set
            {
                Gtk3Wrapper.gtk_header_bar_set_subtitle(this.Handle, value);
            }
        }

        public bool ShowCloseButton
        {
            get
            {
                return Gtk3Wrapper.gtk_header_bar_get_show_close_button(this.Handle);
            }
            set
            {
                Gtk3Wrapper.gtk_header_bar_set_show_close_button(this.Handle, value);
            }
        }

        public HeaderBar() : base(Gtk3Wrapper.gtk_header_bar_new()) { }

        public HeaderBar(IntPtr handle) : base(handle) { }

        public void AttachToWindow(Window window)
        {
            Gtk3Wrapper.gtk_window_set_titlebar(window.Handle, this.Handle);
        }
    }

    public class Popover : Bin
    {
        public Popover(Widget relativeWidget) : base(Gtk3Wrapper.gtk_popover_new(relativeWidget.Handle)) { }

        public Popover(IntPtr handle) : base(handle) { }
    }

    public class MenuButton : ToggleButton
    {
        public Popover Popup
        {
            get
            {
                var ret = Gtk3Wrapper.gtk_menu_button_get_popover(this.Handle);
                return new Popover(ret);
            }
            set
            {
                Gtk3Wrapper.gtk_menu_button_set_popover(this.Handle, value.Handle);
            }
        }

        public MenuButton() : base(Gtk3Wrapper.gtk_menu_button_new()) { }

        public MenuButton(IntPtr handle) : base(handle) { }
    }
}

