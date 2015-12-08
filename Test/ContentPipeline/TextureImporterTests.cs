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
    }
}
