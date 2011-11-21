using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class PSBasicEffect
	{
		internal static string CommonPSInputHeader =
				"uniform sampler2D Texture;\n"
				+"varying vec4 Diffuse;\n"
				+ "varying vec2 TexCoord;\n"
				+ "void main(){\n"
				+ "vec4 dif = Diffuse;\n"
				+ "vec2 tx = TexCoord;\n"
				+ "vec4 tc = texture2D(Texture,TexCoord);\n"
				+ "vec2 tx2 = TexCoord;\n";

		internal static string PSBasic {
			get {

				string code =
				CommonPSInputHeader
				+ "gl_FragColor = gl_Color;\n"
				+ "\n}";


				return code;
			}

		}

		internal static string PSBasicNoFog {
			get {

				string code =
					CommonPSInputHeader
				+ "gl_FragColor = gl_Color;\n"
				+ "\n}";


				return code;
			}

		}

		internal static string PSBasicVc {
			get {

				string code =
					CommonPSInputHeader
				+ "gl_FragColor = Diffuse;\n"
				+ "\n}";


				return code;
			}

		}

		internal static string PSBasicVcNoFog {
			get {

				string code =
					CommonPSInputHeader
				+ "gl_FragColor = Diffuse;\n"
				+ "\n}";


				return code;
			}

		}

		internal static string PSBasicTx {
			get {

				string code =
					CommonPSInputHeader
				+ "vec4 color = texture2D(Texture,TexCoord) * Diffuse;\n"
				// We need to add fog here
				+ "gl_FragColor = color;\n"
				+ "\n}";


				return code;
			}

		}

		internal static string PSBasicTxNoFog {
			get {

				string code =
					CommonPSInputHeader
				+ "gl_FragColor = texture2D(Texture,TexCoord);\n"
				+ "\n}";


				return code;
			}

		}

	}
}

