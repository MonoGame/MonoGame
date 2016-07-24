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
        private Gtk.ScrolledWindow _scrolledWindow;
        private Gtk.TreeView _gtkTreeView;
        private double _scrollVertical, _scrollHorizontal;
        private bool _scroll;

        private void Init()
        {
            _scrollVertical = 0;
            _scrollHorizontal = 0;
            _scroll = false;

            _scrolledWindow = ControlObject as Gtk.ScrolledWindow;
            _gtkTreeView = _scrolledWindow.Children[0] as Gtk.TreeView;

            _gtkTreeView.Selection.Mode = Gtk.SelectionMode.Multiple;
            _gtkTreeView.ButtonPressEvent += TreeView_ButtonPressEvent;
            _gtkTreeView.Selection.Changed += Selection_Changed;
            _gtkTreeView.MapEvent += _scrolledWindow_ScrollChild;
            _scrolledWindow.SizeAllocated += _scrolledWindow_ScrollChild;
        }

        private void _scrolledWindow_ScrollChild(object o, EventArgs args)
        {
            if (_scroll)
            {
                _scrolledWindow.Vadjustment.Value = _scrollVertical;
                _scrolledWindow.Hadjustment.Value = _scrollHorizontal;

                _scroll = false;
            }
        }

        private Gtk.TreePath[] GetSelected()
        {
            _scrollVertical = _scrolledWindow.Vadjustment.Value;
            _scrollHorizontal = _scrolledWindow.Hadjustment.Value;
            _scroll = true;

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
                var item = (Handler as TreeViewHandler).GetItem(path) as TreeItem;
                items.Add(item.Tag as IProjectItem);
            }

            PipelineController.Instance.SelectionChanged(items);
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

