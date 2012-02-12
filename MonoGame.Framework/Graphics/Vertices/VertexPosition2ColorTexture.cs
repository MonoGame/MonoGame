using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	// This should really be XNA's VertexPositionColorTexture
	// but I'm not sure we want to use Vector3s if we don't have to.
	internal struct VertexPosition2ColorTexture
	{
		public Vector2 Position;
		public uint Color;
		
		public Vector2 TextureCoordinate;
		
		public VertexPosition2ColorTexture ( Vector2 position, Color color, Vector2 texCoord )
		{
			Position = position;
			Color = color.PackedValue;
			TextureCoordinate = texCoord;
		}
		
		public static int GetSize()
		{
				return sizeof(float)*4+sizeof(uint);
	    }
	}
}

