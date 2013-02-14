// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for maintaining the vertex data of a GeometryContent.
    /// </summary>
    /// <remarks>This class combines a collection of arbitrarily named data channels with a list of position indices that reference the Positions collection of the parent MeshContent.</remarks>
    public sealed class VertexContent
    {
        VertexChannelCollection channels;
        VertexChannel<int> positionIndices;
        IndirectPositionCollection positions;

        /// <summary>
        /// Gets the list of named vertex data channels in the VertexContent.
        /// </summary>
        /// <value>Collection of vertex data channels.</value>
        public VertexChannelCollection Channels { get { return channels; } }

        /// <summary>
        /// Gets the list of position indices.
        /// </summary>
        /// <value>Position of the position index being retrieved.</value>
        /// <remarks>This list adds a level of indirection between the actual triangle indices and the Positions member of the parent. This indirection preserves the topological vertex identity in cases where a single vertex position is used by triangles that straddle a discontinuity in some other data channel.
        /// For example, the following code gets the position of the first vertex of the first triangle in a GeometryContent object:
        /// parent.Positions[Vertices.PositionIndices[Indices[0]]]</remarks>
        public VertexChannel<int> PositionIndices { get { return positionIndices; } }

        /// <summary>
        /// Gets position data from the parent mesh object.
        /// </summary>
        /// <value>Collection of vertex positions for the mesh.</value>
        /// <remarks>The collection returned from this call provides a virtualized view of the vertex positions for this batch. The collection uses the contents of the PositionIndices property to index into the parent Positions. This collection is read-only. If you need to modify any contained values, edit the PositionIndices or Positions members directly.</remarks>
        public IndirectPositionCollection Positions { get { return positions; } }

        /// <summary>
        /// Number of vertices for the content.
        /// </summary>
        /// <value>Number of vertices.</value>
        public int VertexCount { get { return positions.Count; } }

        /// <summary>
        /// Constructs a VertexContent instance.
        /// </summary>
        internal VertexContent()
        {

        }

        /// <summary>
        /// Appends a new vertex index to the end of the PositionIndices collection.
        /// Other vertex channels will automatically be extended and the new indices populated with default values.
        /// </summary>
        /// <param name="positionIndex">Index into the MeshContent.Positions member of the parent.</param>
        /// <returns>Index of the new entry. This can be added to the Indices member of the parent.</returns>
        public int Add(int positionIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Appends multiple vertex indices to the end of the PositionIndices collection.
        /// Other vertex channels will automatically be extended and the new indices populated with default values.
        /// </summary>
        /// <param name="positionIndexCollection">Index into the Positions member of the parent.</param>
        public void AddRange(IEnumerable<int> positionIndexCollection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts design-time vertex position and channel data into a vertex buffer format that a graphics device can recognize.
        /// </summary>
        /// <returns>A packed vertex buffer.</returns>
        /// <exception cref="InvalidContentException">One or more of the vertex channel types are invalid or an unrecognized name was passed to VertexElementUsage.</exception>
        public VertexBufferContent CreateVertexBuffer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts a new vertex index to the PositionIndices collection.
        /// Other vertex channels will automatically be extended and the new indices populated with default values.
        /// </summary>
        /// <param name="index">Vertex index to be inserted.</param>
        /// <param name="positionIndex">Position of new vertex index in the collection.</param>
        public void Insert(int index, int positionIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts multiple vertex indices to the PositionIndices collection.
        /// Other vertex channels will automatically be extended and the new indices populated with default values.
        /// </summary>
        /// <param name="index">Vertex index to be inserted.</param>
        /// <param name="positionIndexCollection">Position of the first element of the inserted range in the collection.</param>
        public void InsertRange(int index, IEnumerable<int> positionIndexCollection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes a vertex index from the specified location in both PositionIndices and VertexChannel<T>.
        /// </summary>
        /// <param name="index">Index of the vertex to be removed.</param>
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes a range of vertex indices from the specified location in both PositionIndices and VertexChannel<T>.
        /// </summary>
        /// <param name="index">Index of the first vertex index to be removed.</param>
        /// <param name="count">Number of indices to remove.</param>
        public void RemoveRange(int index, int count)
        {
            throw new NotImplementedException();
        }
    }
}
