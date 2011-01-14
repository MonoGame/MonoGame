using System;

using XnaTouch.Framework;
using XnaTouch.Framework.Graphics;

namespace XnaTouch.Framework.Graphics
{
	//[StructLayout(LayoutKind.Sequential)]
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
			Color = color.GLPackedValue;
			TextureCoordinate = texCoord;
		}
	}
}

