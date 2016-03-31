// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    class RenderTargetCubeTest : VisualTestFixtureBase
    {
        [TestCase(1)]
        [TestCase(8)]
        [TestCase(31)]
        public void ShouldClearRenderTargetAndGetData(int size)
        {
            Game.DrawWith += (sender, e) =>
            {
                var dataSize = size * size;
                var renderTargetCube = new RenderTargetCube(Game.GraphicsDevice, size, false, SurfaceFormat.Color, DepthFormat.Depth16);

                var colors = new[]
                {
                    Color.BlanchedAlmond,
                    Color.BlueViolet,
                    Color.DarkSeaGreen,
                    Color.ForestGreen,
                    Color.IndianRed,
                    Color.LightGoldenrodYellow
                };

                for (var i = 0; i < 6; i++)
                {
                    Game.GraphicsDevice.SetRenderTarget(renderTargetCube, (CubeMapFace) i);
                    Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, colors[i], 1.0f, 0);
                    Game.GraphicsDevice.SetRenderTarget(null, (CubeMapFace) i);
                }

                for (var i = 0; i < 6; i++)
                {
                    var readData = new Color[dataSize];
                    renderTargetCube.GetData((CubeMapFace) i, readData);

                    for (var j = 0; j < dataSize; j++)
                        Assert.AreEqual(colors[i], readData[j]);
                }
            };
            Game.Run();
        }
    }
}
