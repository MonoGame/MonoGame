// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Describes the rendering order of the vertices  in a vertex buffer. Use <b>DynamicIndexBuffer</b> for storing
    /// indices for dynamic vertices and <see cref="IndexBuffer"/> for indices of non-dynamic arrays.
    /// </summary>
    /// <remarks>
    /// For more information on drawing with dynamic buffers, see <see cref="DynamicVertexBuffer">DynamicVertexBuffer Class</see>
    /// </remarks>
	public class DynamicIndexBuffer : IndexBuffer
	{
        /// <summary>
        /// Special offset used internally by GraphicsDevice.DrawUserXXX() methods.
        /// </summary>
        internal int UserOffset;

        /// <summary>
        /// Gets a value that indicates whether the index buffer data has been lost due to a lost device event.
        /// </summary>
        public bool IsContentLost { get { return false; } }

        /// <summary>
        /// Initializes a new instance of <b>DynamicIndexBuffer</b> with the specified parameters.
        /// </summary>
        /// <param name="graphicsDevice">The associated graphics device of the index buffer.</param>
        /// <param name="indexElementSize">The size, in bits, of an index element.</param>
        /// <param name="indexCount">Number of indices in the buffer.</param>
        /// <param name="usage">A set of options identifying the behaviors of this index buffer resource.</param>
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
		public DynamicIndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage) :
			base(graphicsDevice, indexElementSize, indexCount, usage, true)
		{
		}

        /// <summary>
        /// Initializes a new instance of <b>DynamicIndexBuffer</b> with the specified parameters.
        /// </summary>
        /// <param name="graphicsDevice">The associated graphics device of the index buffer.</param>
        /// <param name="indexType">Type to use for index values.</param>
        /// <param name="indexCount">Number of indices in the buffer.</param>
        /// <param name="usage">A set of options identifying the behaviors of this index buffer resource.</param>
        /// <exception cref="ArgumentNullException"><paramref name="graphicsDevice"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="indexCount"/> parameter is invalid.  It must be greater than zero, and the index must be
        /// sixteen or thirty-two bits in length. Index buffers can only be created for indices which are sixteen or
        /// thirty-two bits in length.
        /// </exception>
        /// <exception cref="InvalidOperationException">The resource could not be created</exception>
   		public DynamicIndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
            base(graphicsDevice, indexType, indexCount, usage, true)
        {
        }

        /// <summary>
        /// Copies array data to the index buffer.
        /// </summary>
        /// <remarks>
        /// An <see cref="InvalidOperationException"/> is thrown if an attempt is made to modify (for example, calls to
        /// the <b>SetData</b> method) a resource that is currently set on a graphics device.
        /// </remarks>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="offsetInBytes">Number of bytes into the index buffer where copying will start.</param>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <param name="options">Specifies whether existing data in the buffer will be kept after this operation.</param>
        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, options);
        }

        /// <summary>
        /// Copies array data to the index buffer.
        /// </summary>
        /// <remarks>
        /// An <see cref="InvalidOperationException"/> is thrown if an attempt is made to modify (for example, calls to
        /// the <b>SetData</b> method) a resource that is currently set on a graphics device.
        /// </remarks>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <param name="options">Specifies whether existing data in the buffer will be kept after this operation.</param>
        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(0, data, startIndex, elementCount, options);
        }
    }
}

