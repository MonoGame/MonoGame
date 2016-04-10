// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class DeleteDialog : DialogBase
    {
        private IController _controller;
        private TreeGridItem _treeBase;

        public DeleteDialog(IController controller, string[] directories, string[] files)
        {
            InitializeComponent();

            _controller = controller;

            treeView1.Columns.Add(new GridColumn { DataCell = new ImageTextCell(0, 1), AutoSize = true, Resizable = true, Editable = false });
            treeView1.DataStore = _treeBase = new TreeGridItem();

            foreach (var directory in directories)
                ProcessDirectory(_controller.GetFullPath(directory));

            foreach (var file in files)
                Add(_treeBase, file, false, _controller.GetFullPath(file));
        }

        private void ProcessDirectory(string directory)
        {
            Add(_treeBase, _controller.GetRelativePath(directory), true, "");

            var dirs = Directory.GetDirectories(directory);
            var files = Directory.GetFiles(directory);

            foreach (var dir in dirs)
                ProcessDirectory(dir);

            foreach (var file in files)
                Add(_treeBase, _controller.GetRelativePath(file), false, file);
        }

        public TreeGridItem GetItem(TreeGridItem root, string text, bool folder, string fullpath)
        {
            var enumerator = root.Children.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var item = enumerator.Current as TreeGridItem;
                var itemtext = item.GetValue(1).ToString();

                if (itemtext == text)
                    return item;
            }

            var ret = new TreeGridItem();
            var icon = folder ? Global.GetDirectoryIcon() : Global.GetFileIcon(fullpath);
            ret.Values = new object[] { icon, text };
            root.Children.Add(ret);
            root.Expanded = true;
            treeView1.DataStore = _treeBase;

            return ret;
        }

        public void Add(TreeGridItem root, string path, bool folder, string fullpath)
        {
            var split = path.Split('/');
            var file = split.Length == 1 && !folder;
            var item = GetItem(root, split[0], !file, fullpath);

            if (path.Contains("/"))
                Add(item, string.Join("/", split, 1, split.Length - 1), folder, fullpath);
        }
    }
}
