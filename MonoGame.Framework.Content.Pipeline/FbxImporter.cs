// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading AutoDesk (.fbx) files for use in the Content Pipeline.
    /// </summary>
    /// <remarks>
    /// Since OpenAssetImporter supports lots of formats, there's little that stands in the
    /// way of adding more file extensions to the importer attribute and suporting more.
    /// </remarks>
    [ContentImporter(".fbx", DisplayName = "Fbx Importer - MonoGame", DefaultProcessor = "ModelProcessor")]
    public class FbxImporter : ContentImporter<NodeContent>
    {
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            var identity = new ContentIdentity(filename, GetType().Name);
            var importer = new AssimpImporter();
            importer.AttachLogStream(new LogStream((msg, userData) => context.Logger.LogMessage(msg)));
            var scene = importer.ImportFile(filename,
                                            PostProcessSteps.FlipUVs | // So far appears necessary
                                            PostProcessSteps.JoinIdenticalVertices |
                                            PostProcessSteps.Triangulate |
                                            PostProcessSteps.SortByPrimitiveType |
                                            PostProcessSteps.FindInvalidData
                );

            var rootNode = new NodeContent
            {
                Name = scene.RootNode.Name,
                Identity = identity,
                Transform = ToXna(scene.RootNode.Transform)
            };

            // TODO: Materials
            var materials = new List<MaterialContent>();
            foreach (var sceneMaterial in scene.Materials)
            {
                var diffuse = sceneMaterial.GetTexture(TextureType.Diffuse, 0);

                materials.Add(new BasicMaterialContent()
                {
                    Name = sceneMaterial.Name,
                    Identity = identity,
                    Texture = new ExternalReference<TextureContent>(diffuse.FilePath, identity)
                });
            }

            // Meshes
            var meshes = new Dictionary<Mesh, MeshContent>();
            foreach (var sceneMesh in scene.Meshes)
            {
                if (!sceneMesh.HasVertices)
                    continue;

                var mesh = new MeshContent
                    {
                        Name = sceneMesh.Name
                    };

                // Position vertices are shared at the mesh level
                foreach (var vert in sceneMesh.Vertices)
                    mesh.Positions.Add(new Vector3(vert.X, vert.Y, vert.Z));

                var geom = new GeometryContent
                    {
                        Name = string.Empty,
                        //Material = materials[sceneMesh.MaterialIndex]
                    };

                // Geometry vertices reference 1:1 with the MeshContent parent,
                // no indirection is necessary.
                geom.Vertices.Positions.AddRange(mesh.Positions);
                geom.Vertices.AddRange(Enumerable.Range(0, sceneMesh.VertexCount));
                geom.Indices.AddRange(sceneMesh.GetIntIndices());

                // Individual channels go here
                if (sceneMesh.HasNormals)
                    geom.Vertices.Channels.Add(VertexChannelNames.Normal(), ToXna(sceneMesh.Normals));

                for (var i = 0; i < sceneMesh.TextureCoordsChannelCount; i++)
                    geom.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(i),
                                               ToXnaVector2(sceneMesh.GetTextureCoords(i)));

                mesh.Geometry.Add(geom);
                rootNode.Children.Add(mesh);
                meshes.Add(sceneMesh, mesh);
            }

            // Bones
            var bones = new Dictionary<Node, BoneContent>();
            var hierarchyNodes = scene.RootNode.Children.SelectDeep(n => n.Children).ToList();
            foreach (var node in hierarchyNodes)
            {
                var bone = new BoneContent
                    {
                        Name = node.Name,
                        Transform = Matrix.Transpose(ToXna(node.Transform))
                    };

                if (node.Parent == scene.RootNode)
                    rootNode.Children.Add(bone);
                else
                {
                    var parent = bones[node.Parent];
                    parent.Children.Add(bone);
                }

                // Copy the bone's name to the MeshContent - this appears to be
                // the way it comes out of XNA's FBXImporter.
                foreach (var meshIndex in node.MeshIndices)
                    meshes[scene.Meshes[meshIndex]].Name = node.Name;

                bones.Add(node, bone);
            }

            return rootNode;
        }

        public static Matrix ToXna(Matrix4x4 matrix)
        {
            var result = Matrix.Identity;

            result.M11 = matrix.A1;
            result.M12 = matrix.A2;
            result.M13 = matrix.A3;
            result.M14 = matrix.A4;

            result.M21 = matrix.B1;
            result.M22 = matrix.B2;
            result.M23 = matrix.B3;
            result.M24 = matrix.B4;

            result.M31 = matrix.C1;
            result.M32 = matrix.C2;
            result.M33 = matrix.C3;
            result.M34 = matrix.C4;

            result.M41 = matrix.D1;
            result.M42 = matrix.D2;
            result.M43 = matrix.D3;
            result.M44 = matrix.D4;

            return result;
        }

        public static Vector2[] ToXna(Vector2D[] vectors)
        {
            var result = new Vector2[vectors.Length];
            for (var i = 0; i < vectors.Length; i++)
                result[i] = new Vector2(vectors[i].X, vectors[i].Y);

            return result;
        }

        public static Vector2[] ToXnaVector2(Vector3D[] vectors)
        {
            var result = new Vector2[vectors.Length];
            for (var i = 0; i < vectors.Length; i++)
                result[i] = new Vector2(vectors[i].X, 1 - vectors[i].Y);

            return result;
        }

        public static Vector3[] ToXna(Vector3D[] vectors)
        {
            var result = new Vector3[vectors.Length];
            for (var i = 0; i < vectors.Length; i++)
                result[i] = new Vector3(vectors[i].X, vectors[i].Y, vectors[i].Z);

            return result;
        }
    }

    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns each element of a tree structure in heriarchical order.
        /// </summary>
        /// <typeparam name="T">The enumerated type.</typeparam>
        /// <param name="source">The enumeration to traverse.</param>
        /// <param name="selector">A function which returns the children of the element.</param>
        /// <returns>An IEnumerable whose elements are in tree structure heriarchical order.</returns>
        public static IEnumerable<T> SelectDeep<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            var stack = new Stack<T>(source.Reverse());
            while (stack.Count > 0)
            {
                // Return the next item on the stack.
                var item = stack.Pop();
                yield return item;

                // Get the children from this item.
                var children = selector(item);

                // If we have no children then skip it.
                if (children == null)
                    continue;

                // We're using a stack, so we need to push the
                // children on in reverse to get the correct order.
                foreach (var child in children.Reverse())
                    stack.Push(child);
            }
        }

        /// <summary>
        /// Returns an enumerable from a single element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}
