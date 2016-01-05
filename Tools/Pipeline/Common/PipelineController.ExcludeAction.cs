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
            private readonly string[] _folder;

            public ExcludeAction(PipelineController controller, IEnumerable<ContentItem> items, IEnumerable<string> folders)
            {
                _con = controller;
                _folder = (folders == null) ? new string[0] : folders.ToArray();

                if(items == null)
                    _state = new ContentItemState[0];
                else
                {
                    _state = new ContentItemState[items.Count()];
                    
                    var i = 0;
                    foreach (var item in items)
                    {
                        _state[i++] = ContentItemState.Get(item);
                    }
                }
            }

            public bool Do()
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

                foreach(string f in _folder)
                    _con.View.RemoveTreeFolder(f);

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;

                return true;
            }

            public bool Undo()
            {
                _con.View.BeginTreeUpdate();

                foreach(string f in _folder)
                    _con.View.AddTreeFolder(f);

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

                return true;
            }
        }
    }
}