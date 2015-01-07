// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    class VertexBufferTest : VisualTestFixtureBase
    {
        VertexPositionTexture[] savedData = new VertexPositionTexture[] 
        {
            new VertexPositionTexture(new Vector3(1,2,3), new Vector2(0.1f,0.2f)),
            new VertexPositionTexture(new Vector3(4,5,6), new Vector2(0.3f,0.4f)),
            new VertexPositionTexture(new Vector3(7,8,9), new Vector2(0.5f,0.6f)),
            new VertexPositionTexture(new Vector3(10,11,12), new Vector2(0.7f,0.8f))
        };
        VertexPositionTexture vertexZero = new VertexPositionTexture(Vector3.Zero, Vector2.Zero);
        
        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData(bool dynamic)
        {   
            Game.DrawWith += (sender, e) =>
            {   
                var vertexBuffer = (dynamic)
                    ?new DynamicVertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                    :new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
                vertexBuffer.SetData(savedData);

                var readData = new VertexPositionTexture[4];
                vertexBuffer.GetData(readData, 0, 4);
                Assert.AreEqual(savedData, readData);
            };
            Game.RunOneFrame();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData_elementCount(bool dynamic)
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = (dynamic)
                    ? new DynamicVertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                    : new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
                vertexBuffer.SetData(savedData);

                var readData = new VertexPositionTexture[4];
                vertexBuffer.GetData(readData, 0, 2);
                Assert.AreEqual(savedData[0], readData[0]);
                Assert.AreEqual(savedData[1], readData[1]);
                Assert.AreEqual(vertexZero, readData[2]);
                Assert.AreEqual(vertexZero, readData[3]);
            };
            Game.RunOneFrame();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData_startIndex(bool dynamic)
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = (dynamic)
                    ? new DynamicVertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                    : new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
                vertexBuffer.SetData(savedData);

                var readData = new VertexPositionTexture[4];
                vertexBuffer.GetData(readData, 2, 2);
                Assert.AreEqual(vertexZero, readData[0]);
                Assert.AreEqual(vertexZero, readData[1]);
                Assert.AreEqual(savedData[0], readData[2]);
                Assert.AreEqual(savedData[1], readData[3]);
            };
            Game.RunOneFrame();
        }
        
        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData_offsetInBytes(bool dynamic)
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = (dynamic)
                    ? new DynamicVertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                    : new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
                vertexBuffer.SetData(savedData);

                var readData = new VertexPositionTexture[2];
                var vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
                var offsetInBytes = vertexStride * 2;
                vertexBuffer.GetData(offsetInBytes, readData, 0, 2, vertexStride);
                Assert.AreEqual(savedData[2], readData[0]);
                Assert.AreEqual(savedData[3], readData[1]);
            };
            Game.RunOneFrame();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void GetPosition(bool dynamic)
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = (dynamic)
                    ? new DynamicVertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                    : new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
                vertexBuffer.SetData(savedData);

                var readData = new Vector3[4];
                var vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
                vertexBuffer.GetData(0, readData, 0, 4, vertexStride);
                Assert.AreEqual(savedData[0].Position, readData[0]);
                Assert.AreEqual(savedData[1].Position, readData[1]);
                Assert.AreEqual(savedData[2].Position, readData[2]);
                Assert.AreEqual(savedData[3].Position, readData[3]);
            };
            Game.RunOneFrame();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void SetPosition(bool dynamic)
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = (dynamic)
                    ? new DynamicVertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                    : new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
                var positions = new[]
                {
                    savedData[0].Position,
                    savedData[1].Position,
                    savedData[2].Position,
                    savedData[3].Position
                };
                var vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
                vertexBuffer.SetData(0, positions, 0, 4, vertexStride);

                var readData = new Vector3[4];
                vertexBuffer.GetData(0, readData, 0, 4, vertexStride);
                Assert.AreEqual(savedData[0].Position, readData[0]);
                Assert.AreEqual(savedData[1].Position, readData[1]);
                Assert.AreEqual(savedData[2].Position, readData[2]);
                Assert.AreEqual(savedData[3].Position, readData[3]);
            };
            Game.RunOneFrame();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void GetTextureCoordinate(bool dynamic)
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = (dynamic)
                    ? new DynamicVertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                    : new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
                vertexBuffer.SetData(savedData);

                var readData = new Vector2[4];
                var vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;                
                var offsetInBytes = VertexPositionTexture.VertexDeclaration.GetVertexElements()[1].Offset;
                vertexBuffer.GetData(offsetInBytes, readData, 0, 4, vertexStride);
                Assert.AreEqual(savedData[0].TextureCoordinate, readData[0]);
                Assert.AreEqual(savedData[1].TextureCoordinate, readData[1]);
                Assert.AreEqual(savedData[2].TextureCoordinate, readData[2]);
                Assert.AreEqual(savedData[3].TextureCoordinate, readData[3]);
            };
            Game.RunOneFrame();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void SetTextureCoordinate(bool dynamic)
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = (dynamic)
                    ? new DynamicVertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                    : new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
                var texCoords = new[]
                {
                    savedData[0].TextureCoordinate,
                    savedData[1].TextureCoordinate,
                    savedData[2].TextureCoordinate,
                    savedData[3].TextureCoordinate
                };
                var vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
                var offsetInBytes = VertexPositionTexture.VertexDeclaration.GetVertexElements()[1].Offset;
                vertexBuffer.SetData(offsetInBytes, texCoords, 0, 4, vertexStride);

                var readData = new Vector2[4];
                vertexBuffer.GetData(offsetInBytes, readData, 0, 4, vertexStride);
                Assert.AreEqual(savedData[0].TextureCoordinate, readData[0]);
                Assert.AreEqual(savedData[1].TextureCoordinate, readData[1]);
                Assert.AreEqual(savedData[2].TextureCoordinate, readData[2]);
                Assert.AreEqual(savedData[3].TextureCoordinate, readData[3]);
            };
            Game.RunOneFrame();
        }
    }
}
