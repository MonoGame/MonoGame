// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class ReferenceDialog : DialogBase
    {
        DynamicLayout layout1;
        StackLayout stack1;
        GridView grid1;
        Button buttonAdd, buttonRemove;

        private void InitializeComponent()
        {
            Title = "Reference Editor";
            Resizable = true;
            Width = 500;
            Height = 400;

            layout1 = new DynamicLayout();
            layout1.DefaultSpacing = new Size(4, 4);
            layout1.BeginHorizontal();

            grid1 = new GridView();
            grid1.Style = "GridView";
            layout1.Add(grid1, true, true);

            stack1 = new StackLayout();
            stack1.Orientation = Orientation.Vertical;
            stack1.Spacing = 4;

            buttonAdd = new Button();
            buttonAdd.Text = "Add";
            stack1.Items.Add(new StackLayoutItem(buttonAdd, false));

            buttonRemove = new Button();
            buttonRemove.Text = "Remove";
            stack1.Items.Add(new StackLayoutItem(buttonRemove, false));

            layout1.Add(stack1, false, true);

            CreateContent(layout1);

            grid1.SelectionChanged += Grid1_SelectionChanged;
            buttonAdd.Click += ButtonAdd_Click;
            buttonRemove.Click += ButtonRemove_Click;
        }
    }
}
