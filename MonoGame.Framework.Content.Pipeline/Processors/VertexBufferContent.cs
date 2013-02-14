// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Provides methods and properties for managing a design-time vertex buffer that holds packed vertex data.
    /// </summary>
    /// <remarks>This type directly corresponds to the runtime VertexBuffer class, and when a VertexBufferContent object is passed to the content compiler, the vertex data deserializes directly into a VertexBuffer at runtime. VertexBufferContent objects are not directly created by importers. The preferred method is to store vertex data in the more flexible VertexContent class.</remarks>
    public class VertexBufferContent : ContentItem
    {
        byte[] vertexData;
        VertexDeclarationContent vertexDeclarationContent;

        /// <summary>
        /// Gets the array containing the raw bytes of the packed vertex data. Use this method to get and set the contents of the vertex buffer.
        /// </summary>
        /// <value>Raw data of the packed vertex data.</value>
        public byte[] VertexData { get { return vertexData; } }

        /// <summary>
        /// Gets the associated VertexDeclarationContent object.
        /// </summary>
        /// <value>The associated VertexDeclarationContent object.</value>
        public VertexDeclarationContent VertexDeclaration { get { return vertexDeclarationContent; } set { vertexDeclarationContent = value; } }

        /// <summary>
        /// Initializes a new instance of VertexBufferContent.
        /// </summary>
        public VertexBufferContent()
        {

        }

        /// <summary>
        /// Initializes a new instance of VertexBufferContent of the specified size.
        /// </summary>
        /// <param name="size">The size of the vertex buffer content, in bytes.</param>
        public VertexBufferContent(int size)
            : base()
        {

        }

        /// <summary>
        /// Gets the size of the specified type, in bytes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The size of the specified type, in bytes.</returns>
        /// <remarks>Call this method to compute offset parameters for the Write method. If the specified data type cannot be packed into a vertex buffer—for example, if type is not a valid value type—a NotSupportedException is thrown.</remarks>
        /// <exception cref="NotSupportedException">type is not a valid value type</exception>
        public static int SizeOf(Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes additional data into the vertex buffer. Writing begins at the specified byte offset, and each value is spaced according to the specified stride value (in bytes).
        /// </summary>
        /// <typeparam name="T">Type being written.</typeparam>
        /// <param name="offset">Offset to begin writing at.</param>
        /// <param name="stride">Stride of the data being written, in bytes.</param>
        /// <param name="data">Enumerated collection of data.</param>
        /// <exception cref="NotSupportedException">The specified data type cannot be packed into a vertex buffer.</exception>
        public void Write<T>(int offset, int stride, IEnumerable<T> data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes additional data into the vertex buffer. Writing begins at the specified byte offset, and each value is spaced according to the specified stride value (in bytes).
        /// </summary>
        /// <param name="offset">Offset at which to begin writing.</param>
        /// <param name="stride">Stride of the data being written, in bytes.</param>
        /// <param name="dataType">The type of data to be written.</param>
        /// <param name="data">The data to write.</param>
        /// <exception cref="NotSupportedException">The specified data type cannot be packed into a vertex buffer.</exception>
        public void Write(int offset, int stride, Type dataType, IEnumerable data)
        {
            throw new NotSupportedException();
        }
    }
}
