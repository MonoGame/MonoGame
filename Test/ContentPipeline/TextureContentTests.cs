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

            Assert.Throws<NotSupportedException>(() => content.Faces.Clear());
            Assert.Throws<NotSupportedException>(() => content.Faces.RemoveAt(0));
            Assert.Throws<NotSupportedException>(() => content.Faces.Add(new MipmapChain()));
            Assert.Throws<NotSupportedException>(() => content.Faces.Insert(0, new MipmapChain()));
            Assert.Throws<NotSupportedException>(() => content.Faces.Remove(content.Faces[0]));
        }

        [Test]
        public void Texture3DContent()
        {
            var content = new Texture3DContent();

            Assert.NotNull(content.Faces);
            Assert.AreEqual(0, content.Faces.Count);

            var face0 = new MipmapChain(new PixelBitmapContent<Color>(2, 2));
            content.Faces.Add(face0);
            Assert.AreEqual(1, content.Faces.Count);
            Assert.AreEqual(face0, content.Faces[0]);
            Assert.AreEqual(1, content.Faces[0].Count);

            content.Faces[0].Add(new PixelBitmapContent<Color>(1, 1));
            Assert.AreEqual(2, content.Faces[0].Count);

            var face2 = new MipmapChain(new PixelBitmapContent<Color>(2, 2));
            content.Faces.Add(face2);
            Assert.AreEqual(face2, content.Faces[1]);
            Assert.AreEqual(2, content.Faces.Count);

            var face1 = new MipmapChain(new PixelBitmapContent<Color>(2, 2));
            content.Faces.Insert(1, face1);
            Assert.AreEqual(face1, content.Faces[1]);
            Assert.AreEqual(3, content.Faces.Count);

            content.Faces.RemoveAt(0);
            Assert.AreEqual(2, content.Faces.Count);
            Assert.AreEqual(face1, content.Faces[0]);
            Assert.AreEqual(face2, content.Faces[1]);

            content.Faces.Remove(face1);
            Assert.AreEqual(1, content.Faces.Count);
            Assert.AreEqual(face2, content.Faces[0]);

            content.Faces.Clear();
            Assert.AreEqual(0, content.Faces.Count);
        }

        [Test]
        public void TextureCubeContent()
        {
            var content = new TextureCubeContent();

            Assert.NotNull(content.Faces);
            Assert.AreEqual(6, content.Faces.Count);
            Assert.NotNull(content.Faces[0]);
            Assert.NotNull(content.Faces[1]);
            Assert.NotNull(content.Faces[2]);
            Assert.NotNull(content.Faces[3]);
            Assert.NotNull(content.Faces[4]);
            Assert.NotNull(content.Faces[5]);
            Assert.AreEqual(0, content.Faces[0].Count);
            Assert.AreEqual(0, content.Faces[1].Count);
            Assert.AreEqual(0, content.Faces[2].Count);
            Assert.AreEqual(0, content.Faces[3].Count);
            Assert.AreEqual(0, content.Faces[4].Count);
            Assert.AreEqual(0, content.Faces[5].Count);

            var face0 = new MipmapChain(new PixelBitmapContent<Color>(2, 2));
            content.Faces[0] = face0;
            Assert.AreEqual(face0, content.Faces[0]);
            Assert.AreEqual(1, content.Faces[0].Count);

            content.Faces[0].Add(new PixelBitmapContent<Color>(1, 1));
            Assert.AreEqual(2, content.Faces[0].Count);

            content.Faces[1].Add(new PixelBitmapContent<Color>(2, 2));
            content.Faces[2].Add(new PixelBitmapContent<Color>(2, 2));
            content.Faces[3].Add(new PixelBitmapContent<Color>(2, 2));
            content.Faces[4].Add(new PixelBitmapContent<Color>(2, 2));
            content.Faces[5].Add(new PixelBitmapContent<Color>(2, 2));
            Assert.AreEqual(1, content.Faces[1].Count);
            Assert.AreEqual(1, content.Faces[2].Count);
            Assert.AreEqual(1, content.Faces[3].Count);
            Assert.AreEqual(1, content.Faces[4].Count);
            Assert.AreEqual(1, content.Faces[5].Count);

            Assert.Throws<NotSupportedException>(() => content.Faces.Clear());
            Assert.Throws<NotSupportedException>(() => content.Faces.RemoveAt(0));
            Assert.Throws<NotSupportedException>(() => content.Faces.Add(new MipmapChain()));
            Assert.Throws<NotSupportedException>(() => content.Faces.Insert(0, new MipmapChain()));
            Assert.Throws<NotSupportedException>(() => content.Faces.Remove(content.Faces[0]));
        }
    }
}
