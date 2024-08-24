// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class PathDialog : Dialog<bool>
    {
        public string Path { get; set; }

        private readonly string[] symbols = { "Platform", "Configuration", "Config", "Profile" };
        private IController _controller;

        public PathDialog(IController controller, string path)
        {
            InitializeComponent();

            _controller = controller;
            textBox1.Text = path;
        }

        private void ButtonSymbol_Click(object sender, EventArgs e)
        {
            var text = "$(" + (sender as Button).Text + ")";
            int carret;

            if (!string.IsNullOrEmpty(textBox1.SelectedText))
            {
                carret = textBox1.Selection.Start;
                textBox1.Text = textBox1.Text.Remove(carret, textBox1.Selection.End + 1 - carret);
            }
            else
                carret = textBox1.CaretIndex;

            textBox1.Text = textBox1.Text.Insert(carret, text);
            textBox1.Focus();
            textBox1.CaretIndex = carret + text.Length;
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            Path = textBox1.Text;
            DefaultButton.Enabled = !string.IsNullOrWhiteSpace(textBox1.Text);
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            var dialog = new SelectFolderDialog();
            dialog.Directory = _controller.GetFullPath(textBox1.Text);

            if (dialog.Show(this) == DialogResult.Ok)
                textBox1.Text = _controller.GetRelativePath(dialog.Directory);
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
