// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.IO;
using System;

namespace MonoGame.Tools.Pipeline
{
    public partial class PipelineController
    {
        private class IncludeAction : IProjectAction
        {
            private readonly PipelineController _con;
            private readonly List<IncludeItem> _includes;
            private readonly List<IProjectItem> _items;
            private bool _firsttime;

            public IncludeAction(List<IncludeItem> includes)
            {
                _items = new List<IProjectItem>();
                _con = Instance;
                _includes = includes;
                _firsttime = true;
            }

            public IncludeAction(IncludeItem item) : this(new List<IncludeItem> { item })
            {

            }

            /// <summary>
            /// Prepares the item list and creates all the neccecary files.
            /// </summary>
            public void FirstDo()
            {
                var parser = new PipelineProjectParser(_con, _con._project);

                for (int i = 0; i < _includes.Count; i++)
                {
                    var item = _includes[i].IsDirectory ?
                        (IProjectItem)new DirectoryItem("") : new ContentItem();

                    if (_includes[i].IncludeType == IncludeType.Create ||
                        _includes[i].IncludeType == IncludeType.Link)
                    {
                        item.OriginalPath = Util.GetRelativePath(_includes[i].SourcePath, _con.ProjectLocation);
                        item.DestinationPath = _includes[i].RelativeDestPath;

                        if (_includes[i].IncludeType == IncludeType.Create)
                        {
                            if (_includes[i].IsDirectory)
                                Directory.CreateDirectory(_includes[i].SourcePath);
                            else
                            {
                                var conitem = item as ContentItem;
                                var template = _includes[i].ItemTemplate;

                                conitem.ImporterName = template.ImporterName;
                                conitem.ProcessorName = template.ProcessorName;

                                Directory.CreateDirectory(Path.GetDirectoryName(_includes[i].SourcePath));
                                File.Copy(template.TemplateFile, _includes[i].SourcePath);
                            }
                        }
                    }
                    else if (!_includes[i].IsDirectory) // This is only a valid action for files
                    {
                        var sourcePath = _includes[i].SourcePath;
                        var destPath = Path.Combine(_con.ProjectLocation, _includes[i].RelativeDestPath);

                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                        File.Copy(sourcePath, destPath);

                        item.OriginalPath = item.DestinationPath = _includes[i].RelativeDestPath;
                    }

                    var citem = item as ContentItem;
                    if (citem != null)
                    {
                        citem.ProcessorParams = new Microsoft.Xna.Framework.Content.Pipeline.OpaqueDataDictionary();
                        citem.Observer = _con;

                        citem.ResolveTypes();
                    }

                    // Always keep Unix slashes in the .mgcb files for cross platform compatibility
                    item.OriginalPath = item.OriginalPath.Replace('\\', '/');
                    item.DestinationPath = item.DestinationPath.Replace('\\', '/');

                    _items.Add(item);
                }
            }

            public bool Do()
            {
                if (_firsttime)
                {
                    // Generate file list and populate item list
                    _firsttime = false;

                    try
                    {
                        FirstDo();
                    }
                    catch (Exception ex)
                    {
                        _con.View.ShowError(
                            "Include Action Error",
                            "The include action has failed for the following reason: " +
                            Environment.NewLine + ex
                        );
                        return false;
                    }
                }

                _con.View.BeginTreeUpdate();

                foreach (var item in _items)
                {
                    if (item is ContentItem)
                        _con._project.ContentItems.Add(item as ContentItem);

                    _con.View.AddTreeItem(item);
                }

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;

                return true;
            }

            public bool Undo()
            {
                _con.View.BeginTreeUpdate();

                foreach (var item in _items)
                {
                    if (item is ContentItem)
                        _con._project.ContentItems.Remove(item as ContentItem);

                    _con.View.RemoveTreeItem(item);
                }

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;

                return true;
            }
        }
    }
}
