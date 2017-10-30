// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace MonoGame.Tools.Pipeline
{
    public partial class PipelineController
    {
        private class IncludeAction : IProjectAction
        {
            private readonly PipelineController _con;
            private readonly string[] _folder;
            private readonly string[] _files;

            public IncludeAction(PipelineController controller, IEnumerable<string> files) : this(controller, files, null)
            {
                
            }

            public IncludeAction(PipelineController controller, IEnumerable<string> files, IEnumerable<string> folders)
            {
                _con = controller;

                _files = files == null ? new string[0] : files.ToArray();
                _folder = folders == null ? new string[0] : folders.ToArray();

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

            public bool Do()
            {
                var parser = new PipelineProjectParser(_con, _con._project);
                _con.View.BeginTreeUpdate();

                foreach(string f in _folder)
                    if(f != "")
                        _con.View.AddTreeItem(new DirectoryItem(f));

                for (var i = 0; i < _files.Length; i++ )
                {
                    bool skipduplicate = false;

                    switch (Environment.OSVersion.Platform) 
                    {
                        case PlatformID.Win32NT:
                        case PlatformID.Win32S:
                        case PlatformID.Win32Windows:
                        case PlatformID.WinCE:
                            skipduplicate = true;
                            break;
                    }

                    var f = _files[i];
                    if (!parser.AddContent(f, skipduplicate))
                        continue;

                    var item = _con._project.ContentItems.Last();
                    item.Observer = _con;
                    item.ResolveTypes();

                    _files[i] = item.OriginalPath;

                    _con.View.AddTreeItem(item);

                    item.ExpandToThis = true;
                    _con.View.UpdateTreeItem(item);
                }

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;

                return true;
            }

            public bool Undo()
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
                            break;
                        }
                    }
                }

                foreach(string f in _folder)
                    if(f != "")
                        _con.View.RemoveTreeItem(new DirectoryItem(f));

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;

                return true;
            }
        }
    }
}