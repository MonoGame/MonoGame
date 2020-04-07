// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class Pad
    {
        public string Title
        {
            get { return label.Text; }
            set { label.Text = value; }
        }

        public List<Command> Commands;
        private ContextMenu _contextMenu;

        public Pad()
        {
            InitializeComponent();

            Commands = new List<Command>();
            _contextMenu = new ContextMenu();
        }

        public virtual void LoadSettings()
        {
            
        }

        private void ImageSettings_MouseDown(object sender, MouseEventArgs e)
        {
            _contextMenu.Show(imageSettings);
        }

        public void CreateContent(Control control)
        {
            layout.AddRow(control);
        }

        public void AddCommand(Command com)
        {
            imageSettings.Visible = true;

            Commands.Add(com);
            _contextMenu.Items.Add(com.CreateMenuItem());
        }

        public void AddCommand(RadioCommand com)
        {
            imageSettings.Visible = true;

            Commands.Add(com);
            _contextMenu.Items.Add(com);
        }
    }
}

