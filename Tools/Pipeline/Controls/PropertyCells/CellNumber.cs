// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class CellNumber : CellBase
    {
        public CellNumber(string category, string name, object value, EventHandler eventHandler) : base(category, name, value, eventHandler)
        {
            DisplayValue = ((float)value).ToString("0.00");
            DisplayValue = (DisplayValue.Length > Value.ToString().Length) ? DisplayValue : Value.ToString();
        }

        public override void Edit(PixelLayout control)
        {
            var editText = new TextBox();
            editText.Style = "OverrideSize";
            editText.Width = _lastRec.Width;
            editText.Height = _lastRec.Height;
            editText.Text = DisplayValue;

            control.Add(editText, _lastRec.X, _lastRec.Y);

            editText.Focus();
            editText.CaretIndex = editText.Text.Length;

            editText.EnabledChanged += delegate
            {
                if (_eventHandler == null)
                    return;

                try
                {
                    _eventHandler(float.Parse(editText.Text), EventArgs.Empty);
                }
                catch { }
            };
        }
    }
}

