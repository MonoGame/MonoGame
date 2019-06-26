// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonoGame.Tools.Pipeline
{
    public partial class PipelineController
    {        
        private class ExcludeAction : IProjectAction
        {
            private readonly PipelineController _con;
            private readonly List<IProjectItem> _items;
            private readonly List<ContentItem> _subitems;
            private readonly bool _delete;

            public ExcludeAction(PipelineController controller, List<IProjectItem> items, bool delete)
            {
                _items = new List<IProjectItem>();
                _subitems = new List<ContentItem>();

                _con = controller;
                _items.AddRange(items);
                _delete = delete;

                foreach (var item in items)
                    if (item is DirectoryItem)
                        foreach (var citem in _con._project.ContentItems)
                            if (citem.OriginalPath.StartsWith(item.OriginalPath))
                                _subitems.Add(citem);
            }

            public bool Do()
            {
                _con.View.BeginTreeUpdate();

                foreach (var item in _items)
                {
                    if (item is ContentItem)
                        _con._project.ContentItems.Remove(item as ContentItem);
                    _con.View.RemoveTreeItem(item);

                    if (_delete)
                    {
                        // Only delete if the item is in the project folder, otherwise we may (and have done!) delete files/folders outside of the project
                        if (!item.OriginalPath.Contains(".."))
                        {
                            var fullItemPath = _con.GetFullPath(item.OriginalPath);
                            try
                            {
                                if (item is DirectoryItem)
                                    Directory.Delete(_con.GetFullPath(item.OriginalPath), true);
                                else
                                    File.Delete(_con.GetFullPath(item.OriginalPath));
                            }
                            catch (FileNotFoundException)
                            {
                                // No error needed in case file is not found
                            }
                            catch (Exception ex)
                            {
                                _con.View.ShowError("Error while trying to delete the file", ex.Message);
                            }
                        }
                    }
                }

                foreach (var sitem in _subitems)
                    _con._project.ContentItems.Remove(sitem);

                //Since these items are removed from the project, manually clear the selection
                _con.SelectedItems.Clear();
                _con.SelectionChanged(_con.SelectedItems);

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;

                return true;
            }

            public bool Undo()
            {
                if (_delete)
                    return false;

                _con.View.BeginTreeUpdate();

                foreach (var item in _items)
                {
                    if(item is ContentItem)
                        _con._project.ContentItems.Add(item as ContentItem);
                    _con.View.AddTreeItem(item);
                }

                foreach (var item in _subitems)
                {
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
