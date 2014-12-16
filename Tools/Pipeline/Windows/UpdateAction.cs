using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

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
            var item = (ContentItem)_con.GetItem(_state.SourceFile);
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

    internal class UpdateProjectAction : IProjectAction
    {
        private readonly IView _view;
        private readonly IController _con;
        private readonly bool _referencesChanged;

        private ProjectState _state;        

        public UpdateProjectAction(IView view, IController con, PipelineProject item, PropertyDescriptor property, object previousValue)
        {
            _view = view;
            _con = con;

            _state = ProjectState.Get(item);

            switch (property.Name)
            {
                case "OutputDir":
                    _state.OutputDir = (string)previousValue;
                    break;
                case "IntermediateDir":
                    _state.IntermediateDir = (string)previousValue;
                    break;
                case "References":
                    _state.References = new List<string>((List<string>)previousValue);
                    _referencesChanged = true;
                    break;
                case "Platform":
                    _state.Platform = (TargetPlatform)previousValue;
                    break;
                case "Profile":
                    _state.Profile = (GraphicsProfile)previousValue;
                    break;
                case "Config":
                    _state.Config = (string)previousValue;
                    break;
                case "OriginalPath":
                    _state.OriginalPath = (string)previousValue;
                    break;
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
            var item = (PipelineProject)_con.GetItem(_state.OriginalPath);
            var state = ProjectState.Get(item);
            _state.Apply(item);
            _state = state;

            if (_referencesChanged)
                _con.OnReferencesModified();
            else
                _con.OnProjectModified();

            _view.BeginTreeUpdate();
            _view.UpdateProperties(item);
            _view.UpdateTreeItem(item);
            _view.EndTreeUpdate();
        }
    }
}
