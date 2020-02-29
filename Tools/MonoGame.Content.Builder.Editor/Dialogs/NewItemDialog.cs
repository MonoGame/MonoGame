// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class NewItemDialog : Dialog<bool>
    {
        public string Name { get; private set; }
        public ContentItemTemplate Selected { get; private set; }

        private const string _errFileExists = "A file with the same name already exists.";
        private readonly string _errInvalidName, _dir;

        public NewItemDialog(IEnumerator<ContentItemTemplate> enums, string dir)
        {
            InitializeComponent();

            _errInvalidName = "The following characters are not allowed:";
            for (int i = 0; i < Global.NotAllowedCharacters.Length; i++)
                _errInvalidName += " " + Global.NotAllowedCharacters[i];

            _dir = dir;

            while (enums.MoveNext())
            {
                var ret = new ImageListItem();
                ret.Text = enums.Current.Label + " (" + Path.GetExtension(enums.Current.TemplateFile) + ")";
                ret.Tag = enums.Current;

                list1.Items.Add(ret);
            }

            if (list1.Items.Count > 0)
                list1.SelectedIndex = 0;

            textBox1.Text = "File";
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // We need to delay setting of text color because
            // GTK doesn't load text color during initialization
            labelError.TextColor = new Color(labelError.TextColor, 0.5f);
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            ReloadSensitive();
            Name = textBox1.Text;
        }

        private void ReloadSensitive()
        {
            if (!Global.CheckString(textBox1.Text))
            {
                labelError.Text = _errInvalidName;
                DefaultButton.Enabled = false;
            }
            else if (File.Exists(Path.Combine(_dir, textBox1.Text + labelExt.Text)))
            {
                labelError.Text = _errFileExists;
                DefaultButton.Enabled = false;
            }
            else
            {
                labelError.Text = "";
                DefaultButton.Enabled = (textBox1.Text != "") && (list1.SelectedIndex >= 0);
            }
        }

        private void List1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (list1.SelectedIndex < 0)
                return;

            Selected = (ContentItemTemplate)((ImageListItem)list1.SelectedValue).Tag;
            labelExt.Text = Path.GetExtension(Selected.TemplateFile);
            ReloadSensitive();
        }

        private void ButtonCreate_Click(object sender, EventArgs e)
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
