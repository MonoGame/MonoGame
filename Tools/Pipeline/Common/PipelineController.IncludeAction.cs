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
            private readonly string[] _folder;
            private readonly string[] _refs;
            private readonly string[] _files;

            public IncludeAction(PipelineController controller, IEnumerable<string> files) : this(controller, files, null, null)
            {
                
            }

            public IncludeAction(PipelineController controller, IEnumerable<string> files, IEnumerable<string> folders, List<string> refs)
            {
                _con = controller;

                _files = files == null ? new string[0] : files.ToArray();
                _folder = folders == null ? new string[0] : folders.ToArray();
                _refs = refs == null ? new string[0] : refs.ToArray();

                for (int i = 0; i < _folder.Length; i++)
                {
                    if (Path.IsPathRooted(_folder[i]))
                    {
                        string projectloc = controller._project.Location;
                        if (_folder[i].Length >= projectloc.Length + 1)
                            _folder[i] = _folder[i].Substring(projectloc.Length + 1);
                    }

                    if(_folder[i].EndsWith(Path.DirectorySeparatorChar.ToString()))
                        _folder[i] = _folder[i].Remove(_folder[i].Length - 1);
                }
            }

            public void Do()
            {
                var parser = new PipelineProjectParser(_con, _con._project);
                _con.View.BeginTreeUpdate();

                _con.Selection.Clear(_con);

                foreach(string f in _folder)
                    if(f != "")
                        _con.View.AddTreeFolder(f);

                foreach (string r in _refs)
                {
                    _con._project.References.Add(r);
                    _con.View.AddTreeReference(r);
                }

                for (var i = 0; i < _files.Length; i++ )
                {
                    var f = _files[i];
                    if (!parser.AddContent(f, true))
                        continue;

                    var item = _con._project.ContentItems.Last();
                    item.Observer = _con;
                    item.ResolveTypes();

                    _files[i] = item.OriginalPath;

                    _con.View.AddTreeItem(item);
                    _con.Selection.Add(item, _con);
                }

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;
            }

            public void Undo()
            {
                _con.View.BeginTreeUpdate();

                foreach (var f in _files)
                {
                    for (var i = 0; i < _con._project.ContentItems.Count; i++)
                    {
                        var item = _con._project.ContentItems[i];
                        if (item.OriginalPath == f)
                        {
                            _con.View.RemoveTreeItem(item);
                            _con._project.ContentItems.Remove(item);
                            _con.Selection.Remove(item, _con);
                            break;
                        }
                    }
                }

                foreach(string f in _folder)
                    if(f != "")
                        _con.View.RemoveTreeFolder(f);

                foreach (string r in _refs)
                {
                    _con._project.References.Remove(r);
                    _con.View.RemoveTreeReference(r);
                }

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;
            }
        }
    }
}