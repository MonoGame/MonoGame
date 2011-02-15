using System;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class BatchItem
	{
		public int TextureID;
		public float Depth;
		public VertexColorTexture vctTL;
		public VertexColorTexture vctTR;
		public VertexColorTexture vctBL;
		public VertexColorTexture vctBR;
		public BatchItem ()
		{
			vctTL = new VertexColorTexture();
			vctTR = new VertexColorTexture();
			vctBL = new VertexColorTexture();
			vctBR = new VertexColorTexture();
		}
	}
}

