// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        internal protected enum SurfaceType
        {
            Texture,
            RenderTarget,
            SwapChainRenderTarget,
        }

		internal int width;
		internal int height;
        internal int ArraySize;
                
        internal float TexelWidth { get; private set; }
        internal float TexelHeight { get; private set; }

        /// <summary>
        /// Gets the dimensions of the texture
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
				return new Rectangle(0, 0, this.width, this.height);
            }
        }

        /// <summary>
        /// Creates a new texture of the given size
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
            : this(graphicsDevice, width, height, false, SurfaceFormat.Color, SurfaceType.Texture, false, 1)
        {
        }

        /// <summary>
        /// Creates a new texture of a given size with a surface format and optional mipmaps 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipmap"></param>
        /// <param name="format"></param>
        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
            : this(graphicsDevice, width, height, mipmap, format, SurfaceType.Texture, false, 1)
        {
        }

        /// <summary>
        /// Creates a new texture array of a given size with a surface format and optional mipmaps.
        /// Throws ArgumentException if the current GraphicsDevice can't work with texture arrays
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipmap"></param>
        /// <param name="format"></param>
        /// <param name="arraySize"></param>
        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize)
            : this(graphicsDevice, width, height, mipmap, format, SurfaceType.Texture, false, arraySize)
        {
            
        }

        /// <summary>
        ///  Creates a new texture of a given size with a surface format and optional mipmaps.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipmap"></param>
        /// <param name="format"></param>
        /// <param name="type"></param>
        internal Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type)
            : this(graphicsDevice, width, height, mipmap, format, type, false, 1)
        {
        }
        
        protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, int arraySize)
		{
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width","Texture width must be greater than zero");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height","Texture height must be greater than zero");
            if (arraySize > 1 && !graphicsDevice.GraphicsCapabilities.SupportsTextureArrays)
                throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySize");

            this.GraphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            this.TexelWidth = 1f / (float)width;
            this.TexelHeight = 1f / (float)height;

            this._format = format;
            this._levelCount = mipmap ? CalculateMipLevels(width, height) : 1;
            this.ArraySize = arraySize;

            // Texture will be assigned by the swap chain.
		    if (type == SurfaceType.SwapChainRenderTarget)
		        return;

            PlatformConstruct(width, height, mipmap, format, type, shared);
        }

        /// <summary>
        /// Gets the width of the texture in pixels.
        /// </summary>
        public int Width
        {
            get
            {
                return width;
            }
        }

        /// <summary>
        /// Gets the height of the texture in pixels.
        /// </summary>
        public int Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// Changes the pixels of the texture
        /// Throws ArgumentNullException if data is null
        /// Throws ArgumentException if arraySlice is greater than 0, and the GraphicsDevice does not support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">Layer of the texture to modify</param>
        /// <param name="arraySlice">Index inside the texture array</param>
        /// <param name="rect">Area to modify</param>
        /// <param name="data">New data for the texture</param>
        /// <param name="startIndex">Start position of data</param>
        /// <param name="elementCount"></param>
        public void SetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Rectangle checkedRect;
            ValidateParams(level, arraySlice, rect, data, startIndex, elementCount, out checkedRect);
            PlatformSetData(level, arraySlice, checkedRect, data, startIndex, elementCount);
        }

        /// <summary>
        /// Changes the pixels of the texture
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">Layer of the texture to modify</param>
        /// <param name="rect">Area to modify</param>
        /// <param name="data">New data for the texture</param>
        /// <param name="startIndex">Start position of data</param>
        /// <param name="elementCount"></param>
        public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct 
        {
            Rectangle checkedRect;
            ValidateParams(level, 0, rect, data, startIndex, elementCount, out checkedRect);
            if (rect.HasValue)
                PlatformSetData(level, 0, checkedRect, data, startIndex, elementCount);
            else
                PlatformSetData(level, data, startIndex, elementCount);
        }

        /// <summary>
        /// Changes the texture's pixels
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">New data for the texture</param>
        /// <param name="startIndex">Start position of data</param>
        /// <param name="elementCount"></param>
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            Rectangle checkedRect;
            ValidateParams(0, 0, null, data, startIndex, elementCount, out checkedRect);
            PlatformSetData(0, data, startIndex, elementCount);
        }

		/// <summary>
        /// Changes the texture's pixels
        /// </summary>
        /// <typeparam name="T">New data for the texture</typeparam>
        /// <param name="data"></param>
		public void SetData<T>(T[] data) where T : struct
		{
            Rectangle checkedRect;
            ValidateParams(0, 0, null, data, 0, data.Length, out checkedRect);
            PlatformSetData(0, data, 0, data.Length);
        }

        /// <summary>
        /// Retrieves the contents of the texture
        /// Throws ArgumentException if data is null, data.length is too short or
        /// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">Layer of the texture</param>
        /// <param name="arraySlice">Index inside the texture array</param>
        /// <param name="rect">Area of the texture to retrieve</param>
        /// <param name="data">Destination array for the data</param>
        /// <param name="startIndex">Starting index of data where to write the pixel data</param>
        /// <param name="elementCount">Number of pixels to read</param>
        public void GetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Rectangle checkedRect;
            ValidateParams(level, arraySlice, rect, data, startIndex, elementCount, out checkedRect);
            PlatformGetData(level, arraySlice, checkedRect, data, startIndex, elementCount);
        }

        /// <summary>
        /// Retrieves the contents of the texture
        /// Throws ArgumentException if data is null, data.length is too short or
        /// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">Layer of the texture</param>
        /// <param name="rect">Area of the texture</param>
        /// <param name="data">Destination array for the texture data</param>
        /// <param name="startIndex">First position in data where to write the pixel data</param>
        /// <param name="elementCount">Number of pixels to read</param>
        public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            this.GetData(level, 0, rect, data, startIndex, elementCount);
        }

        /// <summary>
        /// Retrieves the contents of the texture
        /// Throws ArgumentException if data is null, data.length is too short or
        /// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Destination array for the texture data</param>
        /// <param name="startIndex">First position in data where to write the pixel data</param>
        /// <param name="elementCount">Number of pixels to read</param>
		public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
		{
			this.GetData(0, null, data, startIndex, elementCount);
		}

        /// <summary>
        /// Retrieves the contents of the texture
        /// Throws ArgumentException if data is null, data.length is too short or
        /// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Destination array for the texture data</param>
        public void GetData<T> (T[] data) where T : struct
		{
		    if (data == null)
		        throw new ArgumentNullException("data");
			this.GetData(0, null, data, 0, data.Length);
		}
		
        /// <summary>
        /// Creates a Texture2D from a stream, supported formats bmp, gif, jpg, png, tif and dds (only for simple textures).
        /// May work with other formats, but will not work with tga files.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device where the texture will be created.</param>
        /// <param name="stream">The stream from which to read the image data.</param>
        /// <returns>The <see cref="SurfaceFormat.Color"/> texture created from the image stream.</returns>
        /// <remarks>Note that different image decoders may generate slight differences between platforms, but perceptually 
        /// the images should be identical.  This call does not premultiply the image alpha, but areas of zero alpha will
        /// result in black color data.
        /// </remarks>
        public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
		{
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (stream == null)
                throw new ArgumentNullException("stream");

            try
            {
                return PlatformFromStream(graphicsDevice, stream);
            }catch(Exception e)
            {
                throw new InvalidOperationException("This image format is not supported", e);
            }
        }

        /// <summary>
        /// Converts the texture to a JPG image
        /// </summary>
        /// <param name="stream">Destination for the image</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SaveAsJpeg(Stream stream, int width, int height)
        {
            PlatformSaveAsJpeg(stream, width, height);
        }

        /// <summary>
        /// Converts the texture to a PNG image
        /// </summary>
        /// <param name="stream">Destination for the image</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SaveAsPng(Stream stream, int width, int height)
        {
            PlatformSaveAsPng(stream, width, height);
        }

        // This method allows games that use Texture2D.FromStream 
        // to reload their textures after the GL context is lost.
        public void Reload(Stream textureStream)
        {
            PlatformReload(textureStream);
        }

        //Converts Pixel Data from ARGB to ABGR
        private static void ConvertToABGR(int pixelHeight, int pixelWidth, int[] pixels)
        {
            int pixelCount = pixelWidth * pixelHeight;
            for (int i = 0; i < pixelCount; ++i)
            {
                uint pixel = (uint)pixels[i];
                pixels[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
            }
        }

        private void ValidateParams<T>(int level, int arraySlice, Rectangle? rect, T[] data,
            int startIndex, int elementCount, out Rectangle checkedRect) where T : struct
        {
            var textureBounds = new Rectangle(0, 0, Math.Max(width >> level, 1), Math.Max(height >> level, 1));
            checkedRect = rect ?? textureBounds;
            if (level < 0 || level >= LevelCount)
                throw new ArgumentException("level must be smaller than the number of levels in this texture.", "level");
            if (arraySlice > 0 && !GraphicsDevice.GraphicsCapabilities.SupportsTextureArrays)
                throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySlice");
            if (arraySlice < 0 || arraySlice >= ArraySize)
                throw new ArgumentException("arraySlice must be smaller than the ArraySize of this texture and larger than 0.", "arraySlice");
            if (!textureBounds.Contains(checkedRect) || checkedRect.Width <= 0 || checkedRect.Height <= 0)
                throw new ArgumentException("Rectangle must be inside the texture bounds", "rect");
            if (data == null)
                throw new ArgumentNullException("data");
            var tSize = ReflectionHelpers.SizeOf<T>.Get();
            var fSize = Format.GetSize();
            if (tSize > fSize || fSize % tSize != 0)
                throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");
            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentException("startIndex must be at least zero and smaller than data.Length.", "startIndex");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data array is too small.");

            int dataByteSize;
            if (Format.IsCompressedFormat())
            {
                int blockWidth, blockHeight;
                Format.GetBlockSize(out blockWidth, out blockHeight);
                int blockWidthMinusOne = blockWidth - 1;
                int blockHeightMinusOne = blockHeight - 1;
                // round x and y down to next multiple of block size; width and height up to next multiple of block size
                var roundedWidth = (checkedRect.Width + blockWidthMinusOne) & ~blockWidthMinusOne;
                var roundedHeight = (checkedRect.Height + blockHeightMinusOne) & ~blockHeightMinusOne;
                checkedRect = new Rectangle(checkedRect.X & ~blockWidthMinusOne, checkedRect.Y & ~blockHeightMinusOne,
#if OPENGL
                    // OpenGL only: The last two mip levels require the width and height to be
                    // passed as 2x2 and 1x1, but there needs to be enough data passed to occupy
                    // a full block.
                    checkedRect.Width < blockWidth && textureBounds.Width < blockWidth ? textureBounds.Width : roundedWidth,
                    checkedRect.Height < blockHeight && textureBounds.Height < blockHeight ? textureBounds.Height : roundedHeight);
#else
                    roundedWidth, roundedHeight);
#endif
                if (Format == SurfaceFormat.RgbPvrtc2Bpp || Format == SurfaceFormat.RgbaPvrtc2Bpp)
                {
                    dataByteSize = (Math.Max(checkedRect.Width, 16) * Math.Max(checkedRect.Height, 8) * 2 + 7) / 8;
                }
                else if (Format == SurfaceFormat.RgbPvrtc4Bpp || Format == SurfaceFormat.RgbaPvrtc4Bpp)
                {
                    dataByteSize = (Math.Max(checkedRect.Width, 8) * Math.Max(checkedRect.Height, 8) * 4 + 7) / 8;
                }
                else
                {
                    dataByteSize = roundedWidth * roundedHeight * fSize / (blockWidth * blockHeight);
                }
            }
            else
            {
                dataByteSize = checkedRect.Width * checkedRect.Height * fSize;
            }
            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, " +
                                            "elementCount * sizeof(T) is {0}, but data size is {1}.",
                                            elementCount * tSize, dataByteSize), "elementCount");
        }
	}
}
