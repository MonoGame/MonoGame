// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MonoGame.Effect.TPGParser;

namespace MonoGame.Effect
{
    class DirectX11ShaderProfile : ShaderProfile
    {
        private static readonly Regex HlslPixelShaderRegex = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_2|9_3))?$", RegexOptions.Compiled);
        private static readonly Regex HlslVertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_2|9_3))?$", RegexOptions.Compiled);

        public DirectX11ShaderProfile()
            : base("DirectX_11", 1)
        {
        }

        internal override void AddMacros(Dictionary<string, string> macros)
        {
            macros.Add("HLSL", "1");
            macros.Add("SM4", "1");
        }

        internal override void ValidateShaderModels(PassInfo pass)
        {
            int major, minor;

            if (!string.IsNullOrEmpty(pass.vsFunction))
            {
                ParseShaderModel(pass.vsModel, HlslVertexShaderRegex, out major, out minor);
                if (major <= 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be SM 4.0 level 9.1 or higher!", pass.vsModel, pass.vsFunction));
            }

            if (!string.IsNullOrEmpty(pass.psFunction))
            {
                ParseShaderModel(pass.psModel, HlslPixelShaderRegex, out major, out minor);
                if (major <= 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be SM 4.0 level 9.1 or higher!", pass.vsModel, pass.psFunction));
            }
        }

        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader, EffectObject effect, ref string errorsAndWarnings)
        {
            var bytecode = EffectObject.CompileHLSL(shaderResult, shaderFunction, shaderProfile, ref errorsAndWarnings);

            var shaderInfo = shaderResult.ShaderInfo;
            var shaderData = ShaderData.CreateHLSL(bytecode, isVertexShader, effect.ConstantBuffers, effect.Shaders.Count, shaderInfo.SamplerStates, shaderResult.Debug);
            effect.Shaders.Add(shaderData);
            return shaderData;
        }
    }
}
