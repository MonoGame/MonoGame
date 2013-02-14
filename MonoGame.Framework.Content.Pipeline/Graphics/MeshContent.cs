// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties and methods that define various aspects of a mesh.
    /// </summary>
    public class MeshContent : NodeContent
    {
        GeometryContentCollection geometry;
        PositionCollection positions;

        /// <summary>
        /// Gets the list of geometry batches for the mesh.
        /// </summary>
        public GeometryContentCollection Geometry
        {
            get
            {
                return geometry;
            }
        }

        /// <summary>
        /// Gets the list of vertex position values.
        /// </summary>
        public PositionCollection Positions
        {
            get
            {
                return positions;
            }
        }

        /// <summary>
        /// Initializes a new instance of MeshContent.
        /// </summary>
        public MeshContent()
        {
            geometry = new GeometryContentCollection(this);
            positions = new PositionCollection();
        }
    }
}
