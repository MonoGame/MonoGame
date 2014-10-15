using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    class TextureContentTests
    {
        [Test]
        public void Texture2DContent()
        {
            var content = new Texture2DContent();

            Assert.NotNull(content.Faces);
            Assert.AreEqual(1, content.Faces.Count);
            Assert.NotNull(content.Faces[0]);
            Assert.AreEqual(0, content.Faces[0].Count);

            Assert.NotNull(content.Mipmaps);
            Assert.AreEqual(content.Faces[0], content.Mipmaps);
            Assert.AreEqual(0, content.Mipmaps.Count);

            content.Faces[0] = new MipmapChain(new PixelBitmapContent<Color>(2,2));
            Assert.AreEqual(content.Faces[0], content.Mipmaps);
            Assert.AreEqual(1, content.Faces[0].Count);
            Assert.AreEqual(1, content.Mipmaps.Count);

            content.Faces[0].Add(new PixelBitmapContent<Color>(1, 1));
            Assert.AreEqual(2, content.Faces[0].Count);
            Assert.AreEqual(2, content.Mipmaps.Count);

            // TODO: Fix MonoGame!
            //Assert.Throws<NotSupportedException>(() => content.Faces.Clear());
            //Assert.Throws<NotSupportedException>(() => content.Faces.RemoveAt(0));
        }
    }
}
