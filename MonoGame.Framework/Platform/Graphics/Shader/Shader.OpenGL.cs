// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Diagnostics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class Shader
    {
        // The shader handle.
        private int _shaderHandle = -1;

        // We keep this around for recompiling on context lost and debugging.
        private string _glslCode;

        public uint FragmentOutputMask;

        private static int PlatformProfile()
        {
            return 0;
        }

        private static uint GetFragmentOutputMask(string glslCode)
        {
            uint fragmentOutputMask = 0;

            // GraphicsDevice supports up to 8 simultaneous render targets.
            // gl_FragColor and gl_FragData[0] both map to fragment output 0.
            if (glslCode.Contains("gl_FragColor", StringComparison.Ordinal) ||
                glslCode.Contains("gl_FragData[0]", StringComparison.Ordinal))
                fragmentOutputMask |= 1u;

            if (glslCode.Contains("gl_FragData[1]", StringComparison.Ordinal))
                fragmentOutputMask |= 1u << 1;

            if (glslCode.Contains("gl_FragData[2]", StringComparison.Ordinal))
                fragmentOutputMask |= 1u << 2;

            if (glslCode.Contains("gl_FragData[3]", StringComparison.Ordinal))
                fragmentOutputMask |= 1u << 3;

            if (glslCode.Contains("gl_FragData[4]", StringComparison.Ordinal))
                fragmentOutputMask |= 1u << 4;

            if (glslCode.Contains("gl_FragData[5]", StringComparison.Ordinal))
                fragmentOutputMask |= 1u << 5;

            if (glslCode.Contains("gl_FragData[6]", StringComparison.Ordinal))
                fragmentOutputMask |= 1u << 6;

            if (glslCode.Contains("gl_FragData[7]", StringComparison.Ordinal))
                fragmentOutputMask |= 1u << 7;

            return fragmentOutputMask;
        }

        private void PlatformConstruct(ShaderStage stage, byte[] shaderBytecode)
        {
            _glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);
            
            FragmentOutputMask = stage == ShaderStage.Pixel
                                 ? GetFragmentOutputMask(_glslCode)
                                 : 0u;

            HashKey = MonoGame.Framework.Utilities.Hash.ComputeHash(shaderBytecode);
        }

        internal int GetShaderHandle()
        {
            // If the shader has already been created then return it.
            if (_shaderHandle != -1)
                return _shaderHandle;
            
            //
            _shaderHandle = GL.CreateShader(Stage == ShaderStage.Vertex ? ShaderType.VertexShader : ShaderType.FragmentShader);
            GraphicsExtensions.CheckGLError();
            GL.ShaderSource(_shaderHandle, _glslCode);
            GraphicsExtensions.CheckGLError();
            GL.CompileShader(_shaderHandle);
            GraphicsExtensions.CheckGLError();
            int compiled = 0;
            GL.GetShader(_shaderHandle, ShaderParameter.CompileStatus, out compiled);
            GraphicsExtensions.CheckGLError();
            if (compiled != (int)Bool.True)
            {
                var errorLog = GL.GetShaderInfoLog(_shaderHandle);

                GraphicsDevice.DisposeShader(_shaderHandle);
                _shaderHandle = -1;

                throw new ShaderCompilerException(SourceFile, Entrypoint, Stage, errorLog, _glslCode);
            }

            return _shaderHandle;
        }

        internal void GetVertexAttributeLocations(int program)
        {
            for (int i = 0; i < Attributes.Length; ++i)
            {
                Attributes[i].location = GL.GetAttribLocation(program, Attributes[i].name);
                GraphicsExtensions.CheckGLError();
            }
        }

        internal int GetAttribLocation(VertexElementUsage usage, int index)
        {
            for (int i = 0; i < Attributes.Length; ++i)
            {
                if ((Attributes[i].usage == usage) && (Attributes[i].index == index))
                    return Attributes[i].location;
            }
            return -1;
        }

        internal void ApplySamplerTextureUnits(int program)
        {
            // Assign the texture unit index to the sampler uniforms.
            foreach (var sampler in Samplers)
            {
                var loc = GL.GetUniformLocation(program, sampler.name);
                GraphicsExtensions.CheckGLError();
                if (loc != -1)
                {
                    GL.Uniform1(loc, sampler.textureSlot);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        private void PlatformGraphicsDeviceResetting()
        {
            if (_shaderHandle != -1)
            {
                GraphicsDevice.DisposeShader(_shaderHandle);
                _shaderHandle = -1;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && _shaderHandle != -1)
            {
                GraphicsDevice.DisposeShader(_shaderHandle);
                _shaderHandle = -1;
            }

            base.Dispose(disposing);
        }
    }
}
