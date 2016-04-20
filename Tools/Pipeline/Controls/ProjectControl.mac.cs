// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class ProjectControl
    {
        private void Init()
        {
            treeView1.SelectionChanged += TreeView1_SelectionChanged;
        }

        private void TreeView1_SelectionChanged(object sender, EventArgs e)
        {
            var selectedItems = new List<IProjectItem>();

            if (treeView1.SelectedItem != null)
                selectedItems.Add((treeView1.SelectedItem as TreeItem).Tag as IProjectItem);

            MainWindow.Controller.SelectionChanged(selectedItems);
        }

        private ITreeItem GetSelected()
        {
            return treeView1.SelectedItem;
        }

        private void SetSelected(ITreeItem item)
        {
            treeView1.SelectedItem = item;
        }
    }
}

