// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonoGame.Tools.Pipeline
{
    internal partial class PipelineController
    {        
        private class ExcludeAction : IProjectAction
        {
            private readonly PipelineController _con;
            private readonly ContentItemState[] _state;

            public ExcludeAction(PipelineController controller, IEnumerable<ContentItem> items)
            {
                _con = controller;
                
                _state = new ContentItemState[items.Count()];
                
                var i = 0;
                foreach (var item in items)
                {
                    _state[i++] = ContentItemState.Get(item);
                }
            }

            public void Do()
            {
                _con._view.BeginTreeUpdate();

                foreach (var obj in _state)
                {
                    for (var i = 0; i < _con._project.ContentItems.Count; i++)
                    {
                        var item = _con._project.ContentItems[i];
                        if (item.OriginalPath == obj.SourceFile)
                        {
                            _con._project.ContentItems.Remove(item);
                            _con._view.RemoveTreeItem(item);
                            break;
                        }
                    }
                }

                _con._view.EndTreeUpdate();
                _con.ProjectDiry = true;
            }

            public void Undo()
            {
                _con._view.BeginTreeUpdate();

                foreach (var obj in _state)
                {
                    var item = new ContentItem()
                        {
                            Controller = _con,
                        };
                    obj.Apply(item);
                    item.ResolveTypes();

                    _con._project.ContentItems.Add(item);
                    _con._view.AddTreeItem(item);                        
                }

                _con._view.EndTreeUpdate();
                _con.ProjectDiry = true;
            }
        }
    }
}