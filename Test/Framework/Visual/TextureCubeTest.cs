// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    class TextureCubeTest : VisualTestFixtureBase
    {
        [TestCase(1)]
        [TestCase(8)]
        [TestCase(31)]
        public void ShouldSetAndGetData(int size)
        {
            Game.DrawWith += (sender, e) =>
            {
                var dataSize = size * size;
                var textureCube = new TextureCube(Game.GraphicsDevice, size, false, SurfaceFormat.Color);

                for (var i = 0; i < 6; i++)
                {
                    var savedData = new Color[dataSize];
                    for (var index = 0; index < dataSize; index++)
                        savedData[index] = new Color(index + i, index + i, index + i);
                    textureCube.SetData((CubeMapFace) i, savedData);

                    var readData = new Color[dataSize];
                    textureCube.GetData((CubeMapFace) i, readData);

                    Assert.AreEqual(savedData, readData);
                }
            };
            Game.Run();
        }
    }
}
