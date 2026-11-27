using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Model mesh builder
    /// </summary>
    public class ModelMeshBuilder
    {
        private readonly List<ModelMeshPart> _parts = new List<ModelMeshPart>();

        /// <summary>
        /// Parts of the model mesh
        /// </summary>
        public IReadOnlyList<ModelMeshPart> Parts => _parts;

        /// <summary>
        /// Adds a mesh part to the model mesh
        /// </summary>
        /// <param name="indexBuffer">The index buffer for this mesh part</param>
        /// <param name="startIndex">The location in the index array at which to start reading vertices</param>
        /// <param name="vertexBuffer">The vertex buffer for this mesh part</param>
        /// <param name="vertexOffset">The offset (in vertices) from the top of vertex buffer</param>
        /// <param name="numVertices">The number of vertices used during a draw call</param>
        /// <param name="primitiveCount">The number of primitives to render</param>
        /// <param name="boundingSphere">The bounding sphere that encompasses this mesh part</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void AddModelMeshPart(IndexBuffer indexBuffer, int startIndex,
            VertexBuffer vertexBuffer, int vertexOffset, int numVertices,
            int primitiveCount, BoundingSphere boundingSphere)
        {
            if (indexBuffer == null)
            {
                throw new ArgumentNullException(nameof(indexBuffer));
            }

            if (startIndex < 0 || startIndex >= indexBuffer.IndexCount)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (vertexBuffer == null)
            {
                throw new ArgumentNullException(nameof(vertexBuffer));
            }

            if (vertexOffset < 0 || vertexOffset >= vertexBuffer.VertexCount)
            {
                throw new ArgumentOutOfRangeException(nameof(vertexOffset));
            }

            if (numVertices <= 0 || numVertices > vertexBuffer.VertexCount)
            {
                throw new ArgumentOutOfRangeException(nameof(numVertices));
            }

            if (primitiveCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(primitiveCount));
            }

            var part = new ModelMeshPart()
            {
                IndexBuffer = indexBuffer,
                StartIndex = startIndex,
                VertexBuffer = vertexBuffer,
                VertexOffset = vertexOffset,
                NumVertices = numVertices,
                PrimitiveCount = primitiveCount,
                BoundingSphere = boundingSphere
            };

            _parts.Add(part);
        }

        /// <summary>
        /// Creates the model mesh
        /// </summary>
        /// <param name="device">Graphics Device</param>
        /// <returns></returns>
        public ModelMesh Create(GraphicsDevice device) => new ModelMesh(device, _parts);
    }

    /// <summary>
    /// Model bone descriptor
    /// </summary>
    public class ModelBoneDesc
    {
        /// <summary>
        /// Name of the model bone
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Transform of the model bone
        /// </summary>
        public Matrix Transform { get; set; }

        /// <summary>
        /// Meshes of the model bone
        /// </summary>
        public readonly List<ModelMesh> Meshes = new List<ModelMesh>();

        /// <summary>
        /// Children of the model bone
        /// </summary>
        public readonly List<ModelBoneDesc> Children = new List<ModelBoneDesc>();

        /// <summary>
        /// User assigned index of the bone. If null, it'll be assigned automatically
        /// </summary>
        public int? CustomIndex { get; set; }

        internal ModelBone Parent;

        /// <summary>
        /// Creates a new ModelBoneDesc
        /// </summary>
        /// <param name="name">name of the new bone</param>
        public ModelBoneDesc(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Grants ability to create a Model at the run-time
    /// </summary>
    public static class ModelBuilder
    {
        private class ModelBuilderContext
        {
            public readonly List<ModelBone> Bones = new List<ModelBone>();
            public readonly List<ModelMesh> Meshes = new List<ModelMesh>();
            public int CurrentIndex = 0;

            public void BuildModelRecursive(ModelBoneDesc desc)
            {
                var bone = new ModelBone
                {
                    Name = desc.Name,
                    Transform = desc.Transform,
                    Parent = desc.Parent
                };

                if (desc.CustomIndex == null)
                {
                    bone.Index = CurrentIndex;
                    ++CurrentIndex;
                }
                else
                {
                    bone.Index = desc.CustomIndex.Value;
                }

                foreach (var mesh in desc.Meshes)
                {
                    mesh.ParentBone = bone;
                    bone.Meshes.Add(mesh);

                    Meshes.Add(mesh);
                }

                Bones.Add(bone);

                // Add the new bone to the parent
                desc.Parent?.AddChild(bone);

                // Process children
                foreach (var child in desc.Children)
                {
                    child.Parent = bone;
                    BuildModelRecursive(child);
                }
            }
        }

        /// <summary>
        /// Creates the model
        /// </summary>
        /// <param name="root">Root bone of the model</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Model Create(ModelBoneDesc root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            var context = new ModelBuilderContext();
            context.BuildModelRecursive(root);

            // Create the model
            var model = new Model(context.Bones, context.Meshes)
            {
                Root = context.Bones[0]
            };

            return model;
        }
    }
}
