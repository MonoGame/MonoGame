// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class CellCombo : CellBase
    {
        public static int Height;

        private object _type;
        private Rectangle _lastRec;

        public CellCombo(string category, string name, object value, object type, EventHandler eventHandler) : base(category, name, value, eventHandler)
        {
            _type = type;

            if (value is ImporterTypeDescription)
                DisplayValue = (value as ImporterTypeDescription).DisplayName;
            else if (value is ProcessorTypeDescription)
                DisplayValue = (value as ProcessorTypeDescription).DisplayName;
        }

        public override void Edit(PixelLayout control)
        {
            var combo = new DropDown();

            if (_type is Enum)
            {
                var values = Enum.GetValues(_type.GetType());
                foreach (var v in values)
                {
                    combo.Items.Add(v.ToString());

                    if (Value != null && v.ToString() == Value.ToString())
                        combo.SelectedIndex = combo.Items.Count - 1;
                }
            }
            else if (_type is ImporterTypeDescription)
            {
                foreach (var v in PipelineTypes.Importers)
                {
                    combo.Items.Add(v.DisplayName);

                    if (Value != null && v.DisplayName == (Value as ImporterTypeDescription).DisplayName)
                        combo.SelectedIndex = combo.Items.Count - 1;
                }
            }
            else
            {
                foreach (var v in PipelineTypes.Processors)
                {
                    combo.Items.Add(v.DisplayName);

                    if (Value != null && v.DisplayName == (Value as ProcessorTypeDescription).DisplayName)
                        combo.SelectedIndex = combo.Items.Count - 1;
                }
            }

            Height = _lastRec.Height;
            combo.Style = "OverrideSize";
            combo.Width = _lastRec.Width + 1;
            combo.Height = _lastRec.Height;
            control.Add(combo, _lastRec.X, _lastRec.Y);

            combo.SelectedIndexChanged += delegate
            {
                if (_eventHandler == null || combo.SelectedIndex < 0)
                    return;

                if (_type is Enum)
                    _eventHandler(Enum.Parse(Value.GetType(), combo.SelectedValue.ToString()), EventArgs.Empty);
                else if (_type is ImporterTypeDescription)
                    _eventHandler(PipelineTypes.Importers[combo.SelectedIndex], EventArgs.Empty);
                else
                    _eventHandler(PipelineTypes.Processors[combo.SelectedIndex], EventArgs.Empty);

                control.Add(combo, _lastRec.X, _lastRec.Y);
            };
        }

        public override void Draw(Graphics g, Rectangle rec, int separatorPos, bool selected)
        {
            _lastRec = rec;
            _lastRec.X += separatorPos;
            _lastRec.Width -= separatorPos;

            base.Draw(g, rec, separatorPos, selected);
        }
    }
}

