#if DESKTOPGL4
// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [NonParallelizable]
    [RunOnUiTestFixture]
    internal class NativeOpenGLTextureThreadingTests : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void BackgroundTextureOperations_QueuedToRenderThread_Complete()
        {
            const int width = 32;
            const int height = 32;

            Task worker = Task.Run(() =>
            {
                using (Texture2D texture = new Texture2D(gd, width, height))
                {
                    Color[] expectedPixels = new Color[width * height];
                    for (int index = 0; index < expectedPixels.Length; index++)
                        expectedPixels[index] = Color.MonoGameOrange;

                    texture.SetData(expectedPixels);

                    Color[] actualPixels = new Color[expectedPixels.Length];
                    texture.GetData(actualPixels);

                    CollectionAssert.AreEqual(expectedPixels, actualPixels);
                }
            });

            DateTime deadline = DateTime.UtcNow.AddSeconds(10);
            while (!worker.IsCompleted && DateTime.UtcNow < deadline)
            {
                gd.Clear(Color.Black);
                gd.Present();
                Thread.Sleep(1);
            }

            Assert.That(worker.IsCompleted, Is.True, "Background texture operations did not complete within ten seconds.");
            worker.GetAwaiter().GetResult();
        }
    }
}
#endif
