using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct VertexColorTexture
	{
		public Vector2 Vertex;
		public uint Color;
		public Vector2 TexCoord;
		public VertexColorTexture ( Vector2 vertex, Color color, Vector2 texCoord )
		{
			Vertex = vertex;
			Color = color.PackedValue;
			TexCoord = texCoord;
		}
	}
}

