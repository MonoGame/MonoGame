using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class PSBasicEffect
	{
		internal static string PSBasicNoFog {
			get {
			
				string code =
				"varying vec4 Diffuse;\n"
				+ "void main(){\n"
				+ "// Setting Each Pixel To Red\n"
				//+ "gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);\n}";
				//+ "gl_FragColor = gl_Color;\n"
				+ "gl_FragColor = Diffuse;\n"
				+ "\n}";


				return code;
			}

		}
	}
}

