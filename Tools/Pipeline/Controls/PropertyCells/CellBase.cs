// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class CellAttribute : Attribute
    {
        public string Name;
        public Type Type;

        public CellAttribute(Type type)
        {
            Type = type;
        }
    }

    public class CellBase
    {
        public string Category { get; set; }
        public object Value { get; set; }
        public string DisplayValue { get; set; }
        public string Text { get; set; }
        public bool Editable { get; set; }
        public int Height { get; set; }
        public Action OnKill;

        protected EventHandler _eventHandler;
        protected Rectangle _lastRec;
        protected Type _type;

        public void Create(string category, string name, object value, Type type, EventHandler eventHandler = null)
        {
            Category = category;
            Value = value;
            DisplayValue = (value == null) ? "" : value.ToString();
            Text = name;
            Editable = true;
            Height = DrawInfo.TextHeight;

            _eventHandler = eventHandler;
            _type = type;

            OnCreate();
        }

        public virtual void OnCreate()
        {
            
        }

        public virtual void Edit(PixelLayout control)
        {

        }

        public virtual void Draw(Graphics g, Rectangle rec, int separatorPos, bool selected)
        {
            if (selected)
                g.FillRectangle(SystemColors.Highlight, rec);

            g.DrawText(SystemFonts.Default(), DrawInfo.GetTextColor(selected, false), rec.X + 5, rec.Y + (rec.Height - Height) / 2, Text);
            g.FillRectangle(DrawInfo.GetBackgroundColor(selected), separatorPos - 6, rec.Y, rec.Width, rec.Height);
            DrawCell(g, rec, separatorPos, selected);
        }

        public virtual void DrawCell(Graphics g, Rectangle rec, int separatorPos, bool selected)
        {
            _lastRec = rec;
            _lastRec.X += separatorPos;
            _lastRec.Width -= separatorPos - 1;

            g.DrawText(SystemFonts.Default(), DrawInfo.GetTextColor(selected, !Editable), separatorPos + 5, rec.Y + (rec.Height - Height) / 2, DisplayValue);
        }
    }
}

