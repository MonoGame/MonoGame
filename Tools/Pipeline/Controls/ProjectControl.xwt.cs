// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Xwt;

namespace MonoGame.Tools.Pipeline
{
    partial class ProjectControl : Pad
    {
        public TreeView TreeView;

        private void InitializeComponent()
        {
            Title = "Project";

            TreeView = new TreeView();
            TreeView.HeadersVisible = false;
            TreeView.SelectionMode = SelectionMode.Multiple;

            CreateContent(TreeView.ToEto());
        }
    }
}

