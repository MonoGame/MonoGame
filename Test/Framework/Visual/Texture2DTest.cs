#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.ContentPipeline;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    class Texture2DTest : VisualTestFixtureBase
    {
        [TestCase(1, 1)]
        [TestCase(8, 8)]
        [TestCase(31, 7)]
        public void ShouldSetAndGetData(int width, int height)
        {
            Game.DrawWith += (sender, e) =>
            {
                var dataSize = width * height;
                var texture2D = new Texture2D(Game.GraphicsDevice, width, height, false, SurfaceFormat.Color);
                var savedData = new Color[dataSize];
                for (var index = 0; index < dataSize; index++) savedData[index] = new Color(index % 255, index % 255, index % 255);
                texture2D.SetData(savedData);

                var readData = new Color[dataSize];
                texture2D.GetData(readData);

                Assert.AreEqual(savedData, readData);
            };
            Game.Run();
        }

        [TestCase(1, 1)]
        [TestCase(8, 8)]
        [TestCase(31, 7)]
        public void ShouldSetAndGetDataForLevel(int width, int height)
        {
            Game.DrawWith += (sender, e) =>
            {
                var texture2D = new Texture2D(Game.GraphicsDevice, width, height, true, SurfaceFormat.Color);

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
            };
            Game.Run();
        }

        [Test]
        public void ShouldGetDataFromRectangle()
        {
            Game.DrawWith += (sender, e) =>
            {
                const int dataSize = 128 * 128;
                var texture2D = new Texture2D(Game.GraphicsDevice, 128, 128, false, SurfaceFormat.Color);
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
            };
            Game.Run();
        }
		
#if !XNA
        [TestCase(SurfaceFormat.Color, false)]
        [TestCase(SurfaceFormat.Color, true)]
        [TestCase(SurfaceFormat.ColorSRgb, false)]
        [TestCase(SurfaceFormat.ColorSRgb, true)]
        public void DrawWithSRgbFormats(SurfaceFormat textureFormat, bool sRgbSourceTexture)
        {
            SpriteBatch spriteBatch = null;
            Texture2D texture = null;

            Game.LoadContentWith += (sender, e) =>
            {
                spriteBatch = new SpriteBatch(Game.GraphicsDevice);

                var width = Game.GraphicsDevice.Viewport.Width;
                var height = Game.GraphicsDevice.Viewport.Height;

                // Create gradient texture. This will highlight the difference
                // between sRGB and non-sRGB textures.

                texture = new Texture2D(
                    Game.GraphicsDevice, width, height,
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
            };

            Game.DrawWith += (sender, e) =>
            {
                Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

                spriteBatch.Begin();
                spriteBatch.Draw(texture, Vector2.Zero, Color.White);
                spriteBatch.End();
            };

            Game.UnloadContentWith += (sender, e) =>
            {
                spriteBatch.Dispose();
                spriteBatch = null;

                texture.Dispose();
                texture = null;
            };

            RunSingleFrameTest();
        }
#endif

#if !XNA
        [TestCase(1, 1)]
        [TestCase(8, 8)]
        [TestCase(31, 7)]
        public void ShouldSetAndGetDataForTextureArray(int width, int height)
        {
            Game.DrawWith += (sender, e) =>
            {
                const int arraySize = 4;
                var texture2D = new Texture2D(Game.GraphicsDevice, width, height, true, SurfaceFormat.Color, arraySize);

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
            };
            Game.Run();
        }
#endif

#if DIRECTX
        [Test]
        public void TextureArrayAsRenderTargetAndShaderResource()
        {
            Game.DrawWith += (sender, e) =>
            {
                var solidColorTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
                solidColorTexture.SetData(new[] { Color.White });

                const int arraySize = 4;

                // Create texture array.
                var textureArray = new RenderTarget2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color,
                    DepthFormat.None, 1, RenderTargetUsage.PlatformContents, false, arraySize);

                var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow };

                var originalRenderTargets = Game.GraphicsDevice.GetRenderTargets();

                // Bind each slice of texture array as render target, and render (different) solid color to each slice.
                var spriteBatch = new SpriteBatch(Game.GraphicsDevice);
                for (var i = 0; i < arraySize; i++)
                {
                    Game.GraphicsDevice.SetRenderTarget(textureArray, i);

                    spriteBatch.Begin();
                    spriteBatch.Draw(solidColorTexture, Game.GraphicsDevice.Viewport.Bounds, colors[i]);
                    spriteBatch.End();
                }

                // Unbind texture array.
                Game.GraphicsDevice.SetRenderTargets(originalRenderTargets);

                // Now render into backbuffer, using texture array as a shader resource.
                var effect = AssetTestUtility.CompileEffect(Game.GraphicsDevice, "TextureArrayEffect.fx");
                effect.Parameters["Texture"].SetValue(textureArray);
                effect.CurrentTechnique.Passes[0].Apply();

                Game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                // Vertex buffer is not actually used, but currently we need to set a
                // vertex buffer before calling DrawPrimitives.
                Game.GraphicsDevice.SetVertexBuffer(new VertexBuffer(Game.GraphicsDevice,
                    typeof(VertexPositionColor), 3, BufferUsage.WriteOnly));

                Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            };
            RunSingleFrameTest();
        }
#endif
    }
}
