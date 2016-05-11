// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class CellBase
    {
        public string Category { get; set; }
        public object Value { get; set; }
        public string DisplayValue { get; set; }
        public string Text { get; set; }
        public bool Editable { get; set; }

        internal EventHandler _eventHandler;

        public CellBase(string category, string name, object value, EventHandler eventHandler = null)
        {
            Category = category;
            Value = value;
            DisplayValue = (value == null) ? "" : value.ToString();
            Text = name;
            Editable = true;

            _eventHandler = eventHandler;
        }

        public virtual void Edit(Control control)
        {

        }

        public virtual void Draw(Graphics g, Rectangle rec, int separatorPos, bool selected)
        {
            if (selected)
                g.FillRectangle(SystemColors.Highlight, rec);

            g.DrawText(SystemFonts.Default(), PropInfo.GetTextColor(selected, false), rec.X + 5, rec.Y + (rec.Height - PropInfo.TextHeight) / 2, Text);
            g.FillRectangle(PropInfo.GetBackgroundColor(selected), separatorPos - 6, rec.Y, rec.Width, rec.Height);
            DrawCell(g, rec, separatorPos, selected);
        }

        public virtual void DrawCell(Graphics g, Rectangle rec, int separatorPos, bool selected)
        {
            g.DrawText(SystemFonts.Default(), PropInfo.GetTextColor(selected, !Editable), separatorPos + 5, rec.Y + (rec.Height - PropInfo.TextHeight) / 2, DisplayValue);
        }
    }
}

