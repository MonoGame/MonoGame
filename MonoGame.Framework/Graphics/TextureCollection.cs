using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using TextureUnit = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class TextureCollection
    {
        private readonly Texture[] _textures;

#if OPENGL
        private readonly TextureTarget[] _targets;
#endif

        private int _dirty;

        internal TextureCollection(int maxTextures)
        {
            _textures = new Texture[maxTextures];
            _dirty = int.MaxValue;
#if OPENGL
            _targets = new TextureTarget[maxTextures];
#endif
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
            {
                _textures[i] = null;
#if OPENGL
                _targets[i] = 0;
#endif
            }

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
#if !WINRT
            Threading.EnsureUIThread();
#endif

            // Skip out if nothing has changed.
            if (_dirty == 0)
                return;

#if DIRECTX
            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.
            var pixelShaderStage = device._d3dContext.PixelShader;
#endif

            for (var i = 0; i < _textures.Length; i++)
            {
                var mask = 1 << i;
                if ((_dirty & mask) == 0)
                    continue;

                var tex = _textures[i];
#if OPENGL
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GraphicsExtensions.CheckGLError();

                // Clear the previous binding if the 
                // target is different from the new one.
                if (_targets[i] != 0 && (tex == null || _targets[i] != tex.glTarget))
                {
                    GL.BindTexture(_targets[i], 0);
                    GraphicsExtensions.CheckGLError();
                }

                if (tex != null)
                {
                    _targets[i] = tex.glTarget;
                    GL.BindTexture(tex.glTarget, tex.glTexture);
                    GraphicsExtensions.CheckGLError();
                }
#elif DIRECTX
                if (_textures[i] == null)
                    pixelShaderStage.SetShaderResource(i, null);
                else
                    pixelShaderStage.SetShaderResource(i, _textures[i].GetShaderResourceView());
#endif

                _dirty &= ~mask;
                if (_dirty == 0)
                    break;
            }

            _dirty = 0;
        }

    }
}
