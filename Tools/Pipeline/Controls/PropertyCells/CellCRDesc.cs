// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    [CellAttribute(typeof(CharacterRegionsDescription))]
    public class CellCRDesc : CellBase
    {
        public override void OnCreate()
        {
            DisplayValue = "Collection";
        }

        public override void Edit(PixelLayout control)
        {
            var dialog = new CRDescDialog(Value as CharacterRegionsDescription);
            if (dialog.Run(control) == DialogResult.Ok)
            {
                Value = dialog.CharRegions;
                _eventHandler(Value, EventArgs.Empty);
            }
        }
    }
}

