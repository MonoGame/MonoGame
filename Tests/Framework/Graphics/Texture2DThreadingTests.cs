// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    [NonParallelizable]
#if DESKTOPGL
    [Ignore("GL doesn't work well with threads.")]
#endif
    class Texture2DThreadingTests : GraphicsDeviceTestFixtureBase
    {
        [Test]
        [RunOnUI]
        public void CreateSetAndGetData()
        {
            const int Width = 32;
            const int Height = 32;
            var fillColor = Color.MonoGameOrange;

            Exception threadException = null;
            Color[] readBack = null;

            var thread = new Thread(() =>
            {
                try
                {
                    using var texture = new Texture2D(gd, Width, Height);

                    var pixels = new Color[Width * Height];
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i] = fillColor;

                    texture.SetData(pixels);

                    readBack = new Color[Width * Height];
                    texture.GetData(readBack);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            thread.Start();

            // Wait for 10 seconds for it to finish.
            var finished = thread.Join(TimeSpan.FromSeconds(10));

            Assert.True(finished, "Threaded SetData/GetData failed!");
            Assert.IsNull(threadException, $"Thread threw an exception: {threadException}");
            Assert.NotNull(readBack, "GetData failed!");            
            Assert.True(readBack.All(c => c == Color.MonoGameOrange), "Read color data does not match!");
        }

        [Test]
        [RunOnUI]
        public void BackgroundLoading()
        {
            const int COUNT = 400;
            const int WIDTH = 128;
            const int HEIGHT = 128;

            var loaded = new List<Texture2D>();
            var barrier = new Barrier(2);

            Exception threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    int count = COUNT;
                    while (count-- > 0)
                    {
                        var tex = new Texture2D(gd, WIDTH, HEIGHT);

                        var pixels = new Color[WIDTH * HEIGHT];
                        for (int i = 0; i < pixels.Length; i++)
                            pixels[i] = Color.MonoGameOrange;

                        barrier.SignalAndWait();
                        tex.SetData(pixels);
                        tex.SetData(pixels);
                        loaded.Add(tex);
                    }
                }
                catch (Exception ex)
                {                    
                    threadException = ex;
                }

                barrier.RemoveParticipant();
            });

            thread.Start();

            int frames = 0;

            while (frames < 2000)
            {
                barrier.SignalAndWait();
                gd.Clear(Color.MonoGameOrange);
                barrier.SignalAndWait();
                gd.Present();
                ++frames;

                if (!thread.IsAlive)
                    break;
            }

            Assert.Null(threadException, $"Thread threw exception: {threadException}");
            Assert.True(frames > 1, "Should have presented more than once!");
            Assert.AreEqual(COUNT, loaded.Count, "Some textures failed to load!");

            foreach (var tex in loaded)
                tex.Dispose();
        }
    }
}
