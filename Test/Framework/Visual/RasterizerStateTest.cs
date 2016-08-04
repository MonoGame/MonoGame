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
