// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class PathDialog : DialogBase
    {
        DynamicLayout layout1;
        StackLayout stack1, stack2;
        Label label1;
        TextBox textBox1;
        Button buttonBrowse;

        private void InitializeComponent()
        {
            Title = "Select Folder";
            Width = 370;
            Height = 165;

            layout1 = new DynamicLayout();
            layout1.DefaultSpacing = new Size(4, 4);
            layout1.Padding = new Padding(6);
            layout1.BeginVertical();

            label1 = new Label();
            label1.Text = "Path to use:";
            layout1.Add(label1);

            stack1 = new StackLayout();
            stack1.Spacing = 4;
            stack1.Orientation = Orientation.Horizontal;

            textBox1 = new TextBox();
            stack1.Items.Add(new StackLayoutItem(textBox1, true));

            buttonBrowse = new Button();
            buttonBrowse.Text = "...";
            buttonBrowse.MinimumSize = new Size(1, 1);
            stack1.Items.Add(new StackLayoutItem(buttonBrowse, false));

            layout1.Add(stack1);

            stack2 = new StackLayout();
            stack2.Spacing = 4;
            stack2.Orientation = Orientation.Horizontal;

            foreach (var symbol in symbols)
            {
                var buttonSymbol = new Button();
                buttonSymbol.Text = symbol;
                buttonSymbol.Click += ButtonSymbol_Click;
                stack2.Items.Add(new StackLayoutItem(buttonSymbol, true));
            }

            layout1.Add(stack2);

            CreateContent(layout1);

            textBox1.TextChanged += TextBox1_TextChanged;
            buttonBrowse.Click += ButtonBrowse_Click;
        }
    }
}
