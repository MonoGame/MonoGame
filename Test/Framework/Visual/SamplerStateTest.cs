// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    internal class SamplerStateTest : VisualTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullSamplerState()
        {
            Game.DrawWith += (sender, e) =>
            {
                Assert.Throws<ArgumentNullException>(() => Game.GraphicsDevice.SamplerStates[0] = null);
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            Game.DrawWith += (sender, e) =>
            {
                var samplerState = new SamplerState();

                // Can mutate before binding.
                DoAsserts(samplerState, Assert.DoesNotThrow);

                // Can't mutate after binding.
                Game.GraphicsDevice.SamplerStates[0] = samplerState;
                DoAsserts(samplerState, d => Assert.Throws<InvalidOperationException>(d));

                // Even after changing to different SamplerState, you still can't mutate a previously-bound object.
                Game.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
                DoAsserts(samplerState, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            Game.DrawWith += (sender, e) =>
            {
                DoAsserts(SamplerState.AnisotropicClamp, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.AnisotropicWrap, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.LinearClamp, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.LinearWrap, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.PointClamp, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.PointWrap, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
        }

        private static void DoAsserts(SamplerState samplerState, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => samplerState.AddressU = TextureAddressMode.Clamp);
            assertMethod(() => samplerState.AddressV = TextureAddressMode.Clamp);
            assertMethod(() => samplerState.AddressW = TextureAddressMode.Clamp);
            assertMethod(() => samplerState.Filter = TextureFilter.Anisotropic);
            assertMethod(() => samplerState.MaxAnisotropy = 0);
            assertMethod(() => samplerState.MaxMipLevel = 0);
            assertMethod(() => samplerState.MipMapLevelOfDetailBias = 0);
        }
    }
}
