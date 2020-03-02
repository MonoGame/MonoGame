// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class AddItemDialog : Dialog<bool>
    {
        DynamicLayout layout1;
        Label label1;
        RadioButton radioCopy, radioLink, radioSkip;
        CheckBox checkBox1;
        Button buttonAdd, buttonCancel;

        private void InitializeComponent()
        {
            DisplayMode = DialogDisplayMode.Attached;
            Width = 400;
            Height = 250;

            buttonAdd = new Button();
            buttonAdd.Text = "Add";
            PositiveButtons.Add(buttonAdd);
            DefaultButton = buttonAdd;

            buttonCancel = new Button();
            buttonCancel.Text = "Cancel";
            NegativeButtons.Add(buttonCancel);
            AbortButton = buttonCancel;

            layout1 = new DynamicLayout();
            layout1.DefaultSpacing = new Size(8, 8);
            layout1.Padding = new Padding(6);
            layout1.BeginVertical();

            label1 = new Label();
            label1.Wrap = WrapMode.Word;
            label1.Style = "Wrap";
            layout1.AddRow(label1);

            radioCopy = new RadioButton();
            radioCopy.Checked = true;
            layout1.AddRow(radioCopy);

            radioLink = new RadioButton(radioCopy);
            layout1.AddRow(radioLink);

            radioSkip = new RadioButton(radioCopy);
            layout1.AddRow(radioSkip);

            var spacing = new Label();
            spacing.Height = 15;
            layout1.Add(spacing, true, true);

            checkBox1 = new CheckBox();
            layout1.AddRow(checkBox1);

            layout1.EndVertical();
            Content = layout1;

            radioCopy.CheckedChanged += RadioButton_CheckedChanged;
            radioLink.CheckedChanged += RadioButton_CheckedChanged;
            radioSkip.CheckedChanged += RadioButton_CheckedChanged;
            checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
            buttonAdd.Click += ButtonOk_Click;
            buttonCancel.Click += ButtonAdd_Click;
        }
    }
}
