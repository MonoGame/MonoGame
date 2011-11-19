using System;
using System.Text;
namespace Microsoft.Xna.Framework.Graphics
{
	internal static class PSBasicNoFog
	{
		internal static string BasicEffectVertexCode ()
		{
			
			StringBuilder sb = new StringBuilder ();

			string code = "void main(){\n"
				+ "// Setting Each Pixel To Red\n"
				//+ "gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);\n}";
				+ "gl_FragColor = gl_Color;\n}";

			return code;

		}
	}
}

