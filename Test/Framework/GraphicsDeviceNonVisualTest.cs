using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    public class GraphicsDeviceNonVisualTest
    {
        private TestGameBase game;
        private GraphicsDeviceManager gdm;

        [SetUp]
        public void SetUp()
        {
            game = new TestGameBase();
            gdm = new GraphicsDeviceManager(game);

            game.InitializeOnly();
        }

        [TearDown]
        public void TearDown()
        {
            game.Dispose();
        }

        [Test]
        public void CtorPresentationParametersNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach, null));
        }

        [Test]
        public void DisposedWhenDisposingInvoked()
        {
            var gd = gdm.GraphicsDevice;

            var count = 0;

            gd.Disposing += (sender, args) =>
            {
                Assert.IsTrue(gd.IsDisposed);
                count++;
            };

            gd.Dispose();
            Assert.AreEqual(1, count);

            // Disposing should not be invoked more than once
            gd.Dispose();
            Assert.AreEqual(1, count);
        }

        [Test][Ignore]
        public void ResetInvokedBeforeDeviceLost()
        {
            game.InitializeOnly();

            var gd = game.GraphicsDevice;

            var resetCount = 0;
            var devLostCount = 0;

            var lostCount = 0;
            var tex = new RenderTarget2D(gdm.GraphicsDevice, 5, 5);
            tex.ContentLost += (sender, args) => lostCount++;

            gd.DeviceReset += (sender, args) =>
            {
                resetCount++;
                Assert.AreEqual(0, devLostCount);
            };

            gd.DeviceLost += (sender, args) =>
            {
                devLostCount++;
                Assert.AreEqual(1, resetCount);
            };

#if XNA
            gd.Reset();
#else
            gd.Reset(new PresentationParameters());
#endif

            Assert.AreEqual(1, lostCount);

            tex.Dispose();
        }

        [Test]
        public void ResetWindowHandleNullThrowsException()
        {
            game.InitializeOnly();

            var gd = game.GraphicsDevice;
            Assert.Throws<ArgumentException>(() => gd.Reset(new PresentationParameters()));
        }


    }
}