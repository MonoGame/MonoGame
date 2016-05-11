// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class CellBool : CellBase
    {
        public CellBool(string category, string name, object value, object type, EventHandler eventHandler) : base(category, name, value, eventHandler)
        {

        }

        public override void Edit(Control control)
        {
            if (Value == null)
                Value = false;
            else
                Value = !((bool)Value);

            _eventHandler(Value, EventArgs.Empty);
        }
    }
}

