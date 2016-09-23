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
            var shaderData = new GlfxShaderData();
            ;
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
