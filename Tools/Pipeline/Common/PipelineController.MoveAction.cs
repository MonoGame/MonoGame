// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace MonoGame.Tools.Pipeline
{
    internal partial class PipelineController
    {        
        private class MoveAction : IProjectAction
        {
            private readonly PipelineController _con;

            private readonly string path;
            private readonly string newpath;
            private readonly FileType type;

            public MoveAction(PipelineController controller, string path, string newpath, FileType type)
            {
                _con = controller;

                this.path = path;
                this.newpath = newpath;
                this.type = type;
            }

            private bool Move(string path, string newpath, FileType type)
            {
                _con.View.BeginTreeUpdate();

                if (type == FileType.File)
                {
                    var item = _con.GetItem(path) as ContentItem;
                    string fullpath = _con.GetFullPath(path);
                    string fullnewpath = _con.GetFullPath(newpath);

                    if (item == null)
                    {
                        _con.View.ShowError("Error", "An internal error has occured.");
                        return false;
                    }

                    try
                    {
                        if (File.Exists(fullnewpath))
                        {
                            _con.View.ShowError("Error", "File: \"" + fullnewpath + "\" already exists.");
                            return false;
                        }

                        File.Move(fullpath, fullnewpath);
                    }
                    catch
                    {
                        _con.View.ShowError("Error", "An error has occurred while trying to move a file.");
                        return false;
                    }

                    MoveFile(item, newpath);
                }
                else if (type == FileType.Folder)
                {
                    string fullpath = _con.GetFullPath(path);
                    string fullnewpath = _con.GetFullPath(newpath);

                    try
                    {
                        if (Directory.Exists(fullnewpath))
                        {
                            _con.View.ShowError("Error", "Directory: \"" + fullnewpath + "\" already exists.");
                            return false;
                        }

                        Directory.Move(fullpath, fullnewpath);
                    }
                    catch
                    {
                        _con.View.ShowError("Error", "An error has occurred while trying to move the directory.");
                        return false;
                    }

                    for (var i = 0; i < _con._project.ContentItems.Count; i++)
                    {
                        var item = _con._project.ContentItems[i];
                        if (item.OriginalPath.StartsWith(path))
                            MoveFile(item, newpath + item.OriginalPath.Substring(path.Length));
                    }
                    _con.View.RemoveTreeFolder(path);
                }
                else
                    _con.MoveProject(newpath);

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;

                return true;
            }

            private void MoveFile(ContentItem item, string newpath)
            {
                _con._project.ContentItems.Remove(item);
                _con.View.RemoveTreeItem(item);

                item.OriginalPath = newpath;
                item.ResolveTypes();

                _con._project.ContentItems.Add(item);
                _con.View.AddTreeItem(item);
            }

            public bool Do()
            {
                return Move(path, newpath, type);
            }

            public bool Undo()
            {
                return Move(newpath, path, type);
            }
        }
    }
}