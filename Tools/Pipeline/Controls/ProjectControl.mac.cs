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
            SelectionChanged += TreeView1_SelectionChanged;
        }

        private void TreeView1_SelectionChanged(object sender, EventArgs e)
        {
            var selectedItems = new List<IProjectItem>();

            if (SelectedItem != null)
                selectedItems.Add((SelectedItem as TreeItem).Tag as IProjectItem);

            PipelineController.Instance.SelectionChanged(selectedItems);
        }

        private ITreeItem GetSelected()
        {
            return SelectedItem;
        }

        private void SetSelected(ITreeItem item)
        {
            SelectedItem = item;
        }
    }
}

