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
    internal class DepthStencilStateTest : VisualTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullDepthStencilState()
        {
            Game.DrawWith += (sender, e) =>
            {
                Assert.Throws<ArgumentNullException>(() => Game.GraphicsDevice.DepthStencilState = null);
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            Game.DrawWith += (sender, e) =>
            {
                var depthStencilState = new DepthStencilState();

                // Can mutate before binding.
                DoAsserts(depthStencilState, Assert.DoesNotThrow);

                // Can't mutate after binding.
                Game.GraphicsDevice.DepthStencilState = depthStencilState;
                DoAsserts(depthStencilState, d => Assert.Throws<InvalidOperationException>(d));

                // Even after changing to different RasterizerState, you still can't mutate a previously-bound object.
                Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                DoAsserts(depthStencilState, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            Game.DrawWith += (sender, e) =>
            {
                DoAsserts(DepthStencilState.Default, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(DepthStencilState.DepthRead, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(DepthStencilState.None, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
        }

        private static void DoAsserts(DepthStencilState depthStencilState, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => depthStencilState.CounterClockwiseStencilDepthBufferFail = StencilOperation.Decrement);
            assertMethod(() => depthStencilState.CounterClockwiseStencilFail = StencilOperation.Decrement);
            assertMethod(() => depthStencilState.CounterClockwiseStencilFunction = CompareFunction.Always);
            assertMethod(() => depthStencilState.CounterClockwiseStencilPass = StencilOperation.Decrement);
            assertMethod(() => depthStencilState.DepthBufferEnable = true);
            assertMethod(() => depthStencilState.DepthBufferFunction = CompareFunction.Always);
            assertMethod(() => depthStencilState.DepthBufferWriteEnable = true);
            assertMethod(() => depthStencilState.ReferenceStencil = 1);
            assertMethod(() => depthStencilState.StencilDepthBufferFail = StencilOperation.Decrement);
            assertMethod(() => depthStencilState.StencilEnable = true);
            assertMethod(() => depthStencilState.StencilFail = StencilOperation.Decrement);
            assertMethod(() => depthStencilState.StencilFunction = CompareFunction.Always);
            assertMethod(() => depthStencilState.StencilMask = 1);
            assertMethod(() => depthStencilState.StencilPass = StencilOperation.Decrement);
            assertMethod(() => depthStencilState.StencilWriteMask = 1);
            assertMethod(() => depthStencilState.TwoSidedStencilMode = true);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void VisualTestDepthBufferEnable(bool depthBufferEnable)
        {
            var cube = new Simple3DCubeComponent(Game);

            Game.PreInitializeWith += (sender, e) =>
            {
                cube.Initialize();
            };

            Game.DrawWith += (sender, e) =>
            {
                Game.GraphicsDevice.DepthStencilState = new DepthStencilState
                {
                    DepthBufferEnable = depthBufferEnable
                };

                Game.GraphicsDevice.Clear(Color.CornflowerBlue);

                cube.CubeColor = Color.Red;
                cube.Draw(e.FrameInfo.GameTime);

                cube.CubePosition = new Vector3(0.4f, 0, 0);
                cube.CubeColor = Color.Green;
                cube.Draw(e.FrameInfo.GameTime);
            };

            RunSingleFrameTest();
        }

        [Test]
        public void VisualTestStencilBuffer()
        {
            var cube = new Simple3DCubeComponent(Game);

            Game.PreInitializeWith += (sender, e) =>
            {
                cube.Initialize();
            };

            Game.DrawWith += (sender, e) =>
            {
                Game.GraphicsDevice.Clear(
                    ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target,
                    Color.CornflowerBlue, 1, 0);

                Game.GraphicsDevice.DepthStencilState = new DepthStencilState
                {
                    ReferenceStencil = 1,
                    StencilEnable = true,
                    StencilFunction = CompareFunction.Always,
                    StencilPass = StencilOperation.Replace,
                    DepthBufferEnable = false
                };

                cube.CubeColor = Color.Red;
                cube.Draw(e.FrameInfo.GameTime);

                Game.GraphicsDevice.DepthStencilState = new DepthStencilState
                {
                    ReferenceStencil = 0,
                    StencilEnable = true,
                    StencilFunction = CompareFunction.Equal,
                    StencilPass = StencilOperation.Keep,
                    DepthBufferEnable = false
                };

                cube.CubePosition = new Vector3(0.4f, 0, 0);
                cube.CubeColor = Color.Green;
                cube.Draw(e.FrameInfo.GameTime);
            };

            RunSingleFrameTest();
        }
    }
}
