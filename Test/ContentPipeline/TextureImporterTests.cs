using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using NUnit.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

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
            Assert.NotNull(content);
            Assert.AreEqual(content.Faces.Count, 1);
            Assert.AreEqual(content.Faces[0].Count, 1);
            Assert.AreEqual(content.Faces[0][0].Width, 126);
            Assert.AreEqual(content.Faces[0][0].Height, 240);
            SurfaceFormat format;
            Assert.True(content.Faces[0][0].TryGetFormat(out format));
            Assert.AreEqual(SurfaceFormat.Rgba64, format);
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
                CheckDdsFace(content, f);
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
        public void ImportDds()
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.dds", SurfaceFormat.Dxt3);
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
            CheckDdsFace(content, 0);
            
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
        /// <summary>
        /// Checks that the face of the texture contains 7 mipmaps and that their sizes decline from 64x64 to 1x1
        /// </summary>
        /// <param name="content">Texture to check</param>
        /// <param name="faceIndex">Index of the face from the texture</param>
        private static void CheckDdsFace(TextureContent content, int faceIndex)
        {
            Assert.AreEqual(content.Faces[faceIndex].Count, 7);
            Assert.AreEqual(content.Faces[faceIndex][0].Width, 64);
            Assert.AreEqual(content.Faces[faceIndex][0].Height, 64);
            Assert.AreEqual(content.Faces[faceIndex][1].Width, 32);
            Assert.AreEqual(content.Faces[faceIndex][1].Height, 32);
            Assert.AreEqual(content.Faces[faceIndex][2].Width, 16);
            Assert.AreEqual(content.Faces[faceIndex][2].Height, 16);
            Assert.AreEqual(content.Faces[faceIndex][3].Width, 8);
            Assert.AreEqual(content.Faces[faceIndex][3].Height, 8);
            Assert.AreEqual(content.Faces[faceIndex][4].Width, 4);
            Assert.AreEqual(content.Faces[faceIndex][4].Height, 4);
            Assert.AreEqual(content.Faces[faceIndex][5].Width, 2);
            Assert.AreEqual(content.Faces[faceIndex][5].Height, 2);
            Assert.AreEqual(content.Faces[faceIndex][6].Width, 1);
            Assert.AreEqual(content.Faces[faceIndex][6].Height, 1);
        }
    }
}
