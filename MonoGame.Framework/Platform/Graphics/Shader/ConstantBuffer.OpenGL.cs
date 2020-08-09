// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class ConstantBuffer
    {
        private ShaderProgram _shaderProgram = null;
        private int _glBuffer = -1;

        static ConstantBuffer _lastConstantBufferApplied = null;

        private void PlatformInitialize()
        {
            GL.GenBuffers(1, out _glBuffer);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformClear()
        {
            if (_glBuffer >= 0)
            {
                GL.DeleteBuffers(1, ref _glBuffer);
                GraphicsExtensions.CheckGLError();
            }

            _glBuffer = -1;
            _shaderProgram = null;
        }

        public unsafe void PlatformApply(GraphicsDevice device, ShaderProgram program)
        {
            // NOTE: We assume here the program has 
            // already been set on the device.

            // If the program changed then apply the state.
            if (_shaderProgram != program)
            {
                _shaderProgram = program;
                _dirty = true;
            }

            // If the shader program is the same, the effect may still be different and have different values in the buffer
            if (!Object.ReferenceEquals(this, _lastConstantBufferApplied))
                _dirty = true;

            // If the buffer content hasn't changed then we're
            // done... use the previously set uniform state.
            if (!_dirty)
                return;

            int uniformBlockIndex = GL.GetUniformBlockIndex(program.Program, _name);
            GraphicsExtensions.CheckGLError();

            if (uniformBlockIndex >= 0)
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, _glBuffer);
                GraphicsExtensions.CheckGLError();

                fixed (byte* bytePtr = _buffer)
                {
                    GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)_buffer.Length, (IntPtr)bytePtr, BufferUsageHint.StreamDraw);
                    GraphicsExtensions.CheckGLError();
                }

                GL.BindBufferBase(BufferTarget.UniformBuffer, uniformBlockIndex, _glBuffer);
                GraphicsExtensions.CheckGLError();
            }

            // Clear the dirty flag.
            _dirty = false;

            _lastConstantBufferApplied = this;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                PlatformClear();

            base.Dispose(disposing);
        }
    }
}
