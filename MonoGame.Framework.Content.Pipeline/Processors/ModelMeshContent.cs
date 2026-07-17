// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Provides methods and properties for loading model mesh data.
    /// </summary>
    public sealed class ModelMeshContent
    {
        internal ModelMeshContent(string? name, MeshContent sourceMesh, ModelBoneContent parentBone, BoundingSphere boundingSphere,
            IList<ModelMeshPartContent> meshParts)
        {
            Name = name;
            SourceMesh = sourceMesh;
            ParentBone = parentBone;
            BoundingSphere = boundingSphere;
            MeshParts = new ModelMeshPartContentCollection(meshParts);
        }

        /// <summary>
        /// Gets the bounding sphere of the mesh.
        /// </summary>
        public BoundingSphere BoundingSphere { get; }

        /// <summary>
        /// Gets the collection of mesh parts contained in this mesh.
        /// </summary>
        public ModelMeshPartContentCollection MeshParts { get; }

        /// <summary>
        /// Gets the name of the mesh.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the parent bone model.
        /// </summary>
        public ModelBoneContent ParentBone { get; }

        /// <summary>
        /// Gets the source mesh.
        /// </summary>
        public MeshContent SourceMesh { get; }

        /// <summary>
        /// Gets or sets the tag associated with the mesh.
        /// </summary>
        public object? Tag { get; set; }
    }
}
