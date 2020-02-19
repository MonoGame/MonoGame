// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    [CellAttribute(typeof(Enum))]
    [CellAttribute(typeof(ImporterTypeDescription))]
    [CellAttribute(typeof(ProcessorTypeDescription))]
    public class CellCombo : CellBase
    {
        public override void OnCreate()
        {
            if (Value is ImporterTypeDescription)
                DisplayValue = (Value as ImporterTypeDescription).DisplayName;
            else if (Value is ProcessorTypeDescription)
                DisplayValue = (Value as ProcessorTypeDescription).DisplayName;
        }

        public override void Edit(PixelLayout control)
        {
            var combo = new DropDown();

            if (_type.IsSubclassOf(typeof(Enum)))
            {
                var values = Enum.GetValues(_type);
                foreach (var v in values)
                {
                    combo.Items.Add(v.ToString());

                    if (Value != null && v.ToString() == Value.ToString())
                        combo.SelectedIndex = combo.Items.Count - 1;
                }
            }
            else if (_type == typeof(ImporterTypeDescription))
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

            combo.Style = "OverrideSize";
            combo.Width = _lastRec.Width;
            combo.Height = _lastRec.Height;
            control.Add(combo, _lastRec.X, _lastRec.Y);

            combo.SelectedIndexChanged += delegate
            {
                if (_eventHandler == null || combo.SelectedIndex < 0)
                    return;

                if (_type.IsSubclassOf(typeof(Enum)))
                    _eventHandler(Enum.Parse(_type, combo.SelectedValue.ToString()), EventArgs.Empty);
                else if (_type == typeof(ImporterTypeDescription))
                    _eventHandler(PipelineTypes.Importers[combo.SelectedIndex], EventArgs.Empty);
                else
                    _eventHandler(PipelineTypes.Processors[combo.SelectedIndex], EventArgs.Empty);

                combo.Enabled = true;
                control.Add(combo, _lastRec.X, _lastRec.Y);
            };
        }
    }
}

