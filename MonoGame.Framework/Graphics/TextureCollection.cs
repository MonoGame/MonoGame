using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class TextureCollection
    {
        private readonly Texture[] _textures;

        private int _dirty;

        internal TextureCollection(int maxTextures)
        {
            _textures = new Texture[maxTextures];
            _dirty = int.MaxValue;
        }

        public Texture this[int index]
        {
            get { return _textures[index]; }
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

            _dirty = int.MaxValue;
        }

#if DIRECTX

        internal void SetTextures(GraphicsDevice device)
        {
            // Skip out if nothing has changed.
            if (_dirty == 0)
                return;

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            var pixelShaderStage = device._d3dContext.PixelShader;
            for (var i = 0; i < _textures.Length; i++)
            {
                var mask = 1 << i;
                if ((_dirty & mask) == 0)
                    continue;

                if (_textures[i] == null)
                    pixelShaderStage.SetShaderResource(i, null);
                else
                    pixelShaderStage.SetShaderResource(i, _textures[i].GetShaderResourceView());

                _dirty &= ~mask;
                if (_dirty == 0)
                    break;
            }

            _dirty = 0;
        }

#endif
    }
}
