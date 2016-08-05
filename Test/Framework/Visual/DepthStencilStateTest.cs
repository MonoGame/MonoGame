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
    internal class DepthStencilStateTest : GraphicsDeviceTestFixtureBase
    {
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
        }

        [Test]
        public void VisualTestStencilBuffer()
        {
            WriteDiffs = WriteSettings.Always;
            PrepareFrameCapture();
            var cube = new Simple3DCubeComponent(gd);
            cube.LoadContent();

            gd.Clear(
                ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target,
                Color.CornflowerBlue, 1, 0);

            gd.DepthStencilState = new DepthStencilState
            {
                ReferenceStencil = 1,
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                DepthBufferEnable = false
            };

            cube.CubeColor = Color.Red;
            cube.Draw();

            gd.DepthStencilState = new DepthStencilState
            {
                ReferenceStencil = 0,
                StencilEnable = true,
                StencilFunction = CompareFunction.Equal,
                StencilPass = StencilOperation.Keep,
                DepthBufferEnable = false
            };

            cube.CubePosition = new Vector3(0.4f, 0, 0);
            cube.CubeColor = Color.Green;
            cube.Draw();

            CheckFrames();
        }
    }
}
