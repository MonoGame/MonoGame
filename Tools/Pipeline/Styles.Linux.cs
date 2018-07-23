// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Eto;
using Eto.Forms;
using Eto.GtkSharp.Forms;
using Eto.GtkSharp.Forms.Controls;
using Eto.GtkSharp.Forms.ToolBar;

namespace MonoGame.Tools.Pipeline
{
    public static class Styles
    {
        [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_cclosure_new (IntPtr callback_func, IntPtr user_data, IntPtr destroy_data);
        
        private static Gtk.AccelGroup _accelGroup;
        private static Gtk.Widget _popovermenu1, _popovermenu2;
        private static Gtk.Widget _buttonbox, _cancelbox, _separator;

        public static void Connect(string action, Command cmd)
        {
            var a = new GLib.SimpleAction(action, null);
            a.Activated += (o, args) => 
            {
                _popovermenu1.Hide();
                _popovermenu2.Hide();
                cmd.Execute();
            };

            cmd.EnabledChanged += (sender, e) => a.Enabled = cmd.Enabled;

            Global.Application.AddAction(a);
        }

        private static void Connect(Command cmd, Gdk.Key key, Gdk.ModifierType modifier = Gdk.ModifierType.None)
        {
            var cclosure = g_cclosure_new(Marshal.GetFunctionPointerForDelegate(
                (Action<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr>)((IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr data) =>
            {
                var command = ((GCHandle)data).Target as Command;

                if (command.Enabled)
                    command.Execute();
            })), (IntPtr)GCHandle.Alloc(cmd), IntPtr.Zero);

            _accelGroup.Connect((uint)key, modifier, Gtk.AccelFlags.Mask, cclosure);
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
                Global.Application = h.Control;

                if (Gtk.Global.MajorVersion >= 3 && Gtk.Global.MinorVersion >= 16)
                    Global.UseHeaderBar = Global.Application.PrefersAppMenu();
                
                if (Global.UseHeaderBar)
                    Global.Application.AppMenu = new GLib.MenuModel((new Gtk.Builder("AppMenu.glade")).GetObject("appmenu").Handle);
            });

            Style.Add<FormHandler>("LogWindow", h =>
            {
                if (!Global.UseHeaderBar)
                    return;
                
                var headerBar = new Gtk.HeaderBar();
                headerBar.ShowCloseButton = true;
                headerBar.Title = h.Control.Title;

                var buttoncopy = LogWindow.ButtonCopy.ToNative() as Gtk.Button;
                buttoncopy.StyleContext.AddClass("suggested-action");
                headerBar.PackStart(buttoncopy);

                h.Control.Titlebar = headerBar;
                headerBar.ShowAll();
            });
                                   
            Style.Add<FormHandler>("MainWindow", h =>
            {
                if (!Global.UseHeaderBar)
                    return;
                
                var builder = new Gtk.Builder("MainWindow.glade");
                var headerBar = new Gtk.HeaderBar(builder.GetObject("headerbar").Handle);

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

                var widget = new Gtk.ModelButton(builder.GetObject("button_debug").Handle);
                widget.Active = MainWindow.Instance.cmdDebugMode.Checked;
                widget.Clicked += (e, sender) =>
                {
                    var newstate = !PipelineSettings.Default.DebugMode;

                    widget.Active = newstate;
                    PipelineSettings.Default.DebugMode = newstate;
                };

                _accelGroup = new Gtk.AccelGroup();

                Connect(MainWindow.Instance.cmdNew, Gdk.Key.N, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdOpen, Gdk.Key.O, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdSave, Gdk.Key.S, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdExit, Gdk.Key.Q, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdUndo, Gdk.Key.Z, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdRedo, Gdk.Key.Y, Gdk.ModifierType.ControlMask);
                Connect(MainWindow.Instance.cmdBuild, Gdk.Key.F6);
                Connect(MainWindow.Instance.cmdHelp, Gdk.Key.F1);

                h.Control.AddAccelGroup(_accelGroup);

                _popovermenu1 = new Gtk.Widget(builder.GetObject("popovermenu1").Handle);
                _popovermenu2 = new Gtk.Widget(builder.GetObject("popovermenu2").Handle);

                h.Control.Titlebar = headerBar;
                headerBar.ShowCloseButton = true;

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
                    headerBar.Subtitle = subtitle;
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

