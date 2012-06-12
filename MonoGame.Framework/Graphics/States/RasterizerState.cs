using System;
using System.Diagnostics;


namespace Microsoft.Xna.Framework.Graphics
{
	public class RasterizerState : GraphicsResource
	{
#if DIRECTX 
        private SharpDX.Direct3D11.RasterizerState _state;
#endif

        // TODO: We should be asserting if the state has
        // been changed after it has been bound to the device!

        public CullMode CullMode { get; set; }
        public float DepthBias { get; set; }
        public FillMode FillMode { get; set; }
        public bool MultiSampleAntiAlias { get; set; }
        public bool ScissorTestEnable { get; set; }
        public float SlopeScaleDepthBias { get; set; }

		public static readonly RasterizerState CullClockwise;		
		public static readonly RasterizerState CullCounterClockwise;
		public static readonly RasterizerState CullNone;

		public RasterizerState ()
		{
			CullMode = CullMode.CullCounterClockwiseFace;
			FillMode = FillMode.Solid;
			DepthBias = 0;
			MultiSampleAntiAlias = true;
			ScissorTestEnable = false;
			SlopeScaleDepthBias = 0;
		}

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

#if DIRECTX

        internal void ApplyState( GraphicsDevice device )
        {
            if (_state == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                graphicsDevice = device;

                // Build the description.
                var desc = new SharpDX.Direct3D11.RasterizerStateDescription();

                switch ( CullMode )
                {
                    case Graphics.CullMode.CullClockwiseFace:
                        desc.CullMode = SharpDX.Direct3D11.CullMode.Front;
                        break;

                    case Graphics.CullMode.CullCounterClockwiseFace:
                        desc.CullMode = SharpDX.Direct3D11.CullMode.Back;
                        break;

                    case Graphics.CullMode.None:
                        desc.CullMode = SharpDX.Direct3D11.CullMode.None;
                        break;
                }

                desc.IsScissorEnabled = ScissorTestEnable;
                desc.IsMultisampleEnabled = MultiSampleAntiAlias;
                desc.DepthBias = (int)DepthBias;
                desc.SlopeScaledDepthBias = SlopeScaleDepthBias;

                if (FillMode == Graphics.FillMode.WireFrame)
                    desc.FillMode = SharpDX.Direct3D11.FillMode.Wireframe;
                else
                    desc.FillMode = SharpDX.Direct3D11.FillMode.Solid;

                // These are new DX11 features we should consider exposing
                // as part of the extended MonoGame API.
                desc.IsFrontCounterClockwise = false;
                desc.DepthBiasClamp = desc.DepthBias;
                desc.IsDepthClipEnabled = true;
                desc.IsAntialiasedLineEnabled = false;

                // Create the state.
                _state = new SharpDX.Direct3D11.RasterizerState(graphicsDevice._d3dDevice, ref desc);
            }
            
            Debug.Assert( graphicsDevice == device, "The state was created for a different device!" );

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Apply the state!
            device._d3dContext.Rasterizer.State = _state;
        }

#endif // DIRECTX

    }
}