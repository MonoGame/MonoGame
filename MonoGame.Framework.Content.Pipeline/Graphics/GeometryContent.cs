// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties that define various aspects of a geometry batch.
    /// </summary>
    public class GeometryContent : ContentItem
    {
        /// <summary>
        /// Creates an instance of GeometryContent.
        /// </summary>
        public GeometryContent()
        {
            Vertices = new VertexContent(this);
        }

        /// <summary>
        /// Gets the list of triangle indices for this geometry batch. Geometry is stored as an indexed triangle list, where each group of three indices defines a single triangle.
        /// </summary>
        public IndexCollection Indices { get; } = [];

        /// <summary>
        /// Gets or sets the material of the parent mesh.
        /// </summary>
        public MaterialContent? Material { get; set; }

        /// <summary>
        /// Gets or sets the parent MeshContent for this object.
        /// </summary>
        public MeshContent? Parent { get; set; }

        /// <summary>
        /// Gets the set of vertex batches for the geometry batch.
        /// </summary>
        public VertexContent Vertices { get; }
    }
}
