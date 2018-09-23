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
        public void MipmapSquarePowerOfTwo()
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

            var width = 8;
            var height = 8;

            var face = new PixelBitmapContent<Color>(width, height);
            Fill(face, Color.Red);
            var input = new Texture2DContent();
            input.Faces[0] = face;

            var output = processor.Process(input, context);

            Assert.NotNull(output);
            Assert.AreEqual(1, output.Faces.Count);
            //Assert.AreNotEqual(face, output.Faces[0][0]);

            var outChain = output.Faces[0];
            Assert.AreEqual(4, outChain.Count);

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
        public void MipmapNonSquarePowerOfTwo()
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

            var width = 16;
            var height = 8;

            var face = new PixelBitmapContent<Color>(width, height);
            Fill(face, Color.Red);
            var input = new Texture2DContent();
            input.Faces[0] = face;

            var output = processor.Process(input, context);

            Assert.NotNull(output);
            Assert.AreEqual(1, output.Faces.Count);

            var outChain = output.Faces[0];
            Assert.AreEqual(5, outChain.Count);

            foreach (var outFace in outChain)
            {
                Assert.AreEqual(width, outFace.Width);
                Assert.AreEqual(height, outFace.Height);

                var bitmap = (PixelBitmapContent<Color>)outFace;
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                        Assert.AreEqual(Color.Red, bitmap.GetPixel(x, y));

                if (width > 1)
                    width /= 2;
                if (height > 1)
                    height /= 2;
            }
        }

        [Test]
        public void MipmapNonSquareNonPowerOfTwo()
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

            var width = 23;
            var height = 5;

            var face = new PixelBitmapContent<Color>(width, height);
            Fill(face, Color.Red);
            var input = new Texture2DContent();
            input.Faces[0] = face;

            var output = processor.Process(input, context);

            Assert.NotNull(output);
            Assert.AreEqual(1, output.Faces.Count);

            var outChain = output.Faces[0];
            Assert.AreEqual(5, outChain.Count);

            foreach (var outFace in outChain)
            {
                Assert.AreEqual(width, outFace.Width);
                Assert.AreEqual(height, outFace.Height);

                var bitmap = (PixelBitmapContent<Color>)outFace;
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                        Assert.AreEqual(Color.Red, bitmap.GetPixel(x, y));

                if (width > 1)
                    width /= 2;
                if (height > 1)
                    height /= 2;
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
        void CompressDefault<T>(TargetPlatform platform, Color color, int width = 16, int height = 16)
        {
            var context = new TestProcessorContext(platform, "dummy.xnb");

            var processor = new TextureProcessor
            {
                ColorKeyEnabled = false,
                GenerateMipmaps = true,
                PremultiplyAlpha = false,
                ResizeToPowerOfTwo = false,
                TextureFormat = TextureProcessorOutputFormat.Compressed
            };

            var face = new PixelBitmapContent<Color>(width, height);
            Fill(face, color);
            var input = new Texture2DContent();
            input.Faces[0] = face;

            var output = processor.Process(input, context);

            Assert.NotNull(output);
            Assert.AreEqual(1, output.Faces.Count, "Expected number of faces");
            Assert.AreEqual(5, output.Faces[0].Count, "Expected number of mipmaps");

            Assert.IsAssignableFrom<T>(output.Faces[0][0], "Incorrect pixel format");
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
        public void CompressDefaultiOSOpaqueSquarePOT()
        {
            CompressDefault<PvrtcRgb4BitmapContent>(TargetPlatform.iOS, Color.Red, 16, 16);
        }

        [Test]
        public void CompressDefaultiOSOpaqueSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgr565>>(TargetPlatform.iOS, Color.Red, 24, 24);
        }

        [Test]
        public void CompressDefaultiOSOpaqueNonSquarePOT()
        {
            CompressDefault<PixelBitmapContent<Bgr565>>(TargetPlatform.iOS, Color.Red, 8, 16);
        }

        [Test]
        public void CompressDefaultiOSOpaqueNonSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgr565>>(TargetPlatform.iOS, Color.Red, 24, 16);
        }

        [Test]
        public void CompressDefaultiOSAlphaSquarePOT()
        {
            CompressDefault<PvrtcRgba4BitmapContent>(TargetPlatform.iOS, Color.Red * 0.5f);
        }

        [Test]
        public void CompressDefaultiOSAlphaSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.iOS, Color.Red * 0.5f, 24, 24);
        }

        [Test]
        public void CompressDefaultiOSAlphaNonSquarePOT()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.iOS, Color.Red * 0.5f, 8, 16);
        }

        [Test]
        public void CompressDefaultiOSAlphaNonSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.iOS, Color.Red * 0.5f, 24, 16);
        }

        [Test]
        public void CompressDefaultAndroidOpaqueSquarePOT()
        {
            CompressDefault<Etc1BitmapContent>(TargetPlatform.Android, Color.Red, 16, 16);
        }

        [Test]
        public void CompressDefaultAndroidOpaqueSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgr565>>(TargetPlatform.Android, Color.Red, 24, 24);
        }

        [Test]
        public void CompressDefaultAndroidOpaqueNonSquarePOT()
        {
            CompressDefault<Etc1BitmapContent>(TargetPlatform.Android, Color.Red, 8, 16);
        }

        [Test]
        public void CompressDefaultAndroidOpaqueNonSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgr565>>(TargetPlatform.Android, Color.Red, 24, 16);
        }

        [Test]
        public void CompressDefaultAndroidAlphaSquarePOT()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.Android, Color.Red * 0.5f);
        }

        [Test]
        public void CompressDefaultAndroidAlphaSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.Android, Color.Red * 0.5f, 24, 24);
        }

        [Test]
        public void CompressDefaultAndroidAlphaNonSquarePOT()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.Android, Color.Red * 0.5f, 8, 16);
        }

        [Test]
        public void CompressDefaultAndroidAlphaNonSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.Android, Color.Red * 0.5f, 24, 16);
        }
#endif
    }
}
