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
        private class FileWatcher
        {
            PipelineController _controller;
            IView _view;

            public FileWatcher(PipelineController controller, IView view)
            {
                _controller = controller;
                _view = view;
            }

            Dictionary<string, FileSystemWatcher> externalWatchers = new Dictionary<string, FileSystemWatcher>();
            FileSystemWatcher projectWatcher = null;
            public void UpdateWatchers()
            {
                if (_controller._project == null)
                {
                    DisposeWatchers();
                    return;
                }

                if (projectWatcher == null)
                {
                    projectWatcher = createFileWatcher(_controller.ProjectLocation, true);
                }
                else if (!projectWatcher.Path.Equals(_controller.ProjectLocation))
                {
                    disposeWatcher(projectWatcher);
                    projectWatcher = createFileWatcher(_controller.ProjectLocation, true);
                }

                var externalFolders = _controller._project.ContentItems
                    .Select(itm => Path.GetFullPath(Path.Combine(_controller.ProjectLocation, itm.Location)))
                    .Where(w => !w.StartsWith(_controller.ProjectLocation)).Distinct().ToArray();


                foreach (var f in externalFolders)
                    if (!externalWatchers.ContainsKey(f))
                        externalWatchers.Add(f, createFileWatcher(f, false));


                var watchersToRemove = externalWatchers.Keys.Where(k => !externalFolders.Contains(k)).ToArray();

                foreach (var r in watchersToRemove)
                {
                    FileSystemWatcher w;
                    if (externalWatchers.TryGetValue(r, out w))
                    {
                        disposeWatcher(w);
                        externalWatchers.Remove(r);
                    }
                }
            }

            private void disposeWatcher(FileSystemWatcher w)
            {
                w.EnableRaisingEvents = false;
                w.Changed -= File_Changed;
                w.Renamed -= File_Removed;
                w.Deleted -= File_Removed;
                w.Dispose();
            }

            private FileSystemWatcher createFileWatcher(string path, bool includeSubdirectories)
            {
                var watcher = new FileSystemWatcher();
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Attributes;
                watcher.Filter = "*.*";
                watcher.Changed += File_Changed;
                watcher.Deleted += File_Removed;
                watcher.Renamed += File_Removed;
                watcher.Path = path;
                watcher.IncludeSubdirectories = includeSubdirectories;
                watcher.EnableRaisingEvents = true;
                return watcher;
            }

            private void File_Removed(object sender, FileSystemEventArgs e)
            {
                _view.Invoke(() =>
                {
                    var deletedItem = _controller._project.ContentItems.FirstOrDefault(item => Path.GetFullPath(Path.Combine(_controller.ProjectLocation, item.Location, item.Name)).Equals(e.FullPath));
                    if (deletedItem != null)
                    {
                        deletedItem.Exists = false;
                        _view.UpdateTreeItem(deletedItem);
                    }
                });
            }

            System.Collections.Concurrent.ConcurrentQueue<FileSystemEventArgs> modifiedItemCollection = new System.Collections.Concurrent.ConcurrentQueue<FileSystemEventArgs>();
            private Task scheduledBuildTask;

            private void File_Changed(object sender, FileSystemEventArgs e)
            {
                if (_controller.EnableAutoBuild)
                {


                    modifiedItemCollection.Enqueue(e);

                    if (this.scheduledBuildTask == null)
                        scheduleBuild();
                }
            }

            private void scheduleBuild(int delayms = 500)
            {
                this.scheduledBuildTask = Task.Delay(delayms);

                scheduledBuildTask.ContinueWith(t =>
                {
                    this.scheduledBuildTask = null;
                    //already dequeued
                    if (modifiedItemCollection.Count == 0)
                        return;

                    //if project is currently building and modifiedItem are present, schedule a new build task
                    if (_controller.ProjectBuilding)
                    {
                        scheduleBuild(1500);
                        return;
                    }

                    _view.Invoke(() =>
                    {

                        if (modifiedItemCollection.Count > 0)
                        {
                            List<IProjectItem> modifiedItems = new List<IProjectItem>();
                            FileSystemEventArgs ev;

                            while (modifiedItemCollection.TryDequeue(out ev))
                            {
                                var modifiedItem = _controller._project.ContentItems
                                .Select(itm => Tuple.Create(itm, Path.GetFullPath(Path.Combine(_controller.ProjectLocation, itm.Location, itm.Name))))
                                .FirstOrDefault(f => f.Item2.Equals(ev.FullPath));

                                if (modifiedItem != null && !modifiedItems.Contains(modifiedItem.Item1) && !IsFileLocked(modifiedItem.Item2))
                                    modifiedItems.Add(modifiedItem.Item1);

                                if (modifiedItem != null && modifiedItem.Item1.Exists == false)
                                {
                                    modifiedItem.Item1.Exists = true;
                                     _view.UpdateTreeItem(modifiedItem.Item1);
                                }
                            }

                            var finalList = modifiedItems.Distinct().ToArray();

                            if (finalList.Length > 0)
                                _controller.BuildItems(finalList, false);
                        }
                    });


                });
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

            public void DisposeWatchers()
            {
                if (projectWatcher != null)
                {
                    disposeWatcher(projectWatcher);
                    projectWatcher = null;
                }

                foreach (var w in this.externalWatchers.ToArray())
                {
                    this.externalWatchers.Remove(w.Key);
                    disposeWatcher(w.Value);
                }
            }
        }
    }
}
