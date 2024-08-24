// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a 3D volume of texels.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A texel represents the smallest unit of a texture that can be read from or written to by the GPU. A
    ///         texel is composed of 1 to 4 components. Specifically, a texel may be any one of the available texture
    ///         formats represented in the <see cref="SurfaceFormat"/> enumeration.
    ///     </para>
    ///     <para>
    ///         A <b>Texture3D</b> resource (also known as a volume texture) contains a 3D volume of texels. Since it is
    ///         a texture resource, it may contain mipmap levels.
    ///     </para>
    ///     <para>
    ///         When a <b>Texture3D</b> mipmap slice is bound as a render target output (by creating a
    ///         <see cref="RenderTargetCube"/>), the <b>Texture3D</b> behaves identically to an array of
    ///         <see cref="Texture2D"/> objects with <i>n</i> array slices, where <i>n</i> is the depth (third dimension)
    ///         of the <b>Texture3D</b>.
    ///     </para>
    /// </remarks>
	public partial class Texture3D : Texture
	{
        private int _width;
        private int _height;
        private int _depth;

        /// <summary>
        /// Gets the width, in pixels, of this texture resource.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }

        /// <summary>
        /// Gets the height, in pixels, of this texture resource.
        /// </summary>
        public int Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Gets the depth, in pixels, of this texture resource.
        /// </summary>
        public int Depth
        {
            get { return _depth; }
        }

        /// <summary>
        /// Creates an uninitialized <b>Texture3D</b> resource with the specified parameters.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         To initialize the texture with data after creating, you can use one of the
        ///         <see cref="SetData{T}(T[])">SetData</see> methods.
        ///     </para>
        ///     <para>
        ///         To initialize a <b>Texture3D</b> from an existing file use the
        ///         <see cref="ContentManager.Load{T}(string)">ContentManager.Load</see> method if loading a pipeline
        ///         preprocessed <b>Texture3D</b> from an .xnb file.
        ///     </para>
        /// </remarks>
        /// <param name="graphicsDevice">The graphics device used to display the texture.</param>
        /// <param name="width">The width, in pixels, of the texture.</param>
        /// <param name="height">The height, in pixels, of the texture.</param>
        /// <param name="depth">The depth, in pixels, of the texture.</param>
        /// <param name="mipMap"><b>true</b> if mimapping is enabled for the texture; otherwise, <b>false</b>.</param>
        /// <param name="format">The surface format of the texture.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="graphicsDevice"/> parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="width"/>, <paramref name="height"/>, and/or <paramref name="depth"/> parameters are less
        /// than or equal to zero.
        /// </exception>
		public Texture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : this(graphicsDevice, width, height, depth, mipMap, format, false)
		{
		}

        /// <summary />
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

        /// <summary>
        /// Copies an array of data to the texture.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">The array of data to copy.</param>
        /// <exception cref="ArgumentException">
        /// One of the following conditions is true:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             The <typeparamref name="T"/> type size is invalid for the format of this texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="data"/> array parameter is too small.
        ///         </description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> parameter is null.</exception>
        public void SetData<T>(T[] data) where T : struct
		{
            if (data == null)
                throw new ArgumentNullException("data");
			SetData(data, 0, data.Length);
		}

        /// <summary>
        /// Copies an array of data to the texture.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <exception cref="ArgumentException">
        /// One of the following conditions is true:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             The <typeparamref name="T"/> type size is invalid for the format of this texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="startIndex"/> parameter is less than zero or is greater than or equal to the
        ///             length of the data array.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="data"/> array parameter is too small.
        ///         </description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> parameter is null.</exception>
		public void SetData<T> (T[] data, int startIndex, int elementCount) where T : struct
		{
			SetData(0, 0, 0, Width, Height, 0, Depth, data, startIndex, elementCount);
		}

        /// <summary>
        /// Copies an array of data to the texture.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="level">The mipmap level where the data will be placed.</param>
        /// <param name="left">Position of the left side of the box on the x-axis.</param>
        /// <param name="top">Position of the top of the box on the y-axis.</param>
        /// <param name="right">Position of the right side of the box on the x-axis.</param>
        /// <param name="bottom">Position of the bottom of the box on the y-axis.</param>
        /// <param name="front">Position of the front of the box on the z-axis.</param>
        /// <param name="back">Position of the back of the box on the z-axis.</param>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <exception cref="ArgumentException">
        /// One of the following conditions is true:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             The <paramref name="level"/> parameter is larger than the number of levels in this texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <typeparamref name="T"/> type size is invalid for the format of this texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="startIndex"/> parameter is less than zero or is greater than or equal to the
        ///             length of the data array.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="data"/> array parameter is too small.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="left"/>, <paramref name="top"/>, <paramref name="back"/>, and/or <paramref name="right"/>
        ///             parameters are less than zero.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="right"/> parameter is greater than the width of the texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="bottom"/> parameter is greater than the height of the texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="top"/> parameter is greater than the depth of the texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="elementCount"/> parameter is the incorrect size.
        ///         </description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> parameter is null.</exception>
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
        /// Copies texture data into an array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="level">The mipmap level where the data will be placed.</param>
        /// <param name="left">Position of the left side of the box on the x-axis.</param>
        /// <param name="top">Position of the top of the box on the y-axis.</param>
        /// <param name="right">Position of the right side of the box on the x-axis.</param>
        /// <param name="bottom">Position of the bottom of the box on the y-axis.</param>
        /// <param name="front">Position of the front of the box on the z-axis.</param>
        /// <param name="back">Position of the back of the box on the z-axis.</param>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <exception cref="ArgumentException">
        /// One of the following conditions is true:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             The <paramref name="level"/> parameter is larger than the number of levels in this texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <typeparamref name="T"/> type size is invalid for the format of this texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="startIndex"/> parameter is less than zero or is greater than or equal to the
        ///             length of the data array.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="data"/> array parameter is too small.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="left"/>, <paramref name="top"/>, <paramref name="back"/>, and/or <paramref name="right"/>
        ///             parameters are less than zero.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="right"/> parameter is greater than the width of the texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="bottom"/> parameter is greater than the height of the texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="top"/> parameter is greater than the depth of the texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="elementCount"/> parameter is the incorrect size.
        ///         </description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> parameter is null.</exception>
        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct
        {
            ValidateParams(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
            PlatformGetData(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
        }

        /// <summary>
        /// Copies texture data into an array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <exception cref="ArgumentException">
        /// One of the following conditions is true:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             The <typeparamref name="T"/> type size is invalid for the format of this texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="startIndex"/> parameter is less than zero or is greater than or equal to the
        ///             length of the data array.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="data"/> array parameter is too small.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="elementCount"/> parameter is the incorrect size.
        ///         </description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> parameter is null.</exception>
        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            GetData(0, 0, 0, _width, _height, 0, _depth, data, startIndex, elementCount);
        }

        /// <summary>
        /// Copies texture data into an array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">The array of data to copy.</param>
        /// <exception cref="ArgumentException">
        /// One of the following conditions is true:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             The <typeparamref name="T"/> type size is invalid for the format of this texture.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="data"/> array parameter is too small.
        ///         </description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> parameter is null.</exception>
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

