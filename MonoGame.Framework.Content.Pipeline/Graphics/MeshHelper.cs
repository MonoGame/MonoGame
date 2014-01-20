using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class MeshHelper
    {
        static bool IsFinite(float v)
        {
            return !float.IsInfinity(v) && !float.IsNaN(v);
        }

        static bool IsFinite(this Vector3 v)
        {
            return IsFinite(v.X) && IsFinite(v.Y) && IsFinite(v.Z);
        }

        public static void CalculateNormals(MeshContent mesh, bool overwriteExistingNormals)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generate the tangents and binormals (tangent frames) for each vertex in the mesh.
        /// </summary>
        /// <param name="mesh">The mesh which will have add tangent and binormal channels added.</param>
        /// <param name="textureCoordinateChannelName">The Vector2 texture coordinate channel used to generate tangent frames.</param>
        /// <param name="tangentChannelName"></param>
        /// <param name="binormalChannelName"></param>
        public static void CalculateTangentFrames(MeshContent mesh, string textureCoordinateChannelName, string tangentChannelName, string binormalChannelName)
        {
            foreach (var geom in mesh.Geometry)
                CalculateTangentFrames(geom, textureCoordinateChannelName, tangentChannelName, binormalChannelName);                            
        }

        public static void CalculateTangentFrames(GeometryContent geom, string textureCoordinateChannelName, string tangentChannelName, string binormalChannelName)
        {
            var verts = geom.Vertices;
            var indices = geom.Indices;
            var channels = geom.Vertices.Channels;

            var normals = channels.Get<Vector3>(VertexChannelNames.Normal(0));
            var uvs = channels.Get<Vector2>(textureCoordinateChannelName);

            Vector3[] tangents, bitangents;
            CalculateTangentFrames(verts.Positions, indices, normals, uvs, out tangents, out bitangents);

            // All the indices are 1:1 with the others, so we 
            // can just add the new channels in place.

            if (!string.IsNullOrEmpty(tangentChannelName))
                channels.Add(tangentChannelName, tangents);

            if (!string.IsNullOrEmpty(binormalChannelName))
                channels.Add(binormalChannelName, bitangents);
        }

        public static void CalculateTangentFrames(IList<Vector3> positions,
                                                  IList<int> indices,
                                                  IList<Vector3> normals,
                                                  IList<Vector2> textureCoords,
                                                  out Vector3[] tangents,
                                                  out Vector3[] bitangents)
        {
            // Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. 
            // Terathon Software 3D Graphics Library, 2001.
            // http://www.terathon.com/code/tangent.html

            // Hegde, Siddharth. "Messing with Tangent Space". Gamasutra, 2007. 
            // http://www.gamasutra.com/view/feature/129939/messing_with_tangent_space.php

            var numVerts = positions.Count;
            var numIndices = indices.Count;

            var tan1 = new Vector3[numVerts];
            var tan2 = new Vector3[numVerts];

            for (var index = 0; index < numIndices; index += 3)
            {
                var i1 = indices[index + 0];
                var i2 = indices[index + 1];
                var i3 = indices[index + 2];

                var w1 = textureCoords[i1];
                var w2 = textureCoords[i2];
                var w3 = textureCoords[i3];

                var s1 = w2.X - w1.X;
                var s2 = w3.X - w1.X;
                var t1 = w2.Y - w1.Y;
                var t2 = w3.Y - w1.Y;

                var denom = s1 * t2 - s2 * t1;
                if (Math.Abs(denom) < float.Epsilon)
                {
                    // The triangle UVs are zero sized one dimension.
                    //
                    // So we cannot calculate the vertex tangents for this
                    // one trangle, but maybe it can with other trangles.
                    continue;
                }

                var r = 1.0f / denom;
                Debug.Assert(IsFinite(r), "Bad r!");

                var v1 = positions[i1];
                var v2 = positions[i2];
                var v3 = positions[i3];

                var x1 = v2.X - v1.X;
                var x2 = v3.X - v1.X;
                var y1 = v2.Y - v1.Y;
                var y2 = v3.Y - v1.Y;
                var z1 = v2.Z - v1.Z;
                var z2 = v3.Z - v1.Z;

                var sdir = new Vector3()
                {
                    X = (t2 * x1 - t1 * x2) * r,
                    Y = (t2 * y1 - t1 * y2) * r,
                    Z = (t2 * z1 - t1 * z2) * r,
                };

                var tdir = new Vector3()
                {
                    X = (s1 * x2 - s2 * x1) * r,
                    Y = (s1 * y2 - s2 * y1) * r,
                    Z = (s1 * z2 - s2 * z1) * r,
                };

                tan1[i1] += sdir;
                Debug.Assert(tan1[i1].IsFinite(), "Bad tan1[i1]!");
                tan1[i2] += sdir;
                Debug.Assert(tan1[i2].IsFinite(), "Bad tan1[i2]!");
                tan1[i3] += sdir;
                Debug.Assert(tan1[i3].IsFinite(), "Bad tan1[i3]!");

                tan2[i1] += tdir;
                Debug.Assert(tan2[i1].IsFinite(), "Bad tan2[i1]!");
                tan2[i2] += tdir;
                Debug.Assert(tan2[i2].IsFinite(), "Bad tan2[i2]!");
                tan2[i3] += tdir;
                Debug.Assert(tan2[i3].IsFinite(), "Bad tan2[i3]!");
            }

            tangents = new Vector3[numVerts];
            bitangents = new Vector3[numVerts];

            // At this point we have all the vectors accumulated, but we need to average
            // them all out. So we loop through all the final verts and do a Gram-Schmidt
            // orthonormalize, then make sure they're all unit length.
            for (var i = 0; i < numVerts; i++)
            {
                var n = normals[i];
                Debug.Assert(n.IsFinite(), "Bad normal!");
                Debug.Assert(n.Length() >= 0.9999f, "Bad normal!");

                var t = tan1[i];
                if (t.LengthSquared() < float.Epsilon)
                {
                    // TODO: Ideally we could spit out a warning to the
                    // content logging here!

                    // We couldn't find a good tanget for this vertex.
                    //
                    // Rather than set them to zero which could produce
                    // errors in other parts of the pipeline, we just take        
                    // a guess at something that may look ok.

                    t = Vector3.Cross(n, Vector3.UnitX);
                    if (t.LengthSquared() < float.Epsilon)
                        t = Vector3.Cross(n, Vector3.UnitY);

                    tangents[i] = Vector3.Normalize(t);
                    bitangents[i] = Vector3.Cross(n, tangents[i]);
                    continue;
                }

                // Gram-Schmidt orthogonalize
                // TODO: This can be zero can cause NaNs on 
                // normalize... how do we fix this?
                var tangent = t - n * Vector3.Dot(n, t);
                tangent = Vector3.Normalize(tangent);
                Debug.Assert(tangent.IsFinite(), "Bad tangent!");
                tangents[i] = tangent;

                // Calculate handedness
                var w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0F) ? -1.0F : 1.0F;
                Debug.Assert(IsFinite(w), "Bad handedness!");

                // Calculate the bitangent
                var bitangent = Vector3.Cross(n, tangent) * w;
                Debug.Assert(bitangent.IsFinite(), "Bad bitangent!");
                bitangents[i] = bitangent;
            }
        }

        /// <summary>
        /// Search for the root bone of the skeletion.
        /// </summary>
        /// <param name="node">The node from which to begin the search for the skeleton.</param>
        /// <returns>The root bone of the skeletion or null if none is found.</returns>
        public static BoneContent FindSkeleton(NodeContent node)
        {
            // We should always get a node to search!
            if (node == null)
                throw new ArgumentNullException("node");

            // Search up thru the hierarchy.
            for (; node != null; node = node.Parent)
            {
                // First if this node is a bone then search up for the root.
                var root = node as BoneContent;
                if (root != null)
                {
                    while (root.Parent is BoneContent)
                        root = (BoneContent)root.Parent;
                    return root;
                }

                // Next try searching the children for a root bone.
                foreach (var nodeContent in node.Children)
                {
                    var bone = nodeContent as BoneContent;
                    if (bone == null) 
                        continue;

                    // If we found a bone
                    if (root != null)
                        throw new InvalidContentException("DuplicateSkeleton", node.Identity);

                    // This is our new root.
                    root = bone;
                }

                // If we found a root bone then return it, else
                // we continue the search to the node parent.
                if (root != null)
                    return root;
            }

            // We didn't find any bones!
            return null;
        }

        public static IList<BoneContent> FlattenSkeleton(BoneContent skeleton)
        {
            throw new NotImplementedException();
        }

        public static void MergeDuplicatePositions(MeshContent mesh, float tolerance)
        {
            throw new NotImplementedException();
        }

        public static void MergeDuplicateVertices(GeometryContent geometry)
        {
            throw new NotImplementedException();
        }

        public static void MergeDuplicateVertices(MeshContent mesh)
        {
            throw new NotImplementedException();
        }

        public static void OptimizeForCache(MeshContent mesh)
        {
            // We don't throw here as non-optimized still works.
        }
        
        /// <summary>
        /// Reverses the triangle winding order of the mesh.
        /// </summary>
        /// <param name="mesh">The mesh which will be modified.</param>
        /// <remarks>
        /// This method is useful when changing the direction of backface culling
        /// like when switching between left/right handed coordinate systems.
        /// </remarks>
        public static void SwapWindingOrder(MeshContent mesh)
        {
            // Gotta have a mesh to run!
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            foreach (var geom in mesh.Geometry)
            {
                for (var i = 0; i < geom.Indices.Count; i += 3)
                {
                    var first = geom.Indices[i];
                    var last = geom.Indices[i+2];
                    geom.Indices[i] = last;
                    geom.Indices[i+2] = first;
                }
            }
        }

        public static void TransformScene(NodeContent scene, Matrix transform)
        {
            throw new NotImplementedException();
        }
    }
}
