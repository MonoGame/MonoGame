// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class CellPath : CellBase
    {
        public CellPath(string category, string name, object value, EventHandler eventHandler) : base(category, name, value, eventHandler)
        {
            if (value == null)
                Value = "";
        }

        public override void Edit(Control control)
        {
            var dialog = new PathDialog(PipelineController.Instance, Value.ToString());
            if (dialog.Run(control) == DialogResult.Ok && _eventHandler != null)
                _eventHandler(dialog.Path, EventArgs.Empty);
        }
    }
}

