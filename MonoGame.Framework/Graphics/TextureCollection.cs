using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class TextureCollection
    {
        private readonly Texture[] _textures;

        private bool _isDirty;

        internal TextureCollection(int maxTextures)
        {
            _textures = new Texture[maxTextures];
            _isDirty = false;
        }

        public Texture this[int index]
        {
            get { return _textures[index]; }
            set
            {
                if (_textures[index] == value)
                    return;

                _textures[index] = value;
                _isDirty = true;
            }
        }

        internal void Clear()
        {
            for (var i = 0; i < _textures.Length; i++)
                _textures[i] = null;

            _isDirty = true;
        }

#if DIRECTX

        internal void SetTextures(GraphicsDevice device)
        {
            // Skip out if nothing has changed.
            if (!_isDirty)
                return;

            var pixelShaderStage = device._d3dContext.PixelShader;
            for (var i = 0; i < _textures.Length; i++)
            {
                if (_textures[i] == null)
                    pixelShaderStage.SetShaderResource(i, null);
                else
                    pixelShaderStage.SetShaderResource(i, _textures[i].GetShaderResourceView());
            }

            // Clear the dirty flag.
            _isDirty = false;
        }

#endif
    }
}
