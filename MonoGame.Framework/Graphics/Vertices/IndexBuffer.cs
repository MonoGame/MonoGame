// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Describes the rendering order of the vertices in a vertex buffer.
    /// </summary>
    public partial class IndexBuffer : GraphicsResource
    {
        private readonly bool _isDynamic;

        /// <summary>
        /// Gets the usage hint for optimizing memory placement of this <b>IndexBuffer</b>.
        /// </summary>
        public BufferUsage BufferUsage { get; private set; }

        /// <summary>
        /// Gets the total number of elements in this <b>IndexBuffer</b>.
        /// </summary>
        public int IndexCount { get; private set; }

        /// <summary>
        /// Gets a value indicating the size of each element in this <b>IndexBuffer</b>
        /// </summary>
        public IndexElementSize IndexElementSize { get; private set; }

        /// <summary>
        /// Creates a new instance of <b>IndexBuffer</b> with the specified parameters.
        /// </summary>
        /// <param name="graphicsDevice">The associated graphics device of the index buffer.</param>
        /// <param name="indexType">Type to use for index values.</param>
        /// <param name="indexCount">Number of indices in the buffer.</param>
        /// <param name="usage">A set of options identifying the behaviors of this index buffer resource.</param>
        /// <param name="dynamic"><b>true</b> if this is a dynamic index buffer; otherwise, <b>false</b>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="indexCount"/> parameter is invalid.  Index buffers can only be created for indices which
        /// are sixteen or thirty-two bits in length
        /// </exception>
        /// <exception cref="InvalidOperationException">The resource could not be created</exception>
   		protected IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage, bool dynamic)
            : this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage, dynamic)
        {
        }

        /// <summary>
        /// Creates a new instance of <b>IndexBuffer</b> with the specified parameters.
        /// </summary>
        /// <param name="graphicsDevice">The associated graphics device of the index buffer.</param>
        /// <param name="indexElementSize">The size, in bits, of an index element.</param>
        /// <param name="indexCount">Number of indices in the buffer.</param>
        /// <param name="usage">A set of options identifying the behaviors of this index buffer resource.</param>
        /// <param name="dynamic"><b>true</b> if this is a dynamic index buffer; otherwise, <b>false</b>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="graphicsDevice"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// One of the following conditions is true
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             The <paramref name="indexCount"/> parameter is invalid.  Index buffers can only be created for
        ///             indices which are sixteen or thirty-two bits in length
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="indexElementSize"/> parameter is invalid.  The resource size must be greater
        ///             than zero.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="indexElementSize"/> parameter is invalid.  The total size of the index
        ///             buffer must be an even multiple of the index element size (either 16 or 32 bits).
        ///         </description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidOperationException">The resource could not be created</exception>
		protected IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool dynamic)
        {
			if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            }
			this.GraphicsDevice = graphicsDevice;
			this.IndexElementSize = indexElementSize;
            this.IndexCount = indexCount;
            this.BufferUsage = usage;

            _isDynamic = dynamic;

            PlatformConstruct(indexElementSize, indexCount);
		}

        /// <summary>
        /// Creates a new instance of <b>IndexBuffer</b> with the specified parameters.
        /// </summary>
        /// <param name="graphicsDevice">The associated graphics device of the index buffer.</param>
        /// <param name="indexElementSize">The size, in bits, of an index element.</param>
        /// <param name="indexCount">Number of indices in the buffer.</param>
        /// <param name="bufferUsage">A set of options identifying the behaviors of this index buffer resource.</param>
        /// <exception cref="ArgumentNullException"><paramref name="graphicsDevice"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// One of the following conditions is true
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             The <paramref name="indexCount"/> parameter is invalid.  Index buffers can only be created for
        ///             indices which are sixteen or thirty-two bits in length
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="indexElementSize"/> parameter is invalid.  The resource size must be greater
        ///             than zero.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The <paramref name="indexElementSize"/> parameter is invalid.  The total size of the index
        ///             buffer must be an even multiple of the index element size (either 16 or 32 bits).
        ///         </description>
        ///     </item>
        /// </list>
        /// </exception>
		public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, indexElementSize, indexCount, bufferUsage, false)
		{
		}

        /// <summary>
        /// Creates a new instance of <b>IndexBuffer</b> with the specified parameters.
        /// </summary>
        /// <param name="graphicsDevice">The associated graphics device of the index buffer.</param>
        /// <param name="indexType">Type to use for index values.</param>
        /// <param name="indexCount">Number of indices in the buffer.</param>
        /// <param name="usage">A set of options identifying the behaviors of this index buffer resource.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="indexCount"/> parameter is invalid.  It must be greater than zero, and the index must be
        /// sixteen or thirty-two bits in length. Index buffers can only be created for indices which are sixteen or
        /// thirty-two bits in length.
        /// </exception>
        /// <exception cref="InvalidOperationException">The resource could not be created</exception>
		public IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
			this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage, false)
		{
		}

        /// <summary>
        /// Gets the relevant IndexElementSize enum value for the given type.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="type">The type to use for the index buffer</param>
        /// <returns>The IndexElementSize enum value that matches the type</returns>
        static IndexElementSize SizeForType(GraphicsDevice graphicsDevice, Type type)
        {
            switch (ReflectionHelpers.ManagedSizeOf(type))
            {
                case 2:
                    return IndexElementSize.SixteenBits;
                case 4:
                    if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
                        throw new NotSupportedException("The profile does not support an elementSize of IndexElementSize.ThirtyTwoBits; use IndexElementSize.SixteenBits or a type that has a size of two bytes.");
                    return IndexElementSize.ThirtyTwoBits;
                default:
                    throw new ArgumentOutOfRangeException("type","Index buffers can only be created for types that are sixteen or thirty two bits in length");
            }
        }

        /// <summary>
        /// The GraphicsDevice is resetting, so GPU resources must be recreated.
        /// </summary>
        internal protected override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }

        /// <summary>
        /// Copies the index buffer into an array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="offsetInBytes">The number of bytes into the index buffer where copying will start.</param>
        /// <param name="data">The array to receive index buffer data.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="data"/> is not the correct size for the amount of data requested.</exception>
        /// <exception cref="NotSupportedException">
        /// This <b>IndexBuffer</b> was created with a usage type of <see cref="BufferUsage.WriteOnly"/>.
        /// </exception>
        public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("This IndexBuffer was created with a usage type of BufferUsage.WriteOnly. Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");

            PlatformGetData<T>(offsetInBytes, data, startIndex, elementCount);
        }

        /// <summary>
        /// Copies the index buffer into an array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="data">The array to receive index buffer data.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="data"/> is not the correct size for the amount of data requested.</exception>
        /// <exception cref="NotSupportedException">
        /// This <b>IndexBuffer</b> was created with a usage type of <see cref="BufferUsage.WriteOnly"/>.
        /// </exception>
        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            this.GetData<T>(0, data, startIndex, elementCount);
        }

        /// <summary>
        /// Copies the index buffer into an array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="data">The array to receive index buffer data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="data"/> is not the correct size for the amount of data requested.</exception>
        /// <exception cref="NotSupportedException">
        /// This <b>IndexBuffer</b> was created with a usage type of <see cref="BufferUsage.WriteOnly"/>.
        /// </exception>
        public void GetData<T>(T[] data) where T : struct
        {
            this.GetData<T>(0, data, 0, data.Length);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offsetInBytes"></param>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="elementCount"></param>
        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, SetDataOptions.None);
        }

        /// <summary>
        /// Copies array data to the index buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="data"/> is not the correct size for the amount of data requested.</exception>
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetDataInternal<T>(0, data, startIndex, elementCount, SetDataOptions.None);
		}

        /// <summary>
        /// Copies array data to the index buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="data">The array of data to copy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="data"/> is not the correct size for the amount of data requested.</exception>
        public void SetData<T>(T[] data) where T : struct
        {
            SetDataInternal<T>(0, data, 0, data.Length, SetDataOptions.None);
        }

        /// <summary>
        /// Copies array data to the index buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="offsetInBytes">Number of bytes into the index buffer where copying will start.</param>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <param name="options">Specifies whether existing data in the buffer will be kept after this operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="data"/> is not the correct size for the amount of data requested.</exception>
        protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");

            PlatformSetData<T>(offsetInBytes, data, startIndex, elementCount, options);
        }
	}
}
