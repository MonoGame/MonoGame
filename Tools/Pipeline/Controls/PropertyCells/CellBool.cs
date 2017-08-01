// MonoGame - Copyright (C) The MonoGame Team
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
        private bool _draw;

        public CellBool()
        {
            _draw = true;
        }

        public override void Edit(PixelLayout control)
        {
            _draw = false;

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
                OnKill = null;

                if (_eventHandler == null || checkbox.Checked == null)
                    return;
                
                _draw = true;
                Value = checkbox.Checked;
                _eventHandler(Value, EventArgs.Empty);
            };
        }

        public override void DrawCell(Graphics g, Rectangle rec, int separatorPos, bool selected)
        {
            if (_draw)
                base.DrawCell(g, rec, separatorPos, selected);
        }
    }
}

