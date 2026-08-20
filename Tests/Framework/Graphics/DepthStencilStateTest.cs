// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.Components;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [NonParallelizable]
    [RunOnUiTestFixture]
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

        [Test]
        public void TwoSidedStencilModeUsesSeparateClockwiseAndCounterClockwiseStencilStates()
        {
            VertexPositionColor[] stencilTriangles = new VertexPositionColor[]
            {
                // Uses counter clockwise stencil state
                new VertexPositionColor(new Vector3(-0.95f, -0.75f, 0f), Color.Red),
                new VertexPositionColor(new Vector3(-0.15f, -0.75f, 0f), Color.Red),
                new VertexPositionColor(new Vector3(-0.55f, 0.55f, 0f), Color.Red),

                // Flip the winding order so this uses regular stencil state.
                new VertexPositionColor(new Vector3(0.55f, 0.55f, 0f), Color.Red),
                new VertexPositionColor(new Vector3(0.95f, -0.75f, 0f), Color.Red),
                new VertexPositionColor(new Vector3(0.15f, -0.75f, 0f), Color.Red)
            };

            VertexPositionColor[] fullScreenTriangle = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector3(-1f, -1f, 0f), Color.Green),
                new VertexPositionColor(new Vector3(3f, -1f, 0f), Color.Green),
                new VertexPositionColor(new Vector3(-1f, 3f, 0f), Color.Green)
            };

            using RenderTarget2D renderTarget = new RenderTarget2D(gd, 64, 64, false, SurfaceFormat.Color,DepthFormat.Depth24Stencil8);
            using BasicEffect effect = new BasicEffect(gd) { VertexColorEnabled = true };

            // Disable color writes for first pass, only modify stencil buffer.
            using BlendState stencilOnlyBlendState = new BlendState() { ColorWriteChannels = ColorWriteChannels.None };

            // counter-clockwise face writes 3 to the stencil buffer
            using DepthStencilState twoSidedStencilState = new DepthStencilState()
            {
                    DepthBufferEnable = false,
                    DepthBufferWriteEnable = false,
                    StencilEnable = true,
                    StencilFunction = CompareFunction.Always,
                    StencilPass = StencilOperation.Keep,
                    StencilFail = StencilOperation.Keep,
                    StencilDepthBufferFail = StencilOperation.Keep,
                    TwoSidedStencilMode = true,
                    CounterClockwiseStencilFunction = CompareFunction.Always,
                    CounterClockwiseStencilPass = StencilOperation.Replace,
                    CounterClockwiseStencilFail = StencilOperation.Keep,
                    CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep,
                    ReferenceStencil = 3
            };

            // only draw where the stencil value is 3
            using DepthStencilState stencilTestState = new DepthStencilState()
            {
                    DepthBufferEnable = false,
                    DepthBufferWriteEnable = false,
                    StencilEnable = true,
                    StencilFunction = CompareFunction.Equal,
                    StencilPass = StencilOperation.Keep,
                    StencilFail = StencilOperation.Keep,
                    StencilDepthBufferFail = StencilOperation.Keep,
                    ReferenceStencil = 3
            };
            using RasterizerState rasterizerState = new RasterizerState() { CullMode = CullMode.None };

            gd.SetRenderTarget(renderTarget!);
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1f, 1);

            gd.BlendState = stencilOnlyBlendState!;
            gd.DepthStencilState = twoSidedStencilState!;
            gd.RasterizerState = rasterizerState!;

            // Draw both windings into the stencil buffer
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, stencilTriangles, 0, 2);
            }

            gd.BlendState = BlendState.Opaque;
            gd.DepthStencilState = stencilTestState!;

            // Draw over the entire target.
            // Only pixels where the stencil value is 3 should be changed to green.
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, fullScreenTriangle, 0, 1);
            }

            gd.SetRenderTarget(null);

            Color[] pixels = renderTarget.GetColorData();
            int leftIndex = (renderTarget.Height / 2) * renderTarget.Width + (renderTarget.Width / 4);
            int rightIndex = (renderTarget.Height / 2) * renderTarget.Width + ((renderTarget.Width * 3) / 4);
            int backgroundIndex = (renderTarget.Height / 8) * renderTarget.Width + (renderTarget.Width / 2);

            // counter-clockwise triangle write 3, regular triangle did not
            // untouched pixels should have remained clear color
            Assert.AreEqual(Color.Green, pixels[leftIndex]);
            Assert.AreEqual(Color.Black, pixels[rightIndex]);
            Assert.AreEqual(Color.Black, pixels[backgroundIndex]);
        }
    }
}
