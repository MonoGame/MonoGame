// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
#endif
#elif DESKTOPGL
using OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class ConstantBuffer
    {
        private ShaderProgram _shaderProgram = null;
        private BufferedParam[] _bufferedParams;
        private int[] _locations;

        static ConstantBuffer _lastConstantBufferApplied = null;

        /// <summary>
        /// A hash value which can be used to compare constant buffers.
        /// </summary>
        internal int HashKey { get; private set; }

        private void PlatformInitialize()
        {
            var data = new byte[_parameters.Length];
            for (var i = 0; i < _parameters.Length; i++)
            {
                unchecked
                {
                    data[i] = (byte)(_parameters[i] | _offsets[i]);
                }
            }

            HashKey = MonoGame.Utilities.Hash.ComputeHash(data);

            _locations = new int[_bufferedParams.Length];
        }

        private void PlatformClear()
        {
            // Force the uniform location to be looked up again
            _shaderProgram = null;
        }

        public unsafe void PlatformApply(GraphicsDevice device, ShaderProgram program)
        {
            // NOTE: We assume here the program has 
            // already been set on the device.

            // If the program changed then lookup all
            // uniforms again and apply the state.
            if (_shaderProgram != program)
            {
                for (var i = 0; i < _bufferedParams.Length; i++)
                {
                    var param = _bufferedParams[i];
                    _locations[i] = program.GetUniformLocation(param.Name);
                }

                _shaderProgram = program;
                _dirty = true;
            }

            // If the shader program is the same, the effect may still be different and have different values in the buffer
            if (!ReferenceEquals(this, _lastConstantBufferApplied))
                _dirty = true;

            // If the buffer content hasn't changed then we're
            // done... use the previously set uniform state.
            if (!_dirty)
                return;

            // now we actually set the values
            var handle = GCHandle.Alloc(_buffer);
            var bufferPtr = handle.AddrOfPinnedObject();
            foreach (var param in _bufferedParams)
            {
                var ptr = bufferPtr + param.BufferOffset;
                switch (param.Type)
                {
                    
                }
                
            }
            handle.Free();
            fixed (byte* bytePtr = _buffer)
            {
                // TODO: We need to know the type of buffer float/int/bool
                // and cast this correctly... else it doesn't work as i guess
                // GL is checking the type of the uniform.

                GL.Uniform4(_location, _buffer.Length / 16, (float*)bytePtr);
                GraphicsExtensions.CheckGLError();
            }

            // Clear the dirty flag.
            _dirty = false;

            _lastConstantBufferApplied = this;
        }

        private class BufferedParam
        {
            public readonly string Name;
            public readonly int BufferOffset;
            public readonly int Size;

            public BufferedParam(string name, int bufferOffset, int size)
            {
                Name = name;
                BufferOffset = bufferOffset;
                Size = size;
            }

            //TODO
            public int Type { get; set; }
        }
    }
}
