// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tools.Pipeline
{
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
