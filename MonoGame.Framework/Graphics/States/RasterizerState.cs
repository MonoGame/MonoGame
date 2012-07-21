using System;
using System.Diagnostics;


#if GLES
using OpenTK.Graphics.ES20;
using EnableCap = OpenTK.Graphics.ES20.All;
using FrontFaceDirection = OpenTK.Graphics.ES20.All;
using BlendEquationMode = OpenTK.Graphics.ES20.All;
using CullFaceMode = OpenTK.Graphics.ES20.All;
using StencilFunction = OpenTK.Graphics.ES20.All;
using StencilOp = OpenTK.Graphics.ES20.All;
using BlendingFactorSrc = OpenTK.Graphics.ES20.All;
using BlendingFactorDest = OpenTK.Graphics.ES20.All;
using DepthFunction = OpenTK.Graphics.ES20.All;
#elif MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#endif


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

        private bool offscreenCull = false;

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

        internal void ApplyState( GraphicsDevice device )
        {
            var prevState = device.prevRasterizerState;
            offscreenCull = device.GetRenderTargets().Length > 0;

            if (CullMode != prevState.CullMode || 
                offscreenCull != prevState.offscreenCull)
            {
                if (CullMode == Microsoft.Xna.Framework.Graphics.CullMode.None &&
                    prevState.CullMode != CullMode)
                {
                    GL.Disable(EnableCap.CullFace);
                }
                else if (CullMode != CullMode.None)
                {
                    // Have to do this check again. Could be moving between offscreen surfaces
                    // and not need to re-enable cullFace.
                   if (CullMode != prevState.CullMode)
                    {
                        GL.Enable(EnableCap.CullFace);
                        // set it to Back
                        GL.CullFace(CullFaceMode.Back);
                    }

                    if ( CullMode == Microsoft.Xna.Framework.Graphics.CullMode.CullCounterClockwiseFace )
                    {
                        // I know this seems weird and maybe it is but based
                        //  on the samples these seem to be reversed in OpenGL and DirectX
                        //Also reversed again if we render offscreen, since we flip all the verticies
                        if (offscreenCull)
                        {
                            GL.FrontFace(FrontFaceDirection.Ccw);
                        }
                        else
                        {
                            GL.FrontFace(FrontFaceDirection.Cw);
                        }
                    }
                    else
                    {
                        // I know this seems weird and maybe it is but based
                        //  on the samples these seem to be reversed in OpenGL and DirectX

                        //Also reversed again if we render offscreen, since we flip all the verticies
                        if (offscreenCull)
                        {
                            GL.FrontFace(FrontFaceDirection.Cw);
                        }
                        else
                        {
                            GL.FrontFace(FrontFaceDirection.Ccw);
                        }
                    }
                }
            }


            // Set Fill Mode.

#if MONOMAC || WINDOWS || LINUX
            switch (FillMode)
            {
                case FillMode.Solid:
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    break;
                case FillMode.WireFrame:
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    break;
            }
#else
            if (FillMode != FillMode.Solid)
                throw new NotImplementedException();
#endif

            if (ScissorTestEnable && !prevState.ScissorTestEnable )
                GL.Enable (EnableCap.ScissorTest);
            else if (prevState.ScissorTestEnable)
                GL.Disable(EnableCap.ScissorTest);

#if DIRECTX
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
                desc.IsAntialiasedLineEnabled = false;

                // To support feature level 9.1 these must 
                // be set to these exact values.
                desc.DepthBiasClamp = 0.0f;
                desc.IsDepthClipEnabled = true;

                // Create the state.
                _state = new SharpDX.Direct3D11.RasterizerState(graphicsDevice._d3dDevice, ref desc);
            }
            
            Debug.Assert( graphicsDevice == device, "The state was created for a different device!" );

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Apply the state!
            device._d3dContext.Rasterizer.State = _state;
#endif // DIRECTX
        }



    }
}