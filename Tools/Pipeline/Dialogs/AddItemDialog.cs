// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class AddItemDialog : Dialog<bool>
    {
        public bool ApplyForAll { get; private set; }
        public CopyAction Responce { get; private set; }

        public AddItemDialog(string fileloc, bool exists, FileType filetype)
        {
            InitializeComponent();

            Title = "Add " + filetype;

            label1.Text = "The file '" + fileloc + "' is outside of target directory. What would you like to do?";

            radioCopy.Text = "Copy the " + filetype.ToString().ToLower() + " to the directory";
            radioLink.Text = "Add a link to the " + filetype.ToString().ToLower();
            radioSkip.Text = "Skip adding the " + filetype.ToString().ToLower();

            checkBox1.Text = "Use the same action for all the selected " + filetype.ToString().ToLower() + "s";

            if (exists)
            {
                radioLink.Checked = true;
                radioCopy.Enabled = false;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            // Delay setting of Width so that label wrapping would be correct.
            Width = 400;

            base.OnShown(e);
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (radioCopy.Checked)
                Responce = CopyAction.Copy;
            else if (radioLink.Checked)
                Responce = CopyAction.Link;
            else
                Responce = CopyAction.Skip;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ApplyForAll = (bool)checkBox1.Checked;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Result = true;
            Close();
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
