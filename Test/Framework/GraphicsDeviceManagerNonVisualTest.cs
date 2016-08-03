using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    public class GraphicsDeviceManagerNonVisualTest
    {
        private GraphicsDeviceManager gdm;
        private TestGameBase game;

        [SetUp]
        public void SetUp()
        {
            game = new TestGameBase();
            gdm = new GraphicsDeviceManager(game);
        }

        [TearDown]
        public void TearDown()
        {
            game.Dispose();
        }

        [Test]
        public void DefaultParameterValidation()
        {
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
        }

        [Test]
        public void InitializeEventCount()
        {
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
            gdm.Disposed += (s, a) => dispCount++;

            Assert.AreEqual(0, resettingCount);
            Assert.AreEqual(0, resetCount);
            Assert.AreEqual(1, preparingCount);
            Assert.AreEqual(1, createdCount);
            Assert.AreEqual(0, devDispCount);
            Assert.AreEqual(0, dispCount);
        }

        [Test]
        public void DoNotModifyPresentationParametersDirectly()
        {
            bool invoked = false;

            game.InitializeWith += (sender, args) =>
            {
                var gd = game.GraphicsDevice;

                var oldpp = gd.PresentationParameters;
                gdm.PreferredBackBufferWidth = 100;
                gdm.ApplyChanges();
                var newpp = gd.PresentationParameters;
                Assert.AreNotSame(oldpp, newpp);

                invoked = true;
            };

            game.InitializeOnly();
            Assert.IsTrue(invoked);
        }

        [Test]
        public void PreparingDeviceSettings()
        {
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
        }

        [Test]
        public void PreparingDeviceSettingsEventChangeGraphicsProfile()
        {
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
        }

        [Test]
        public void PreparingDeviceSettingsArgsPresentationParametersAreApplied()
        {
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
        }

        [Test]
        public void PreparingDeviceSettingsArgsThrowsWhenPPSetToNull()
        {
            var invoked = false;

            gdm.PreparingDeviceSettings += (s, a) =>
            {
                a.GraphicsDeviceInformation.PresentationParameters = null;
                invoked = true;
            };

            Assert.Throws<NullReferenceException>(() => game.InitializeOnly());
            Assert.That(invoked);
        }

        public void ApplyChangesReturnsWhenNoSetterCalled()
        {
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
        }

        [Test]
        public void ApplyChangesInvokesPreparingDeviceSettings()
        {
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
        }

        [Test]
        public void ApplyChangesResetsDevice()
        {
            var count = 0;

            gdm.DeviceReset += (sender, args) => count++;

            game.InitializeOnly();

            gdm.PreferredBackBufferWidth = gdm.PreferredBackBufferWidth;
            gdm.ApplyChanges();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void DeviceDisposingInvokedAfterDeviceDisposed()
        {
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
}