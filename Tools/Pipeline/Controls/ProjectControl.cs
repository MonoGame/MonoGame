// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class ProjectControl : TreeView
    {
        private Icon _iconRoot;
        private TreeItem _treeBase, _treeRoot;
        private bool _rootExists;
        private ContextMenu _contextMenu;

        public ProjectControl()
        {
            InitializeComponent();

            _iconRoot = Icon.FromResource("TreeView.Root.png");

            DataStore = _treeBase = new TreeItem();
            Expanded += TreeView1_SaveExpanded;
            Collapsed += TreeView1_SaveExpanded;

            Init();
        }

        private void TreeView1_SaveExpanded(object sender, TreeViewItemEventArgs e)
        {
            // This fixes a bug with Eto Froms
            e.Item.Expanded = e.Item.Expanded;
        }

        public void SetContextMenu(ContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
        }

        public void SetRoot(IProjectItem item)
        {
            if (item == null)
            {
                DataStore = _treeBase = new TreeItem();
                _rootExists = false;
                ContextMenu = null;
                return;
            }

            if (!_rootExists)
            {
                _treeRoot = new TreeItem();
                _treeBase.Children.Add(_treeRoot);

                _rootExists = true;
            }

            _treeRoot.Image = _iconRoot;
            _treeRoot.Text = item.Name;
            _treeRoot.Tag = item;
            _treeRoot.Expanded = true;

            RefreshItem(_treeRoot);
            ContextMenu = _contextMenu;
        }

        public void AddItem(IProjectItem citem)
        {
            AddItem(_treeRoot, citem, citem.OriginalPath, "");
        }

        public void AddItem(TreeItem root, IProjectItem citem, string path, string currentPath)
        {
            var split = path.Split('/');
            var item = GetorAddItem(root, split.Length > 1 ? new DirectoryItem(split[0], currentPath) { Exists = citem.Exists } : citem);

            if (path.Contains("/"))
                AddItem(item, citem, string.Join("/", split, 1, split.Length - 1), (currentPath + Path.DirectorySeparatorChar + split[0]));
        }

        public void RemoveItem(IProjectItem item)
        {
            TreeItem titem;
            if (FindItem(_treeRoot, item.OriginalPath, out titem))
            {
                var parrent = titem.Parent as TreeItem;
                parrent.Children.Remove(titem);
                RefreshItem(parrent);
            }
        }

        public void UpdateItem(IProjectItem item)
        {
            TreeItem titem;
            if (FindItem(_treeRoot, item.OriginalPath, out titem))
            {
                var parrent = titem.Parent as TreeItem;
                var selected = GetSelected();

                if (item.ExpandToThis)
                {
                    parrent.Expanded = true;
                    RefreshItem(parrent);
                    item.ExpandToThis = false;
                }

                SetExists(titem, item.Exists);

                if (item.SelectThis)
                {
                    SelectedItem = titem;
                    item.SelectThis = false;
                }
                else
                    SetSelected(selected);
            }
        }

        public void SetExists(TreeItem titem, bool exists)
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
                    var citem = enumerator.Current as TreeItem;
                    if (!(citem.Tag as IProjectItem).Exists)
                        fex = false;
                }

                titem.Image = Global.GetDirectoryIcon(fex);
            }
            else
                titem.Image = Global.GetFileIcon(PipelineController.Instance.GetFullPath(item.OriginalPath), exists);

            var parrent = titem.Parent as TreeItem;
            RefreshItem(parrent);
            SetExists(parrent, exists);
        }

        private bool FindItem(TreeItem root, string path, out TreeItem item)
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

        private bool GetItem(TreeItem root, string text, out TreeItem item)
        {
            var enumerator = root.Children.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var citem = enumerator.Current as TreeItem;

                if (citem.Text == text)
                {
                    item = citem;
                    return true;
                }
            }

            item = _treeRoot;
            return false;
        }

        private TreeItem GetorAddItem(TreeItem root, IProjectItem item)
        {
            var enumerator = root.Children.GetEnumerator();
            var folder = item is DirectoryItem;

            var items = new List<string>();
            int pos = 0;

            while (enumerator.MoveNext())
            {
                var citem = enumerator.Current as TreeItem;

                if (citem.Text == item.Name)
                    return citem;

                if (folder)
                {
                    if (citem.Tag is DirectoryItem)
                        items.Add(citem.Text);
                }
                else
                {
                    if (citem.Tag is DirectoryItem)
                        pos++;
                    else
                        items.Add(citem.Text);
                }
            }

            items.Add(item.Name);
            items.Sort();
            pos += items.IndexOf(item.Name);

            var ret = new TreeItem();

            if(item is DirectoryItem)
                ret.Image = Global.GetDirectoryIcon(item.Exists);
            else
                ret.Image = Global.GetFileIcon(PipelineController.Instance.GetFullPath(item.OriginalPath), item.Exists);

            ret.Text = item.Name;
            ret.Tag = item;

            root.Children.Insert(pos, ret);
            RefreshItem(root);

            return ret;
        }
    }
}

