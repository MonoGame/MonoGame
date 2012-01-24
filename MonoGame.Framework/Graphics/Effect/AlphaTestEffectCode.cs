using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class AlphaTestEffectCode
	{
		internal static string AlphaTestEffectFragmentCode() {
			
			var sb = new StringBuilder();

const string code = "uniform sampler2D Texture;\n"
                    + "uniform vec4 DiffuseColor;\n"
                    + "uniform vec4 AlphaTest;\n"
                    + "uniform vec3 FogColor;\n"
                    + "uniform vec4 FogVector;\n"
                    + "uniform mat4 WorldViewProj;\n"
                    + "uniform int ShaderIndex;\n"

                    + "void main() {\n"

                    // Defined here so that they show up as parameters
                    + "vec4 dc = DiffuseColor;\n"
                    + "vec4 at = AlphaTest;\n"
                    + "vec3 fc = FogColor;\n"
                    + "vec4 fv = FogVector;\n"
                    + "mat4 proj = WorldViewProj;\n"
                    + "int shaderIndex = ShaderIndex;\n"
                    // End Defines

                    + "vec4 color = texture2D(Texture, gl_TexCoord[0].xy);\n"

                    // 7 is the index that is being passed for Eq/Ne with diffuse and no fog
                    + "float retDiscard = 0.0;\n"
                    + "if (ShaderIndex == 7) {\n"
                    + "	float absRet = abs(color.a - AlphaTest.x);\n"
                    + "	if (absRet < AlphaTest.y)\n"
                    + "		retDiscard = AlphaTest.z;\n"
                    + "	else\n"
                    + "		retDiscard = AlphaTest.w;\n"
                    + "}\n"
                    + "else { // Here we will default to Lt/Gt\n"

                    + "	if (color.a < AlphaTest.x)\n"
                    + "		retDiscard = AlphaTest.z;\n"
                    + "	else\n"
                    + "		retDiscard = AlphaTest.w;\n"
                    + "}\n"

                    + "if (retDiscard == -1.0) {\n"
                    + "	discard;\n"
                    + "}\n"

                    + "gl_FragColor = gl_Color * color;\n"

                    + "}\n";


			return code;
			
		}
	}
}

