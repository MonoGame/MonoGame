// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class DialogBase : Dialog
    {
        DynamicLayout layout1;

        private void InitializeComponent()
        {
            layout1 = new DynamicLayout();
            layout1.DefaultSpacing = new Size(8, 8);
            layout1.Padding = new Padding(6);

            DefaultButton = new Button();
            DefaultButton.Text = "OK";
            DefaultButton.Click += delegate
            {
                _result = DialogResult.Ok;
                Close();
            };

            AbortButton = new Button();
            AbortButton.Text = "Cancel";
            AbortButton.Click += (sender, e) => Close();

            Content = layout1;
        }

        public void CreateContent(Control content)
        {
            layout1.BeginVertical();
            layout1.Add(content, true, true);

            if (Global.UseHeaderBar)
                this.Style = "HeaderBar";
            else if(Global.Unix)
                layout1.AddSeparateRow(null, AbortButton, DefaultButton);
            else
                layout1.AddSeparateRow (null, DefaultButton, AbortButton);

            layout1.EndVertical();
        }
    }
}
