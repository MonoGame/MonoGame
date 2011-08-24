using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class SpriteEffectCode
	{
		internal static string SpriteEffectFragmentCode() {
			
			StringBuilder sb = new StringBuilder();
			sb.Append("uniform sampler2D tex;\n");
			sb.Append("void main(){\n");
			sb.Append("gl_FragColor = gl_Color * texture2D(tex, gl_TexCoord[0].xy);}");
			return sb.ToString();
			
		}
	}
}

