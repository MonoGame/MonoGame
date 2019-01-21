// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture3D : Texture
	{
        private int _width;
        private int _height;
        private int _depth;

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public int Depth
        {
            get { return _depth; }
        }

		public Texture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : this(graphicsDevice, width, height, depth, mipMap, format, false)
		{
		}

		protected Texture3D (GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
		{
		    if (graphicsDevice == null)
		        throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width","Texture width must be greater than zero");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height","Texture height must be greater than zero");
            if (depth <= 0)
                throw new ArgumentOutOfRangeException("depth","Texture depth must be greater than zero");

		    this.GraphicsDevice = graphicsDevice;
            this._width = width;
            this._height = height;
            this._depth = depth;
            this._levelCount = 1;
		    this._format = format;

            PlatformConstruct(graphicsDevice, width, height, depth, mipMap, format, renderTarget);
        }

        public void SetData<T>(T[] data) where T : struct
		{
            if (data == null)
                throw new ArgumentNullException("data");
			SetData(data, 0, data.Length);
		}

		public void SetData<T> (T[] data, int startIndex, int elementCount) where T : struct
		{
			SetData(0, 0, 0, Width, Height, 0, Depth, data, startIndex, elementCount);
		}

		public void SetData<T> (int level,
		                        int left, int top, int right, int bottom, int front, int back,
		                        T[] data, int startIndex, int elementCount) where T : struct
		{
            ValidateParams(level, left, top, right, bottom, front, back, data, startIndex, elementCount);

            var width = right - left;
            var height = bottom - top;
            var depth = back - front;

            PlatformSetData(level, left, top, right, bottom, front, back, data, startIndex, elementCount, width, height, depth);
		}

        /// <summary>
        /// Gets a copy of 3D texture data, specifying a mipmap level, source box, start index, and number of elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="level">Mipmap level.</param>
        /// <param name="left">Position of the left side of the box on the x-axis.</param>
        /// <param name="top">Position of the top of the box on the y-axis.</param>
        /// <param name="right">Position of the right side of the box on the x-axis.</param>
        /// <param name="bottom">Position of the bottom of the box on the y-axis.</param>
        /// <param name="front">Position of the front of the box on the z-axis.</param>
        /// <param name="back">Position of the back of the box on the z-axis.</param>
        /// <param name="data">Array of data.</param>
        /// <param name="startIndex">Index of the first element to get.</param>
        /// <param name="elementCount">Number of elements to get.</param>
        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct
        {
            ValidateParams(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
            PlatformGetData(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
        }

        /// <summary>
        /// Gets a copy of 3D texture data, specifying a start index and number of elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">Array of data.</param>
        /// <param name="startIndex">Index of the first element to get.</param>
        /// <param name="elementCount">Number of elements to get.</param>
        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            GetData(0, 0, 0, _width, _height, 0, _depth, data, startIndex, elementCount);
        }

        /// <summary>
        /// Gets a copy of 3D texture data.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">Array of data.</param>
        public void GetData<T>(T[] data) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            GetData(data, 0, data.Length);
        }

        private void ValidateParams<T>(int level,
		                        int left, int top, int right, int bottom, int front, int back,
		                        T[] data, int startIndex, int elementCount) where T : struct
        {
            var texWidth = Math.Max(Width >> level, 1);
            var texHeight = Math.Max(Height >> level, 1);
            var texDepth = Math.Max(Depth >> level, 1);
            var width = right - left;
            var height = bottom - top;
            var depth = back - front;

            if (left < 0 || top < 0 || back < 0 || right > texWidth || bottom > texHeight || front > texDepth)
                throw new ArgumentException("Area must remain inside texture bounds");
            // Disallow negative box size
            if (left >= right || top >= bottom || front >= back)
                throw new ArgumentException("Neither box size nor box position can be negative");
            if (level < 0 || level >= LevelCount)
                throw new ArgumentException("level must be smaller than the number of levels in this texture.");
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

            var dataByteSize = width*height*depth*fSize;
            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, " +
                                            "elementCount * sizeof(T) is {0}, but data size is {1}.",
                                            elementCount * tSize, dataByteSize), "elementCount");
        }
	}
}

