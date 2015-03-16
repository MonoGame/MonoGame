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
                _con.View.BeginTreeUpdate();

                foreach (var obj in _state)
                {
                    for (var i = 0; i < _con._project.ContentItems.Count; i++)
                    {
                        var item = _con._project.ContentItems[i];
                        if (item.OriginalPath == obj.SourceFile)
                        {
                            _con._project.ContentItems.Remove(item);
                            _con.View.RemoveTreeItem(item);
                            break;
                        }
                    }
                }

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;
            }

            public void Undo()
            {
                _con.View.BeginTreeUpdate();

                foreach (var obj in _state)
                {
                    var item = new ContentItem()
                        {
                            Observer = _con,
                            Exists = File.Exists(System.IO.Path.GetDirectoryName(_con._project.OriginalPath) + Path.DirectorySeparatorChar + obj.SourceFile)
                        };
                    obj.Apply(item);
                    item.ResolveTypes();

                    _con._project.ContentItems.Add(item);
                    _con.View.AddTreeItem(item);
                }

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;
            }
        }
    }
}