// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class RasterizerState : GraphicsResource
	{
        // TODO: We should be asserting if the state has
        // been changed after it has been bound to the device!

        public CullMode CullMode { get; set; }
        public float DepthBias { get; set; }
        public FillMode FillMode { get; set; }
        public bool MultiSampleAntiAlias { get; set; }
        public bool ScissorTestEnable { get; set; }
        public float SlopeScaleDepthBias { get; set; }

        public static RasterizerState CullClockwise
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.CullClockwise);
                return GraphicsDeviceContext.Current.CullClockwise.Value;
            }
        }

        public static RasterizerState CullCounterClockwise
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.CullCounterClockwise);
                return GraphicsDeviceContext.Current.CullCounterClockwise.Value;
            }
        }

        public static RasterizerState CullNone
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.CullNone);
                return GraphicsDeviceContext.Current.CullNone.Value;
            }
        }

        public RasterizerState()
		{
			CullMode = CullMode.CullCounterClockwiseFace;
			FillMode = FillMode.Solid;
			DepthBias = 0;
			MultiSampleAntiAlias = true;
			ScissorTestEnable = false;
			SlopeScaleDepthBias = 0;
		}

    }
}