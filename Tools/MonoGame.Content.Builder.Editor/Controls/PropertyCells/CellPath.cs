// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    [CellAttribute(typeof(string), Name = "IntermediateDir")]
    [CellAttribute(typeof(string), Name = "OutputDir")]
    public class CellPath : CellBase
    {
        public override void OnCreate()
        {
            HasDialog = true;
            if (Value == null)
                Value = "";
        }

        public override void Edit(PixelLayout control)
        {
            var dialog = new PathDialog(PipelineController.Instance, Value.ToString());
            if (dialog.Show(control) && _eventHandler != null)
                _eventHandler(dialog.Path, EventArgs.Empty);
        }
    }
}

