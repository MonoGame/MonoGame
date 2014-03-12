using System;

#if MONOMAC
using MonoMac.OpenGL;
#elif (WINDOWS && !DIRECTX) || LINUX
using OpenTK.Graphics.OpenGL;
#elif PSM
using Sce.PlayStation.Core.Graphics;
#elif GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    internal sealed class ConstantBufferCollection
    {
        private readonly ConstantBuffer[] _buffers;

        private ShaderStage _stage;
        private ShaderStage Stage { get { return this._stage; } }

        private int _valid;

        internal ConstantBufferCollection(ShaderStage stage, int maxBuffers)
        {
            _stage = stage;
            _buffers = new ConstantBuffer[maxBuffers];
            _valid = 0;
        }

        public ConstantBuffer this[int index]
        {
            get { return _buffers[index]; }
            set
            {
                if (_buffers[index] == value)
                    return;

                if (value != null)
                {
                    _buffers[index] = value;
                    _valid |= 1 << index;
                }
                else
                {
                    _buffers[index] = null;
                    _valid &= ~(1 << index);
                }
            }
        }

        internal void Clear()
        {
            for (var i = 0; i < _buffers.Length; i++)
                _buffers[i] = null;

            _valid = 0;
        }

#if DIRECTX
        internal void SetConstantBuffers(GraphicsDevice device)
#elif OPENGL || PSM
        internal void SetConstantBuffers(GraphicsDevice device, int shaderProgram)
#endif
        {
            // If there are no constant buffers then skip it.
            if (_valid == 0)
                return;

            var valid = _valid;

            for (var i = 0; i < _buffers.Length; i++)
            {
                var buffer = _buffers[i];
                if (buffer != null)
                {
#if DIRECTX
                    buffer.PlatformApply(device, _stage, i);
#elif OPENGL || PSM
                    buffer.PlatformApply(device, shaderProgram);
#endif
                }

                // Early out if this is the last one.
                valid &= ~(1 << i);
                if (valid == 0)
                    return;
            }
        }

    }
}
