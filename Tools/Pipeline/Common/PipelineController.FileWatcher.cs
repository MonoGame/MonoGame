// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MonoGame.Tools.Pipeline
{
    public partial class PipelineController
    {
        private class FileWatcher : IDisposable
        {
            Thread _thread;

            PipelineController _controller;
            IView _view;

            public FileWatcher(PipelineController controller, IView view)
            {
                _controller = controller;
                _view = view;
            }

            public void Run()
            {
                Stop();
                exit = false;
                _thread = new Thread(new ThreadStart(ExistsThread));
                _thread.Start();


            }

            Dictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

            public void UpdateWatchers(string[] locations)
            {
                foreach (var l in locations)
                {
                    if (!watchers.ContainsKey(l))
                    {
                        var watcher = new FileSystemWatcher();
                        watcher.NotifyFilter = NotifyFilters.LastWrite;
                        watcher.Filter = "*.*";
                        watcher.Changed += File_Changed;
                        watcher.Path = l;
                        watcher.IncludeSubdirectories = false;
                        watcher.EnableRaisingEvents = true;
                        watchers.Add(l, watcher);
                    }
                }
                var watchersToRemove = watchers.Keys.Where(k => !locations.Contains(k)).ToArray();

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

            System.Collections.Concurrent.BlockingCollection<ContentItem> modifiedItemCollection = new System.Collections.Concurrent.BlockingCollection<ContentItem>();
            private void File_Changed(object sender, FileSystemEventArgs e)
            {
                var modifiedItem = _controller._project.ContentItems.FirstOrDefault(item => Path.GetFullPath(Path.Combine(_controller.ProjectLocation, item.Location, item.Name)).Equals(e.FullPath));

                if (modifiedItem != null && !modifiedItemCollection.Contains(modifiedItem))
                    modifiedItemCollection.Add(modifiedItem);
            }

            public void Stop()
            {
                if (_thread == null)
                    return;

                exit = true;
                _thread.Join();
                _thread = null;

                UpdateWatchers(new string[0]);
            }

            volatile bool exit = false;

            private void ExistsThread()
            {
                while (!exit)
                {
                    // Can't lock without major code modifications
                    try
                    {
                        var items = _controller._project.ContentItems.ToArray();
                        var folders = items.Select(s => Path.Combine(_controller.ProjectLocation, s.Location)).Distinct().ToArray();
                        UpdateWatchers(folders);

                        if (modifiedItemCollection.Count != 0 && !_controller.ProjectBuilding)
                        {
                            List<IProjectItem> itemsToBuild = new List<IProjectItem>();
                            ContentItem item;
                            while (modifiedItemCollection.TryTake(out item))
                                itemsToBuild.Add(item);
                       
                            if (_controller.EnableAutoBuild)
                                _view.Invoke(() => _controller.BuildItems(itemsToBuild, false));
                        }



                        foreach (var item in items)
                        {
                            if (exit) return;
                            else Thread.Sleep(10);

                            if (item.Exists == File.Exists(_controller.GetFullPath(item.OriginalPath)))
                                continue;

                            item.Exists = !item.Exists;
                            _view.Invoke(() => _view.UpdateTreeItem(item));
                        }
                    }
                    catch (ThreadAbortException ex)
                    {
                        return;
                    }
                    catch(Exception ex)
                    {

                    }
                    finally
                    {
                        Thread.Sleep(500);
                    }

                }
            }

            public void Dispose()
            {
                Stop();
            }
        }
    }
}
