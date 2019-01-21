// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
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
        public void InitializeEventCount()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            var resettingCount = 0;
            var resetCount = 0;
            var preparingCount = 0;
            var createdCount = 0;
            var devDispCount = 0;
            var dispCount = 0;

            gdm.DeviceResetting += (s, a) => resettingCount++;
            gdm.DeviceReset += (s, a) => resetCount++;
            gdm.PreparingDeviceSettings += (s, a) => preparingCount++;
            gdm.DeviceCreated += (s, a) => createdCount++;
            gdm.DeviceDisposing += (s, a) => devDispCount++;
            // TODO remove MonoMac
#if !MONOMAC
            gdm.Disposed += (s, a) => dispCount++;
#endif

            game.InitializeOnly();

            Assert.AreEqual(0, resettingCount);
            Assert.AreEqual(0, resetCount);
            Assert.AreEqual(1, preparingCount);
            Assert.AreEqual(1, createdCount);
            Assert.AreEqual(0, devDispCount);
            Assert.AreEqual(0, dispCount);

            game.Dispose();
        }

        [Test]
        public void DoNotModifyPresentationParametersDirectly()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            game.InitializeWith += (sender, args) =>
            {
                var gd = game.GraphicsDevice;

                var oldpp = gd.PresentationParameters;
                gdm.PreferredBackBufferWidth = 100;
                gdm.ApplyChanges();
                var newpp = gd.PresentationParameters;
                Assert.AreNotSame(oldpp, newpp);
            };

            game.InitializeOnly();
            game.Dispose();
        }

        [Test]
        public void PreparingDeviceSettings()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            var count = 0;

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

                count++;
            };

            game.InitializeOnly();
            Assert.AreEqual(1, count);
            game.Dispose();
        }

        [Test]
        public void PreparingDeviceSettingsEventChangeGraphicsProfile()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            Assert.AreEqual(GraphicsProfile.Reach, gdm.GraphicsProfile);

            game.InitializeOnly();

            var invoked = false;
            gdm.PreparingDeviceSettings += (s, a) =>
            {
                a.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
                invoked = true;
            };

            // make sure that changing the graphics profile creates a new device and does not reset
            var creationCount = 0;
            gdm.DeviceCreated += (sender, args) => creationCount++;
            var resetCount = 0;
            gdm.DeviceReset += (sender, args) => resetCount++;

            // make a change so ApplyChanges actually does something
            gdm.PreferredBackBufferWidth = 100;
            gdm.ApplyChanges();

            // assert that PreparingDeviceSettings is invoked, but the GraphicsProfile of the gdm did not change
            Assert.That(invoked);
            Assert.AreEqual(GraphicsProfile.Reach, gdm.GraphicsProfile);
            Assert.AreEqual(GraphicsProfile.HiDef, gdm.GraphicsDevice.GraphicsProfile);

            Assert.AreEqual(creationCount, 1);
            Assert.AreEqual(resetCount, 0);

            game.Dispose();
        }

        [Test]
        public void PreparingDeviceSettingsArgsPresentationParametersAreApplied()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            var invoked = false;

            game.PreInitializeWith += (sender, args) =>
            {
                Assert.AreEqual(RenderTargetUsage.DiscardContents,
                    gdm.GraphicsDevice.PresentationParameters.RenderTargetUsage);

                gdm.PreparingDeviceSettings += (s, a) =>
                {
                    a.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
                    a.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
                    invoked = true;
                };
            };

            game.InitializeOnly();

            // make a change so ApplyChanges actually does something
            gdm.PreferredBackBufferWidth = 100;
            gdm.ApplyChanges();

            Assert.That(invoked);
            Assert.AreEqual(RenderTargetUsage.PreserveContents, gdm.GraphicsDevice.PresentationParameters.RenderTargetUsage);

            game.Dispose();
        }

        [Test]
        public void PreparingDeviceSettingsArgsThrowsWhenPPSetToNull()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            var invoked = false;

            gdm.PreparingDeviceSettings += (s, a) =>
            {
                a.GraphicsDeviceInformation.PresentationParameters = null;
                invoked = true;
            };

            Assert.Throws(Is.InstanceOf(typeof(Exception)), () => game.InitializeOnly());
            Assert.That(invoked);

            game.Dispose();
        }

        [Test]
        public void ApplyChangesReturnsWhenNoSetterCalled()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            var invoked = false;

            game.PreInitializeWith += (sender, args) =>
            {
                gdm.PreparingDeviceSettings += (s, a) =>
                {
                    invoked = true;
                };
            };

            game.InitializeOnly();

            gdm.ApplyChanges();
            Assert.IsFalse(invoked);

            // this proves that XNA does not check for equality, but just registers that setters are used
            gdm.PreferredBackBufferWidth = gdm.PreferredBackBufferWidth;

            gdm.ApplyChanges();
            Assert.That(invoked);

            game.Dispose();
        }

        [Test]
        public void ApplyChangesInvokesPreparingDeviceSettings()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            var invoked = false;

            game.InitializeWith += (sender, args) =>
            {
                gdm.PreparingDeviceSettings += (s, a) =>
                {
                    invoked = true;
                };
            };

            game.InitializeOnly();

            gdm.PreferredBackBufferWidth = gdm.PreferredBackBufferWidth;
            gdm.ApplyChanges();
            Assert.That(invoked);

            game.Dispose();
        }

        [Test]
        public void ApplyChangesResetsDevice()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            var count = 0;

            gdm.DeviceReset += (sender, args) => count++;

            game.InitializeOnly();

            gdm.PreferredBackBufferWidth = gdm.PreferredBackBufferWidth;
            gdm.ApplyChanges();
            Assert.AreEqual(1, count);

            game.Dispose();
        }

        [Test]
        public void DeviceDisposingInvokedAfterDeviceDisposed()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);

            var invoked = false;

            gdm.DeviceDisposing += (sender, args) =>
            {
                invoked = true;
                Assert.IsTrue(gdm.GraphicsDevice.IsDisposed);
            };

            game.InitializeOnly();

            Assert.IsFalse(gdm.GraphicsDevice.IsDisposed);
            Assert.IsFalse(invoked);
            // change the graphics profile so the current device needs to be disposed
            gdm.GraphicsProfile = GraphicsProfile.HiDef;
            gdm.ApplyChanges();
            Assert.IsTrue(invoked);

            game.Dispose();

        }
    }

    internal class GraphicsDeviceManagerFixtureTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ResettingDeviceTriggersResetEvents()
        {
            var resetCount = 0;
            var resettingCount = 0;
            gdm.DeviceReset += (sender, args) =>
            {
                resetCount++;
            };

            gdm.DeviceResetting += (sender, args) =>
            {
                resettingCount++;
            };

            gd.Reset();

            Assert.AreEqual(1, resetCount);
            Assert.AreEqual(1, resettingCount);
        }
        
        [Test]
        public void NewDeviceDoesNotTriggerReset()
        {
            var resetCount = 0;
            var devLostCount = 0;

            gd.DeviceReset += (sender, args) =>
            {
                resetCount++;
            };
            gd.DeviceLost += (sender, args) =>
            {
                devLostCount++;
            };

            // changing the profile requires creating a new device
            gdm.GraphicsProfile = GraphicsProfile.Reach;
            gdm.ApplyChanges();

            Assert.AreEqual(0, resetCount);
            Assert.AreEqual(0, devLostCount);
        }

        [Test]
        public void ClientSizeChangedOnDeviceReset()
        {
            var count = 0;
            game.Window.ClientSizeChanged += (sender, args) =>
            {
                count++;
            };
            gdm.GraphicsProfile = GraphicsProfile.HiDef;
            gdm.ApplyChanges();
            Assert.AreEqual(0, count);

            gdm.PreferredBackBufferWidth = 100;
            gdm.ApplyChanges();
            Assert.AreEqual(0, count);

            // changing the profile will trigger a device reset
            gdm.GraphicsProfile = GraphicsProfile.Reach;
            gdm.ApplyChanges();
            // not even that will trigger the event
            Assert.AreEqual(0, count);
        }

        [Test]
#if DESKTOPGL
        [Ignore("Expected 2 but got 3. Needs Investigating")]
#endif
        public void MultiSampleCountRoundsDown()
        {
            gdm.PreferMultiSampling = true;

            gdm.PreparingDeviceSettings += (sender, args) =>
            {
                var pp = args.GraphicsDeviceInformation.PresentationParameters;
                pp.MultiSampleCount = 3;
            };

            gdm.ApplyChanges();

            Assert.AreEqual(2, gd.PresentationParameters.MultiSampleCount);

        }

        [TestCase(false)]
        [TestCase(true)]
#if DESKTOPGL
        [Ignore("Expected not 1024 but got 1024. Needs Investigating")]
#endif
        public void MSAAEnabled(bool enabled)
        {
            gdm.PreferMultiSampling = enabled;
            gdm.GraphicsProfile = GraphicsProfile.HiDef;

            // first hook an event to do some checks
            gdm.PreparingDeviceSettings += (sender, args) =>
            {
                var pp = args.GraphicsDeviceInformation.PresentationParameters;
                if (!enabled)
                    Assert.AreEqual(0, pp.MultiSampleCount);
                else
                    Assert.Less(0, pp.MultiSampleCount);
            };

            // then create a GraphicsDevice
            gdm.ApplyChanges();

            var tex = new Texture2D(gd, 1, 1);
            tex.SetData(new[] { Color.White.PackedValue });
            var spriteBatch = new SpriteBatch(gd);

            if (enabled)
            {
                var pp = gd.PresentationParameters;
                Assert.Less(0, pp.MultiSampleCount);
                Assert.AreNotEqual(1024, pp.MultiSampleCount);
            }

            gd.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(tex, new Vector2(800 / 2, 480 / 2), null, Color.White, MathHelper.ToRadians(45), new Vector2(0.5f), 200, SpriteEffects.None, 0);
            spriteBatch.End();

            var data = new Color[800 * 480];
            gd.GetBackBufferData(data);

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

            tex.Dispose();
            spriteBatch.Dispose();
        }

        [Test]
        public void UnsupportedMultiSampleCountDoesNotThrowException()
        {
            gdm.PreferMultiSampling = true;

            gdm.PreparingDeviceSettings += (sender, args) =>
            {
                var pp = args.GraphicsDeviceInformation.PresentationParameters;
                pp.MultiSampleCount = 33; // Set too high. In DX11 is max 32.
            };

            Assert.DoesNotThrow(()=>gdm.ApplyChanges(), "GraphicDeviceManager.ApplyChanges()");
            Assert.DoesNotThrow(() =>
            {
                var pp = gdm.GraphicsDevice.PresentationParameters.Clone();
                pp.MultiSampleCount = 10000; // Set too high. In DX11 is max 32.
                gdm.GraphicsDevice.Reset(pp);
            }, "GraphicsDevice.Reset(PresentationParameters)");
        }

#if DIRECTX
        [Test]
        public void TooHighMultiSampleCountClampedToMaxSupported()
        {
            var maxMultiSampleCount = gd.GraphicsCapabilities.MaxMultiSampleCount;
            gdm.PreferMultiSampling = true;

            gdm.PreparingDeviceSettings += (sender, args) =>
            {
                var pp1 = args.GraphicsDeviceInformation.PresentationParameters;
                pp1.MultiSampleCount = maxMultiSampleCount + 1;
            };
            gdm.ApplyChanges();
            Assert.AreEqual
                (maxMultiSampleCount, gdm.GraphicsDevice.PresentationParameters.MultiSampleCount);

            // Test again for GraphicsDevice.Reset(PresentationParameters)
            var pp2 = gdm.GraphicsDevice.PresentationParameters.Clone();
            pp2.MultiSampleCount = 0;
            gdm.GraphicsDevice.Reset(pp2);
            Assert.AreEqual
                (0, gdm.GraphicsDevice.PresentationParameters.MultiSampleCount);

            var pp3 = gdm.GraphicsDevice.PresentationParameters.Clone();
            pp3.MultiSampleCount = 500; // Set too high. max is maxMultiSampleCount
            gdm.GraphicsDevice.Reset(pp3);
            Assert.AreEqual
                (maxMultiSampleCount, gdm.GraphicsDevice.PresentationParameters.MultiSampleCount);
            
        }
#endif
    }
}
