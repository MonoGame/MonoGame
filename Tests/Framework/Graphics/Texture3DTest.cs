// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
#if DESKTOPGL
    [Ignore("Texture3D is not implemented for the OpenGL backend.")]
#endif
    [TestFixture]
    [NonParallelizable]
    class Texture3DTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        [TestCase(1, 1, 1)]
        [TestCase(8, 8, 8)]
        [TestCase(31, 7, 13)]
        [RunOnUI]
        public void ShouldSetAndGetData(int width, int height, int depth)
        {
            var dataSize = width * height * depth;
            var texture3D = new Texture3D(gd, width, height, depth, false, SurfaceFormat.Color);
            var savedData = new Color[dataSize];
            for (var index = 0; index < dataSize; index++) savedData[index] = new Color(index, index, index);
            texture3D.SetData(savedData);

            var readData = new Color[dataSize];
            texture3D.GetData(readData);

            Assert.AreEqual(savedData, readData);

            texture3D.Dispose();
        }
    }
}
