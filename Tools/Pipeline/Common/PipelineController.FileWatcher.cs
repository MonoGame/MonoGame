// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonoGame.Tools.Pipeline
{
    public partial class PipelineController
    {
        private class FileWatcher : IDisposable
        {
            PipelineController _controller;
            IView _view;

            public FileWatcher(PipelineController controller, IView view)
            {
                _controller = controller;
                _view = view;
                checkModificationTimer = new System.Timers.Timer(500);
                checkModificationTimer.Elapsed += _buildModified;
            }

            Dictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

            void updateWatchers()
            {
                if (_controller._project == null)
                    return;

                var items = _controller._project.ContentItems.ToArray();
                var folders = items.Select(s => Path.GetFullPath(Path.Combine(_controller.ProjectLocation, s.Location))).Distinct().ToArray();


                foreach (var l in folders)
                {
                    if (!watchers.ContainsKey(l))
                    {
                        var watcher = new FileSystemWatcher();
                        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Attributes;
                        watcher.Filter = "*.*";
                        watcher.Changed += File_Changed;
                        watcher.Deleted += File_Removed;
                        watcher.Renamed += File_Removed;
                        watcher.Path = l;
                        watcher.IncludeSubdirectories = false;
                        watcher.EnableRaisingEvents = true;
                        watchers.Add(l, watcher);
                    }
                }
                var watchersToRemove = watchers.Keys.Where(k => !folders.Contains(k)).ToArray();

                foreach (var r in watchersToRemove)
                {
                    FileSystemWatcher w;
                    if (watchers.TryGetValue(r, out w))
                    {
                        w.EnableRaisingEvents = false;
                        w.Dispose();
                        watchers.Remove(r);
                    }
                }
            }

            private void File_Removed(object sender, FileSystemEventArgs e)
            {
                var deletedItem = _controller._project.ContentItems.FirstOrDefault(item => Path.GetFullPath(Path.Combine(_controller.ProjectLocation, item.Location, item.Name)).Equals(e.FullPath));
                if (deletedItem != null)
                {
                    deletedItem.Exists = false;
                    _view.Invoke(() => _view.UpdateTreeItem(deletedItem));
                }
            }

            System.Collections.Concurrent.BlockingCollection<IProjectItem> modifiedItemCollection = new System.Collections.Concurrent.BlockingCollection<IProjectItem>();

            private void File_Changed(object sender, FileSystemEventArgs e)
            {
                var modifiedItem = _controller._project.ContentItems.FirstOrDefault(item => Path.GetFullPath(Path.Combine(_controller.ProjectLocation, item.Location, item.Name)).Equals(e.FullPath));

                if (_controller.EnableAutoBuild && modifiedItem != null && !modifiedItemCollection.Contains(modifiedItem))
                        modifiedItemCollection.Add(modifiedItem);

                if(modifiedItem != null && modifiedItem.Exists == false)
                {
                    modifiedItem.Exists = true;
                    _view.Invoke(() => _view.UpdateTreeItem(modifiedItem));
                }
            }

            private void _buildModified(object sender, System.Timers.ElapsedEventArgs e)
            {
                checkModificationTimer.Stop();
                updateWatchers();

                if (modifiedItemCollection.Count != 0 && !_controller.ProjectBuilding)
                {
                    List<IProjectItem> itemList = new List<IProjectItem>();
                    IProjectItem item;
                    while (modifiedItemCollection.TryTake(out item))
                        itemList.Add(item);

                    //check if another process is still writing the file, let's enqueue the item back
                    foreach (var itm in itemList.ToArray())
                    {
                        var path = Path.GetFullPath(Path.Combine(_controller.ProjectLocation, itm.Location, itm.Name));
                        if (IsFileLocked(path))
                        {
                            itemList.Remove(itm);
                            modifiedItemCollection.Add(itm);
                        }

                    }
                    var finalList = itemList.Distinct().ToArray();

                    if (finalList.Length > 0)
                        _view.Invoke(() => _controller.BuildItems(finalList, false));
                }
                checkModificationTimer.Start();
            }

            protected virtual bool IsFileLocked(string file)
            {
                FileStream stream = null;

                try
                {
                    stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
                }
                catch (IOException)
                {
                    return true;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }

                return false;
            }

            System.Timers.Timer checkModificationTimer;

            public void Start()
            {
                if (!checkModificationTimer.Enabled)
                    checkModificationTimer.Start();
            }

            public void Stop()
            {
                if (checkModificationTimer.Enabled)
                    checkModificationTimer.Stop();
            }

            public void Dispose()
            {
                checkModificationTimer.Dispose();
                checkModificationTimer = null;
                foreach (var w in this.watchers.ToArray())
                {
                    this.watchers.Remove(w.Key);
                    w.Value.Dispose();
                }
            }
        }
    }
}
