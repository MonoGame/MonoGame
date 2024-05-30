// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a list of 3D vertices to be streamed to the graphics device.  Use <b>DynamicVertexBuffer</b> for
    /// dynamic vertex arrays and <see cref="VertexBuffer"/> for non-dynamic vertex arrays.
    /// </summary>
    /// <remarks>
    /// In situations where your game frequently modifies a vertex buffer, it is recommended that the buffer be instantiated
    /// or derived from <b>DynamicVertexBuffer</b> instead of the <see cref="VertexBuffer"/> class.  <b>DynamicVertexBuffer</b>
    /// is optimized for frequent vertex data modification.
    /// </remarks>
	public class DynamicVertexBuffer : VertexBuffer
    {
        /// <summary>
        /// Special offset used internally by GraphicsDevice.DrawUserXXX() methods.
        /// </summary>
        internal int UserOffset;

        /// <summary>
        /// Gets a value that indicates if the vertex buffer data has been lost due to a lost device event.
        /// </summary>
        /// <remarks>
        /// This property will always return <b>false</b>.  It is included for XNA compatibility.
        /// </remarks>
        [Obsolete("This is provided for XNA compatibility only and will always return false")]
        public bool IsContentLost { get { return false; } }

        /// <summary>
        /// Initializes a new instance of <b>DynamicVertexBuffer</b> with the specified parameters
        /// </summary>
        /// <param name="graphicsDevice">The associated graphics device of the vertex buffer.</param>
        /// <param name="vertexDeclaration">The vertex declaration, which describes per-vertex data.</param>
        /// <param name="vertexCount">The number of vertices in this vertex buffer.</param>
        /// <param name="bufferUsage">A set of options identifying the behaviors of this vertex buffer resource.</param>
        /// <exception cref="ArgumentNullException"><paramref name="graphicsDevice"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="vertexCount"/> must be greater than zero.</exception>
        /// <exception cref="InvalidOperationException">This resource could not be created.</exception>
        public DynamicVertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage)
            : base(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of <b>DynamicVertexBuffer</b> with the specified parameters
        /// </summary>
        /// <param name="graphicsDevice">The associated graphics device of the vertex buffer.</param>
        /// <param name="type">The type used that describes the per-vertex data. Must derive from <see cref="IVertexType"/>.</param>
        /// <param name="vertexCount">The number of vertices in this vertex buffer.</param>
        /// <param name="bufferUsage">A set of options identifying the behaviors of this vertex buffer resource.</param>
        /// <exception cref="ArgumentException"><paramref name="type"/>
        /// /// One of the following conditions is true:
        /// <list type="bullet">
        ///     <item>
        ///         <description>The <paramref name="type"/> parameter is not a value type.</description>
        ///     </item>
        ///     <item>
        ///         <description>The <paramref name="type"/> parameter does not derive from <see cref="IVertexType"/>.</description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// One of the following conditions is true:
        /// <list type="bullet">
        ///     <item>
        ///         <description>The <paramref name="graphicsDevice"/> parameter is null.</description>
        ///     </item>
        ///     <item>
        ///         <description>The <paramref name="type"/> parameter is null.</description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="vertexCount"/> parameter  must be greater than zero.</exception>
        /// <exception cref="Exception">
        /// A new <see cref="VertexDeclaration"/> instance cannot be created from the type provided by the
        /// <paramref name="type"/> parameter.
        /// </exception>
        /// <exception cref="InvalidOperationException">This resource could not be created.</exception>
		public DynamicVertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage)
            : base(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, bufferUsage, true)
        {
        }

        /// <summary>
        /// Copies array data to the vertex buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="offsetInBytes">Number of bytes into the vertex buffer where copying will start.</param>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <param name="vertexStride">The size, in bytes, of the elements in the vertex buffer.</param>
        /// <param name="options">Specifies whether existing data in the buffer will be kept after this operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// One of the following conditions is true:
        /// <list type="bullet">
        ///     <item>
        ///         <description>The <paramref name="vertexStride"/> parameter larger than the vertex buffer size.</description>
        ///     </item>
        ///     <item>
        ///         <description>The <paramref name="vertexStride"/> parameter is less than the size of the specified data.</description>
        ///     </item>
        ///     <item>
        ///         <description>The <paramref name="data"/> parameter is not the correct size for the amount of data requested.</description>
        ///     </item>
        /// </list>
        /// </exception>
        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, vertexStride, options);
        }

        /// <summary>
        /// Copies array data to the vertex buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="data">The array of data to copy.</param>
        /// <param name="startIndex">The index of the element in the array at which to start copying.</param>
        /// <param name="elementCount">The number of elements to copy.</param>
        /// <param name="options">Specifies whether existing data in the buffer will be kept after this operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="data"/> parameter is not the correct size for the amount of data requested.
        /// </exception>
        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
            base.SetDataInternal<T>(0, data, startIndex, elementCount, elementSizeInBytes, options);
        }
    }
}

