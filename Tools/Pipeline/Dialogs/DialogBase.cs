// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class DialogBase : Dialog
    {
        private DialogResult _result;

        public DialogBase()
        {
            InitializeComponent();

            _result = DialogResult.Cancel;
        }

        public DialogResult Run(Window window)
        {
            this.ShowModal(window);
            return _result;
        }

        public DialogResult Run(Control control)
        {
            this.ShowModal(control);
            return _result;
        }
    }
}
