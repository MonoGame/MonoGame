// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class CellRefs : CellBase
    {
        public CellRefs(string category, string name, object value, EventHandler eventHandler) : base(category, name, value, eventHandler)
        {
            if (Value == null)
                Value = new List<string>();

            DisplayValue = (Value as List<string>).Count > 0 ? "Collection" : "None";
        }

        public override void Edit(Control control)
        {
            var dialog = new ReferenceDialog(PipelineController.Instance, (Value as List<string>).ToArray());
            if (dialog.Run(control) == DialogResult.Ok && _eventHandler != null)
                _eventHandler(dialog.References, EventArgs.Empty);
        }
    }
}

