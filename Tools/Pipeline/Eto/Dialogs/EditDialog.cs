﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class EditDialog : DialogBase
    {
        public string Text { get; private set; }

        private bool _file;

        public EditDialog(string title, string label, string text, bool file)
        {
            InitializeComponent();

            Title = title;
            label1.Text = label;
            textBox1.Text = text;

            Text = text;
            _file = file;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // We need to delay setting of text color because
            // GTK doesn't load text color during initialization
            label2.TextColor = new Color(label2.TextColor, 0.5f);
            label2.Visible = false;

            var index = textBox1.Text.IndexOf('.');
            if (_file && index != -1)
                textBox1.Selection = new Range<int>(0, index - 1);
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!_file)
                return;

            var stringOk = Global.CheckString(textBox1.Text);

            DefaultButton.Enabled = (stringOk && (textBox1.Text != ""));
            label2.Visible = !stringOk;

            Text = textBox1.Text;
        }
    }
}
