// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class BufferResourceCollection
    {
        struct BufferInfo
        {
            public BufferResource buffer;
            public string bufferName;
        }

        private readonly BufferInfo[] _readonlyBuffers;
        private readonly BufferInfo[] _writeableBuffers;

        private ShaderStage _stage;

        public int MaxReadleBuffers { get { return _readonlyBuffers.Length; } }
        public int MaxWriteableBuffers { get { return _writeableBuffers.Length; } }

        internal BufferResourceCollection(ShaderStage stage, int maxReadableBuffers, int maxWriteableBuffers)
		{
            _stage = stage;

            _readonlyBuffers = new BufferInfo[maxReadableBuffers];
            _writeableBuffers = new BufferInfo[maxWriteableBuffers];
        }

        public void SetBufferAtIndex(BufferResource buffer, string bufferName, int index, bool writeAccess)
        {
            if (writeAccess && _stage != ShaderStage.Compute)
                throw new ArgumentException("Only a compute shader can use RWStructuredBuffer currently. Uae a regular StructuredBuffer instead and assign it the same buffer.");

            var buffers = writeAccess ? _writeableBuffers : _readonlyBuffers;

            buffers[index] = new BufferInfo
            {
                buffer = buffer,
                bufferName = bufferName,
            };
        }

        internal void Clear()
        {
            for (var i = 0; i < _readonlyBuffers.Length; i++)
                _readonlyBuffers[i] = new BufferInfo();

            for (var i = 0; i < _writeableBuffers.Length; i++)
                _writeableBuffers[i] = new BufferInfo();
        }

#if WEB
        internal void SetBufferResources(GraphicsDevice device, int shaderProgram)
#elif OPENGL
        internal void SetBufferResources(GraphicsDevice device, ShaderProgram shaderProgram)
#else
        internal void SetBufferResources(GraphicsDevice device)
#endif
        {
            for (var i = 0; i < _readonlyBuffers.Length; i++)
            {
                var buffer = _readonlyBuffers[i].buffer;
                if (buffer != null && !buffer.IsDisposed)
                {
#if OPENGL || WEB
                    buffer.PlatformApply(device, shaderProgram, _readonlyBuffers[i].bufferName, i, false);
#else
                    buffer.PlatformApply(device, _stage, i, false);
#endif
                }
            }

            for (var i = 0; i < _writeableBuffers.Length; i++)
            {
                var buffer = _writeableBuffers[i].buffer;
                if (buffer != null && !buffer.IsDisposed)
                {
#if OPENGL || WEB
                    buffer.PlatformApply(device, shaderProgram, _writeableBuffers[i].bufferName, i, true);
#else
                    buffer.PlatformApply(device, _stage, i, true);
#endif
                }
            }
        }
    }
}
