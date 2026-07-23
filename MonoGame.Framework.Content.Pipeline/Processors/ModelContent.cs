// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Provides properties and methods for managing model content.
    /// </summary>
    public sealed class ModelContent
    {
        internal ModelContent(ModelBoneContent root, IList<ModelBoneContent> bones, IList<ModelMeshContent> meshes)
        {
            Root = root;
            Bones = new ModelBoneContentCollection(bones);
            Meshes = new ModelMeshContentCollection(meshes);
        }

        /// <summary>
        /// Returns the bone content collection.
        /// </summary>
        public ModelBoneContentCollection Bones { get; }

        /// <summary>
        /// Returns the bone mesh collection.
        /// </summary>
        public ModelMeshContentCollection Meshes { get; }

        /// <summary>
        /// Returns the root bone.
        /// </summary>
        public ModelBoneContent Root { get; }

        /// <summary>
        /// Gets or sets an object that can be used to tag this model content.
        /// </summary>
        public object? Tag { get; set; }
    }
}
