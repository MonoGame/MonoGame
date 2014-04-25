// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState : GraphicsResource
    {
        // TODO: We should be asserting if the state has
        // been changed after it has been bound to the device!

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
			DepthBufferFunction = CompareFunction.LessEqual;
			StencilEnable = false;
			StencilFunction = CompareFunction.Always;
			StencilPass = StencilOperation.Keep;
			StencilFail = StencilOperation.Keep;
			StencilDepthBufferFail = StencilOperation.Keep;
			TwoSidedStencilMode = false;
			CounterClockwiseStencilFunction = CompareFunction.Always;
			CounterClockwiseStencilFail = StencilOperation.Keep;
			CounterClockwiseStencilPass = StencilOperation.Keep;
			CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep;
			StencilMask = Int32.MaxValue;
			StencilWriteMask = Int32.MaxValue;
			ReferenceStencil = 0;
		}

        private static readonly Utilities.ObjectFactoryWithReset<DepthStencilState> _default;
        private static readonly Utilities.ObjectFactoryWithReset<DepthStencilState> _depthRead;
        private static readonly Utilities.ObjectFactoryWithReset<DepthStencilState> _none;

        public static DepthStencilState Default { get { return _default.Value; } }
        public static DepthStencilState DepthRead { get { return _depthRead.Value; } }
        public static DepthStencilState None { get { return _none.Value; } }
		
		static DepthStencilState ()
		{
			_default = new Utilities.ObjectFactoryWithReset<DepthStencilState>(() => new DepthStencilState
            {
                Name = "DepthStencilState.Default",
				DepthBufferEnable = true,
				DepthBufferWriteEnable = true
			});
			
			_depthRead = new Utilities.ObjectFactoryWithReset<DepthStencilState>(() => new DepthStencilState
            {
                Name = "DepthStencilState.DepthRead",
                DepthBufferEnable = true,
				DepthBufferWriteEnable = false
			});
			
			_none = new Utilities.ObjectFactoryWithReset<DepthStencilState>(() => new DepthStencilState
            {
                Name = "DepthStencilState.None",
                DepthBufferEnable = false,
				DepthBufferWriteEnable = false
			});
		}

        internal static void ResetStates()
        {
            _default.Reset();
            _depthRead.Reset();
            _none.Reset();
        }
	}
}

