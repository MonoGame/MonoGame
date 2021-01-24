using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StbImageSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Graphics
{
    /// <summary>
    /// 2D Image in the <see cref="SurfaceFormat.Color"/> format
    /// </summary>
    public class Image2D
    {
        private readonly byte[] _data;

        /// <summary>
        /// Width of the image
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the image
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Data of the image in the RGBA format
        /// </summary>
        public byte[] Data
        {
            get { return _data; }
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
                return new Color(_data[basePos],
                    _data[basePos + 1],
                    _data[basePos + 2],
                    _data[basePos + 3]);
            }

            set
            {
                var basePos = (y * Width + x) * 4;
                _data[basePos] = value.R;
                _data[basePos + 1] = value.G;
                _data[basePos + 2] = value.B;
                _data[basePos + 3] = value.A;
            }
        }


        /// <summary>
        /// Constructs a <see cref="Image2D"/> of the specified size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Image2D(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height");
            }

            Width = width;
            Height = height;

            _data = new byte[width * height * 4];
        }

        /// <summary>
        /// Constructs a <see cref="Image2D"/> of the specified size and containing specified data
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="data"></param>
        public Image2D(int width, int height, byte[] data)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var length = width * height * 4;
            if (data.Length != length)
            {
                throw new ArgumentException(string.Format("Inconsistent data length: expected={0}, provided={1}", length,
                    data.Length));
            }

            Width = width;
            Height = height;
            _data = data;
        }

        /// <summary>
        /// Creates Texture2D from this image
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <returns></returns>
        public Texture2D CreateTexture2D(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, Width, Height, false, SurfaceFormat.Color);
            texture.SetData(_data);
            return texture;
        }

        /// <summary>
        /// Loads an image in PNG, JPG, BMP, GIF or TGA format from the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public unsafe static Image2D FromStream(Stream stream)
        {
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

            return new Image2D(result.Width, result.Height, result.Data);
        }
    }
}
