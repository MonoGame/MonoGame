// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading;

namespace MonoGame.Tools.Pipeline
{
    internal partial class PipelineController
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

                _thread = new Thread(new ThreadStart(ExistsThread));
                _thread.Start();
            }

            public void Stop()
            {
                if (_thread == null)
                    return;
                
                _thread.Abort();
                _thread = null;
            }

            private void ExistsThread()
            {
                while (true)
                {
                    // Can't lock without major code modifications
                    try
                    {
                        var items = _controller._project.ContentItems.ToArray();

                        foreach (var item in items)
                        {
                            Thread.Sleep(100);

                            if (item.Exists == File.Exists(_controller.GetFullPath(item.OriginalPath)))
                                continue;

                            item.Exists = !item.Exists;
                            _view.ItemExistanceChanged (item);
                        }
                    }
                    catch 
                    {
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

