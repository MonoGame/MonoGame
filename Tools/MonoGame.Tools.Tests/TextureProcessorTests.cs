using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoGame.Framework.Content;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    [TestFixture]
    class TextureProcessorTests
    {
        private const string Dxt1CubeMipsPath = "Assets/Textures/SampleCube64DXT1Mips.dds";
        private const string Dxt3NonPowerOfTwoPath = "Assets/Textures/Dxt3NonPowerOfTwo.dds";
        private const string Dxt3ProcessorOptionsPath = "Assets/Textures/Dxt3ProcessorOptions.dds";

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

        private static Texture2DContent ImportDxt3(string path)
        {
            TextureImporter importer = new TextureImporter();
            TestImporterContext context = new TestImporterContext("TestObj", "TestBin");
            Texture2DContent content = (Texture2DContent)importer.Import(path, context);
            SurfaceFormat format;

            Assert.IsTrue(content.Faces[0][0].TryGetFormat(out format));
            Assert.AreEqual(SurfaceFormat.Dxt3, format);

            return content;
        }

        private static TextureCubeContent ImportDxt1Cubemap()
        {
            TextureImporter importer = new TextureImporter();
            TestImporterContext context = new TestImporterContext("TestObj", "TestBin");
            TextureCubeContent content = (TextureCubeContent)importer.Import(Dxt1CubeMipsPath, context);
            SurfaceFormat format;

            Assert.IsTrue(content.Faces[0][0].TryGetFormat(out format));
            Assert.AreEqual(SurfaceFormat.Dxt1, format);

            return content;
        }

        [Test]
        public void Process_CompressedDxt3_DefaultSettings_DecodesAndAppliesDefaults()
        {
            TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            TextureProcessor processor = new TextureProcessor();
            Texture2DContent input = ImportDxt3(Dxt3ProcessorOptionsPath);

            TextureContent output = processor.Process(input, context);

            Assert.AreEqual(1, output.Faces.Count);
            Assert.AreEqual(1, output.Faces[0].Count);
            Assert.IsInstanceOf<PixelBitmapContent<Color>>(output.Faces[0][0]);

            PixelBitmapContent<Color> bitmap = (PixelBitmapContent<Color>)output.Faces[0][0];
            Assert.AreEqual(new Color(136, 0, 0, 136), bitmap.GetPixel(0, 0));
            Assert.AreEqual(Color.Transparent, bitmap.GetPixel(4, 0));
            Assert.AreEqual(Color.Lime, bitmap.GetPixel(0, 4));
            Assert.AreEqual(Color.Blue, bitmap.GetPixel(4, 4));
        }

        [Test]
        public void Process_CompressedDxt1Cubemap_AllFacesAndMips_DecodesEveryBitmap()
        {
            TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            TextureProcessor processor = new TextureProcessor
            {
                ColorKeyEnabled = false,
                PremultiplyAlpha = false,
                TextureFormat = TextureProcessorOutputFormat.Color
            };
            TextureCubeContent input = ImportDxt1Cubemap();

            TextureContent output = processor.Process(input, context);

            Assert.AreEqual(6, output.Faces.Count);

            foreach (MipmapChain face in output.Faces)
            {
                Assert.AreEqual(7, face.Count);

                int size = 64;
                foreach (BitmapContent bitmap in face)
                {
                    Assert.IsInstanceOf<PixelBitmapContent<Color>>(bitmap);
                    Assert.AreEqual(size, bitmap.Width);
                    Assert.AreEqual(size, bitmap.Height);
                    size /= 2;
                }
            }
        }

        [Test]
        public void Process_CompressedDxt3_PremultiplyAlphaEnabled_PremultipliesAlpha()
        {
            TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            TextureProcessor processor = new TextureProcessor
            {
                ColorKeyEnabled = false,
                PremultiplyAlpha = true,
                TextureFormat = TextureProcessorOutputFormat.Color
            };
            Texture2DContent input = ImportDxt3(Dxt3ProcessorOptionsPath);

            TextureContent output = processor.Process(input, context);

            Assert.IsInstanceOf<PixelBitmapContent<Color>>(output.Faces[0][0]);

            PixelBitmapContent<Color> bitmap = (PixelBitmapContent<Color>)output.Faces[0][0];

            // The top-left block of the test image is red (255, 0, 0) with an alpha of 53.3%.
            // If premultiply alpha works, red is 255 * 0.533 = 135.9, which rounds to 136.
            // So the expected color is (136, 0, 0, 136).
            Assert.AreEqual(new Color(136, 0, 0, 136), bitmap.GetPixel(0, 0));
        }

        [Test]
        public void Process_CompressedDxt3_GenerateMipmapsEnabled_GeneratesMipmapChain()
        {
            TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            TextureProcessor processor = new TextureProcessor
            {
                ColorKeyEnabled = false,
                GenerateMipmaps = true,
                PremultiplyAlpha = false,
                TextureFormat = TextureProcessorOutputFormat.Color
            };
            Texture2DContent input = ImportDxt3(Dxt3ProcessorOptionsPath);

            TextureContent output = processor.Process(input, context);

            Assert.AreEqual(1, output.Faces.Count);
            Assert.AreEqual(4, output.Faces[0].Count);

            int size = 8;
            foreach (BitmapContent bitmap in output.Faces[0])
            {
                Assert.IsInstanceOf<PixelBitmapContent<Color>>(bitmap);
                Assert.AreEqual(size, bitmap.Width);
                Assert.AreEqual(size, bitmap.Height);
                size /= 2;
            }
        }

        [Test]
        public void Process_CompressedDxt3_PremultiplyAlphaAndMipmapsEnabled_AppliesBothOptions()
        {
            TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            TextureProcessor processor = new TextureProcessor
            {
                ColorKeyEnabled = false,
                GenerateMipmaps = true,
                PremultiplyAlpha = true,
                TextureFormat = TextureProcessorOutputFormat.Color
            };
            Texture2DContent input = ImportDxt3(Dxt3ProcessorOptionsPath);

            TextureContent output = processor.Process(input, context);

            // Mipmaps generated
            Assert.AreEqual(4, output.Faces[0].Count);
            Assert.IsInstanceOf<PixelBitmapContent<Color>>(output.Faces[0][0]);

            // Premultiply alpha applied
            PixelBitmapContent<Color> bitmap = (PixelBitmapContent<Color>)output.Faces[0][0];
            Assert.AreEqual(new Color(136, 0, 0, 136), bitmap.GetPixel(0, 0));
        }

        [Test]
        public void Process_CompressedDxt3_ResizeToPowerOfTwoEnabled_ResizesToNextPowerOfTwo()
        {
            TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            TextureProcessor processor = new TextureProcessor
            {
                ColorKeyEnabled = false,
                PremultiplyAlpha = false,
                ResizeToPowerOfTwo = true,
                TextureFormat = TextureProcessorOutputFormat.Color
            };
            Texture2DContent input = ImportDxt3(Dxt3NonPowerOfTwoPath);

            TextureContent output = processor.Process(input, context);

            Assert.IsInstanceOf<PixelBitmapContent<Color>>(output.Faces[0][0]);

            PixelBitmapContent<Color> bitmap = (PixelBitmapContent<Color>)output.Faces[0][0];

            // Image is 6x5, resize power of two should have resized it to 8x8
            Assert.AreEqual(8, bitmap.Width);
            Assert.AreEqual(8, bitmap.Height);
            Assert.AreEqual(Color.Lime, bitmap.GetPixel(0, 0));

            // Ensure pixels are correct in the new resized region
            Assert.AreEqual(Color.Lime, bitmap.GetPixel(7, 7));
        }

        [Test]
        public void Process_CompressedDxt3_ColorKeyEnabled_ReplacesMatchingColor()
        {
            TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            TextureProcessor processor = new TextureProcessor
            {
                ColorKeyColor = Color.Magenta,
                ColorKeyEnabled = true,
                PremultiplyAlpha = false,
                TextureFormat = TextureProcessorOutputFormat.Color
            };
            Texture2DContent input = ImportDxt3(Dxt3ProcessorOptionsPath);

            TextureContent output = processor.Process(input, context);

            Assert.IsInstanceOf<PixelBitmapContent<Color>>(output.Faces[0][0]);

            PixelBitmapContent<Color> bitmap = (PixelBitmapContent<Color>)output.Faces[0][0];

            // Top-right block of the image is magenta, so with color key enabled
            // it should now be transparent.
            Assert.AreEqual(Color.Transparent, bitmap.GetPixel(4, 0));

            // And it should not have affected other colors.
            Assert.AreEqual(Color.Lime, bitmap.GetPixel(0, 4));
        }

        [Test]
        public void Process_CompressedDxt3_NoChangeWithColorKey_PreservesDxt3Output()
        {
            TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            TextureProcessor processor = new TextureProcessor
            {
                ColorKeyColor = Color.Magenta,
                ColorKeyEnabled = true,
                PremultiplyAlpha = false,
                TextureFormat = TextureProcessorOutputFormat.NoChange
            };
            Texture2DContent input = ImportDxt3(Dxt3ProcessorOptionsPath);

            using IDisposable scope = ContextScopeFactory.BeginContext(context);
            TextureContent output = processor.Process(input, context);

            Assert.IsInstanceOf<Dxt3BitmapContent>(output.Faces[0][0]);

            PixelBitmapContent<Vector4> bitmap = new PixelBitmapContent<Vector4>(8, 8);
            BitmapContent.Copy(output.Faces[0][0], bitmap);

            // Ensures that the color key enabled change still occurs even
            // though the output format is set to no change to ensure that
            // the texture was processed before it was converted back to DXT3.
            Assert.AreEqual(0f, bitmap.GetPixel(4, 0).W);
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

            using var scope = ContextScopeFactory.BeginContext(context);
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

            using var scope = ContextScopeFactory.BeginContext(context);
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

            using var scope = ContextScopeFactory.BeginContext(context);
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

            using var scope = ContextScopeFactory.BeginContext(context);
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

            using var scope = ContextScopeFactory.BeginContext(context);
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
            CompressDefault<Etc2BitmapContent>(TargetPlatform.Android, Color.Red * 0.5f);
        }

        [Test]
        public void CompressDefaultAndroidAlphaSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.Android, Color.Red * 0.5f, 24, 24);
        }

        [Test]
        public void CompressDefaultAndroidAlphaNonSquarePOT()
        {
            CompressDefault<Etc2BitmapContent>(TargetPlatform.Android, Color.Red * 0.5f, 8, 16);
        }

        [Test]
        public void CompressDefaultAndroidAlphaNonSquareNPOT()
        {
            CompressDefault<PixelBitmapContent<Bgra4444>>(TargetPlatform.Android, Color.Red * 0.5f, 24, 16);
        }
#endif
    }
}
