// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.Components;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    internal class RasterizerStateTest : VisualTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullRasterizerState()
        {
            Game.DrawWith += (sender, e) =>
            {
                Assert.Throws<ArgumentNullException>(() => Game.GraphicsDevice.RasterizerState = null);
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            Game.DrawWith += (sender, e) =>
            {
                var rasterizerState = new RasterizerState();

                // Can mutate before binding.
                DoAsserts(rasterizerState, Assert.DoesNotThrow);

                // Can't mutate after binding.
                Game.GraphicsDevice.RasterizerState = rasterizerState;
                DoAsserts(rasterizerState, d => Assert.Throws<InvalidOperationException>(d));

                // Even after changing to different RasterizerState, you still can't mutate a previously-bound object.
                Game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                DoAsserts(rasterizerState, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            Game.DrawWith += (sender, e) =>
            {
                DoAsserts(RasterizerState.CullClockwise, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(RasterizerState.CullCounterClockwise, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(RasterizerState.CullNone, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
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

        [TestCase(CullMode.CullClockwiseFace)]
        [TestCase(CullMode.CullCounterClockwiseFace)]
        [TestCase(CullMode.None)]
        public void VisualTestCullMode(CullMode cullMode)
        {
            Game.Components.Add(new Colored3DCubeComponent(Game));

            Game.PreDrawWith += (sender, e) =>
            {
                Game.GraphicsDevice.RasterizerState = new RasterizerState
                {
                    CullMode = cullMode
                };
            };

            RunSingleFrameTest();
        }

        [TestCase(FillMode.Solid)]
        [TestCase(FillMode.WireFrame)]
        public void VisualTestFillMode(FillMode fillMode)
        {
            Game.Components.Add(new Colored3DCubeComponent(Game));

            Game.PreDrawWith += (sender, e) =>
            {
                Game.GraphicsDevice.RasterizerState = new RasterizerState
                {
                    FillMode = fillMode
                };
            };

            RunSingleFrameTest();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void VisualTestScissorTestEnable(bool scissorTestEnable)
        {
            Game.Components.Add(new Colored3DCubeComponent(Game));

            Game.PreDrawWith += (sender, e) =>
            {
                Game.GraphicsDevice.RasterizerState = new RasterizerState
                {
                    ScissorTestEnable = scissorTestEnable
                };

                var viewport = Game.GraphicsDevice.Viewport;
                Game.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0,
                    viewport.Width / 2, viewport.Height / 2);
            };

            RunSingleFrameTest();
        }

#if !XNA
        [TestCase(false)]
        [TestCase(true)]
        public void VisualTestDepthClipEnable(bool depthClipEnable)
        {
            Game.Components.Add(new Colored3DCubeComponent(Game)
            {
                CubePosition = new Vector3(0, 0, 3)
            });

            Game.PreDrawWith += (sender, e) =>
            {
                Game.GraphicsDevice.RasterizerState = new RasterizerState
                {
                    DepthClipEnable = depthClipEnable
                };
            };

            RunSingleFrameTest();
        }
#endif
    }
}
