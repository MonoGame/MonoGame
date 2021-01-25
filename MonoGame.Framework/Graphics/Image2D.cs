using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StbImageSharp;
using System;
using System.IO;

namespace MonoGame.Framework.Graphics
{
    /// <summary>
    /// 2D Image in the <see cref="SurfaceFormat.Color"/> format
    /// </summary>
    public class Image2D : IDisposable
    {
        private readonly byte[] _pixelData;
        private bool _isDisposed;

        /// <summary>
        /// Width of the image
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the image
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Data of the image in the RGBA format.
        /// </summary>
        public byte[] PixelData
        {
            get { return _pixelData; }
        }

        /// <summary>
        /// Color at the specified coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color this[int x, int y]
        {
            get
            {
                var basePos = (y * Width + x) * 4;
                return new Color(_pixelData[basePos],
                    _pixelData[basePos + 1],
                    _pixelData[basePos + 2],
                    _pixelData[basePos + 3]);
            }

            set
            {
                var basePos = (y * Width + x) * 4;
                _pixelData[basePos] = value.R;
                _pixelData[basePos + 1] = value.G;
                _pixelData[basePos + 2] = value.B;
                _pixelData[basePos + 3] = value.A;
            }
        }

        /// <summary>
        /// Constructs a <see cref="Image2D"/> of the specified size,
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Image2D(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height");

            Width = width;
            Height = height;
            _pixelData = new byte[width * height * 4];
        }

        /// <summary>
        /// Constructs a <see cref="Image2D"/> of the specified size and containing specified data.
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Image2D(byte[] pixelData, int width, int height)
        {
            if (pixelData == null)
                throw new ArgumentNullException("data");
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height");

            int length = width * height * 4;
            if (pixelData.Length != length)
            {
                throw new ArgumentException(string.Format("Inconsistent data length: expected={0}, provided={1}", length,
                    pixelData.Length));
            }

            Width = width;
            Height = height;
            _pixelData = pixelData;
        }

        /// <summary>
        /// Creates Texture2D from this image.
        /// </summary>
        public Texture2D CreateTexture2D(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, Width, Height, false, SurfaceFormat.Color);
            texture.SetData(_pixelData);
            return texture;
        }

        /// <summary>
        /// Loads an image in PNG, JPG, BMP, GIF or TGA format from the stream.
        /// </summary>
        public unsafe static Image2D FromStream(Stream stream)
        {
            // TODO: reduce allocations here by not using seeking at all

            ImageResult result;
            if (stream.CanSeek)
            {
                result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            }
            else
            {
                // If stream doesnt provide seek functionaly, use MemoryStream instead
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    result = ImageResult.FromStream(ms, ColorComponents.RedGreenBlueAlpha);
                }
            }
            return new Image2D(result.Data, result.Width, result.Height);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Dispose is empty for now but may be utilized in the future.

            if (!_isDisposed)
            {
                if (disposing)
                {
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
