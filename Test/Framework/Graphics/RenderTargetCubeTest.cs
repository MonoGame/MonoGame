// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    class RenderTargetCubeTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ZeroSizeShouldFailTest()
        {
            RenderTargetCube renderTarget;
            Assert.Throws<ArgumentOutOfRangeException>(() => renderTarget = new RenderTargetCube(gd, 0, false, SurfaceFormat.Color, DepthFormat.None));
        }

        [TestCase(1)]
        [TestCase(8)]
        [TestCase(31)]
        public void ShouldClearRenderTargetAndGetData(int size)
        {
            var dataSize = size * size;
            var renderTargetCube = new RenderTargetCube(gd, size, false, SurfaceFormat.Color, DepthFormat.Depth16);

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
                gd.SetRenderTarget(renderTargetCube, (CubeMapFace) i);
                gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, colors[i], 1.0f, 0);
                gd.SetRenderTarget(null, (CubeMapFace) i);
            }

            for (var i = 0; i < 6; i++)
            {
                var readData = new Color[dataSize];
                renderTargetCube.GetData((CubeMapFace) i, readData);

                for (var j = 0; j < dataSize; j++)
                    Assert.AreEqual(colors[i], readData[j]);
            }

            renderTargetCube.Dispose();
        }
    }
}
