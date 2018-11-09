// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TwoMGFX.TPGParser;

namespace TwoMGFX
{
    class OpenGLShaderProfile : ShaderProfile
    {
        private static readonly Regex GlslPixelShaderRegex = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)$", RegexOptions.Compiled);
        private static readonly Regex GlslVertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)$", RegexOptions.Compiled);

        public OpenGLShaderProfile()
            : base("OpenGL", 0)
        {                
        }

        internal override void AddMacros(Dictionary<string, string> macros)
        {
            macros.Add("GLSL", "1");
            macros.Add("OPENGL", "1");                
        }

        internal override void ValidateShaderModels(PassInfo pass)
        {
            int major, minor;

            if (!string.IsNullOrEmpty(pass.vsFunction))
            {
                ParseShaderModel(pass.vsModel, GlslVertexShaderRegex, out major, out minor);
                if (major > 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be SM 3.0 or lower!", pass.vsModel, pass.vsFunction));
            }

            if (!string.IsNullOrEmpty(pass.psFunction))
            {
                ParseShaderModel(pass.psModel, GlslPixelShaderRegex, out major, out minor);
                if (major > 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be SM 3.0 or lower!", pass.vsModel, pass.psFunction));
            }
        }

        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader, EffectObject effect, ref string errorsAndWarnings)
        {
            // For now GLSL is only supported via translation
            // using MojoShader which works from HLSL bytecode.
            var bytecode = EffectObject.CompileHLSL(shaderResult, shaderFunction, shaderProfile, ref errorsAndWarnings);

            // First look to see if we already created this same shader.
            foreach (var shader in effect.Shaders)
            {
                if (bytecode.SequenceEqual(shader.Bytecode))
                    return shader;
            }

            var shaderInfo = shaderResult.ShaderInfo;
            var shaderData = ShaderData.CreateGLSL(bytecode, isVertexShader, effect.ConstantBuffers, effect.Shaders.Count, shaderInfo.SamplerStates, shaderResult.Debug);
            effect.Shaders.Add(shaderData);

            return shaderData;
        }
            
        internal override bool Supports(string platform)
        {
            if (platform == "iOS" ||
                platform == "Android" ||
                platform == "DesktopGL" ||
                platform == "MacOSX" ||
                platform == "RaspberryPi")
                return true;

            return false;
        }
    }
}