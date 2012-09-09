using System;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class SpriteBatchItem
	{
		public Texture2D Texture;
		public float Depth;

        public VertexPosition2ColorTexture vertexTL;
		public VertexPosition2ColorTexture vertexTR;
		public VertexPosition2ColorTexture vertexBL;
		public VertexPosition2ColorTexture vertexBR;
		public SpriteBatchItem ()
		{
			vertexTL = new VertexPosition2ColorTexture();
            vertexTR = new VertexPosition2ColorTexture();
            vertexBL = new VertexPosition2ColorTexture();
            vertexBR = new VertexPosition2ColorTexture();            
		}
		
		public void Set ( float x, float y, float w, float h, Color color, Vector2 texCoordTL, Vector2 texCoordBR )
		{
			vertexTL.Position.X = x;
            vertexTL.Position.Y = y;
            vertexTL.Color = color;
            vertexTL.TextureCoordinate.X = texCoordTL.X;
            vertexTL.TextureCoordinate.Y = texCoordTL.Y;

			vertexTR.Position.X = x+w;
            vertexTR.Position.Y = y;
            vertexTR.Color = color;
            vertexTR.TextureCoordinate.X = texCoordBR.X;
            vertexTR.TextureCoordinate.Y = texCoordTL.Y;

			vertexBL.Position.X = x;
            vertexBL.Position.Y = y+h;
            vertexBL.Color = color;
			vertexBL.TextureCoordinate.X = texCoordTL.X;
            vertexBL.TextureCoordinate.Y = texCoordBR.Y;

			vertexBR.Position.X = x+w;
            vertexBR.Position.Y = y+h;
            vertexBR.Color = color;
			vertexBR.TextureCoordinate.X = texCoordBR.X;
            vertexBR.TextureCoordinate.Y = texCoordBR.Y;
		}

		public void Set ( float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vector2 texCoordTL, Vector2 texCoordBR )
		{
			vertexTL.Position.X = x+dx*cos-dy*sin;
            vertexTL.Position.Y = y+dx*sin+dy*cos;
            vertexTL.Color = color;
            vertexTL.TextureCoordinate.X = texCoordTL.X;
            vertexTL.TextureCoordinate.Y = texCoordTL.Y;

			vertexTR.Position.X = x+(dx+w)*cos-dy*sin;
            vertexTR.Position.Y = y+(dx+w)*sin+dy*cos;
            vertexTR.Color = color;
            vertexTR.TextureCoordinate.X = texCoordBR.X;
            vertexTR.TextureCoordinate.Y = texCoordTL.Y;

			vertexBL.Position.X = x+dx*cos-(dy+h)*sin;
            vertexBL.Position.Y = y+dx*sin+(dy+h)*cos;
            vertexBL.Color = color;
            vertexBL.TextureCoordinate.X = texCoordTL.X;
            vertexBL.TextureCoordinate.Y = texCoordBR.Y;

			vertexBR.Position.X = x+(dx+w)*cos-(dy+h)*sin;
            vertexBR.Position.Y = y+(dx+w)*sin+(dy+h)*cos;
            vertexBR.Color = color;
            vertexBR.TextureCoordinate.X = texCoordBR.X;
            vertexBR.TextureCoordinate.Y = texCoordBR.Y;
		}
	}
}

