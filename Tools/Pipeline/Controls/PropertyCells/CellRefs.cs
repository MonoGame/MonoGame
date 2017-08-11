// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    [CellAttribute(typeof(List<string>), Name = "References")]
    public class CellRefs : CellBase
    {
        public override void OnCreate()
        {
            if (Value == null)
                Value = new List<string>();

            var list = Value as List<string>;
            var displayValue = "";

            foreach (var value in list)
                displayValue += Environment.NewLine + Path.GetFileNameWithoutExtension (value);

            DisplayValue = (Value as List<string>).Count > 0 ? displayValue.Trim(Environment.NewLine.ToCharArray()) : "None";
            Height = Height * Math.Max(list.Count, 1);
        }

        public override void Edit(PixelLayout control)
        {
            var dialog = new ReferenceDialog(PipelineController.Instance, (Value as List<string>).ToArray());
            if (dialog.ShowModal(control) && _eventHandler != null)
            {
                _eventHandler(dialog.References, EventArgs.Empty);
                PipelineController.Instance.OnReferencesModified();
            }
        }
    }
}

