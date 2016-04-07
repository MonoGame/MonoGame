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

        [TestCase(false)]
        [TestCase(true)]
        public void MSAAEnabled(bool enabled)
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);
            gdm.PreferMultiSampling = enabled;
            gdm.GraphicsProfile = GraphicsProfile.HiDef;

            gdm.PreparingDeviceSettings += (sender, args) =>
            {
                var pp = args.GraphicsDeviceInformation.PresentationParameters;
                if (!enabled)
                    Assert.AreEqual(0, pp.MultiSampleCount);
                else
                {
                    Assert.Less(0, pp.MultiSampleCount);
                    pp.MultiSampleCount = 1024;
                }
            };

            Texture2D tex = null;
            SpriteBatch spriteBatch = null;

            game.InitializeWith += (sender, args) =>
            {
                tex = new Texture2D(game.GraphicsDevice, 1, 1);
                tex.SetData(new[] { Color.White.PackedValue });
                spriteBatch = new SpriteBatch(game.GraphicsDevice);
            };

            game.PreDrawWith += (sender, args) =>
            {
                if (enabled)
                {
                    var pp = game.GraphicsDevice.PresentationParameters;
                    Assert.Less(0, pp.MultiSampleCount);
                    Assert.AreNotEqual(1024, pp.MultiSampleCount);
                }

                game.GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin();
                spriteBatch.Draw(tex, new Vector2(800 / 2, 480 / 2), null, Color.White, MathHelper.ToRadians(45), new Vector2(0.5f), 200, SpriteEffects.None, 0);
                spriteBatch.End();
            };

#if XNA
            var data = new Color[800 * 480];
            game.DrawWith += (sender, args) =>
            {
                game.GraphicsDevice.GetBackBufferData(data);
            };
#endif

            game.ExitCondition = x => x.DrawNumber > 1;
            game.Run();

#if XNA
            float black = 0;
            float white = 0;
            float grey = 0;
            foreach (var c in data)
            {
                if (c == Color.Black)
                    ++black;
                else if (c == Color.White)
                    ++white;
                else if (c.R == c.G && c.G == c.B && c.R > 0 && c.R < 255)
                    ++grey;
            }

            // General percentage of black and white pixels we should be getting.
            black /= data.Length;
            white /= data.Length;
            Assert.Less(black, 0.9f);
            Assert.Greater(black, 0.8f);
            Assert.Less(white, 0.2f);
            Assert.Greater(white, 0.1f);

            // If enabled we should have at least a few grey pixels
            // else we should have zero grey pixels.
            grey /= data.Length;
            if (!enabled)
                Assert.AreEqual(0, grey);
            else
            {
                Assert.Less(grey, 0.01f);
                Assert.Greater(grey, 0.001f);
            }
#endif

            tex.Dispose();
            spriteBatch.Dispose();
            game.Dispose();
        }
    }
}