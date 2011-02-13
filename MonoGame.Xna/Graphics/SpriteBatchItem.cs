using System;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class SpriteBatchItem
	{
		public int TextureID;
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
			vertexTL.Position = new Vector2(x,y);
			vertexTL.Color = color.GLPackedValue;
			vertexTL.TextureCoordinate = texCoordTL;

			vertexTR.Position = new Vector2(x+w,y);
			vertexTR.Color = color.GLPackedValue;
			vertexTR.TextureCoordinate = new Vector2(texCoordBR.X,texCoordTL.Y);

			vertexBL.Position = new Vector2(x,y+h);
			vertexBL.Color = color.GLPackedValue;
			vertexBL.TextureCoordinate = new Vector2(texCoordTL.X,texCoordBR.Y);

			vertexBR.Position = new Vector2(x+w,y+h);
			vertexBR.Color = color.GLPackedValue;
			vertexBR.TextureCoordinate = texCoordBR;
		}
		public void Set ( float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vector2 texCoordTL, Vector2 texCoordBR )
		{
			vertexTL.Position = new Vector2(x+dx*cos-dy*sin,y+dx*sin+dy*cos);
			vertexTL.Color = color.GLPackedValue;
			vertexTL.TextureCoordinate = texCoordTL;

			vertexTR.Position = new Vector2(x+(dx+w)*cos-dy*sin,y+(dx+w)*sin+dy*cos);
			vertexTR.Color = color.GLPackedValue;
			vertexTR.TextureCoordinate = new Vector2(texCoordBR.X,texCoordTL.Y);

			vertexBL.Position = new Vector2(x+dx*cos-(dy+h)*sin,y+dx*sin+(dy+h)*cos);
			vertexBL.Color = color.GLPackedValue;
			vertexBL.TextureCoordinate = new Vector2(texCoordTL.X,texCoordBR.Y);

			vertexBR.Position = new Vector2(x+(dx+w)*cos-(dy+h)*sin,y+(dx+w)*sin+(dy+h)*cos);
			vertexBR.Color = color.GLPackedValue;
			vertexBR.TextureCoordinate = texCoordBR;
		}
	}
}

