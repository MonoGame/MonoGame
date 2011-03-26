using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public class RasterizerState : GraphicsResource
	{
        public CullMode CullMode { get; set; }
        public float DepthBias { get; set; }
        public FillMode FillMode { get; set; }
        public bool MultiSampleAntiAlias { get; set; }
        public bool ScissorTestEnable { get; set; }
        public float SlopeScaleDepthBias { get; set; }
		
		public static readonly RasterizerState CullClockwise;		
		public static readonly RasterizerState CullCounterClockwise;
		public static readonly RasterizerState CullNone;
		
		static RasterizerState ()
		{
			CullClockwise = new RasterizerState () {
				CullMode = CullMode.CullClockwiseFace
			};
			CullCounterClockwise = new RasterizerState () {
				CullMode = CullMode.CullCounterClockwiseFace
			};
			CullNone = new RasterizerState () {
				CullMode = CullMode.None
			};
		}
	}
}