// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    public sealed class ModelBoneContent
    {
        private ModelBoneContentCollection _children;
        private int _index;
        private string _name;
        private ModelBoneContent _parent;
        private Matrix _transform;

        internal ModelBoneContent() { }

        internal ModelBoneContent(string name, int index, Matrix transform, ModelBoneContent parent)
        {
            _name = name;
            _index = index;
            _transform = transform;
            _parent = parent;
        }

        public ModelBoneContentCollection Children
        {
            get { return _children; }
            internal set { _children = value; }
        }

        public int Index
        {
            get { return _index; }
        }

        public string Name
        {
            get { return _name; }
        }

        public ModelBoneContent Parent
        {
            get { return _parent; }
        }

        public Matrix Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }
    }
}
