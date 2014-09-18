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
        private class NewAction : IProjectAction
        {
            private readonly PipelineController _con;
            private readonly string _name;
            private readonly string _location;
            private readonly ContentItemTemplate _template;

            public NewAction(PipelineController controller, string name, string location, ContentItemTemplate template)
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

                _con.Selection.Clear(_con);

                if (parser.AddContent(fullpath, true))
                {
                    var item = _con._project.ContentItems.Last();
                    item.Observer = _con;
                    item.ImporterName = _template.ImporterName;
                    item.ProcessorName = _template.ProcessorName;
                    item.ResolveTypes();

                    _con._view.AddTreeItem(item);
                    _con.Selection.Add(item, _con);
                }

                _con._view.EndTreeUpdate();
                _con.ProjectDirty = true;
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
                    var path = Path.GetFullPath(_con._project.Location + "\\" + item.OriginalPath);

                    if (fullpath == path)
                    {
                        _con._project.ContentItems.Remove(item);
                        _con._view.RemoveTreeItem(item);
                        _con.Selection.Remove(item, _con);
                    }
                }
                    
                _con._view.EndTreeUpdate();
                _con.ProjectDirty = true;
            }
        }            
    }
}