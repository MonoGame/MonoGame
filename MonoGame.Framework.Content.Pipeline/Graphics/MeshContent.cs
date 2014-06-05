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

        /// <summary>
        /// Applies a transform directly to position and normal channels. Node transforms are unaffected.
        /// </summary>
        internal void TransformContents(ref Matrix xform)
        {
            // Transform positions
            var updateBuffer = new Vector3[positions.Count];

            positions.CopyTo(updateBuffer, 0);
            Vector3.Transform(updateBuffer, ref xform, updateBuffer);
            positions.Clear();
            foreach (var pos in updateBuffer)
                positions.Add(pos);

            foreach (var geom in geometry)
            {
                // Transform all vectors too
                var geomBuffer = new Vector3[geom.Vertices.VertexCount];
                foreach (var channel in geom.Vertices.Channels)
                {
                    if (channel.Name.StartsWith("Normal") ||
                        channel.Name.StartsWith("Binormal") ||
                        channel.Name.StartsWith("Tangent"))
                    {
                        channel.Items.CopyTo(geomBuffer, 0);
                        Vector3.TransformNormal(geomBuffer, ref xform, geomBuffer);
                        channel.Items.Clear();
                        foreach (var item in geomBuffer)
                            channel.Items.Add(item);
                    }
                }
            }
        }
    }
}
