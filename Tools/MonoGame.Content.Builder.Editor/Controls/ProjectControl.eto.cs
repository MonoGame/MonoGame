// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class ProjectControl : Pad
    {
        private TreeGridView _treeView;
        private Image _iconRoot;
        private TreeGridItem _treeBase, _treeRoot;
        private bool _rootExists;
        private ContextMenu _contextMenu;

        public ProjectControl()
        {
            Title = "Project";
            _treeView = new TreeGridView();
            _treeView.ShowHeader = false;
            _treeView.AllowMultipleSelection = true;
            _treeView.Columns.Add(new GridColumn { DataCell = new ImageTextCell(0, 1), AutoSize = true });
            _treeView.DataStore = _treeBase = new TreeGridItem();
            CreateContent(_treeView);

            _iconRoot = Bitmap.FromResource("TreeView.Root.png").WithSize(16, 16);

            _treeView.Activated += TreeView_Activated;
            _treeView.SelectionChanged += TreeView_SelectedItemChanged;
            _treeView.SizeChanged += (o, e) =>
            {
                if (!Global.Unix && _treeView.Columns[0].Width < _treeView.Width - 2)
                    _treeView.Columns[0].Width = _treeView.Width - 2;
            };
        }

        private void TreeView_Activated(object sender, EventArgs e)
        {
            MainWindow.Instance.CmdOpenItem_Executed(sender, e);
        }

        private void TreeView_SelectedItemChanged(object sender, EventArgs e)
        {
            var items = new List<IProjectItem>();

            foreach (TreeGridItem selected in _treeView.SelectedItems)
                if (selected.Tag is IProjectItem)
                    items.Add(selected.Tag as IProjectItem);

            PipelineController.Instance.SelectionChanged(items);
        }

        public void SetContextMenu(ContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
        }

        public void ExpandBase()
        {
            _treeRoot.Expanded = true;
        }

        public void SetRoot(IProjectItem item)
        {
            if (item == null)
            {
                _treeView.DataStore = _treeBase = new TreeGridItem();
                _rootExists = false;
                _treeView.ContextMenu = null;
                return;
            }

            if (!_rootExists)
            {
                _treeRoot = new TreeGridItem();
                _treeBase.Children.Add(_treeRoot);

                _rootExists = true;
            }

            _treeRoot.SetValue(0, _iconRoot);
            _treeRoot.SetValue(1, item.Name);
            _treeRoot.Tag = item;
            _treeRoot.Expanded = true;

            _treeView.ContextMenu = _contextMenu;
            _treeView.DataStore = _treeBase;
            _treeView.ReloadData();
        }

        public void AddItem(IProjectItem citem)
        {
            AddItem(_treeRoot, citem, citem.DestinationPath, "");
        }

        public void AddItem(TreeGridItem root, IProjectItem citem, string path, string currentPath)
        {
            var split = path.Split('/');
            var item = GetorAddItem(root, split.Length > 1 ? new DirectoryItem(split[0], currentPath) : citem);

            if (path.Contains("/"))
                AddItem(item, citem, string.Join("/", split, 1, split.Length - 1), (currentPath + Path.DirectorySeparatorChar + split[0]));
        }

        public void RemoveItem(IProjectItem item)
        {
            TreeGridItem titem;
            if (FindItem(_treeRoot, item.DestinationPath, out titem))
            {
                var parrent = titem.Parent as TreeGridItem;
                parrent.Children.Remove(titem);
                _treeView.ReloadItem(parrent);
            }
        }

        public void UpdateItem(IProjectItem item)
        {
            // Does nothing right now 
        }

        private bool FindItem(TreeGridItem root, string path, out TreeGridItem item)
        {
            var split = path.Split('/');

            if (GetItem(root, split[0], out item))
            {
                if (split.Length != 1)
                    return FindItem(item, string.Join("/", split, 1, split.Length - 1), out item);

                return true;
            }

            return false;
        }

        private bool GetItem(TreeGridItem root, string text, out TreeGridItem item)
        {
            var enumerator = root.Children.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var citem = enumerator.Current as TreeGridItem;

                if (citem.GetValue(1).ToString() == text)
                {
                    item = citem;
                    return true;
                }
            }

            item = _treeRoot;
            return false;
        }

        private TreeGridItem GetorAddItem(TreeGridItem root, IProjectItem item)
        {
            var enumerator = root.Children.GetEnumerator();
            var folder = item is DirectoryItem;

            var items = new List<string>();
            int pos = 0;

            while (enumerator.MoveNext())
            {
                var citem = enumerator.Current as TreeGridItem;

                if (citem.GetValue(1).ToString() == Path.GetFileName(item.DestinationPath))
                    return citem;

                if (folder)
                {
                    if (citem.Tag is DirectoryItem)
                        items.Add(citem.GetValue(1).ToString());
                }
                else
                {
                    if (citem.Tag is DirectoryItem)
                        pos++;
                    else
                        items.Add(citem.GetValue(1).ToString());
                }
            }

            items.Add(Path.GetFileName(item.DestinationPath));
            items.Sort();
            pos += items.IndexOf(Path.GetFileName(item.DestinationPath));

            var ret = new TreeGridItem();

            if (item is DirectoryItem)
                ret.SetValue(0, Global.GetEtoDirectoryIcon());
            else
                ret.SetValue(0, Global.GetEtoFileIcon(PipelineController.Instance.GetFullPath(item.OriginalPath), item.OriginalPath != item.DestinationPath));

            ret.SetValue(1, Path.GetFileName(item.DestinationPath));
            ret.Tag = item;

            root.Children.Insert(pos, ret);
            _treeView.ReloadItem(root);

            return ret;
        }
    }
}
