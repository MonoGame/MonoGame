// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties that define various aspects of a geometry batch.
    /// </summary>
    public class GeometryContent : ContentItem
    {
        IndexCollection indices;
        MaterialContent material;
        MeshContent parent;
        VertexContent vertices;

        /// <summary>
        /// Gets the list of triangle indices for this geometry batch. Geometry is stored as an indexed triangle list, where each group of three indices defines a single triangle.
        /// </summary>
        public IndexCollection Indices
        {
            get
            {
                return indices;
            }
        }

        /// <summary>
        /// Gets or sets the material of the parent mesh.
        /// </summary>
        public MaterialContent Material
        {
            get
            {
                return material;
            }
            set
            {
                material = value;
            }
        }

        /// <summary>
        /// Gets or sets the parent MeshContent for this object.
        /// </summary>
        public MeshContent Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        /// <summary>
        /// Gets the set of vertex batches for the geometry batch.
        /// </summary>
        public VertexContent Vertices
        {
            get
            {
                return vertices;
            }
        }

        /// <summary>
        /// Creates an instance of GeometryContent.
        /// </summary>
        public GeometryContent()
        {
            indices = new IndexCollection();
            vertices = new VertexContent(this);
        }
    }
}
