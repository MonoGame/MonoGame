// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    internal class GraphicsDeviceManagerTest
    {
        [Test]
        public void DefaultParameterValidation()
        {
            var game = new Game();
            var gdm = new GraphicsDeviceManager(game);

            // TODO: Some of these defaults will be different
            // per-platform... we will #if those cases in here
            // as we encounter them in the future.

            Assert.AreEqual(800, gdm.PreferredBackBufferWidth);
            Assert.AreEqual(480, gdm.PreferredBackBufferHeight);
            Assert.AreEqual(SurfaceFormat.Color, gdm.PreferredBackBufferFormat);
            Assert.AreEqual(DepthFormat.Depth24, gdm.PreferredDepthStencilFormat);
            Assert.False(gdm.IsFullScreen);
            Assert.False(gdm.PreferMultiSampling);
            Assert.AreEqual(GraphicsProfile.Reach, gdm.GraphicsProfile);
            Assert.True(gdm.SynchronizeWithVerticalRetrace);
            Assert.Null(gdm.GraphicsDevice);
            Assert.AreEqual(DisplayOrientation.Default, gdm.SupportedOrientations);

            game.Dispose();
        }

        [Test]
        public void PreparingDeviceSettings()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            gdm.PreparingDeviceSettings += (sender, args) =>
            {
                Assert.NotNull(args.GraphicsDeviceInformation);

                Assert.NotNull(args.GraphicsDeviceInformation.Adapter);
                Assert.AreEqual(GraphicsProfile.Reach, args.GraphicsDeviceInformation.GraphicsProfile);

                var pp = args.GraphicsDeviceInformation.PresentationParameters;
                Assert.NotNull(pp);

                Assert.AreEqual(800, pp.BackBufferWidth);
                Assert.AreEqual(480, pp.BackBufferHeight);
                Assert.AreEqual(SurfaceFormat.Color, pp.BackBufferFormat);
                Assert.AreEqual(DepthFormat.Depth24, pp.DepthStencilFormat);
                Assert.False(pp.IsFullScreen);
                Assert.AreEqual(PresentInterval.One, pp.PresentationInterval);
                Assert.AreEqual(new Rectangle(0, 0, 800, 480), pp.Bounds);
                Assert.AreNotEqual(IntPtr.Zero, pp.DeviceWindowHandle);
                Assert.AreEqual(DisplayOrientation.Default, pp.DisplayOrientation);
                Assert.AreEqual(RenderTargetUsage.DiscardContents, pp.RenderTargetUsage);
                Assert.AreEqual(0, pp.MultiSampleCount);
            };

            game.ExitCondition = x => x.DrawNumber > 1;
            game.Run();
            game.Dispose();
        }
    }
}