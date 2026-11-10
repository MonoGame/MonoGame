// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Diagnostics;
using System.Text;
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

            var glslCode = _glslCode;
            if (GL.BoundApi == GL.RenderApi.ES && GraphicsDevice.glMajorVersion >= 3)
                glslCode = UpgradeEs2ShaderSourceToEs3(glslCode, Stage);

            GL.ShaderSource(_shaderHandle, glslCode);
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

                throw new ShaderCompilerException(SourceFile, Entrypoint, Stage, errorLog, glslCode);
            }

            return _shaderHandle;
        }

        private static string UpgradeEs2ShaderSourceToEs3(string glslCode, ShaderStage stage)
        {
            // Let's hack the emitted code to be ES 3.x syntax, see what happens.
            var upgraded = glslCode;

            if (!upgraded.Contains("#version"))
                upgraded = "#version 300 es\n" + upgraded;

            upgraded = upgraded
                .Replace("texture2D(", "texture(")
                .Replace("textureCube(", "texture(")
                .Replace("texture3D(", "texture(");

            if (stage == ShaderStage.Vertex)
            {
                upgraded = upgraded
                    .Replace("attribute ", "in ")
                    .Replace("varying ", "out ");

                return upgraded;
            }

            upgraded = upgraded.Replace("varying ", "in ");

            if (upgraded.Contains("gl_FragColor"))
            {
                upgraded = upgraded.Replace("gl_FragColor", "_mgColor0");

                if (!upgraded.Contains("layout(location = 0) out vec4 _mgColor0;"))
                {
                    var marker = "#endif\n";
                    var decl = "layout(location = 0) out vec4 _mgColor0;\n";
                    var markerIndex = upgraded.IndexOf(marker, StringComparison.Ordinal);
                    if (markerIndex >= 0)
                        upgraded = upgraded.Insert(markerIndex + marker.Length, decl);
                    else
                        upgraded = decl + upgraded;
                }
            }

            for (var i = 0; i < 8; i++)
            {
                var legacyDefine = $"#define ps_oC{i} gl_FragData[{i}]";
                if (!upgraded.Contains(legacyDefine))
                    continue;

                var replacement = new StringBuilder();
                replacement.AppendLine($"layout(location = {i}) out vec4 _mgColor{i};");
                replacement.Append($"#define ps_oC{i} _mgColor{i}");

                upgraded = upgraded.Replace(legacyDefine, replacement.ToString());
            }

            return upgraded;
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
