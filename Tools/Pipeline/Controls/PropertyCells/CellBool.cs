// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    [CellAttribute(typeof(bool))]
    public class CellBool : CellBase
    {
        public override void Edit(PixelLayout control)
        {
            if (Value == null)
                Value = false;
            else
                Value = !((bool)Value);

            _eventHandler(Value, EventArgs.Empty);
        }
    }
}

