// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class CellText : CellBase
    {
        public CellText(string category, string name, object value, EventHandler eventHandler, bool editable) : base(category, name, value, eventHandler)
        {
            Editable = editable && value is string;
        }

        public override void Edit(PixelLayout control)
        {
            var editText = new TextBox();
            editText.Style = "OverrideSize";
            editText.Width = _lastRec.Width;
            editText.Height = _lastRec.Height;
            editText.Text = Value.ToString();

            control.Add(editText, _lastRec.X, _lastRec.Y);

            editText.EnabledChanged += delegate
            {
                if (_eventHandler == null)
                    return;
                
                _eventHandler(editText.Text, EventArgs.Empty);
            };
        }
    }
}

