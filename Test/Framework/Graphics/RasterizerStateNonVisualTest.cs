using System;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    internal class RasterizerStateNonVisualTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullRasterizerState()
        {
            Assert.Throws<ArgumentNullException>(() => gd.RasterizerState = null);
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            var rasterizerState = new RasterizerState();

            // Can mutate before binding.
            DoAsserts(rasterizerState, Assert.DoesNotThrow);

            // Can't mutate after binding.
            gd.RasterizerState = rasterizerState;
            DoAsserts(rasterizerState, d => Assert.Throws<InvalidOperationException>(d));

            // Even after changing to different RasterizerState, you still can't mutate a previously-bound object.
            gd.RasterizerState = RasterizerState.CullCounterClockwise;
            DoAsserts(rasterizerState, d => Assert.Throws<InvalidOperationException>(d));
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            DoAsserts(RasterizerState.CullClockwise, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(RasterizerState.CullCounterClockwise, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(RasterizerState.CullNone, d => Assert.Throws<InvalidOperationException>(d));
        }

        private static void DoAsserts(RasterizerState rasterizerState, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => rasterizerState.CullMode = CullMode.CullClockwiseFace);
            assertMethod(() => rasterizerState.DepthBias = 0.1f);
            assertMethod(() => rasterizerState.FillMode = FillMode.WireFrame);
            assertMethod(() => rasterizerState.MultiSampleAntiAlias = true);
            assertMethod(() => rasterizerState.ScissorTestEnable = true);
            assertMethod(() => rasterizerState.SlopeScaleDepthBias = 0.2f);
#if !XNA
            assertMethod(() => rasterizerState.DepthClipEnable = false);
#endif
        }


    }
}