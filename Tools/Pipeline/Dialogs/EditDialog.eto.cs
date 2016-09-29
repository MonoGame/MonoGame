// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class EditDialog : DialogBase
    {
        DynamicLayout layout1;
        Label label1, label2;
        TextBox textBox1;

        private void InitializeComponent()
        {
            Width = 370;
            Height = 160;

            layout1 = new DynamicLayout();
            layout1.DefaultSpacing = new Size(4, 4);
            layout1.Padding = new Padding(6);
            layout1.BeginVertical();

            label1 = new Label();
            layout1.AddRow(label1);

            textBox1 = new TextBox();
            layout1.AddRow(textBox1);
            
            label2 = new Label();
            label2.TextColor = new Color(SystemColors.ControlText, 0.5f);
            layout1.AddRow(label2);

            layout1.EndVertical();
            CreateContent(layout1);

            textBox1.TextChanged += TextBox1_TextChanged;
        }
    }
}
