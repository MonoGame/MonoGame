﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using NUnit.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    class IndexBufferTest: GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ShouldSetAndGetData()
        {   
            var savedData = new short[] { 1, 2, 3, 4 };
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);

            var readData = new short[4];
            indexBuffer.GetData(readData, 0, 4);
            Assert.AreEqual(savedData, readData);
            
            indexBuffer.Dispose();
        }

        [Test]
        public void ShouldSetAndGetData_elementCount()
        {
            var savedData = new short[] { 1, 2, 3, 4 };
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);
            
            var readData = new short[4];
            indexBuffer.GetData(readData, 0, 2);
            Assert.AreEqual(1, readData[0]);
            Assert.AreEqual(2, readData[1]);
            Assert.AreEqual(0, readData[2]);
            Assert.AreEqual(0, readData[3]);

            indexBuffer.Dispose();
        }

        [Test]
        public void ShouldSetAndGetData_startIndex()
        {
            var savedData = new short[] { 1, 2, 3, 4 };
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);

            var readData = new short[4];
            indexBuffer.GetData(readData, 2, 2);
            Assert.AreEqual(0, readData[0]);
            Assert.AreEqual(0, readData[1]);
            Assert.AreEqual(1, readData[2]);
            Assert.AreEqual(2, readData[3]);

            indexBuffer.Dispose();
        }

        [Test]
        public void ShouldSetAndGetData_offsetInBytes()
        {
            var savedData = new short[] { 1, 2, 3, 4 };
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);

            var readData = new short[2];
            indexBuffer.GetData(sizeof(short) * 2, readData, 0, 2);
            Assert.AreEqual(3, readData[0]);
            Assert.AreEqual(4, readData[1]);

            indexBuffer.Dispose();
        }
        
        [Test]
        public void NullDeviceShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
            {
                var indexBuffer = new IndexBuffer(null, IndexElementSize.SixteenBits, 3, BufferUsage.None);
                indexBuffer.Dispose();
            });
            GC.GetTotalMemory(true); // collect uninitialized IndexBuffer
        }
    }
}
