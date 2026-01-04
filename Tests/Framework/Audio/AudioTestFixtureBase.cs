// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MonoGame.Tests.Audio
{
    abstract public class AudioTestFixtureBase
    {
        protected ContentManagerProxy _content;

        [SetUp]
        public void Setup()
        {
            // Necessary to get audio initialized.
            FrameworkDispatcher.Update();

            var services = new GameServiceContainer();
            services.AddService<IGraphicsDeviceService>(new GraphicsDeviceProxy());
            _content = new ContentManagerProxy(services);
        }


        [TearDown]
        public void Teardown()
        {
            _content.Dispose();
        }


        class GraphicsDeviceProxy : IGraphicsDeviceService
        {
            public GraphicsDevice GraphicsDevice
            {
                get { return null; }
            }

            public event EventHandler<EventArgs> DeviceCreated;
            public event EventHandler<EventArgs> DeviceDisposing;
            public event EventHandler<EventArgs> DeviceReset;
            public event EventHandler<EventArgs> DeviceResetting;
        }

        protected class ContentManagerProxy : ContentManager
        {
            public ContentManagerProxy(IServiceProvider services) : base(services) { }

            protected override Stream OpenStream(string assetName)
            {
                var fileName = Path.Combine(RootDirectory, assetName + ".xnb");
                if (File.Exists(fileName))
                    return new FileStream(fileName, FileMode.Open, FileAccess.Read);
                return base.OpenStream(assetName);
            }
        }

        protected static void SleepWhileDispatching(int ms)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int cycles = ms / 10;
            for (int i = 0; i < cycles; i++)
            {
                FrameworkDispatcher.Update();
                Thread.Sleep(10);

                if (stopwatch.Elapsed.TotalMilliseconds > ms)
                {
                    stopwatch.Stop();
                    break;
                }
            }
        }
    }
}
