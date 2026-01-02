// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MonoGame.Tests.Audio
{
    [Category("Song")]
    public class SongTests
    {
        [SetUp]
        public void Setup()
        {
            // Necessary to get audio initialized.
            FrameworkDispatcher.Update();
        }

        [Test]
        public void SongPlayPauseStop()
        {
            var services = new GameServiceContainer();
            services.AddService<IGraphicsDeviceService>(new GraphicsDeviceProxy());
            var content = new ContentManagerProxy(services);

            var song = content.Load<Song>("Assets/Audio/Song/rock_loop_stereo");

            MediaPlayer.Play(song);
            SleepWhileDispatching(4000);
            Assert.AreEqual(MediaState.Playing, MediaPlayer.State);
            MediaPlayer.Pause();
            SleepWhileDispatching(500);
            Assert.AreEqual(MediaState.Paused, MediaPlayer.State);
            MediaPlayer.Stop();
            SleepWhileDispatching(500);
            Assert.AreEqual(MediaState.Stopped, MediaPlayer.State);
        }

        // Proxy for the content manager used in SoundEffectFromContent
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
        
        class ContentManagerProxy : ContentManager
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


        private static void SleepWhileDispatching(int ms)
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
