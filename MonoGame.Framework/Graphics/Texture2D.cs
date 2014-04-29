// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

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

        public Rectangle Bounds
        {
            get
            {
				return new Rectangle(0, 0, this.width, this.height);
            }
        }

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
            : this(graphicsDevice, width, height, false, SurfaceFormat.Color, SurfaceType.Texture, false)
        {
        }

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
            : this(graphicsDevice, width, height, mipmap, format, SurfaceType.Texture, false)
        {
        }

        internal Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type)
            : this(graphicsDevice, width, height, mipmap, format, type, false)
        {
        }

        protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
		{
            if (graphicsDevice == null)
                throw new ArgumentNullException("Graphics Device Cannot Be Null");

            this.GraphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            this._format = format;
            this._levelCount = mipmap ? CalculateMipLevels(width, height) : 1;

            // Texture will be assigned by the swap chain.
		    if (type == SurfaceType.SwapChainRenderTarget)
		        return;

            PlatformConstruct(width, height, mipmap, format, type, shared);
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct 
        {
            if (data == null)
				throw new ArgumentNullException("data");

            PlatformSetData<T>(level, rect, data, startIndex, elementCount);
        }

		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            this.SetData(0, null, data, startIndex, elementCount);
        }
		
		public void SetData<T>(T[] data) where T : struct
        {
			this.SetData(0, null, data, 0, data.Length);
        }
		
		public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("data cannot be null");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");

            PlatformGetData<T>(level, rect, data, startIndex, elementCount);
        }

		public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
		{
			this.GetData(0, null, data, startIndex, elementCount);
		}
		
		public void GetData<T> (T[] data) where T : struct
		{
			this.GetData(0, null, data, 0, data.Length);
		}
		
		public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
		{
            return PlatformFromStream(graphicsDevice, stream);
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
            PlatformSaveAsJpeg(stream, width, height);
        }

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
	}
}

