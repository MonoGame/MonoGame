// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class DeleteDialog : DialogBase
    {
        DynamicLayout layout1;
        Label label1;
        TreeGridView treeView1;

        private void InitializeComponent()
        {
            Title = "Delete Items";
            Resizable = true;
            Width = 450;
            Height = 300;

            layout1 = new DynamicLayout();
            layout1.DefaultSpacing = new Size(2, 2);
            layout1.BeginVertical();

            label1 = new Label();
            label1.Wrap = WrapMode.Word;
            label1.Text = "The following items will be deleted (this action cannot be undone):";
            layout1.Add(label1, true, false);

            treeView1 = new TreeGridView();
            treeView1.ShowHeader = false;
            layout1.Add(treeView1, true, true);

            DefaultButton.Text = "Delete";

            CreateContent(layout1);
        }
    }
}
