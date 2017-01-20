// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;
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
        private ShaderProgram _shaderProgram;
        private int _location;

        static ConstantBuffer _lastConstantBufferApplied;

        private EffectParameterClass _paramClass;
        private EffectParameterType _paramType;
        private int _arraySize;
        private int _vectorSize;

        private unsafe delegate void UpdateParamDelegate(int location, int size, byte* values);
        private UpdateParamDelegate _updateParam;

        private void PlatformInitialize()
        {
        }

        internal void InitializeParameterData(EffectParameter param)
        {
            // we need this to be 1 for scalars because that's what the glUniform* calls expect
            _arraySize = param.Elements.Count == 0 ? 1 : param.Elements.Count;
            _paramClass = param.ParameterClass;
            _paramType = param.ParameterType;
            _vectorSize = param.ColumnCount;

            unsafe
            {
                _updateParam = GetUpdateParam();
            }
        }

        private void PlatformClear()
        {
            // Force the uniform location to be looked up again
            _shaderProgram = null;
        }

        public void PlatformApply(GraphicsDevice device, ShaderProgram program)
        {
            // NOTE: We assume here the program has
            // already been set on the device.

            // If the program changed then lookup the
            // uniform again and apply the state.
            if (_shaderProgram != program)
            {
                var location = program.GetUniformLocation(_name);
                if (location == -1)
                    return;

                _shaderProgram = program;
                _location = location;
                _dirty = true;
            }

            // If the shader program is the same, the effect may still be different and have different values in the buffer
            if (!Object.ReferenceEquals(this, _lastConstantBufferApplied))
                _dirty = true;

            // If the buffer content hasn't changed then we're
            // done... use the previously set uniform state.
            if (!_dirty)
                return;

            unsafe
            {
                fixed (byte* ptr = _buffer)
                    _updateParam(_location, _arraySize, ptr);
            }

            // Clear the dirty flag.
            _dirty = false;

            _lastConstantBufferApplied = this;
        }

        private unsafe UpdateParamDelegate GetUpdateParam()
        {
            switch (_paramClass)
            {
                case EffectParameterClass.Scalar:
                    switch (_paramType)
                    {
                        case EffectParameterType.Bool:
                        case EffectParameterType.Int32:
                            return new UpdateParamDelegate(GL.Uniform1iv);
                        case EffectParameterType.Single:
                            return new UpdateParamDelegate(GL.Uniform1fv);
                    }
                    break;
                case EffectParameterClass.Vector:
                    switch (_vectorSize)
                    {
                        case 2:
                            switch (_paramType)
                            {
                                case EffectParameterType.Bool:
                                case EffectParameterType.Int32:
                                    return new UpdateParamDelegate(GL.Uniform2iv);
                                case EffectParameterType.Single:
                                    return new UpdateParamDelegate(GL.Uniform2fv);
                            }
                            break;
                        case 3:
                            switch (_paramType)
                            {
                                case EffectParameterType.Bool:
                                case EffectParameterType.Int32:
                                    return new UpdateParamDelegate(GL.Uniform3iv);
                                case EffectParameterType.Single:
                                    return new UpdateParamDelegate(GL.Uniform3fv);
                            }
                            break;
                        case 4:
                            switch (_paramType)
                            {
                                case EffectParameterType.Bool:
                                case EffectParameterType.Int32:
                                    return new UpdateParamDelegate(GL.Uniform4iv);
                                case EffectParameterType.Single:
                                    return new UpdateParamDelegate(GL.Uniform4fv);
                            }
                            break;
                    }
                    break;
                case EffectParameterClass.Matrix:
                    switch (_vectorSize)
                    {
                        case 2:
                            return GL.UniformMatrix2;
                        case 3:
                            return GL.UniformMatrix3;
                        case 4:
                            return GL.UniformMatrix4;
                    }
                    break;
            }
            throw new Exception("Did not find right delegate for parameter.");
        }
    }
}
