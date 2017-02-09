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

namespace MonoGame.Tests.Graphics
{
    class Texture2DTest : GraphicsDeviceTestFixtureBase
    {

#if !XNA
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
            var effect = AssetTestUtility.CompileEffect(gd, "TextureArrayEffect.fx");
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
    }
}
