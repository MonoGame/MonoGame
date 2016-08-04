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
