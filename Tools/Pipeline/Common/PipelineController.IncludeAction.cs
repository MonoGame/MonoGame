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
        private class IncludeAction : IProjectAction
        {
            private readonly PipelineController _con;
            private readonly string[] _files;

            public IncludeAction(PipelineController controller, IEnumerable<string> files)
            {
                _con = controller;
                _files = files.ToArray();
            }

            public void Do()
            {
                var parser = new PipelineProjectParser(_con, _con._project);
                _con._view.BeginTreeUpdate();

                _con.Selection.Clear(_con);

                for (var i = 0; i < _files.Length; i++ )
                {
                    var f = _files[i];
                    if (!parser.AddContent(f, true))
                        continue;

                    var item = _con._project.ContentItems.Last();                    
                    item.Controller = _con;
                    item.ResolveTypes();

                    _files[i] = item.OriginalPath;

                    _con._view.AddTreeItem(item);                    
                    _con.Selection.Add(item, _con);                    
                }

                _con._view.EndTreeUpdate();
                _con.ProjectDiry = true;
            }

            public void Undo()
            {
                _con._view.BeginTreeUpdate();

                foreach (var f in _files)
                {
                    for (var i = 0; i < _con._project.ContentItems.Count; i++)
                    {
                        var item = _con._project.ContentItems[i];
                        if (item.OriginalPath == f)
                        {
                            _con._project.ContentItems.Remove(item);
                            _con.Selection.Remove(item, _con);
                            break;
                        }
                    }
                }

                _con._view.EndTreeUpdate();
                _con.ProjectDiry = true;
            }
        }
    }
}