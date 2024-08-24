// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

#if WEB
        internal void SetConstantBuffers(GraphicsDevice device, int shaderProgram)
#elif OPENGL
        internal void SetConstantBuffers(GraphicsDevice device, ShaderProgram shaderProgram)
#else
        internal void SetConstantBuffers(GraphicsDevice device)
#endif
        {
            // If there are no constant buffers then skip it.
            if (_valid == 0)
                return;

            var valid = _valid;

            for (var i = 0; i < _buffers.Length; i++)
            {
                var buffer = _buffers[i];
                if (buffer != null && !buffer.IsDisposed)
                {
#if OPENGL || WEB
                    buffer.PlatformApply(device, shaderProgram);
#else
                    buffer.PlatformApply(device, _stage, i);
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
