// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.Components;
using MonoGame.Tests.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    internal class RasterizerStateTest : GraphicsDeviceTestFixtureBase
    {
        [TestCase(CullMode.CullClockwiseFace)]
        [TestCase(CullMode.CullCounterClockwiseFace)]
        [TestCase(CullMode.None)]
        public void VisualTestCullMode(CullMode cullMode)
        {
            PrepareFrameCapture();

            var cube = new Colored3DCubeComponent(gd);
            cube.LoadContent();

            gd.RasterizerState = new RasterizerState
            {
                CullMode = cullMode
            };
            cube.Draw();

            CheckFrames();
        }

        [TestCase(FillMode.Solid)]
        [TestCase(FillMode.WireFrame)]
        public void VisualTestFillMode(FillMode fillMode)
        {
            PrepareFrameCapture();

            var cube = new Colored3DCubeComponent(gd);
            cube.LoadContent();

            gd.RasterizerState = new RasterizerState
            {
                FillMode = fillMode
            };
            cube.Draw();

            CheckFrames();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void VisualTestScissorTestEnable(bool scissorTestEnable)
        {
            PrepareFrameCapture();

            var cube = new Colored3DCubeComponent(gd);
            cube.LoadContent();

            gd.RasterizerState = new RasterizerState
            {
                ScissorTestEnable = scissorTestEnable
            };

            var viewport = gd.Viewport;
            gd.ScissorRectangle = new Rectangle(0, 0,
                viewport.Width / 2, viewport.Height / 2);

            cube.Draw();

            CheckFrames();
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

            gd.RasterizerState = new RasterizerState
            {
                DepthClipEnable = depthClipEnable
            };
            cube.Draw();

            CheckFrames();
        }
#endif
    }
}
