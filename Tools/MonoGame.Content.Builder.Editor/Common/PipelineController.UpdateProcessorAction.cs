// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace MonoGame.Tools.Pipeline
{
    public class UpdateProcessorAction : IProjectAction
    {
        private readonly IView _view;
        private readonly List<ContentItem> _objects;
        private readonly string _property;

        private List<object> _values;

        public UpdateProcessorAction(IView view, List<ContentItem> objects, string property, object value)
        {
            _view = view;
            _objects = objects;
            _property = property;

            _values = new List<object>();
            for (int i = 0; i < _objects.Count; i++)
                _values.Add(value);
        }

        public bool Do()
        {
            Toggle();
            return true;
        }

        public bool Undo()
        {
            Toggle();
            return true;
        }

        private void Toggle()
        {
            var oldValues = new List<object>();

            for (int i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];

                oldValues.Add(obj.ProcessorParams[_property]);
                obj.ProcessorParams[_property] = _values[i];
            }

            _view.UpdateProperties();
            _values = oldValues;
        }
    }
}
