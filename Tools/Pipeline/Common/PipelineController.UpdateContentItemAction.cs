// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;

namespace MonoGame.Tools.Pipeline
{
    internal class UpdateContentItemAction : IProjectAction
    {
        private readonly IView _view;
        private readonly IController _con;
        private List<ContentItemState> _states;

        public UpdateContentItemAction(IView view, IController con, IEnumerable<ContentItemState> states)
        {
            _view = view;
            _con = con;
            _states = states.ToList();
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
            for (int i = 0; i < _states.Count; i++)
            {
                var item = (ContentItem)_con.GetItem(_states[i].SourceFile);
                var state = ContentItemState.Get(item);
                _states[i].Apply(item);
                _states[i] = state;

                item.ResolveTypes();

                _view.BeginTreeUpdate();
                _view.UpdateProperties(item);
                _view.UpdateTreeItem(item);
                _view.EndTreeUpdate();
            }
        }
    }
}
