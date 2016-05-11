// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class CellCombo : CellBase
    {
        private object _type;

        public CellCombo(string category, string name, object value, object type, EventHandler eventHandler) : base(category, name, value, eventHandler)
        {
            _type = type;

            if (value is ImporterTypeDescription)
                DisplayValue = (value as ImporterTypeDescription).DisplayName;
            else if (value is ProcessorTypeDescription)
                DisplayValue = (value as ProcessorTypeDescription).DisplayName;
        }

        public override void Edit(Control control)
        {
            var dialog = new DialogBase();
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

            dialog.CreateContent(combo);
            if (dialog.Run(control) != DialogResult.Ok || _eventHandler == null || combo.SelectedIndex < 0)
                return;

            if (_type is Enum)
                _eventHandler(Enum.Parse(Value.GetType(), combo.SelectedValue.ToString()), EventArgs.Empty);
            else if (_type is ImporterTypeDescription)
                _eventHandler(PipelineTypes.Importers[combo.SelectedIndex], EventArgs.Empty);
            else
                _eventHandler(PipelineTypes.Processors[combo.SelectedIndex], EventArgs.Empty);
        }
    }
}

