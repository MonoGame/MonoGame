using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MonoGame.Tools.Pipeline
{
    internal class UpdateContentItemAction : IProjectAction
    {
        private readonly IView _view;
        private readonly IController _con;
        private ContentItemState _state;

        public UpdateContentItemAction(IView view, IController con, ContentItem item, PropertyDescriptor property, object previousValue)
        {
            _view = view;
            _con = con;

            _state = ContentItemState.Get(item);

            var name = property.Name;
            var value = previousValue;

            if (name == "Importer")
            {
                name = "ImporterName";
                value = ((ImporterTypeDescription)value).TypeName;
            }

            if (name == "Processor")
            {
                name = "ProcessorName";
                value = ((ProcessorTypeDescription)value).TypeName;
            }

            var field = _state.GetType().GetMember(name).SingleOrDefault() as FieldInfo;
            if (field == null)
            {
                if (!_state.ProcessorParams.ContainsKey(name))
                    throw new Exception();

                _state.ProcessorParams[name] = value;
            }
            else
            {
                field.SetValue(_state, value);
            }
        }

        public void Do()
        {
            Toggle();
        }

        public void Undo()
        {
            Toggle();
        }

        private void Toggle()
        {
            var item = _con.GetItem(_state.SourceFile);
            var state = ContentItemState.Get(item);
            _state.Apply(item);
            _state = state;

            item.ResolveTypes();

            _view.BeginTreeUpdate();
            _view.UpdateProperties(item);
            _view.UpdateTreeItem(item);
            _view.EndTreeUpdate();
        }
    }
}
