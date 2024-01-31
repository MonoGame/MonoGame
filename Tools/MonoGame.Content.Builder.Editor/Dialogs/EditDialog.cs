// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class EditDialog : Dialog<bool>
    {
        public string Text { get; private set; }

        private string _errInvalidName;
        private bool _file;

        public EditDialog(string title, string label, string text, bool file)
        {
            InitializeComponent();

            _errInvalidName = "The following characters are not allowed:";
            for (int i = 0; i < Global.NotAllowedCharacters.Length; i++)
                _errInvalidName += " " + Global.NotAllowedCharacters[i];
            
            Title = title;
            label1.Text = label;
            textBox1.Text = text;

            Text = text;
            _file = file;

            TextBox1_TextChanged(this, EventArgs.Empty);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            textBox1.Focus();
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
            label2.Text = !stringOk ? _errInvalidName : "";

            Text = textBox1.Text;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Result = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
