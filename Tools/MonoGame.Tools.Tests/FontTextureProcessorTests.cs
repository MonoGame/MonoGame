// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGame.Tests.ContentPipeline
{
    class FontTextureProcessorTests
    {
        [Test]
        public void ValidateDefaults()
        {
            var processor = new FontTextureProcessor();
            Assert.AreEqual(' ', processor.FirstCharacter);
            Assert.AreEqual(true, processor.PremultiplyAlpha);
            Assert.AreEqual(TextureProcessorOutputFormat.Color, processor.TextureFormat);
        }

        [Test]
        public void ColorConversion()
        {
            var context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");

            var processor = new FontTextureProcessor
            {
                FirstCharacter = '0',
                PremultiplyAlpha = false
            };

            var face = new PixelBitmapContent<Color>(32, 9);

            var O = Color.Transparent.PackedValue;
            var M = Color.Fuchsia.PackedValue;
            var R = Color.Red.PackedValue;
            var G = Color.Green.PackedValue;
            var B = Color.Blue.PackedValue;
            var W = Color.White.PackedValue;
            var pixelData = new[]
                {
                    M, M, M, M, M, M, M, M,     M, M, M, M, M, M, M, M,     M, M, M, M, M, M, M, M,     M, M, M, M, M, M, M, M,
                    M, O, O, O, O, O, O, M,     M, O, O, O, O, O, O, M,     M, O, O, O, O, O, O, M,     M, O, O, O, O, O, O, M,
                    M, O, O, W, W, O, O, M,     M, O, O, O, R, O, O, M,     M, O, G, G, G, O, O, M,     M, O, B, B, B, B, O, M,
                    M, O, W, O, O, W, O, M,     M, O, O, R, R, O, O, M,     M, O, O, O, O, G, O, M,     M, O, O, O, B, O, O, M,
                    M, O, W, O, O, W, O, M,     M, O, O, O, R, O, O, M,     M, O, O, G, G, O, O, M,     M, O, O, B, B, O, O, M,
                    M, O, W, O, O, W, O, M,     M, O, O, O, R, O, O, M,     M, O, G, O, O, O, O, M,     M, O, O, O, O, B, O, M,
                    M, O, O, W, W, O, O, M,     M, O, R, R, R, R, O, M,     M, O, G, G, G, G, O, M,     M, O, B, B, B, O, O, M,
                    M, O, O, O, O, O, O, M,     M, O, O, O, O, O, O, M,     M, O, O, O, O, O, O, M,     M, O, O, O, O, O, O, M,
                    M, M, M, M, M, M, M, M,     M, M, M, M, M, M, M, M,     M, M, M, M, M, M, M, M,     M, M, M, M, M, M, M, M,
                };
            var pixelBytes = new byte[face.Width * 4 * face.Height];
            Buffer.BlockCopy(pixelData, 0, pixelBytes, 0, pixelData.Length * 4);
            face.SetPixelData(pixelBytes);
            var input = new Texture2DContent();
            input.Faces[0] = face;

            var output = processor.Process(input, context);

            Assert.NotNull(output);

            Assert.AreEqual(output.CharacterMap[0], '0');
            Assert.AreEqual(output.CharacterMap[1], '1');
            Assert.AreEqual(output.CharacterMap[2], '2');
            Assert.AreEqual(output.CharacterMap[3], '3');

            Assert.AreEqual(1, output.Texture.Faces.Count);
            Assert.AreEqual(1, output.Texture.Faces[0].Count);

            Assert.IsAssignableFrom<PixelBitmapContent<Color>>(output.Texture.Faces[0][0]);
            var outFace = (PixelBitmapContent<Color>)output.Texture.Faces[0][0];

            for (var i = 0; i < 4; ++i)
            {
                // (2, 2, 4, 5) is the top,left and width,height of the first glyph. All test glyphs are 4x5
                var inRect = new Rectangle(i * 8 + 2, 2, 4, 5);
                var outRect = output.Glyphs[i];
                Assert.AreEqual(inRect.Width, outRect.Width);
                Assert.AreEqual(inRect.Height, outRect.Height);
                for (var y = 0; y < inRect.Height; ++y)
                    for (var x = 0; x < inRect.Width; ++x)
                        Assert.AreEqual(pixelData[(x + inRect.Left) + (y + inRect.Top) * face.Width], outFace.GetPixel(x + outRect.Left, y + outRect.Top).PackedValue);
            }
            
            Assert.AreEqual(new Rectangle(1, 1, 6, 7), output.Cropping[0]);
            Assert.AreEqual(new Rectangle(1, 1, 6, 7), output.Cropping[1]);
            Assert.AreEqual(new Rectangle(1, 1, 6, 7), output.Cropping[2]);
            Assert.AreEqual(new Rectangle(1, 1, 6, 7), output.Cropping[3]);
        }
    }
}
