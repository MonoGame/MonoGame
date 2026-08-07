// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.IO;

namespace MonoGame.Tests.Graphics
{
    [NonParallelizable]
    [RunOnUiTestFixture]
    class VertexBufferTest : GraphicsDeviceTestFixtureBase
    {
        VertexPositionTexture[] savedData = new VertexPositionTexture[] 
        {
            new VertexPositionTexture(new Vector3(1,2,3), new Vector2(0.1f,0.2f)),
            new VertexPositionTexture(new Vector3(4,5,6), new Vector2(0.3f,0.4f)),
            new VertexPositionTexture(new Vector3(7,8,9), new Vector2(0.5f,0.6f)),
            new VertexPositionTexture(new Vector3(10,11,12), new Vector2(0.7f,0.8f))
        };
        public Span<VertexPositionTexture> savedDataAsSpan => savedData.AsSpan();
        VertexPositionTexture vertexZero = new VertexPositionTexture(Vector3.Zero, Vector2.Zero);
        
        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData(bool dynamic)
        {   
            var vertexBuffer = (dynamic)
                ?new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                :new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            var readData = new VertexPositionTexture[4];
            vertexBuffer.GetData(readData, 0, 4);
            Assert.AreEqual(savedData, readData);

            vertexBuffer.Dispose();
        }

        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData_elementCount(bool dynamic)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            var readData = new VertexPositionTexture[4];
            vertexBuffer.GetData(readData, 0, 2);
            Assert.AreEqual(savedData[0], readData[0]);
            Assert.AreEqual(savedData[1], readData[1]);
            Assert.AreEqual(vertexZero, readData[2]);
            Assert.AreEqual(vertexZero, readData[3]);

            vertexBuffer.Dispose();
        }

        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData_startIndex(bool dynamic)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            var readData = new VertexPositionTexture[4];
            vertexBuffer.GetData(readData, 2, 2);
            Assert.AreEqual(vertexZero, readData[0]);
            Assert.AreEqual(vertexZero, readData[1]);
            Assert.AreEqual(savedData[0], readData[2]);
            Assert.AreEqual(savedData[1], readData[3]);

            vertexBuffer.Dispose();
        }
        
        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData_offsetInBytes(bool dynamic)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            var readData = new VertexPositionTexture[2];
            var vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
            var offsetInBytes = vertexStride * 2;
            vertexBuffer.GetData(offsetInBytes, readData, 0, 2, vertexStride);
            Assert.AreEqual(savedData[2], readData[0]);
            Assert.AreEqual(savedData[3], readData[1]);

            vertexBuffer.Dispose();
        }

        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetDataBytes(bool dynamic)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            var savedDataBytes = ArrayUtil.ConvertFrom(savedData);
            vertexBuffer.SetData(savedDataBytes);

            if (dynamic)
            {
                var dynamicVertexBuffer = vertexBuffer as DynamicVertexBuffer;
                dynamicVertexBuffer.SetData(savedDataBytes, 0, savedDataBytes.Length, SetDataOptions.None);
            }

            var readData = new VertexPositionTexture[4];
            vertexBuffer.GetData(readData, 0, 4);
            Assert.AreEqual(savedData, readData);

            vertexBuffer.Dispose();
        }

        [Test]
        //[TestCase(true)]
        [TestCase(false, -1, 0, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 0, 0, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 0, 1, true, null)]
        [TestCase(false, 0, -1, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 0, 80, true, null)]
        [TestCase(false, 0, 81, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 1, 0, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 1, 1, true, null)]
        [TestCase(false, 1, 79, true, null)]
        [TestCase(false, 1, 80, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 79, 1, true, null)]
        [TestCase(false, 79, 2, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 80, 0, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 80, 1, false, typeof(ArgumentOutOfRangeException))]
        public void SetDataWithElementCount(bool dynamic, int startIndex, int elementCount, bool shouldSucceed, Type expectedExceptionType)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,
                    BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,
                    BufferUsage.None);
            var savedDataBytes = ArrayUtil.ConvertFrom(savedData);

            if (!shouldSucceed)
                Assert.Throws(expectedExceptionType, () => vertexBuffer.SetData(savedDataBytes, startIndex, elementCount));
            else
            {
                vertexBuffer.SetData(savedDataBytes, startIndex, elementCount);

                var readDataBytes = new byte[savedDataBytes.Length];
                vertexBuffer.GetData(readDataBytes, startIndex, elementCount);
                Assert.AreEqual(
                    savedDataBytes.Skip(startIndex).Take(elementCount).ToArray(),
                    readDataBytes.Skip(startIndex).Take(elementCount).ToArray());
            }

            vertexBuffer.Dispose();
        }

        [Test]
        [TestCase(false, 1, -1, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 0, 0, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 80, 0, null)]
        [TestCase(false, 80, 1, null)]
        [TestCase(false, 1, 2, null)]
        [TestCase(false, 1, 40, null)]
        [TestCase(false, 2, 40, null)]
        [TestCase(false, 2, 80, typeof(InvalidOperationException))]
        [TestCase(false, 1, 80, null)]
        [TestCase(false, 4, 12, null)]
#if XNA
        [TestCase(false, 1, 81, null)]
        [TestCase(false, 2, 81, typeof(InvalidOperationException))]
#else
        // We throw when the vertex stride is too large
        [TestCase(false, 1, 81, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 2, 81, typeof(ArgumentOutOfRangeException))]
#endif
        public void SetDataWithElementCountAndVertexStride(bool dynamic, int elementCount, int vertexStride, Type expectedExceptionType)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,
                    BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,
                    BufferUsage.None);
            var savedDataBytes = ArrayUtil.ConvertFrom(savedData);

            if (expectedExceptionType != null)
                Assert.Throws(expectedExceptionType, () => vertexBuffer.SetData(0, savedDataBytes, 0, elementCount, vertexStride));
            else
            {
                vertexBuffer.SetData(0, savedDataBytes, 0, elementCount, vertexStride);

                var readDataBytes = new byte[savedDataBytes.Length];
                vertexBuffer.GetData(0, readDataBytes, 0, elementCount, vertexStride);
                Assert.AreEqual(
                    savedDataBytes.Take(elementCount).ToArray(), 
                    readDataBytes.Take(elementCount).ToArray());
            }

            vertexBuffer.Dispose();
        }

        [Test]
        public void BetterGetSetDataVertexStrideTest()
        {
            const int size = 5;
            var data = new VertexPositionTexture[size];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = new VertexPositionTexture(
                    new Vector3(i * 3, i * 3 + 1, i * 3 + 2),
                    new Vector2(i * 2 / (float) 10, (i * 2 + 1) / (float) 10));
            }

            var vb = new VertexBuffer(gd, VertexPositionTexture.VertexDeclaration, data.Length, BufferUsage.None);
            vb.SetData(data);

            var textureCoords = new Vector2[2 * size + 1];
            textureCoords[0] = new Vector2(-42, 42);
            vb.GetData(3 * 4, textureCoords, 1, size, 20);

            // first one should not be overwritten
            Assert.AreEqual(new Vector2(-42, 42), textureCoords[0]);
            for (var i = 0; i < size; i++)
            {
                var index = i + 1;
                var expected = new Vector2(i * 2 / (float) 10, (i * 2 + 1) / (float) 10);
                Assert.AreEqual(expected, textureCoords[index]);
            }

            vb.SetData(3 * 4, textureCoords, 1, size, 20);
            vb.GetData(3 * 4, textureCoords, 1, size, 20);

            // first one should not be overwritten
            Assert.AreEqual(new Vector2(-42, 42), textureCoords[0]);
            for (var i = 0; i < size; i++)
            {
                var index = i + 1;
                var expected = new Vector2(i * 2 / (float) 10, (i * 2 + 1) / (float) 10);
                Assert.AreEqual(expected, textureCoords[index]);
            }
        }

        [Test]
        //[TestCase(true)]
        [TestCase(false, 1, 20, true, null)]
        [TestCase(false, 3, 20, true, null)]
        [TestCase(false, 4, 0, true, null)]
        [TestCase(false, 4, 16, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 4, 20, true, null)]
        [TestCase(false, 5, 20, false, typeof(ArgumentOutOfRangeException))]
        public void SetDataStructWithElementCountAndVertexStride(bool dynamic, int elementCount, int vertexStride, bool shouldSucceed, Type expectedExceptionType)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,
                    BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,
                    BufferUsage.None);

            if (!shouldSucceed)
                Assert.Throws(expectedExceptionType, () => vertexBuffer.SetData(0, savedData, 0, elementCount, vertexStride));
            else
            {
                vertexBuffer.SetData(0, savedData, 0, elementCount, vertexStride);

                var readData = new VertexPositionTexture[savedData.Length];
                vertexBuffer.GetData(0, readData, 0, elementCount, vertexStride);
                Assert.AreEqual(
                    savedData.Take(elementCount).ToArray(),
                    readData.Take(elementCount).ToArray());
            }

            vertexBuffer.Dispose();
        }

#if VULKAN || DIRECTX12
        [Test]
        //[TestCase(true)]
        [TestCase(false, 0, 4, true, null)]
        [TestCase(false, 1, 3, true, null)]
        [TestCase(false, 1, 2, true, null)]
        [TestCase(false, 4, 1, false, typeof(ArgumentOutOfRangeException))]
        public void SetDataStructWithSpan(bool dynamic, int destinationStartIndex, int elementCount, bool shouldSucceed, Type expectedExceptionType)
        {
            const int size = 4;
            var testData = new VertexPositionTexture[size];
            for (var i = 0; i < size; i++)
            {
                testData[i] = new VertexPositionTexture(
                    new Vector3(i * 3, i * 3 + 1, i * 3 + 2),
                    new Vector2(i * 2 / (float)10, (i * 2 + 1) / (float)10));
            }

            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,
                    BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,
                    BufferUsage.None);
            var dataSpan = new Span<VertexPositionTexture>();
            if (shouldSucceed)
            { 
                dataSpan = new Span<VertexPositionTexture>(testData, destinationStartIndex, elementCount);
            }
            else
            {
                dataSpan = new Span<VertexPositionTexture>(testData);
            }

            var vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;

            // initialize data with standard call
            vertexBuffer.SetData(savedData);

            if (!shouldSucceed)
                Assert.Throws(expectedExceptionType, () => vertexBuffer.SetData(destinationStartIndex, savedDataAsSpan));
            else
            {
                // initialize with standard call
                vertexBuffer.SetData(destinationStartIndex, dataSpan);

                var readData = new VertexPositionTexture[savedData.Length];
                vertexBuffer.GetData(0, readData, 0, savedData.Length, vertexStride);
                Assert.AreEqual(
                    dataSpan.ToArray(),
                    readData.Take(destinationStartIndex..(destinationStartIndex + elementCount)).ToArray());
                for(int i = 0; i < savedData.Length; i++)
                {
                    if (i < destinationStartIndex || i >= destinationStartIndex + elementCount)
                    {
                        Assert.AreEqual(savedData[i], readData[i]);
                    }
                }
            }

            vertexBuffer.Dispose();
        }
#endif

        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public void GetPosition(bool dynamic)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            var readData = new Vector3[4];
            var vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
            vertexBuffer.GetData(0, readData, 0, 4, vertexStride);
            Assert.AreEqual(savedData[0].Position, readData[0]);
            Assert.AreEqual(savedData[1].Position, readData[1]);
            Assert.AreEqual(savedData[2].Position, readData[2]);
            Assert.AreEqual(savedData[3].Position, readData[3]);

            vertexBuffer.Dispose();
        }

        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public void SetPosition(bool dynamic)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
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

            vertexBuffer.Dispose();
        }

        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public void GetTextureCoordinate(bool dynamic)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            var readData = new Vector2[4];
            var vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;                
            var offsetInBytes = VertexPositionTexture.VertexDeclaration.GetVertexElements()[1].Offset;
            vertexBuffer.GetData(offsetInBytes, readData, 0, 4, vertexStride);
            Assert.AreEqual(savedData[0].TextureCoordinate, readData[0]);
            Assert.AreEqual(savedData[1].TextureCoordinate, readData[1]);
            Assert.AreEqual(savedData[2].TextureCoordinate, readData[2]);
            Assert.AreEqual(savedData[3].TextureCoordinate, readData[3]);

            vertexBuffer.Dispose();
        }

        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public void SetTextureCoordinate(bool dynamic)
        {
            var vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
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

            vertexBuffer.Dispose();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VertexTextureCoordinateTest : IVertexType
        {
            public Vector3 Normal;
            public Vector2 TextureCoordinate;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get { return VertexDeclaration; }
            }
        }

        [Test]
        public void ShouldSucceedWhenVertexFormatDoesMatchShader()
        {
            var vertexBuffer = new VertexBuffer(
                gd, VertexPositionTexture.VertexDeclaration, 3,
                BufferUsage.None);
            gd.SetVertexBuffer(vertexBuffer);

            var effect = new BasicEffect(gd);
            effect.CurrentTechnique.Passes[0].Apply();

            Assert.DoesNotThrow(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));

            vertexBuffer.Dispose();
        }

        [Test]
#if DESKTOPGL
        [Ignore("we should figure out if there's a way to check this in OpenGL")]
#endif
        public void ShouldThrowHelpfulExceptionWhenVertexFormatDoesNotMatchShader()
        {
            var vertexBuffer = new VertexBuffer(
                gd, VertexTextureCoordinateTest.VertexDeclaration, 3,
                BufferUsage.None);
            gd.SetVertexBuffer(vertexBuffer);

            var effect = new BasicEffect(gd);
            effect.CurrentTechnique.Passes[0].Apply();

            var ex = Assert.Throws<InvalidOperationException>(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));
#if XNA
            Assert.That(ex.Message, Is.EqualTo("The current vertex declaration does not include all the elements required by the current vertex shader. Position0 is missing."));
#else
            Assert.That(ex.Message, Is.EqualTo("An error occurred while preparing to draw. "
                + "This is probably because the current vertex declaration does not include all the elements "
                + "required by the current vertex shader. The current vertex declaration includes these elements: "
#if VULKAN || DIRECTX12
                + "POSITION0."));
#else
                + "NORMAL0, TEXCOORD0."));
#endif
#endif

            vertexBuffer.Dispose();
        }

        [Test]
        public void NullDeviceShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
            {
                var vertexBuffer = new VertexBuffer(null, typeof(VertexPositionTexture), 3, BufferUsage.None);
                vertexBuffer.Dispose();
            });
            GC.GetTotalMemory(true); // collect uninitialized vertexBuffer
        }

        [Test]
        public void TestVertexInterpolation_NormalizedShort4()
        {
            TestVertexInterpolation(new NormalizedShort4[]
            {
                new NormalizedShort4(0, 0, 0, 0),
                new NormalizedShort4(0, 1, 0, 0),
                new NormalizedShort4(1, 0, 0, 0),
                new NormalizedShort4(1, 1, 0, 0)
            },
            VertexElementFormat.NormalizedShort4);
        }

        [Test]
        public void TestVertexInterpolation_NormalizedShort2()
        {
            TestVertexInterpolation(new NormalizedShort2[]
            {
                new NormalizedShort2(0, 0),
                new NormalizedShort2(0, 1),
                new NormalizedShort2(1, 0),
                new NormalizedShort2(1, 1)
            },
            VertexElementFormat.NormalizedShort2);
        }

        private void TestVertexInterpolation<TVertex>(TVertex[] data, VertexElementFormat format)
            where TVertex : struct
        {
            var effect = content.Load<Effect>(Paths.CompiledEffect("VertexInterpolationTest"));

            RenderTarget2D rt = null;
            VertexBuffer vb_pos = null;
            VertexDeclaration decl = null;
            VertexBuffer vb_data = null;

            try
            {
                rt = new RenderTarget2D(gd, 256, 256, false,
                    SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

                var pos_data = new Vector3[]
                {
                    new Vector3(-1, -1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(1, 1, 0),
                };
                vb_pos = new VertexBuffer(gd, VertexPosition.VertexDeclaration, 4, BufferUsage.WriteOnly);
                vb_pos.SetData(pos_data);

                decl = new VertexDeclaration(
                    new VertexElement
                    {
                        Offset = 0,
                        UsageIndex = 0,
                        VertexElementFormat = format,
                        VertexElementUsage = VertexElementUsage.TextureCoordinate
                    });

                vb_data = new VertexBuffer(gd, decl, 4, BufferUsage.WriteOnly);
                vb_data.SetData<TVertex>(data);

                gd.SetRenderTarget(rt);

                effect.CurrentTechnique.Passes[0].Apply();

                gd.BlendState = BlendState.Opaque;
                gd.RasterizerState = RasterizerState.CullNone;
                gd.DepthStencilState = DepthStencilState.None;

                gd.SetVertexBuffers(new VertexBufferBinding[]
                {
                    new VertexBufferBinding(vb_pos),
                    new VertexBufferBinding(vb_data)
                });

                gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

                gd.SetRenderTarget(null);

                // For testing!
                using (var stream = File.OpenWrite("vertexInterp_GL.png"))
                    rt.SaveAsPng(stream, rt.Width, rt.Height);

                // Take advantage of an internal API here.
                var color = rt.GetColorData();


                // TODO: The exact test fails on OpenGL *i think*
                // because of the interpolation standard on triangles.
                // Need to investigate.
                /*
                // Test the four corners for correct interploated values.
                Assert.AreEqual(new Color(0, 255, 0, 255), color[0]);
                Assert.AreEqual(new Color(255, 255, 0, 255), color[rt.Width - 1]);
                Assert.AreEqual(new Color(0, 0, 0, 255), color[(rt.Height * rt.Width) - rt.Width]);
                Assert.AreEqual(new Color(255, 0, 0, 255), color[(rt.Height * rt.Width) - 1]);
                */

                var corners = new Vector3[] {
                    color[0].ToVector3(),
                    color[rt.Width - 1].ToVector3(),
                    color[(rt.Height * rt.Width) - rt.Width].ToVector3(),
                    color[(rt.Height * rt.Width) - 1].ToVector3(),
                };

                var truth = new Vector3[]
                {
                    new Color(0, 255, 0, 255).ToVector3(),
                    new Color(255, 255, 0, 255).ToVector3(),
                    new Color(0, 0, 0, 255).ToVector3(),
                    new Color(255, 0, 0, 255).ToVector3(),
                };

                Assert.Less((corners[0] - truth[0]).LengthSquared(), 0.001f);
                Assert.Less((corners[1] - truth[1]).LengthSquared(), 0.001f);
                Assert.Less((corners[2] - truth[2]).LengthSquared(), 0.001f);
                Assert.Less((corners[3] - truth[3]).LengthSquared(), 0.001f);
            }
            finally
            {
                rt?.Dispose();
                vb_pos?.Dispose();
                decl?.Dispose();
                vb_data?.Dispose();
            }
        }
    }
}
