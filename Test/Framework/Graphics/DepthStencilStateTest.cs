// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.Components;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    internal class DepthStencilStateTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullDepthStencilState()
        {
            Assert.Throws<ArgumentNullException>(() => gd.DepthStencilState = null);
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            var depthStencilState = new DepthStencilState();

            // Can mutate before binding.
            DoAsserts(depthStencilState, Assert.DoesNotThrow);

            // Can't mutate after binding.
            gd.DepthStencilState = depthStencilState;
            DoAsserts(depthStencilState, d => Assert.Throws<InvalidOperationException>(d));

            // Even after changing to different RasterizerState, you still can't mutate a previously-bound object.
            gd.DepthStencilState = DepthStencilState.Default;
            DoAsserts(depthStencilState, d => Assert.Throws<InvalidOperationException>(d));

            depthStencilState.Dispose();
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            DoAsserts(DepthStencilState.Default, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(DepthStencilState.DepthRead, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(DepthStencilState.None, d => Assert.Throws<InvalidOperationException>(d));
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
            PrepareFrameCapture();

            var cube = new Simple3DCubeComponent(gd);
            cube.LoadContent();

            gd.DepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = depthBufferEnable
            };

            gd.Clear(Color.CornflowerBlue);

            cube.CubeColor = Color.Red;
            cube.Draw();

            cube.CubePosition = new Vector3(0.4f, 0, 0);
            cube.CubeColor = Color.Green;
            cube.Draw();

            CheckFrames();

            cube.UnloadContent();
        }

        [Test]
        public void VisualTestStencilBuffer()
        {
            PrepareFrameCapture();
            var cube = new Simple3DCubeComponent(gd);
            cube.LoadContent();

            gd.Clear(
                ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target,
                Color.CornflowerBlue, 1, 0);

            var depthStencilState = new DepthStencilState
            {
                ReferenceStencil = 1,
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                DepthBufferEnable = false
            };
            gd.DepthStencilState = depthStencilState;

            cube.CubeColor = Color.Red;
            cube.Draw();

            depthStencilState.Dispose();
            depthStencilState = new DepthStencilState
            {
                ReferenceStencil = 0,
                StencilEnable = true,
                StencilFunction = CompareFunction.Equal,
                StencilPass = StencilOperation.Keep,
                DepthBufferEnable = false
            };
            gd.DepthStencilState = depthStencilState;

            cube.CubePosition = new Vector3(0.4f, 0, 0);
            cube.CubeColor = Color.Green;
            cube.Draw();

            CheckFrames();

            depthStencilState.Dispose();
            cube.UnloadContent();
        }

        [TestCase(0)]
        [TestCase(45)]
        [TestCase(60)]
        public void VisualTestStencilBufferClip(int angle)
        {
            var spriteBatch = new SpriteBatch(gd);
            var plainWhite = new Texture2D(gd, 1, 1);
            plainWhite.SetData(new Color[] { Color.White });

            var stencilWriteOnly = new DepthStencilState()
            {
                DepthBufferEnable = false,
                DepthBufferWriteEnable = false,
                StencilFunction = CompareFunction.Always,
                StencilFail = StencilOperation.IncrementSaturation,
                StencilPass = StencilOperation.IncrementSaturation,
                ReferenceStencil = 0,
                StencilEnable = true,
                StencilMask = 0,
            };

            var stencilReadOnly = new DepthStencilState()
            {
                DepthBufferEnable = false,
                DepthBufferWriteEnable = false,
                StencilFunction = CompareFunction.Less,
                StencilFail = StencilOperation.Keep,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 0,
                StencilEnable = true
            };

            PrepareFrameCapture();

            gd.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, stencilWriteOnly);
            spriteBatch.Draw(plainWhite, new Rectangle(400, 240, 300, 300), null, Color.Transparent, MathHelper.ToRadians((float)angle), Vector2.One / 2, SpriteEffects.None, 0);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, stencilReadOnly);
            spriteBatch.Draw(plainWhite, new Rectangle(250, 90, 300, 300), Color.Red);
            spriteBatch.End();

            CheckFrames();

            stencilReadOnly.Dispose();
            stencilReadOnly = null;
            stencilWriteOnly.Dispose();
            stencilWriteOnly = null;
            plainWhite.Dispose();
            plainWhite = null;
            spriteBatch.Dispose();
            spriteBatch = null;
        }
    }
}
