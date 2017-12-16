// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Eto;
using Eto.Forms;
using Eto.GtkSharp.Forms;
using Eto.GtkSharp.Forms.Controls;
using Eto.GtkSharp.Forms.ToolBar;

namespace MonoGame.Tools.Pipeline
{
    static partial class Gtk3Wrapper
    {
        public const string gtklibpath = "libgtk-3.so.0";

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_header_bar_new();

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_window_set_titlebar(IntPtr window, IntPtr widget);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_header_bar_pack_start(IntPtr bar, IntPtr child);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_header_bar_pack_end(IntPtr bar, IntPtr child);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_header_bar_set_show_close_button(IntPtr bar, bool setting);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_header_bar_set_subtitle(IntPtr handle, string text);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_application_set_app_menu (IntPtr application, IntPtr app_menu);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_application_add_window (IntPtr application, IntPtr window);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_simple_action_new (string name, IntPtr parameter_type);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void g_action_map_add_action (IntPtr action_map, IntPtr action);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void g_simple_action_set_enabled (IntPtr simple, bool enabled);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_accel_group_new();

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gtk_accel_group_connect (IntPtr accel_group, Gdk.Key accel_key, Gdk.ModifierType accel_mods, Gtk.AccelFlags accel_flags, IntPtr closure);
    
        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_cclosure_new (IntPtr callback_func, IntPtr user_data, IntPtr destroy_data);

        [DllImport(gtklibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gtk_window_add_accel_group (IntPtr window, IntPtr accel_group);
    }

    public class SimpleAction : GLib.Object
    {
        public delegate void ActivateHandler(object o, EventArgs args);
        public event ActivateHandler Activate
        {
            add { AddSignalHandler("activate", value, typeof(EventArgs)); }
            remove { RemoveSignalHandler("activate", value); }
        }

        public bool Enabled
        {
            get { return false; }
            set { Gtk3Wrapper.g_simple_action_set_enabled(Handle, value); }
        }

        public SimpleAction(string name) : base(Gtk3Wrapper.g_simple_action_new(name, IntPtr.Zero))
        {
            
        }
    }

    public class ModalButton : Gtk.Button
    {
        [GLib.Property("active")]
        public bool Active
        {
            set
            {
                this.SetProperty("active", new GLib.Value(value));
            }
        }

        public ModalButton(IntPtr handle) : base(handle) { }
    }

    public static class Styles
    {
        private static IntPtr _actionGroup;
        private static Gtk.Widget _popovermenu1, _popovermenu2;
        private static Gtk.RadioButton _mainbutton;
        private static Gtk.Widget _buttonbox, _cancelbox, _separator;

        [GLib.ConnectBefore]
        public static void TreeView_ButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
        {
            var treeview = o as Gtk.TreeView;

            if (args.Event.Button == 3)
            {
                Gtk.TreeViewDropPosition pos;
                Gtk.TreePath path;
                Gtk.TreeIter iter;

                if (treeview.GetDestRowAtPos((int)args.Event.X, (int)args.Event.Y, out path, out pos) && treeview.Model.GetIter(out iter, path))
                {
                    var paths = treeview.Selection.GetSelectedRows().ToList();
                    if (paths.Contains(path))
                        args.RetVal = true;
                }
            }
        }

        public static void Connect(string action, Command cmd)
        {
            var a = new SimpleAction(action);
            a.Activate += (o, args) => 
            {
                _popovermenu1.Hide();
                _popovermenu2.Hide();
                cmd.Execute();
            };

            cmd.EnabledChanged += (sender, e) => a.Enabled = cmd.Enabled;

            Gtk3Wrapper.g_action_map_add_action(Global.ApplicationHandle, a.Handle);
        }

        private static void Connect(Command cmd, Gdk.Key key, Gdk.ModifierType modifier = Gdk.ModifierType.None)
        {
            var cclosure = Gtk3Wrapper.g_cclosure_new(Marshal.GetFunctionPointerForDelegate(
                (Action<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr>)((IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr data) =>
            {
                var command = ((GCHandle)data).Target as Command;

                if (command.Enabled)
                    command.Execute();
            })), (IntPtr)GCHandle.Alloc(cmd), IntPtr.Zero);

            Gtk3Wrapper.gtk_accel_group_connect(_actionGroup, key, modifier, Gtk.AccelFlags.Mask, cclosure);
        }

        private static void RejectActive(IntPtr handle)
        {
            var rb = new Gtk.RadioButton(handle);
            rb.JoinGroup(_mainbutton);
            rb.Toggled += (sender, e) => 
            {
                if (rb.Active)
                    _mainbutton.Active = true;
            };
        }

        private static void ReloadBuildbox()
        {
            var b = MainWindow.Instance.cmdBuild.Enabled;
            var c = MainWindow.Instance.cmdCancelBuild.Enabled;

            _buttonbox.Visible = b;
            _cancelbox.Visible = c;
            _separator.Visible = b || c;
        }

        public static void Load()
        {
            Style.Add<ApplicationHandler>("PipelineTool", h =>
            {
                Global.ApplicationHandle = h.Control.Handle;

                if (Gtk.Global.MajorVersion >= 3 && Gtk.Global.MinorVersion >= 16)
                    Global.UseHeaderBar = Gtk3Wrapper.gtk_application_prefers_app_menu(h.Control.Handle);
                
                if (Global.UseHeaderBar)
                    Gtk3Wrapper.gtk_application_set_app_menu(h.Control.Handle, (new Gtk.Builder("AppMenu.glade")).GetObject("appmenu").Handle);
            });

            Style.Add<FormHandler>("MainWindow", h =>
            {
                if (!Global.UseHeaderBar)
                    return;
                
                var builder = new Gtk.Builder("MainWindow.glade");
                var headerBar = new Gtk.Widget(builder.GetObject("headerbar").Handle);

                h.Menu = null;
                h.ToolBar = null;

                Connect("new", MainWindow.Instance.cmdNew);
                Connect("open", MainWindow.Instance.cmdOpen);
                Connect("save", MainWindow.Instance.cmdSave);
                Connect("saveas", MainWindow.Instance.cmdSaveAs);
                Connect("import", MainWindow.Instance.cmdImport);
                Connect("close", MainWindow.Instance.cmdClose);
                Connect("help", MainWindow.Instance.cmdHelp);
                Connect("about", MainWindow.Instance.cmdAbout);
                Connect("quit", MainWindow.Instance.cmdExit);
                Connect("undo", MainWindow.Instance.cmdUndo);
                Connect("redo", MainWindow.Instance.cmdRedo);
                Connect("build", MainWindow.Instance.cmdBuild);
                Connect("rebuild", MainWindow.Instance.cmdRebuild);
                Connect("clean", MainWindow.Instance.cmdClean);
                Connect("cancel", MainWindow.Instance.cmdCancelBuild);

                _actionGroup = Gtk3Wrapper.gtk_accel_group_new();

                Connect(MainWindow.Instance.cmdNew, Gdk.Key.N, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdOpen, Gdk.Key.O, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdSave, Gdk.Key.S, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdExit, Gdk.Key.Q, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdUndo, Gdk.Key.Z, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdRedo, Gdk.Key.Y, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdBuild, Gdk.Key.F6);
                Connect(MainWindow.Instance.cmdHelp, Gdk.Key.F1);

                Gtk3Wrapper.gtk_window_add_accel_group(h.Control.Handle, _actionGroup);

                _popovermenu1 = new Gtk.Widget(builder.GetObject("popovermenu1").Handle);
                _popovermenu2 = new Gtk.Widget(builder.GetObject("popovermenu2").Handle);

                Gtk3Wrapper.gtk_window_set_titlebar(h.Control.Handle, headerBar.Handle);
                Gtk3Wrapper.gtk_header_bar_set_show_close_button(headerBar.Handle, true);

                _mainbutton = new Gtk.RadioButton("");
                RejectActive(builder.GetObject("build_button").Handle);
                RejectActive(builder.GetObject("rebuild_button").Handle);
                RejectActive(builder.GetObject("clean_button").Handle);

                _buttonbox = new Gtk.Widget(builder.GetObject("build_buttonbox").Handle);
                _cancelbox = new Gtk.Widget(builder.GetObject("cancel_button").Handle);
                _separator = new Gtk.Widget(builder.GetObject("separator1").Handle);
                MainWindow.Instance.cmdBuild.EnabledChanged += (sender, e) => ReloadBuildbox();
                MainWindow.Instance.cmdCancelBuild.EnabledChanged += (sender, e) => ReloadBuildbox();

                MainWindow.Instance.TitleChanged += delegate
                {
                    var title = MainWindow.TitleBase;
                    var subtitle = "";

                    if (PipelineController.Instance.ProjectOpen)
                    {
                        title = (PipelineController.Instance.ProjectDirty) ? "*" : "";
                        title += Path.GetFileName(PipelineController.Instance.ProjectItem.OriginalPath);
                        subtitle = Path.GetDirectoryName(PipelineController.Instance.ProjectItem.OriginalPath);
                    }

                    h.Control.Title = title;
                    Gtk3Wrapper.gtk_header_bar_set_subtitle(headerBar.Handle, subtitle);
                };

                var treeview1 = new Gtk.TreeView(builder.GetObject("treeview1").Handle);
                var store = new Gtk.TreeStore(typeof(string), typeof(string));
                var column = new Gtk.TreeViewColumn();
                var textCell = new Gtk.CellRendererText();
                var dataCell = new Gtk.CellRendererText();
                dataCell.Visible = false;
                column.PackStart(textCell, false);
                column.PackStart(dataCell, false);
                treeview1.AppendColumn(column);
                column.AddAttribute(textCell, "markup", 0);
                column.AddAttribute(dataCell, "text", 1);
                treeview1.Model = store;

                MainWindow.Instance.RecentChanged += (sender, e) =>
                {
                    store.Clear();
                    var recentList = sender as List<string>;

                    foreach (var project in recentList)
                        store.InsertWithValues(0, "<b>" + Path.GetFileName(project) + "</b>\n" +
                                               Path.GetDirectoryName(project), project);
                };

                treeview1.RowActivated += (o, args) =>
                {
                    _popovermenu2.Hide();

                    Gtk.TreeIter iter;
                    if (!store.GetIter(out iter, args.Path))
                        return;

                    var project = store.GetValue(iter, 1).ToString();
                    PipelineController.Instance.OpenProject(project);
                };

                headerBar.Show();
            });

            Style.Add<ButtonHandler>("Destuctive", h => h.Control.StyleContext.AddClass("destructive-action"));

            Style.Add<LabelHandler>("Wrap", h => h.Control.MaxWidthChars = 55);

            Style.Add<ToolBarHandler>("ToolBar", h =>
            {
                h.Control.ToolbarStyle = Gtk.ToolbarStyle.Icons;
                h.Control.IconSize = Gtk.IconSize.SmallToolbar;
            });

            Style.Add<DrawableHandler>("Stretch", h =>
            {
                var parent = h.Control.Parent.Parent.Parent.Parent.Parent.Parent;

                parent.SizeAllocated += delegate
                {
                    var al = h.Control.Allocation;
                    al.Width = parent.AllocatedWidth - 2;
                    h.Control.SetAllocation(al);
                };
            });

            Style.Add<PixelLayoutHandler>("Stretch", h =>
            {
                var parent = h.Control.Parent.Parent.Parent.Parent.Parent;

                parent.SizeAllocated += delegate
                {
                     var al = h.Control.Allocation;
                     al.Width = parent.AllocatedWidth;
                     h.Control.SetAllocation(al);
                };
            });
        }
    }
}

