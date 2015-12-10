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

        void ImportStandard(string filename)
        {
            var importer = new TextureImporter();
            var context = new TestImporterContext(intermediateDirectory, outputDirectory);
            var content = importer.Import(filename, context);
            Assert.NotNull(content);
            Assert.AreEqual(content.Faces.Count, 1);
            Assert.AreEqual(content.Faces[0].Count, 1);
            Assert.AreEqual(content.Faces[0][0].Width, 64);
            Assert.AreEqual(content.Faces[0][0].Height, 64);
            SurfaceFormat format;
            Assert.True(content.Faces[0][0].TryGetFormat(out format));
            Assert.AreEqual(format, SurfaceFormat.Color);
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
        public void ImportBmp()
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.bmp");
        }

        [Test]
        public void ImportGif()
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.gif");
        }

        [Test]
        public void ImportJpg()
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.jpg");
        }

        [Test]
        public void ImportPng()
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.png");
        }

        [Test]
        public void ImportTga()
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.tga");
        }

        [Test]
        public void ImportTif()
        {
            ImportStandard("Assets/Textures/LogoOnly_64px.tif");
        }
    }
}
