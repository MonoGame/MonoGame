// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    [CellAttribute(typeof(Microsoft.Xna.Framework.Color))]
    public class CellColor : CellBase
    {
        private Color color;

        public override void OnCreate()
        {
            HasDialog = true;

            if (Value != null)
            {
                var tmp = (Microsoft.Xna.Framework.Color)Value;
                color = new Color(tmp.R / 255f, tmp.G / 255f, tmp.B / 255f, tmp.A / 255f);
            }
            else
                color = new Color();
        }

        public override void Edit(PixelLayout control)
        {
            var dialog = new ColorDialog();
            dialog.Color = color;

            if (dialog.Show(control) == DialogResult.Ok && _eventHandler != null && dialog.Color != color)
            {
                var col = new Microsoft.Xna.Framework.Color(dialog.Color.Rb, dialog.Color.Gb, dialog.Color.Bb, dialog.Color.Ab);
                _eventHandler(col, EventArgs.Empty);
            }
        }

        public override void DrawCell(Graphics g, Rectangle rec, int separatorPos, bool selected)
        {
            var border = rec.Height / 5;
            g.FillRectangle(color, separatorPos + border, rec.Y + border, rec.Width - separatorPos - 2 * border, rec.Height - 2 * border);
        }
    }
}

