// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class ProjectControl : Scrollable
    {
        TreeView treeView1;

        private void InitializeComponent()
        {
            treeView1 = new TreeView();

            Content = treeView1;
        }
    }
}

