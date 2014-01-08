// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public struct VertexBufferBinding
    {
        static VertexBufferBinding _none = new VertexBufferBinding(null);
        VertexBuffer _vertexBuffer;
        int _vertexOffset;
        int _instanceFrequency;

        /// <summary>
        /// A null vertex buffer binding for unused vertex buffer slots.
        /// </summary>
        static internal VertexBufferBinding None { get { return _none; } }

        /// <summary>
        /// Gets the instance frequency. A value of 0 means no instancing.
        /// </summary>
        public int InstanceFrequency { get { return _instanceFrequency; } }

        /// <summary>
        /// Gets the vertex buffer.
        /// </summary>
        public VertexBuffer VertexBuffer { get { return _vertexBuffer; } }

        /// <summary>
        /// Gets the offset in bytes from the beginning of the vertex buffer to the first vertex to use.
        /// </summary>
        public int VertexOffset { get { return _vertexOffset; } }

        /// <summary>
        /// Creates an instance of VertexBufferBinding.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer to bind.</param>
        public VertexBufferBinding(VertexBuffer vertexBuffer)
        {
            _vertexBuffer = vertexBuffer;
            _vertexOffset = 0;
            _instanceFrequency = 0;
        }

        /// <summary>
        /// Creates an instance of VertexBufferBinding.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer to bind.</param>
        /// <param name="vertexOffset">The offset in bytes to the first vertex to use.</param>
        public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset)
        {
            _vertexBuffer = vertexBuffer;
            _vertexOffset = vertexOffset;
            _instanceFrequency = 0;
        }

        /// <summary>
        /// Creates an instance of VertexBufferBinding.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer to bind.</param>
        /// <param name="vertexOffset">The offset in bytes to the first vertex to use.</param>
        /// <param name="instanceFrequency">Number of instances to draw for each draw call. Use 0 if not using instanced drawing.</param>
        public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset, int instanceFrequency)
        {
            _vertexBuffer = vertexBuffer;
            _vertexOffset = vertexOffset;
            _instanceFrequency = instanceFrequency;
        }
    }
}
