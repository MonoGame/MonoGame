using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using NUnit.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace MonoGame.Tests.ContentPipeline
{
    class TextureProcessorTests
    {
        [Test]
        public void ValidateDefaults()
        {
            var processor = new TextureProcessor();
            Assert.AreEqual(new Color(255, 0, 255, 255), processor.ColorKeyColor);
            Assert.AreEqual(true, processor.ColorKeyEnabled);
            Assert.AreEqual(false, processor.GenerateMipmaps);
            Assert.AreEqual(true, processor.PremultiplyAlpha);
            Assert.AreEqual(false, processor.ResizeToPowerOfTwo);
            Assert.AreEqual(TextureProcessorOutputFormat.Color, processor.TextureFormat);
        }

        private static void Fill(PixelBitmapContent<Color> content, Color color)
        {
            var src = Enumerable.Repeat(color.PackedValue, content.Width * content.Height).ToArray();
            var dest = new byte[Marshal.SizeOf(typeof(Color)) * content.Width * content.Height];
            Buffer.BlockCopy(src, 0, dest, 0, dest.Length);
            content.SetPixelData(dest);
        }

        [Test]
        public void ColorKey()
        {
            var context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");

            var processor = new TextureProcessor
            {
                ColorKeyColor = Color.Red,
                ColorKeyEnabled = true,
                GenerateMipmaps = false,
                PremultiplyAlpha = false,
                ResizeToPowerOfTwo = false,
                TextureFormat = TextureProcessorOutputFormat.Color
            };

            var face = new PixelBitmapContent<Color>(8, 8);
            Fill(face, Color.Red);
            var input = new Texture2DContent();
            input.Faces[0] = face;

            var output = processor.Process(input, context);

            Assert.NotNull(output);
            Assert.AreEqual(1, output.Faces.Count);
            Assert.AreEqual(1, output.Faces[0].Count);

            Assert.IsAssignableFrom<PixelBitmapContent<Color>>(output.Faces[0][0]);
            var outFace = (PixelBitmapContent<Color>)output.Faces[0][0];
            Assert.AreEqual(8, outFace.Width);
            Assert.AreEqual(8, outFace.Height);

            for (var y=0; y < outFace.Height; y++)
                for (var x = 0; x < outFace.Width; x++)
                    Assert.AreEqual(Color.Transparent, outFace.GetPixel(x, y));
        }

        [Test]
        public void Mipmap()
        {
            var context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");

            var processor = new TextureProcessor
            {
                ColorKeyEnabled = false,
                GenerateMipmaps = true,
                PremultiplyAlpha = false,
                ResizeToPowerOfTwo = false,
                TextureFormat = TextureProcessorOutputFormat.Color
            };

            var face = new PixelBitmapContent<Color>(8, 8);
            Fill(face, Color.Red);
            var input = new Texture2DContent();
            input.Faces[0] = face;

            var output = processor.Process(input, context);

            Assert.NotNull(output);
            Assert.AreEqual(1, output.Faces.Count);
            //Assert.AreNotEqual(face, output.Faces[0][0]);

            var outChain = output.Faces[0];
            Assert.AreEqual(4, outChain.Count);

            var width = 8;
            var height = 8;

            foreach (var outFace in outChain)
            {
                Assert.AreEqual(width, outFace.Width);
                Assert.AreEqual(height, outFace.Height);

                var bitmap = (PixelBitmapContent<Color>)outFace;
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                        Assert.AreEqual(Color.Red, bitmap.GetPixel(x, y));

                width = width >> 1;
                height = height >> 1;
            }
        }

        [Test]
        public void ResizePowerOfTwo()
        {
            var context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");

            var processor = new TextureProcessor
            {
                ColorKeyEnabled = false,
                GenerateMipmaps = false,
                PremultiplyAlpha = false,
                ResizeToPowerOfTwo = true,
                TextureFormat = TextureProcessorOutputFormat.Color
            };

            var face = new PixelBitmapContent<Color>(3, 7);
            Fill(face, Color.Red);
            var input = new Texture2DContent();
            input.Faces[0] = face;

            var output = processor.Process(input, context);

            Assert.NotNull(output);
            Assert.AreEqual(1, output.Faces.Count);
            Assert.AreEqual(1, output.Faces[0].Count);

            Assert.IsAssignableFrom<PixelBitmapContent<Color>>(output.Faces[0][0]);
            var outFace = (PixelBitmapContent<Color>)output.Faces[0][0];
            Assert.AreEqual(4, outFace.Width);
            Assert.AreEqual(8, outFace.Height);

            for (var y = 0; y < outFace.Height; y++)
                for (var x = 0; x < outFace.Width; x++)
                    Assert.AreEqual(Color.Red, outFace.GetPixel(x, y));
        }

#if !XNA
        void CompressDefault<T>(TargetPlatform platform, Color color)
        {
            var context = new TestProcessorContext(platform, "dummy.xnb");

            var processor = new TextureProcessor
            {
                ColorKeyEnabled = false,
                GenerateMipmaps = false,
                PremultiplyAlpha = false,
                ResizeToPowerOfTwo = false,
                TextureFormat = TextureProcessorOutputFormat.Compressed
            };

            var face = new PixelBitmapContent<Color>(16, 16);
            Fill(face, color);
            var input = new Texture2DContent();
            input.Faces[0] = face;

            var output = processor.Process(input, context);

            Assert.NotNull(output);
            Assert.AreEqual(1, output.Faces.Count);
            Assert.AreEqual(1, output.Faces[0].Count);

            Assert.IsAssignableFrom<T>(output.Faces[0][0]);
        }

        [Test]
        public void CompressDefaultWindowsOpaque()
        {
            CompressDefault<Dxt1BitmapContent>(TargetPlatform.Windows, Color.Red);
        }

        [Test]
        public void CompressDefaultWindowsCutOut()
        {
            CompressDefault<Dxt3BitmapContent>(TargetPlatform.Windows, Color.Transparent);
        }

        [Test]
        public void CompressDefaultWindowsAlpha()
        {
            CompressDefault<Dxt5BitmapContent>(TargetPlatform.Windows, Color.Red * 0.5f);
        }

        [Test]
        public void CompressDefaultiOSOpaque()
        {
            CompressDefault<PvrtcRgb4BitmapContent>(TargetPlatform.iOS, Color.Red);
        }

        [Test]
        public void CompressDefaultiOSAlpha()
        {
            CompressDefault<PvrtcRgba4BitmapContent>(TargetPlatform.iOS, Color.Red * 0.5f);
        }

        [Test]
        public void CompressDefaultAndroidOpaque()
        {
            CompressDefault<Etc1BitmapContent>(TargetPlatform.Android, Color.Red);
        }

        [Test]
        public void CompressDefaultAndroidAlpha()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.Android, Color.Red * 0.5f);
        }
#endif
    }
}
