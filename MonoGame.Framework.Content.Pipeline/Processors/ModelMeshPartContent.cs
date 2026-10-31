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
        internal ModelMeshPartContent(VertexBufferContent? vertexBuffer, IndexCollection? indices, int vertexOffset, int numVertices, int startIndex, int primitiveCount)
        {
            VertexBuffer = vertexBuffer;
            IndexBuffer = indices;
            VertexOffset = vertexOffset;
            NumVertices = numVertices;
            StartIndex = startIndex;
            PrimitiveCount = primitiveCount;
        }

        /// <summary>
        /// Gets the collection of indices for this mesh part.
        /// </summary>
        public IndexCollection? IndexBuffer { get; }

        /// <summary>
        /// Gets the material of this mesh part.
        /// </summary>
        public MaterialContent? Material { get; set; }

        /// <summary>
        /// Gets the number of vertices used in this mesh part.
        /// </summary>
        public int NumVertices { get; }

        /// <summary>
        /// Gets the number of primitives to render for this mesh part.
        /// </summary>
        public int PrimitiveCount { get; }

        /// <summary>
        /// Gets the location in the index buffer at which to start reading vertices.
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// Gets a user-defined tag object.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Gets the collection of vertices for this mesh part.
        /// </summary>
        public VertexBufferContent? VertexBuffer { get; }

        /// <summary>
        /// Gets the offset from the start of the index buffer to the first vertex index.
        /// </summary>
        public int VertexOffset { get; }
    }
}
