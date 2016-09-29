// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines how a vertex buffer is bound to the graphics device for rendering.
    /// </summary>
    public struct VertexBufferBinding
    {
        private readonly VertexBuffer _vertexBuffer;
        private readonly int _vertexOffset;
        private readonly int _instanceFrequency;

        /// <summary>
        /// Gets the vertex buffer.
        /// </summary>
        /// <value>The vertex buffer.</value>
        public VertexBuffer VertexBuffer
        {
            get { return _vertexBuffer; }
        }

        /// <summary>
        /// Gets the index of the first vertex in the vertex buffer to use.
        /// </summary>
        /// <value>The index of the first vertex in the vertex buffer to use.</value>
        public int VertexOffset
        {
            get { return _vertexOffset; }
        }

        /// <summary>
        /// Gets the number of instances to draw using the same per-instance data before advancing
        /// in the buffer by one element.
        /// </summary>
        /// <value>
        /// The number of instances to draw using the same per-instance data before advancing in the
        /// buffer by one element. This value must be 0 for an element that contains per-vertex
        /// data and greater than 0 for per-instance data.
        /// </value>
        public int InstanceFrequency
        {
            get { return _instanceFrequency; }
        }

        /// <summary>
        /// Creates an instance of <see cref="VertexBufferBinding"/>.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer to bind.</param>
        public VertexBufferBinding(VertexBuffer vertexBuffer)
            : this(vertexBuffer, 0, 0)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="VertexBufferBinding"/>.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer to bind.</param>
        /// <param name="vertexOffset">
        /// The index of the first vertex in the vertex buffer to use.
        /// </param>
        public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset)
            : this(vertexBuffer, vertexOffset, 0)
        {
        }

        /// <summary>
        /// Creates an instance of VertexBufferBinding.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer to bind.</param>
        /// <param name="vertexOffset">
        /// The index of the first vertex in the vertex buffer to use.
        /// </param>
        /// <param name="instanceFrequency">
        /// The number of instances to draw using the same per-instance data before advancing in the
        /// buffer by one element. This value must be 0 for an element that contains per-vertex data
        /// and greater than 0 for per-instance data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="vertexBuffer"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="vertexOffset"/> or <paramref name="instanceFrequency"/> is invalid.
        /// </exception>
        public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset, int instanceFrequency)
        {
            if (vertexBuffer == null)
                throw new ArgumentNullException("vertexBuffer");
            if (vertexOffset < 0 || vertexOffset >= vertexBuffer.VertexCount)
                throw new ArgumentOutOfRangeException("vertexOffset");
            if (instanceFrequency < 0)
                throw new ArgumentOutOfRangeException("instanceFrequency");

            _vertexBuffer = vertexBuffer;
            _vertexOffset = vertexOffset;
            _instanceFrequency = instanceFrequency;
        }
    }
}
