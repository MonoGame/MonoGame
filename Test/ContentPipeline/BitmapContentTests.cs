using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoGame.Tests.ContentPipeline
{
    class BitmapContentTests
    {
        private static void Fill(PixelBitmapContent<Color> content, Color color)
        {
            var src = Enumerable.Repeat(color.PackedValue, content.Width * content.Height).ToArray();
            var dest = new byte[Marshal.SizeOf(typeof(Color)) * content.Width * content.Height];
            Buffer.BlockCopy(src, 0, dest, 0, dest.Length);
            content.SetPixelData(dest);
        }

        [Test]
        public void BitmapCopyFullNoResize()
        {
            var b1 = new PixelBitmapContent<Color>(8, 8);
            Fill(b1, Color.Red);
            var b2 = new PixelBitmapContent<Color>(8, 8);
            BitmapContent.Copy(b1, b2);

            for (var y = 0; y < b2.Height; y++)
                for (var x = 0; x < b2.Width; x++)
                    Assert.AreEqual(Color.Red, b2.GetPixel(x, y));
        }

        [Test]
        public void BitmapCopyFullResize()
        {
            var b1 = new PixelBitmapContent<Color>(8, 8);
            Fill(b1, Color.Red);
            var b2 = new PixelBitmapContent<Color>(4, 4);
            BitmapContent.Copy(b1, b2);

            for (var y = 0; y < b2.Height; y++)
                for (var x = 0; x < b2.Width; x++)
                    Assert.AreEqual(Color.Red, b2.GetPixel(x, y));
        }

        [Test]
        public void BitmapConvertFullNoResize()
        {
            var b1 = new PixelBitmapContent<Color>(8, 8);
            Fill(b1, Color.Red);
            var b2 = new PixelBitmapContent<Bgr565>(8, 8);
            BitmapContent.Copy(b1, b2);

            var packed = new Bgr565(1.0f, 0.0f, 0.0f);
            for (var y = 0; y < b2.Height; y++)
                for (var x = 0; x < b2.Width; x++)
                    Assert.AreEqual(packed, b2.GetPixel(x, y));
        }

        [Test]
        public void BitmapCompressFullNoResize()
        {
            var b1 = new PixelBitmapContent<Color>(8, 8);
            Fill(b1, Color.Red);
            var b2 = new Dxt1BitmapContent(8, 8);
            BitmapContent.Copy(b1, b2);
        }

        [Test]
        public void BitmapCompressFullResize()
        {
            var b1 = new PixelBitmapContent<Color>(16, 16);
            Fill(b1, Color.Red);
            var b2 = new Dxt1BitmapContent(8, 8);
            BitmapContent.Copy(b1, b2);
        }

        [Test]
        public void BitmapCopySameRegionNoResize()
        {
            var b1 = new PixelBitmapContent<Color>(8, 8);
            Fill(b1, Color.Red);
            var b2 = new PixelBitmapContent<Color>(8, 8);
            Fill(b2, Color.Blue);
            BitmapContent.Copy(b1, new Rectangle(0, 0, 4, 4), b2, new Rectangle(0, 0, 4, 4));

            for (var y = 0; y < b2.Height; y++)
                for (var x = 0; x < b2.Width; x++)
                    Assert.AreEqual(x < 4 && y < 4 ? Color.Red : Color.Blue, b2.GetPixel(x, y));
        }

        [Test]
        public void BitmapCopyMoveRegionNoResize()
        {
            var b1 = new PixelBitmapContent<Color>(8, 8);
            Fill(b1, Color.Red);
            var b2 = new PixelBitmapContent<Color>(8, 8);
            Fill(b2, Color.Blue);
            BitmapContent.Copy(b1, new Rectangle(0, 0, 4, 4), b2, new Rectangle(4, 4, 4, 4));

            for (var y = 0; y < b2.Height; y++)
                for (var x = 0; x < b2.Width; x++)
                    Assert.AreEqual(x >= 4 && y >= 4 ? Color.Red : Color.Blue, b2.GetPixel(x, y));
        }

        [Test]
        public void BitmapCopyRegionResize()
        {
            var b1 = new PixelBitmapContent<Color>(8, 8);
            Fill(b1, Color.Red);
            var b2 = new PixelBitmapContent<Color>(8, 8);
            Fill(b2, Color.Blue);
            BitmapContent.Copy(b1, new Rectangle(0, 0, 4, 4), b2, new Rectangle(0, 0, 3, 6));

            for (var y = 0; y < b2.Height; y++)
                for (var x = 0; x < b2.Width; x++)
                    Assert.AreEqual(x < 3 && y < 6 ? Color.Red : Color.Blue, b2.GetPixel(x, y));
        }
    }
}
