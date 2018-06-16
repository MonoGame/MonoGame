// MonoGame - Copyright (C) The MonoGame Team
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

            _treeView.SelectionChanged += TreeView_SelectedItemChanged;
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

            _treeView.ReloadItem(_treeRoot);
            _treeView.ContextMenu = _contextMenu;
        }

        public void AddItem(IProjectItem citem)
        {
            AddItem(_treeRoot, citem, citem.OriginalPath, "");
        }

        public void AddItem(TreeGridItem root, IProjectItem citem, string path, string currentPath)
        {
            var split = path.Split('/');
            var item = GetorAddItem(root, split.Length > 1 ? new DirectoryItem(split[0], currentPath) { Exists = citem.Exists } : citem);

            if (path.Contains("/"))
                AddItem(item, citem, string.Join("/", split, 1, split.Length - 1), (currentPath + Path.DirectorySeparatorChar + split[0]));
        }

        public void RemoveItem(IProjectItem item)
        {
            TreeGridItem titem;
            if (FindItem(_treeRoot, item.OriginalPath, out titem))
            {
                var parrent = titem.Parent as TreeGridItem;
                parrent.Children.Remove(titem);
                _treeView.ReloadItem(parrent);
            }
        }

        public void UpdateItem(IProjectItem item)
        {
            TreeGridItem titem;
            if (FindItem(_treeRoot, item.OriginalPath, out titem))
            {
                var parrent = titem.Parent as TreeGridItem;
                var selected = _treeView.SelectedItem;

                if (item.ExpandToThis)
                {
                    parrent.Expanded = true;
                    _treeView.ReloadItem(parrent);
                    item.ExpandToThis = false;
                }

                SetExists(titem, item.Exists);

                if (item.SelectThis)
                {
                    _treeView.SelectedItem = titem;
                    item.SelectThis = false;
                }
                else
                    _treeView.SelectedItem = selected;
            }
        }

        private void SetExists(TreeGridItem titem, bool exists)
        {
            var item = titem.Tag as IProjectItem;

            if (item is PipelineProject)
                return;

            if (item is DirectoryItem)
            {
                bool fex = exists;

                var enumerator = titem.Children.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var citem = enumerator.Current as TreeGridItem;
                    if (!(citem.Tag as IProjectItem).Exists)
                        fex = false;
                }

                titem.SetValue(0, Global.GetEtoDirectoryIcon(fex));
            }
            else
                titem.SetValue(0, Global.GetEtoFileIcon(PipelineController.Instance.GetFullPath(item.OriginalPath), exists));

            var parrent = titem.Parent as TreeGridItem;
            _treeView.ReloadItem(parrent);
            SetExists(parrent, exists);
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

                if (citem.GetValue(1).ToString() == item.Name)
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

            items.Add(item.Name);
            items.Sort();
            pos += items.IndexOf(item.Name);

            var ret = new TreeGridItem();

            if (item is DirectoryItem)
                ret.SetValue(0, Global.GetEtoDirectoryIcon(item.Exists));
            else
                ret.SetValue(0, Global.GetEtoFileIcon(PipelineController.Instance.GetFullPath(item.OriginalPath), item.Exists));

            ret.SetValue(1, item.Name);
            ret.Tag = item;

            root.Children.Insert(pos, ret);
            _treeView.ReloadItem(root);

            return ret;
        }
    }
}
