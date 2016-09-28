// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace TwoMGFX
{
    public class GlfxShaderProfile : ShaderProfile
    {
        public GlfxShaderProfile() : base("GLFX", 2)
        {
        }

        internal override void AddMacros(Dictionary<string, string> macros)
        {
            macros.Add("GLSL", "1");
            macros.Add("OPENGL", "1");                
            macros.Add("GLFX", "1");                
        }

        internal override void ValidateShaderModels(PassInfo pass)
        {
        }

        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader,
            EffectObject effect, ref string errorsAndWarnings)
        {
            var shaderInfo = shaderResult.ShaderInfo;
            var content = shaderResult.FileContent;
            ParseTreeTools.WhitespaceNodes(TokenType.Semantic, shaderInfo.ParseTree.Nodes, ref content);


            // we should have pure GLSL now so we can pass it to the optimizer

            errorsAndWarnings = string.Empty;
            return null;
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
