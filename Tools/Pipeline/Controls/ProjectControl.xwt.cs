// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xwt;
using Xwt.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class ProjectControl : Pad
    {
        private Xwt.TreeView _treeView;
        private Control _etoView;
        private readonly DataField<Image> _dataImage;
        private readonly DataField<string> _dataText;
        private readonly DataField<IProjectItem> _dataTag;
        private readonly TreeStore _treeStore;

        private Image _iconRoot;
        private TreeNavigator _treeRoot;
        private bool _rootExists;

        private Eto.Forms.ContextMenu _contextMenu;
        private bool _showContextMenu;

        public ProjectControl()
        {
            Title = "Project";

            _treeView = new Xwt.TreeView();
            _treeView.HeadersVisible = false;
            _treeView.SelectionMode = SelectionMode.Multiple;

            _iconRoot = Image.FromResource("TreeView.Root.png");

            _dataImage = new DataField<Image>();
            _dataText = new DataField<string>();
            _dataTag = new DataField<IProjectItem>();

            _treeStore = new TreeStore(_dataImage, _dataText, _dataTag);

            _treeView.DataSource = _treeStore;
            _treeView.Columns.Add("", _dataImage, _dataText);

            var nativeWidget = Toolkit.CurrentEngine.GetNativeWidget(_treeView);
#if WINDOWS
            var widget = nativeWidget as System.Windows.Controls.Control;
#else
            var widget = nativeWidget as Gtk.Widget;
#endif
            _etoView = widget.ToEto();
            CreateContent(_etoView);

#if LINUX
            (nativeWidget as Gtk.ScrolledWindow).Child.ButtonPressEvent += Styles.TreeView_ButtonPressEvent;
#endif

            _treeView.RowActivated += TreeView_RowActivated;
            _treeView.ButtonReleased += Handle_ButtonReleased;
            _treeView.SelectionChanged += ProjectControl_SelectionChanged;

            _treeView.SetDragDropTarget(TransferDataType.Text, TransferDataType.Uri);
            _treeView.DragOver += TreeView_DragOver;
            _treeView.DragDrop += TreeView_DragDrop;
        }

        private void TreeView_RowActivated(object sender, TreeViewRowEventArgs e)
        {
            if (PipelineController.Instance.ProjectOpen)
                _contextMenu.Show(_etoView);
        }

        private void TreeView_DragOver(object sender, DragOverEventArgs e)
        {
            if (!PipelineController.Instance.ProjectOpen)
                return;
            
            e.AllowedAction = DragDropAction.Copy;

            RowDropPosition rowpos;
            TreePosition pos;

            if (_treeView.GetDropTargetRow(e.Position.X, e.Position.Y, out rowpos, out pos))
                if (rowpos == RowDropPosition.Into && _treeStore.GetNavigatorAt(pos).GetValue(_dataTag) is ContentItem)
                    e.AllowedAction = DragDropAction.None;
        }

        private void TreeView_DragDrop(object sender, DragEventArgs e)
        {
            var initialDirectory = PipelineController.Instance.ProjectLocation;
            var folders = new List<string>();
            var files = new List<string>();
            RowDropPosition rowpos;
            TreePosition pos;

            foreach (var u in e.Data.Uris)
            {
                if (Directory.Exists(u.LocalPath))
                    folders.Add(u.LocalPath);
                else
                    files.Add(u.LocalPath);
            }

            if (_treeView.GetDropTargetRow(e.Position.X, e.Position.Y, out rowpos, out pos))
            {
                var item = _treeStore.GetNavigatorAt(pos).GetValue(_dataTag);

                if (!(item is DirectoryItem && rowpos == RowDropPosition.Into))
                {
                    var nav = _treeStore.GetNavigatorAt(pos);
                    if (nav.MoveToParent())
                        item = nav.GetValue(_dataTag);
                }

                if (!(item is PipelineProject))
                    initialDirectory = Path.Combine(initialDirectory, item.OriginalPath);
            }

            PipelineController.Instance.DragDrop(initialDirectory, folders.ToArray(), files.ToArray());
            e.Success = true;
        }

        private void Handle_ButtonReleased(object sender, ButtonEventArgs e)
        {
            if (e.Button == PointerButton.Right)
            {
#if WINDOWS
                var crow = _treeView.GetRowAtPosition(e.X, e.Y);

                if (crow == null)
                    return;

                if (!_treeView.SelectedRows.ToList().Contains(crow))
                {
                    _treeView.UnselectAll();
                    _treeView.SelectRow(crow);
                    _treeView.FocusedRow = crow;
                }
#endif

                if (_showContextMenu && _contextMenu.Items.Count > 0)
                    _contextMenu.Show(_etoView);
            }
        }

        private void ProjectControl_SelectionChanged(object sender, System.EventArgs e)
        {
            var items = new List<IProjectItem>();

            foreach (var row in _treeView.SelectedRows)
                items.Add(_treeStore.GetNavigatorAt(row).GetValue(_dataTag) as IProjectItem);

            PipelineController.Instance.SelectionChanged(items);
        }

        public void SetContextMenu(Eto.Forms.ContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
        }

        public void ExpandBase()
        {
            if (_rootExists)
                _treeView.ExpandRow(_treeRoot.CurrentPosition, false);
        }

        public void SetRoot(IProjectItem item)
        {
            if (item == null)
            {
                _treeStore.Clear();
                _rootExists = false;

                _showContextMenu = false;
                PipelineController.Instance.SelectionChanged(new List<IProjectItem>());

                return;
            }

            if (!_rootExists)
            {
                _treeRoot = _treeStore.AddNode();

                _rootExists = true;
            }

            _treeRoot.SetValue(_dataImage, _iconRoot);
            _treeRoot.SetValue(_dataText, item.Name);
            _treeRoot.SetValue(_dataTag, item);

            _showContextMenu = true;
        }

        public void AddItem(IProjectItem citem)
        {
            AddItem(_treeRoot, citem, citem.OriginalPath, "");
        }

        public void AddItem(TreeNavigator root, IProjectItem citem, string path, string currentPath)
        {
            var split = path.Split('/');
            var item = GetorAddItem(root, split.Length > 1 ? new DirectoryItem(split[0], currentPath) { Exists = citem.Exists } : citem);

            if (path.Contains("/"))
                AddItem(item, citem, string.Join("/", split, 1, split.Length - 1), (currentPath + Path.DirectorySeparatorChar + split[0]));
        }

        public void RemoveItem(IProjectItem item)
        {
            TreeNavigator titem;
            if (FindItem(_treeRoot, item.OriginalPath, out titem))
                titem.Remove();
        }

        public void UpdateItem(IProjectItem item)
        {
            TreeNavigator titem;
            if (FindItem(_treeRoot, item.OriginalPath, out titem))
            {
                if (item.ExpandToThis)
                {
                    _treeView.ExpandToRow(titem.CurrentPosition);
                    item.ExpandToThis = false;
                }

                SetExists(titem, item.Exists);

                if (item.SelectThis)
                {
                    _treeView.SelectRow(titem.CurrentPosition);
                    item.SelectThis = false;
                }
            }
        }

        public void SetExists(TreeNavigator titem, bool exists)
        {
            var item = titem.GetValue(_dataTag) as IProjectItem;

            if (item is PipelineProject)
                return;

            if (item is DirectoryItem)
            {
                var fex = exists;
                var nav = titem.Clone();

                if (nav.MoveToChild())
                {
                    do
                    {
                        if (!(titem.GetValue(_dataTag) as IProjectItem).Exists)
                            fex = false;
                    }
                    while (nav.MoveNext());
                }

                titem.SetValue(_dataImage, Global.GetXwtDirectoryIcon(fex));
            }
            else
                titem.SetValue(_dataImage, Global.GetXwtFileIcon(PipelineController.Instance.GetFullPath(item.OriginalPath), exists));

            if (titem.MoveToParent())
                SetExists(titem, exists);
        }

        private bool FindItem(TreeNavigator root, string path, out TreeNavigator item)
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

        private bool GetItem(TreeNavigator root, string text, out TreeNavigator item)
        {
            var nav = root.Clone();

            if (nav.MoveToChild())
            {
                do
                {
                    if (nav.GetValue(_dataText) == text)
                    {
                        item = nav;
                        return true;
                    }
                }
                while (nav.MoveNext());
            }

            item = _treeRoot;
            return false;
        }

        private TreeNavigator GetorAddItem(TreeNavigator root, IProjectItem item)
        {
            var nav = root.Clone();
            var folder = item is DirectoryItem;
            TreePosition pos = null;
            TreePosition child = null;

            if (nav.MoveToChild())
            {
                child = nav.CurrentPosition;

                do
                {
                    if (nav.GetValue(_dataText).ToString() == item.Name)
                        return nav;

                    if (nav.GetValue(_dataTag) is DirectoryItem)
                    {
                        if (folder && string.Compare(nav.GetValue(_dataText), item.Name) < 0 || !folder)
                            pos = nav.CurrentPosition;
                    }
                    else if(!folder && nav.GetValue(_dataTag) is ContentItem)
                    {
                        if (string.Compare(nav.GetValue(_dataText), item.Name) < 0)
                            pos = nav.CurrentPosition;
                    }
                }
                while (nav.MoveNext());
            }

            TreeNavigator ret;

            if (pos == null)
            {
                if (child == null)
                    ret = _treeStore.AddNode(root.CurrentPosition);
                else
                    ret = _treeStore.InsertNodeBefore(child);
            }
            else
                ret = _treeStore.InsertNodeAfter(pos);

            if(item is DirectoryItem)
                ret.SetValue(_dataImage, Global.GetXwtDirectoryIcon(item.Exists));
            else
                ret.SetValue(_dataImage, Global.GetXwtFileIcon(PipelineController.Instance.GetFullPath(item.OriginalPath), item.Exists));

            ret.SetValue(_dataText, item.Name);
            ret.SetValue(_dataTag, item);

            return ret;
        }
    }
}

