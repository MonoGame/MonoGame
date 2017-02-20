// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class ProjectControl : Pad
    {
        private TreeGridView _treeView;

        private void InitializeComponent()
        {
            Title = "Project";

            _treeView = new TreeGridView();
            _treeView.MouseDown += (sender, e) => { };
            _treeView.ShowHeader = false;
            _treeView.AllowMultipleSelection = true;
            _treeView.Columns.Add(new GridColumn {
                DataCell = new ImageTextCell(0, 1),
                AutoSize = true
            });

            CreateContent(_treeView);

            _treeView.SelectionChanged += TreeView_SelectedItemChanged;
        }
    }
}