// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties and methods that define various aspects of a mesh.
    /// </summary>
    public class MeshContent : NodeContent
    {
        /// <summary>
        /// Gets the list of geometry batches for the mesh.
        /// </summary>
        public GeometryContentCollection Geometry { get; }

        /// <summary>
        /// Gets the list of vertex position values.
        /// </summary>
        public PositionCollection Positions { get; }

        /// <summary>
        /// Initializes a new instance of MeshContent.
        /// </summary>
        public MeshContent()
        {
            Geometry = new GeometryContentCollection(this);
            Positions = [];
        }

        /// <summary>
        /// Applies a transform directly to position and normal channels. Node transforms are unaffected.
        /// </summary>
        internal void TransformContents(ref Matrix xform)
        {
            // Transform positions
            for (int i = 0; i < Positions.Count; i++)
                Positions[i] = Vector3.Transform(Positions[i], xform);

            // Transform all vectors too:
            // Normals are "tangent covectors", which need to be transformed using the
            // transpose of the inverse matrix!
            var inverseTranspose = Matrix.Transpose(Matrix.Invert(xform));
            foreach (var geom in Geometry)
            {
                foreach (var channel in geom.Vertices.Channels)
                {
                    if (channel is not VertexChannel<Vector3> vector3Channel)
                        continue;

                    if (channel.Name.StartsWith("Normal") ||
                        channel.Name.StartsWith("Binormal") ||
                        channel.Name.StartsWith("Tangent"))
                    {
                        for (int i = 0; i < vector3Channel.Count; i++)
                        {
                            var normal = vector3Channel[i];
                            Vector3.TransformNormal(ref normal, ref inverseTranspose, out normal);
                            Vector3.Normalize(ref normal, out normal);
                            vector3Channel[i] = normal;
                        }
                    }
                }
            }

            // Swap winding order when faces are mirrored.
            if (MeshHelper.IsLeftHanded(ref xform))
                MeshHelper.SwapWindingOrder(this);
        }
    }
}
