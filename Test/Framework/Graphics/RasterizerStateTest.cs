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
    internal class RasterizerStateTest : GraphicsDeviceTestFixtureBase
    {
        [TestCase(-1f)]
#if DESKTOPGL
        [TestCase(1f), Ignore ("fails similarity test. Needs Investigating")]
#else
        [TestCase(1f)]
#endif
        [TestCase(-0.0004f)]
        public void DepthBiasVisualTest(float depthBias)
        {
            var effect = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                World = Matrix.Identity,
                View = Matrix.Identity,
                Projection = Matrix.Identity,
            };
            RasterizerState rs;
            var data = new VertexPositionColor[3];
            var step = depthBias / 4;

            PrepareFrameCapture();
            for (var i = 0; i < 4; i++)
            {
                var r = i * MathHelper.PiOver2;
                var bias = i * step;
                var c = new Color(new Vector3(i / 4f));

                rs = new RasterizerState();
                rs.DepthBias = bias;

                var rot = Matrix.CreateRotationZ(-r);
                var v1 = Vector3.Transform(new Vector3(-0.5f, 0f, 0f), rot);
                var v2 = Vector3.Transform(new Vector3(0.2f, 0.9f, 0f), rot);
                var v3 = Vector3.Transform(new Vector3(0.2f, -0.9f, 0f), rot);

                data[0] = new VertexPositionColor(v1, c);
                data[1] = new VertexPositionColor(v2, c);
                data[2] = new VertexPositionColor(v3, c);

                effect.CurrentTechnique.Passes[0].Apply();
                gd.RasterizerState = rs;
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, data, 0, 1, VertexPositionColor.VertexDeclaration);

                rs.Dispose();
            }

            CheckFrames();

            effect.Dispose();
        }

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

            rasterizerState.Dispose();
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

        [TestCase(CullMode.CullClockwiseFace)]
        [TestCase(CullMode.CullCounterClockwiseFace)]
        [TestCase(CullMode.None)]
        public void VisualTestCullMode(CullMode cullMode)
        {
            PrepareFrameCapture();

            var cube = new Colored3DCubeComponent(gd);
            cube.LoadContent();

            var rasterizerState = new RasterizerState
            {
                CullMode = cullMode
            };
            gd.RasterizerState = rasterizerState;

            cube.Draw();

            CheckFrames();

            cube.UnloadContent();
            rasterizerState.Dispose();
        }

        [TestCase(FillMode.Solid)]
        [TestCase(FillMode.WireFrame)]
        public void VisualTestFillMode(FillMode fillMode)
        {
            PrepareFrameCapture();

            var cube = new Colored3DCubeComponent(gd);
            cube.LoadContent();

            var rasterizerState = new RasterizerState
            {
                FillMode = fillMode
            };
            gd.RasterizerState = rasterizerState;

            cube.Draw();

            CheckFrames();

            cube.UnloadContent();
            rasterizerState.Dispose();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void VisualTestScissorTestEnable(bool scissorTestEnable)
        {
            PrepareFrameCapture();

            var cube = new Colored3DCubeComponent(gd);
            cube.LoadContent();

            var rasterizerstate = new RasterizerState
            {
                ScissorTestEnable = scissorTestEnable
            };
            gd.RasterizerState = rasterizerstate;

            var viewport = gd.Viewport;
            gd.ScissorRectangle = new Rectangle(0, 0,
                viewport.Width / 2, viewport.Height / 2);

            cube.Draw();

            CheckFrames();

            cube.UnloadContent();
            rasterizerstate.Dispose();
        }

#if !XNA
        [TestCase(false)]
        [TestCase(true)]
        public void VisualTestDepthClipEnable(bool depthClipEnable)
        {
            PrepareFrameCapture();

            var cube = new Colored3DCubeComponent(gd)
            {
                CubePosition = new Vector3(0, 0, 3)
            };
            cube.LoadContent();

            var rasterizerstate = new RasterizerState
            {
                DepthClipEnable = depthClipEnable
            };
            gd.RasterizerState = rasterizerstate;

            cube.Draw();

            CheckFrames();

            cube.UnloadContent();
            rasterizerstate.Dispose();
        }
#endif
    }
}
