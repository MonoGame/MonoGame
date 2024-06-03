// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Stores design-time data for a <see cref="ModelMeshPart"/> asset.
    /// </summary>
    public sealed class ModelMeshPartContent
    {
        private IndexCollection _indexBuffer;
        private MaterialContent _material;
        private int _numVertices;
        private int _primitiveCount;
        private int _startIndex;
        private VertexBufferContent _vertexBuffer;
        private int _vertexOffset;

        internal ModelMeshPartContent() { }

        internal ModelMeshPartContent(VertexBufferContent vertexBuffer, IndexCollection indices, int vertexOffset,
                                      int numVertices, int startIndex, int primitiveCount)
        {
            _vertexBuffer = vertexBuffer;
            _indexBuffer = indices;
            _vertexOffset = vertexOffset;
            _numVertices = numVertices;
            _startIndex = startIndex;
            _primitiveCount = primitiveCount;
        }


        /// <summary>
        /// Gets the collection of indices for this mesh part.
        /// </summary>
        public IndexCollection IndexBuffer
        {
            get { return _indexBuffer; }
        }

        /// <summary>
        /// Gets the material of this mesh part.
        /// </summary>
        public MaterialContent Material
        {
            get { return _material; }
            set { _material = value; }
        }

        /// <summary>
        /// Gets the number of vertices used in this mesh part.
        /// </summary>
        public int NumVertices
        {
            get { return _numVertices; }
        }

        /// <summary>
        /// Gets the number of primitives to render for this mesh part.
        /// </summary>
        public int PrimitiveCount
        {
            get { return _primitiveCount; }
        }

        /// <summary>
        /// Gets the location in the index buffer at which to start reading vertices.
        /// </summary>
        public int StartIndex
        {
            get { return _startIndex; }
        }

        /// <summary>
        /// Gets a user-defined tag object.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets the collection of vertices for this mesh part.
        /// </summary>
        public VertexBufferContent VertexBuffer
        {
            get { return _vertexBuffer; }
        }

        /// <summary>
        /// Gets the offset from the start of the index buffer to the first vertex index.
        /// </summary>
        public int VertexOffset
        {
            get { return _vertexOffset; }
        }
    }
}
