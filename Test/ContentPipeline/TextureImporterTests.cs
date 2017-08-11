﻿using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    class TextureImporterTests
    {
        const string intermediateDirectory = "TestObj";
        const string outputDirectory = "TestBin";

        void ImportStandard(string filename, SurfaceFormat expectedSurfaceFormat)
        {
            var importer = new TextureImporter( );
            var context = new TestImporterContext(intermediateDirectory, outputDirectory);
            var content = importer.Import(filename, context);
            Assert.NotNull(content);
            Assert.AreEqual(content.Faces.Count, 1);
            Assert.AreEqual(content.Faces[0].Count, 1);
            Assert.AreEqual(content.Faces[0][0].Width, 64);
            Assert.AreEqual(content.Faces[0][0].Height, 64);
            SurfaceFormat format;
            Assert.True(content.Faces[0][0].TryGetFormat(out format));
            Assert.AreEqual(expectedSurfaceFormat, format);
            // Clean-up the directories it may have produced, ignoring DirectoryNotFound exceptions
            try
            {
                Directory.Delete(intermediateDirectory, true);
                Directory.Delete(outputDirectory, true);
            }
            catch(DirectoryNotFoundException)
            {
            }
        }

        [Test]
        public void ImportBmp( )
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.bmp", SurfaceFormat.Color);
        }

        [Test]
        public void ImportBmpRGB555( )
        {
            ImportStandard("Assets/Textures/Logo555.bmp", SurfaceFormat.Color);
        }

        [Test]
        public void ImportBmpRGB565( )
        {
            ImportStandard("Assets/Textures/Logo565.bmp", SurfaceFormat.Color);
        }

        [Test]
        public void ImportBmp4bits( )
        {
            ImportStandard("Assets/Textures/LogoOnly_64px-4bits.bmp", SurfaceFormat.Color);
        }

        [Test]
        public void ImportBmpMonochrome( )
        {
            ImportStandard("Assets/Textures/LogoOnly_64px-monochrome.bmp", SurfaceFormat.Color);
        }

        [Test]
        public void ImportGif( )
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.gif", SurfaceFormat.Color);
        }

        [Test]
        public void ImportJpg( )
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.jpg", SurfaceFormat.Color);
        }

        [Test]
        public void ImportPng( )
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.png", SurfaceFormat.Color);
        }

        [Test]
        public void ImportTga( )
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.tga", SurfaceFormat.Color);
        }

        [Test]
        public void ImportTif( )
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.tif", SurfaceFormat.Color);
        }

        /// <summary>
        /// This test tries to load a tiff file encoded in rgbf, but freeimage seems to be failing to read files with this encoding
        /// Might be necessary to modify this test with future updates of freeimage.
        /// 
        /// Note that the image was created with Freeimage from a bitmap
        /// </summary>
        [Test]
        public void ImportImageWithBadContent( )
        {
            Assert.Throws(typeof(InvalidContentException), ( ) => ImportStandard("Assets/Textures/rgbf.tif", SurfaceFormat.Vector4));
            //ImportStandard("Assets/Textures/rgbf.tif", SurfaceFormat.Color);
        }

        [Test]
        public void ImportRGBA16Png()
        {
            var importer = new TextureImporter();
            var context = new TestImporterContext(intermediateDirectory, outputDirectory);
            var content = importer.Import("Assets/Textures/RGBA16.png", context);
            ulong expectedPixelValue = 5714832815570484476;
            Assert.NotNull(content);
            Assert.AreEqual(content.Faces.Count, 1);
            Assert.AreEqual(content.Faces[0].Count, 1);
            Assert.AreEqual(content.Faces[0][0].Width, 126);
            Assert.AreEqual(content.Faces[0][0].Height, 240);
            SurfaceFormat format;
            Assert.True(content.Faces[0][0].TryGetFormat(out format));
            Assert.AreEqual(SurfaceFormat.Rgba64, format);
            Assert.AreEqual(expectedPixelValue, ((PixelBitmapContent<Rgba64>)content.Faces[0][0]).GetRow(1)[12].PackedValue);
            // Clean-up the directories it may have produced, ignoring DirectoryNotFound exceptions
            try
            {
                Directory.Delete(intermediateDirectory, true);
                Directory.Delete(outputDirectory, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        [Test]
        public void ImportDdsCubemapDxt1()
        {
            var importer = new TextureImporter();
            var context = new TestImporterContext(intermediateDirectory, outputDirectory);
            var content = importer.Import("Assets/Textures/SampleCube64DXT1Mips.dds", context);
            Assert.NotNull(content);
            Assert.AreEqual(content.Faces.Count, 6);
            for (int f = 0; f < 6; ++f)
            {
                CheckDdsFace(content, f, 7, 64, 64);
            }
            SurfaceFormat format;
            Assert.True(content.Faces[0][0].TryGetFormat(out format));
            Assert.AreEqual(format, SurfaceFormat.Dxt1);
            // Clean-up the directories it may have produced, ignoring DirectoryNotFound exceptions
            try
            {
                Directory.Delete(intermediateDirectory, true);
                Directory.Delete(outputDirectory, true);
            }
            catch (DirectoryNotFoundException)
            { }
        }

        [Test]
        public void ImportDdsCubemapColor()
        {
            var importer = new TextureImporter();
            var context = new TestImporterContext(intermediateDirectory, outputDirectory);
            var content = importer.Import("Assets/Textures/Sunset.dds", context);
            Assert.NotNull(content);
            Assert.AreEqual(content.Faces.Count, 6);
            for (int f = 0; f < 6; ++f)
            {
                CheckDdsFace(content, f, 1, 512, 512);
            }
            SurfaceFormat format;
            Assert.True(content.Faces[0][0].TryGetFormat(out format));
            // Ensure the red and blue bytes have been correctly swapped
            Assert.AreEqual(format, SurfaceFormat.Color);
            var bytes = content.Faces[0][0].GetPixelData();
            Assert.AreEqual(bytes[0], 208);
            Assert.AreEqual(bytes[2], 62);
            // Clean-up the directories it may have produced, ignoring DirectoryNotFound exceptions
            try
            {
                Directory.Delete(intermediateDirectory, true);
                Directory.Delete(outputDirectory, true);
            }
            catch (DirectoryNotFoundException)
            { }
        }

        [Test]
        public void ImportDds()
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.dds", SurfaceFormat.Dxt3);
            ImportStandard("Assets/Textures/LogoOnly_64px-R8G8B8.dds", SurfaceFormat.Color);
            ImportStandard("Assets/Textures/LogoOnly_64px-X8R8G8B8.dds", SurfaceFormat.Color);
        }

        [Test]
        public void ImportDdsMipMap()
        {
            //ImportStandard("Assets/Textures/LogoOnly_64px-mipmaps.dds", SurfaceFormat.Color);
            var importer = new TextureImporter();
            var context = new TestImporterContext(intermediateDirectory, outputDirectory);
            var content = importer.Import("Assets/Textures/LogoOnly_64px-mipmaps.dds", context);
            Assert.NotNull(content);
            Assert.AreEqual(content.Faces.Count, 1);
            CheckDdsFace(content, 0, 7, 64, 64);

            SurfaceFormat format;
            Assert.True(content.Faces[0][0].TryGetFormat(out format));
            Assert.AreEqual(format, SurfaceFormat.Dxt3);
            // Clean-up the directories it may have produced, ignoring DirectoryNotFound exceptions
            try
            {
                Directory.Delete(intermediateDirectory, true);
                Directory.Delete(outputDirectory, true);
            }
            catch(DirectoryNotFoundException)
            {
            }
        }

        private static void CheckDdsFace(TextureContent content, int faceIndex, int mipMapCount, int width, int height)
        {
            Assert.AreEqual(content.Faces[faceIndex].Count, mipMapCount);
            for (int i = 0; i < mipMapCount; ++i)
            {
                Assert.AreEqual(content.Faces[faceIndex][i].Width, width >> i);
                Assert.AreEqual(content.Faces[faceIndex][i].Height, height >> i);
            }
        }
    }
}
