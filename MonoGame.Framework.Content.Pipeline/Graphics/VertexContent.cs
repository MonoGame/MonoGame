#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
 All rights reserved.
 
 This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
 accept the license, do not use the software.
 
 1. Definitions
 The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
 U.S. copyright law.
 
 A "contribution" is the original software, or any additions or changes to the software.
 A "contributor" is any person that distributes its contribution under this license.
 "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 
 2. Grant of Rights
 (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 
 3. Conditions and Limitations
 (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 your patent license from such contributor to the software ends automatically.
 (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 notices that are present in the software.
 (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
 code form, you may only do so under a license that complies with this license.
 (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
 or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
 permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 purpose and non-infringement.
 */
#endregion License

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
