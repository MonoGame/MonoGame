// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [NonParallelizable]
    [RunOnUiTestFixture]
    class Texture2DTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        [TestCase(1, 1)]
        [TestCase(8, 8)]
        [TestCase(31, 7)]
        public void ShouldSetAndGetData(int width, int height)
        {
            var dataSize = width * height;
            var texture2D = new Texture2D(gd, width, height, false, SurfaceFormat.Color);
            var savedData = new Color[dataSize];
            for (var index = 0; index < dataSize; index++) savedData[index] = new Color(index % 255, index % 255, index % 255);
            texture2D.SetData(savedData);

            var readData = new Color[dataSize];
            texture2D.GetData(readData);

            Assert.AreEqual(savedData, readData);

            texture2D.Dispose();
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(8, 8)]
        [TestCase(31, 7)]
        public void ShouldSetAndGetDataForLevel(int width, int height)
        {
            var texture2D = new Texture2D(gd, width, height, true, SurfaceFormat.Color);

            for (int i = 0; i < texture2D.LevelCount; i++)
            {
                var levelSize = Math.Max(width >> i, 1) * Math.Max(height >> i, 1);

                var savedData = new Color[levelSize];
                for (var index = 0; index < levelSize; index++)
                    savedData[index] = new Color(index % 255, index % 255, index % 255);
                texture2D.SetData(i, null, savedData, 0, savedData.Length);

                var readData = new Color[levelSize];
                texture2D.GetData(i, null, readData, 0, savedData.Length);

                Assert.AreEqual(savedData, readData);
            }

            texture2D.Dispose();
        }

        [Test]
        public void ShouldGetDataFromRectangle()
        {
            const int dataSize = 128 * 128;
            var texture2D = new Texture2D(gd, 128, 128, false, SurfaceFormat.Color);
            var savedData = new Color[dataSize];
            for (var index = 0; index < dataSize; index++) savedData[index] = new Color(index % 255, index % 255, index % 255);
            texture2D.SetData(savedData);

            var readData = new Color[4];
            texture2D.GetData(0, new Rectangle(126, 126, 2, 2), readData, 0, 4);

            var expectedData = new[]
            {
                new Color(189, 189, 189),
                new Color(190, 190, 190),
                new Color(62, 62, 62),
                new Color(63, 63, 63)
            };
            Assert.AreEqual(expectedData, readData);

            texture2D.Dispose();
        }
		
#if DIRECTX
        [Test]
        [TestCase(SurfaceFormat.Color, false)]
        [TestCase(SurfaceFormat.Color, true)]
        [TestCase(SurfaceFormat.ColorSRgb, false)]
        [TestCase(SurfaceFormat.ColorSRgb, true)]
        public void DrawWithSRgbFormats(SurfaceFormat textureFormat, bool sRgbSourceTexture)
        {
            PrepareFrameCapture();

            var spriteBatch = new SpriteBatch(gd);

            var width = gd.Viewport.Width;
            var height = gd.Viewport.Height;

            // Create gradient texture. This will highlight the difference
            // between sRGB and non-sRGB textures.

            var texture = new Texture2D(
                gd, width, height,
                false, textureFormat);

            var heightOver3 = height / 3;

            var textureData = new Color[width * height];
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var colorValue = x / (float) width;

                    // Approximation of sRGB - it's not actually as simple as this,
                    // but it will suffice for these tests.
                    if (sRgbSourceTexture)
                        colorValue = (float) Math.Pow(colorValue, 1 / 2.2);

                    var color = (y < heightOver3) ? new Color(colorValue, 0, 0)
                        : (y < heightOver3 * 2) ? new Color(0, colorValue, 0)
                        : new Color(0, 0, colorValue);
                    textureData[(y * width) + x] = color;
                }
            texture.SetData(textureData);

            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

            spriteBatch.Begin();
            spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            spriteBatch.End();

            CheckFrames();

            spriteBatch.Dispose();
            texture.Dispose();
        }
#endif

#if !XNA
        [Test]
        [TestCase(1, 1)]
        [TestCase(8, 8)]
        [TestCase(31, 7)]
#if DESKTOPGL
        [Ignore("Not yet implemented in OpenGL")]
#endif
        public void ShouldSetAndGetDataForTextureArray(int width, int height)
        {
            const int arraySize = 4;
            var texture2D = new Texture2D(gd, width, height, true, SurfaceFormat.Color, arraySize);

            for (var i = 0; i < arraySize; i++)
                for (var j = 0; j < texture2D.LevelCount; j++)
                {
                    var levelSize = Math.Max(width >> j, 1) * Math.Max(height >> j, 1);

                    var savedData = new Color[levelSize];
                    for (var index = 0; index < levelSize; index++)
                        savedData[index] = new Color((index + i) % 255, (index + i) % 255, (index + i) % 255);
                    texture2D.SetData(j, i, null, savedData, 0, savedData.Length);

                    var readData = new Color[levelSize];
                    texture2D.GetData(j, i, null, readData, 0, readData.Length);

                    Assert.AreEqual(savedData, readData);
                }

            texture2D.Dispose();
        }
#endif

#if DIRECTX
        [Test]
        public void TextureArrayAsRenderTargetAndShaderResource()
        {
            PrepareFrameCapture();

            var solidColorTexture = new Texture2D(gd, 1, 1);
            solidColorTexture.SetData(new[] { Color.White });

            const int arraySize = 4;

            // Create texture array.
            var textureArray = new RenderTarget2D(gd, 1, 1, false, SurfaceFormat.Color,
                DepthFormat.None, 1, RenderTargetUsage.PlatformContents, false, arraySize);

            var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow };

            var originalRenderTargets = gd.GetRenderTargets();

            // Bind each slice of texture array as render target, and render (different) solid color to each slice.
            var spriteBatch = new SpriteBatch(gd);
            for (var i = 0; i < arraySize; i++)
            {
                gd.SetRenderTarget(textureArray, i);

                spriteBatch.Begin();
                spriteBatch.Draw(solidColorTexture, gd.Viewport.Bounds, colors[i]);
                spriteBatch.End();
            }

            // Unbind texture array.
            gd.SetRenderTargets(originalRenderTargets);

            // Now render into backbuffer, using texture array as a shader resource.
            var effect = AssetTestUtility.LoadEffect(content, "TextureArrayEffect");
            effect.Parameters["Texture"].SetValue(textureArray);
            effect.CurrentTechnique.Passes[0].Apply();

            gd.SamplerStates[0] = SamplerState.LinearClamp;

            // Vertex buffer is not actually used, but currently we need to set a
            // vertex buffer before calling DrawPrimitives.
            var vb = new VertexBuffer(gd, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);
            gd.SetVertexBuffer(vb);

            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);

            CheckFrames();

            solidColorTexture.Dispose();
            spriteBatch.Dispose();
            textureArray.Dispose();
            effect.Dispose();
            vb.Dispose();
        }
#endif

        [Test]
        public void SetDataRowPitch()
        {
            PrepareFrameCapture();

            var t = content.Load<Texture2D>(Paths.Texture("Logo_65x64_16bit"));
            var sb = new SpriteBatch(gd);
            sb.Begin();
            sb.Draw(t, new Rectangle(100, 100, 300, 300), null, Color.White);
            sb.End();

            t.Dispose();
            sb.Dispose();

            CheckFrames();
        }

        static readonly Color[] sampleTextureColors = [Color.CornflowerBlue, Color.Red, Color.Green, Color.Blue, Color.White, Color.Black, Color.Transparent, new Color(Color.DarkSlateBlue, 0.5f), Color.Chartreuse];
        static Texture2D MakeSampleTexture(GraphicsDevice gd, int width, int height, out Color[] pixels)
        {
            // Make an array of colors matching the width and height
            static Color getExpectedColor(int index) => sampleTextureColors[index % sampleTextureColors.Length];
            pixels =
                Enumerable.Range(0, width * height)
                .Select(getExpectedColor)
                .ToArray();

            // Make a texture with the data
            Texture2D tex = new Texture2D(gd, width, height);
            tex.SetData(pixels);

            return tex;
        }

        [Test]
        // Nice small square
        [TestCase(64, 64)]
        // One pixel
        [TestCase(1, 1)]
        // Large square
        [TestCase(2048, 2048)]
        // Large non-square
        [TestCase(2048, 1234)]
        // Small non-power-of-2 square
        [TestCase(7, 7)]
        // Small non-square
        [TestCase(15, 31)]
        public void SaveAsPngShouldWork(int width, int height)
        {
            using var source = MakeSampleTexture(gd, width, height, out var expectedPixels);
            // Save the texture to a memory stream
            using var stream = new MemoryStream(width * height * 4); // 4 bytes in a Color;
            source.SaveAsPng(stream, width, height);

            source.Dispose();

            // Create a new texture from the stream
            using var result = Texture2D.FromStream(gd, stream);

            // The result should be the same size
            Assert.AreEqual(result.width, width);
            Assert.AreEqual(result.height, height);

            Color[] resultPixels = new Color[width * height];
            result.GetData(resultPixels);

            // It should have the same pixels
            Assert.AreEqual(expectedPixels, resultPixels);

            result.Dispose();
        }

        [Test]
        // Nice small square
        [TestCase(64, 64)]
        // One pixel
        [TestCase(1, 1)]
        // Large square
        [TestCase(1024, 1024)]
        // Large non-square
        [TestCase(1024, 1234)]
        // Small non-power-of-2 square
        [TestCase(7, 7)]
        // Small non-square
        [TestCase(15, 31)]
        public void SaveAsJpegShouldWork(int width, int height)
        {
            // Make a test texture and save it to a memory stream
            using var source = MakeSampleTexture(gd, width, height, out var expectedPixels);
            using var stream = new MemoryStream(width * height * 4); // 4 bytes in a Color;
            source.SaveAsJpeg(stream, width, height);

            // Create a new texture from the stream.
            using var result = Texture2D.FromStream(gd, stream);
            // Read back pixels
            var resultPixels = new Color[width * height];
            result.GetData(resultPixels);

            // The maximum value by which a source pixel and the destination pixel can vary in one color channel
            const int tolerance = 35;// This is arbitrary and was chosen just because it works with the test cases and the generated sample images

            // Compare the source pixels to the result pixels, with tolerance
            for (int i = 0; i < expectedPixels.Length; i++)
            {
                Color expectedPixel = expectedPixels[i];
                Color resultPixel = resultPixels[i];
                int rDiff = Math.Abs(expectedPixel.R - resultPixel.R);
                int gDiff = Math.Abs(expectedPixel.G - resultPixel.G);
                int bDiff = Math.Abs(expectedPixel.B - resultPixel.B);
                // Don't test alpha because jpeg doesn't support it

                bool withinTolerance =
                    rDiff <= tolerance &&
                    gDiff <= tolerance &&
                    bDiff <= tolerance;
                Assert.True(withinTolerance,
                    "Pixel {0} ({1}, {2}) differs beyond tolerance: {3} expected, got {4}.",
                    i,
                    i % width,
                    i / width,
                    expectedPixel,
                    resultPixel
                    );
            }
        }
    }
}
