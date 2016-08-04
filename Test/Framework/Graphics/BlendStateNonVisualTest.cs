using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    internal class BlendStateNonVisualTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullBlendState()
        {
            Assert.Throws<ArgumentNullException>(() => game.GraphicsDevice.BlendState = null);
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            var blendState = new BlendState();

            // Can mutate before binding.
            DoAsserts(blendState, Assert.DoesNotThrow);

            // Can't mutate after binding.
            game.GraphicsDevice.BlendState = blendState;
            DoAsserts(blendState, d => Assert.Throws<InvalidOperationException>(d));

            // Even after changing to different BlendState, you still can't mutate a previously-bound object.
            game.GraphicsDevice.BlendState = BlendState.Opaque;
            DoAsserts(blendState, d => Assert.Throws<InvalidOperationException>(d));
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            DoAsserts(BlendState.Additive, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(BlendState.AlphaBlend, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(BlendState.NonPremultiplied, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(BlendState.Opaque, d => Assert.Throws<InvalidOperationException>(d));
        }

        private static void DoAsserts(BlendState blendState, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => blendState.AlphaBlendFunction = BlendFunction.Add);
            assertMethod(() => blendState.AlphaDestinationBlend = Blend.BlendFactor);
            assertMethod(() => blendState.AlphaSourceBlend = Blend.BlendFactor);
            assertMethod(() => blendState.BlendFactor = Color.White);
            assertMethod(() => blendState.ColorBlendFunction = BlendFunction.Add);
            assertMethod(() => blendState.ColorDestinationBlend = Blend.BlendFactor);
            assertMethod(() => blendState.ColorSourceBlend = Blend.BlendFactor);
            assertMethod(() => blendState.ColorWriteChannels = ColorWriteChannels.All);
            assertMethod(() => blendState.ColorWriteChannels1 = ColorWriteChannels.All);
            assertMethod(() => blendState.ColorWriteChannels2 = ColorWriteChannels.All);
            assertMethod(() => blendState.ColorWriteChannels3 = ColorWriteChannels.All);
#if !XNA
            assertMethod(() => blendState.IndependentBlendEnable = true);
#endif
            assertMethod(() => blendState.MultiSampleMask = 0);

#if !XNA
            for (var i = 0; i < 4; i++)
            {
                assertMethod(() => blendState[0].AlphaBlendFunction = BlendFunction.Add);
                assertMethod(() => blendState[0].AlphaDestinationBlend = Blend.BlendFactor);
                assertMethod(() => blendState[0].AlphaSourceBlend = Blend.BlendFactor);
                assertMethod(() => blendState[0].ColorBlendFunction = BlendFunction.Add);
                assertMethod(() => blendState[0].ColorDestinationBlend = Blend.BlendFactor);
                assertMethod(() => blendState[0].ColorSourceBlend = Blend.BlendFactor);
                assertMethod(() => blendState[0].ColorWriteChannels = ColorWriteChannels.All);
            }
#endif
        }
    }
}