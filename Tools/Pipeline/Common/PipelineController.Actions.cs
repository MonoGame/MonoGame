// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MonoGame.Tools.Pipeline
{
    internal partial class PipelineController : IController
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

                for (var i = 0; i < _files.Length; i++ )
                {
                    var f = _files[i];
                    if (!parser.AddContent(f, true))
                        continue;

                    var item = _con._project.ContentItems.Last();                    
                    item.Controller = _con;
                    item.ResolveTypes();

                    _files[i] = item.SourceFile;

                    _con._view.AddTreeItem(item);
                    _con._view.SelectTreeItem(item);
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
                        if (item.SourceFile == f)
                        {
                            _con._project.ContentItems.Remove(item);
                            _con._view.RemoveTreeItem(item);                            
                            break;
                        }
                    }
                    
                    // Error if item doesnt exist? Would only happen if our actionstack is messed up.
                }

                _con._view.EndTreeUpdate();
                _con.ProjectDiry = true;
            }
        }

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
                        if (item.SourceFile == obj.SourceFile)
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

        private class NewItemAction : IProjectAction
        {
            private readonly PipelineController _con;
            private readonly string _name;
            private readonly string _location;
            private readonly ContentItemTemplate _template;

            public NewItemAction(PipelineController controller, string name, string location, ContentItemTemplate template)
            {
                _con = controller;
                _name = name;
                _location = location;
                _template = template;                
            }

            public void Do()
            {
                var ext = Path.GetExtension(_template.TemplateFile);
                var filename = Path.ChangeExtension(_name, ext);
                var fullpath = Path.Combine(_location, filename);

                if (File.Exists(fullpath))
                {
                    _con._view.ShowError("Error", string.Format("File already exists: '{0}'.", fullpath));
                    return;
                }

                File.Copy(_template.TemplateFile, fullpath);

                var parser = new PipelineProjectParser(_con, _con._project);
                _con._view.BeginTreeUpdate();

                if (parser.AddContent(fullpath, true))
                {
                    var item = _con._project.ContentItems.Last();
                    item.Controller = _con;
                    item.ImporterName = _template.ImporterName;
                    item.ProcessorName = _template.ProcessorName;
                    item.ResolveTypes();
                    _con._view.AddTreeItem(item);
                    _con._view.SelectTreeItem(item);
                }

                _con._view.EndTreeUpdate();
                _con.ProjectDiry = true;
            }

            public void Undo()
            {
                var ext = Path.GetExtension(_template.TemplateFile);
                var filename = Path.ChangeExtension(_name, ext);
                var fullpath = Path.Combine(_location, filename);

                if (!File.Exists(fullpath))
                {
                    _con._view.ShowError("Error", string.Format("File does not exist: '{0}'.", fullpath));
                    return;
                }

                File.Delete(fullpath);
                
                _con._view.BeginTreeUpdate();

                for (var i = 0; i < _con._project.ContentItems.Count; i++)
                {
                    var item = _con._project.ContentItems[i];
                    var path = Path.GetFullPath(_con._project.Location + "\\" + item.SourceFile);

                    if (fullpath == path)
                    {
                        _con._project.ContentItems.Remove(item);
                        _con._view.RemoveTreeItem(item);
                    }
                }
                    
                _con._view.EndTreeUpdate();
                _con.ProjectDiry = true;
            }
        }            
    }
}