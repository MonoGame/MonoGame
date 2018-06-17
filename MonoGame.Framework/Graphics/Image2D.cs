using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;
using System;
using System.IO;

namespace MonoGame.Framework.Graphics
{
    /// <summary>
    /// 2D Image in the <see cref="SurfaceFormat.Color"/> format
    /// </summary>
    public class Image2D
    {
        private readonly Color[] _data;

        /// <summary>
        /// Width of the image
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the image
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Data of the image
        /// </summary>
        public Color[] Data
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
                return _data[y * Width + x];
            }

            set
            {
                _data[y * Width + x] = value;
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

            _data = new Color[width * height];
        }

        /// <summary>
        /// Constructs a <see cref="Image2D"/> of the specified size and containing specified data
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="data"></param>
        public Image2D(int width, int height, Color[] data)
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

            var length = width * height;
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
        public static Image2D FromStream(Stream stream)
        {
            int x, y;
            int comp;

            var data = ImageReader.ReadColor(stream, out x, out y, out comp);

            return new Image2D(x, y, data);
        }
    }
}
