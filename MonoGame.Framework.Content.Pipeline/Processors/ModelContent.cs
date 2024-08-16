// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    public sealed class ModelContent
    {
        private ModelBoneContentCollection _bones;
        private ModelMeshContentCollection _meshes;
        private ModelBoneContent _root;

        internal ModelContent() { }

        internal ModelContent(ModelBoneContent root, IList<ModelBoneContent> bones, IList<ModelMeshContent> meshes)
        {
            _root = root;
            _bones = new ModelBoneContentCollection(bones);
            _meshes = new ModelMeshContentCollection(meshes);
        }

        public ModelBoneContentCollection Bones
        {
            get { return _bones; }
        }

        public ModelMeshContentCollection Meshes
        {
            get { return _meshes; }
        }

        public ModelBoneContent Root
        {
            get { return _root; }
        }

        public object Tag { get; set; }
    }
}
