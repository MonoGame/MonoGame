using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public class DepthStencilState : GraphicsResource
	{
        public bool DepthBufferEnable { get; set; }
        public bool DepthBufferWriteEnable { get; set; }
        public StencilOperation CounterClockwiseStencilDepthBufferFail { get; set; }
        public StencilOperation CounterClockwiseStencilFail { get; set; }
        public CompareFunction CounterClockwiseStencilFunction { get; set; }
        public StencilOperation CounterClockwiseStencilPass { get; set; }
        public CompareFunction DepthBufferFunction { get; set; }
        public int ReferenceStencil { get; set; }
        public StencilOperation StencilDepthBufferFail { get; set; }
        public bool StencilEnable { get; set; }
        public StencilOperation StencilFail { get; set; }
        public CompareFunction StencilFunction { get; set; }
        public int StencilMask { get; set; }
        public StencilOperation StencilPass { get; set; }
        public int StencilWriteMask { get; set; }
        public bool TwoSidedStencilMode { get; set; }

		public DepthStencilState ()
		{
            DepthBufferEnable = true;
            DepthBufferWriteEnable = true;
			StencilMask = Int32.MaxValue;
			StencilWriteMask = Int32.MaxValue;
		}
		
		static DepthStencilState defaultState;
		
		public static DepthStencilState Default {
			get {
				if (defaultState == null) {
					defaultState = new DepthStencilState () {
						DepthBufferEnable = true,
						DepthBufferWriteEnable = true
					};
				}
				
				return defaultState;
			}
		}
		
		static DepthStencilState depthReadState;
		
		public static DepthStencilState DepthRead {
			get {
				if (depthReadState == null) {
					depthReadState = new DepthStencilState () {
						DepthBufferEnable = true,
						DepthBufferWriteEnable = false
					};
				}
				
				return depthReadState;
			}
		}
		
		static DepthStencilState noneState;
		public static DepthStencilState None {
			get {
				if (noneState == null) {
					noneState = new DepthStencilState () {
						DepthBufferEnable = false,
						DepthBufferWriteEnable = false
					};
				}
				
				return noneState;
			}
		}
	}
}

