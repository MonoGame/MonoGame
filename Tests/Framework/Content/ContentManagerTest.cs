using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Content
{
    [TestFixture]
    [NonParallelizable]
    internal class ContentManagerTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        // Tests loading a texture from a XNB file
        [RunOnUI]
        public void CorrectlyLoadTextureFromXnb()
        {
            ContentManager content = new ContentManager(game.Services);
            
            Texture2D texture = content.Load<Texture2D>(Paths.Texture("MonoGameIcon"));

            Assert.IsNotNull(texture);
        }

        [Test]
        [TestCase("UniquePng")]
        [TestCase("UniqueBmp")]
        [TestCase("UniqueJpg")]
        [TestCase("UniqueJpeg")]
        // Tests loading from a PNG/JPG/JPEG/BMP file when a corresponding XNB file doesn't exist
        [RunOnUI]
        public void CorrectlyLoadTextureFromAlternativeImageFormatsWhenNoXnb(string assetName)
        {
            ContentManager content = new ContentManager(game.Services);

            Texture2D texture = content.Load<Texture2D>(Paths.Texture(assetName));

            Assert.IsNotNull(texture);
        }

        [Test]
        // Tests that an exception is raised when no XNB, PNG, JPG, JPEG or BMP exists for a content name
        [RunOnUI]
        public void ThrowExceptionIfNoAssetInAnySupportedImageFormats()
        {
            ContentManager content = new ContentManager(game.Services);

            var exception = Assert.Throws<ContentLoadException>(() => content.Load<Texture2D>(Paths.Texture("NotExisting")));
            StringAssert.StartsWith("The content file was not found.", exception.Message);
            Assert.IsInstanceOf(typeof(FileNotFoundException), exception.InnerException);
        }

        [Test]
        // Tests that an exception is raised when a non XNB format exists but the content type requested is not a Texture
        [RunOnUI]
        public void ThrowExceptionIfTypeIsNotTexture()
        {
            ContentManager content = new ContentManager(game.Services);

            var exception = Assert.Throws<ContentLoadException>(() => content.Load<SoundEffect>(Paths.Texture("UniquePng")));
            StringAssert.StartsWith("The content file was not found.", exception.Message);
            Assert.IsInstanceOf(typeof(FileNotFoundException), exception.InnerException);
        }
    }
}
