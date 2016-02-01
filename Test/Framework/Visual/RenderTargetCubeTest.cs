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

                for (var i = 0; i < 6; i++)
                {
                    var cubeMapFace = (CubeMapFace) i;

                    Game.GraphicsDevice.SetRenderTarget(renderTargetCube, cubeMapFace);
                    Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.BlanchedAlmond, 1.0f, 0);
                    Game.GraphicsDevice.SetRenderTarget(null, cubeMapFace);

                    var readData = new Color[dataSize];
                    renderTargetCube.GetData(cubeMapFace, readData);

                    for (var j = 0; j < dataSize; j++)
                        Assert.AreEqual(Color.BlanchedAlmond, readData[j]);
                }
            };
            Game.Run();
        }
    }
}
