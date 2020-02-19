// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    [CellAttribute(typeof(string))]
    public class CellText : CellBase
    {
        public override void Edit(PixelLayout control)
        {
            var editText = new TextBox();
            editText.Tag = this;
            editText.Style = "OverrideSize";
            editText.Width = _lastRec.Width;
            editText.Height = _lastRec.Height;
            editText.Text = (Value == null) ? "" : Value.ToString();

            control.Add(editText, _lastRec.X, _lastRec.Y);

            editText.Focus();
            editText.CaretIndex = editText.Text.Length;
            
            OnKill += delegate
            {
                OnKill = null;

                if (_eventHandler == null)
                    return;

                _eventHandler(editText.Text, EventArgs.Empty);
            };

            editText.KeyDown += (sender, e) =>
            {
                if (e.Key == Keys.Enter)
                    OnKill.Invoke();
            };
        }
    }
}

