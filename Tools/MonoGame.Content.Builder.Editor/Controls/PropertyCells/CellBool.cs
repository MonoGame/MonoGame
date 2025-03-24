// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    [CellAttribute(typeof(bool))]
    public class CellBool : CellBase
    {
        public override void Edit(PixelLayout control)
        {
            SkipCellDraw = true;

            var checkbox = new CheckBox();
            checkbox.Tag = this;
            checkbox.Checked = (bool?)Value;
            checkbox.ThreeState = (Value == null);
            checkbox.Text = (checkbox.Checked == null) ? "Not Set" : checkbox.Checked.ToString();
            checkbox.Width = _lastRec.Width - 10;
            checkbox.Height = _lastRec.Height;
            control.Add(checkbox, _lastRec.X + 10, _lastRec.Y);

            checkbox.CheckedChanged += (sender, e) => checkbox.Text = (checkbox.Checked == null) ? "Not Set" : checkbox.Checked.ToString();

            OnKill += delegate
            {
                SkipCellDraw = false;
                OnKill = null;

                if (_eventHandler == null || checkbox.Checked == null)
                    return;
                
                Value = checkbox.Checked;
                _eventHandler(Value, EventArgs.Empty);
            };
        }
    }
}

