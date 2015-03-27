// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {
        private readonly Texture[] _textures;
        private readonly bool _applyToVertexStage;
        private int _dirty;

        internal TextureCollection(int maxTextures, bool applyToVertexStage)
        {
            _textures = new Texture[maxTextures];
            _applyToVertexStage = applyToVertexStage;
            _dirty = int.MaxValue;
            PlatformInit();
        }

        public Texture this[int index]
        {
            get
            {
                return _textures[index];
            }
            set
            {
                if (_textures[index] == value)
                    return;

                _textures[index] = value;
                _dirty |= 1 << index;
            }
        }

        internal void Clear()
        {
            for (var i = 0; i < _textures.Length; i++)
                _textures[i] = null;

            PlatformClear();
            _dirty = int.MaxValue;
        }

        /// <summary>
        /// Marks all texture slots as dirty.
        /// </summary>
        internal void Dirty()
        {
            _dirty = int.MaxValue;
        }

        internal void SetTextures(GraphicsDevice device)
        {
            PlatformSetTextures(device);
        }
    }
}
