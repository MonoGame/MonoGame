// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.GtkSharp.Forms.Controls;

namespace MonoGame.Tools.Pipeline
{
    partial class ProjectControl
    {
        Gtk.TreeView _gtkTreeView;

        private void Init()
        {
            _gtkTreeView = (treeView1.ControlObject as Gtk.ScrolledWindow).Children[0] as Gtk.TreeView;
            _gtkTreeView.Selection.Mode = Gtk.SelectionMode.Multiple;

            _gtkTreeView.ButtonPressEvent += TreeView_ButtonPressEvent;
            _gtkTreeView.Selection.Changed += Selection_Changed;
        }

        private Gtk.TreePath[] GetSelected()
        {
            return _gtkTreeView.Selection.GetSelectedRows();
        }

        private void SetSelected(Gtk.TreePath[] paths)
        {
            _gtkTreeView.Selection.UnselectAll();
            foreach (var path in paths)
                _gtkTreeView.Selection.SelectPath(path);
        }

        private void Selection_Changed(object sender, EventArgs e)
        {
            var items = new List<IProjectItem>();
            var paths = GetSelected();

            foreach (var path in paths)
            {
                var item = (treeView1.Handler as TreeViewHandler).GetItem(path) as TreeItem;
                items.Add(item.Tag as IProjectItem);
            }

            MainWindow.Controller.SelectionChanged(items);
        }

        [GLib.ConnectBefore]
        private void TreeView_ButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
        {
            if (args.Event.Button == 3)
            {
                Gtk.TreeViewDropPosition pos;
                Gtk.TreePath path;
                Gtk.TreeIter iter;

                if (_gtkTreeView.GetDestRowAtPos((int)args.Event.X, (int)args.Event.Y, out path, out pos) && _gtkTreeView.Model.GetIter(out iter, path))
                {
                    var paths = _gtkTreeView.Selection.GetSelectedRows().ToList();
                    if (paths.Contains(path))
                        args.RetVal = true;
                }
            }
        }
    }
}

