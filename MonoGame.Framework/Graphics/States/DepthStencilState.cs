using System;
using System.Diagnostics;

#if MONOMAC
using MonoMac.OpenGL;
using GLStencilFunction = MonoMac.OpenGL.StencilFunction;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
using GLStencilFunction = OpenTK.Graphics.OpenGL.StencilFunction;
#elif PSM
using Sce.PlayStation.Core.Graphics;
#elif GLES
using OpenTK.Graphics.ES20;
using EnableCap = OpenTK.Graphics.ES20.All;
using GLStencilFunction = OpenTK.Graphics.ES20.All;
using StencilOp = OpenTK.Graphics.ES20.All;
using DepthFunction = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class DepthStencilState : GraphicsResource
    {
#if DIRECTX 
        private SharpDX.Direct3D11.DepthStencilState _state;
#endif

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
		
		public static readonly DepthStencilState Default;
		public static readonly DepthStencilState DepthRead;
		public static readonly DepthStencilState None;
		
		static DepthStencilState ()
		{
			Default = new DepthStencilState () 
            {
				DepthBufferEnable = true,
				DepthBufferWriteEnable = true
			};
			
			DepthRead = new DepthStencilState () 
            {
				DepthBufferEnable = true,
				DepthBufferWriteEnable = false
			};
			
			None = new DepthStencilState () 
            {
				DepthBufferEnable = false,
				DepthBufferWriteEnable = false
			};
		}

#if OPENGL

        internal void ApplyState(GraphicsDevice device)
        {
            if (!DepthBufferEnable)
            {
                GL.Disable(EnableCap.DepthTest);
                GraphicsExtensions.CheckGLError();
            }
            else
            {
                // enable Depth Buffer
                GL.Enable(EnableCap.DepthTest);
                GraphicsExtensions.CheckGLError();

                DepthFunction func;
                switch (DepthBufferFunction)
                {
                    default:
                    case CompareFunction.Always:
                        func = DepthFunction.Always;
                        break;
                    case CompareFunction.Equal:
                        func = DepthFunction.Equal;
                        break;
                    case CompareFunction.Greater:
                        func = DepthFunction.Greater;
                        break;
                    case CompareFunction.GreaterEqual:
                        func = DepthFunction.Gequal;
                        break;
                    case CompareFunction.Less:
                        func = DepthFunction.Less;
                        break;
                    case CompareFunction.LessEqual:
                        func = DepthFunction.Lequal;
                        break;
                    case CompareFunction.Never:
                        func = DepthFunction.Never;
                        break;
                    case CompareFunction.NotEqual:
                        func = DepthFunction.Notequal;
                        break;
                }

                GL.DepthFunc(func);
                GraphicsExtensions.CheckGLError();
            }

            GL.DepthMask(DepthBufferWriteEnable);
            GraphicsExtensions.CheckGLError();

            if (!StencilEnable)
            {
                GL.Disable(EnableCap.StencilTest);
                GraphicsExtensions.CheckGLError();
            }
            else
            {
                // enable Stencil
                GL.Enable(EnableCap.StencilTest);
                GraphicsExtensions.CheckGLError();

                // Set color mask - not needed
                //GL.ColorMask(false, false, false, false); //Disable drawing colors to the screen
                // set function
                GLStencilFunction func;
                switch (StencilFunction)
                {
                    default:
                    case CompareFunction.Always:
                        func = GLStencilFunction.Always;
                        break;
                    case CompareFunction.Equal:
                        func = GLStencilFunction.Equal;
                        break;
                    case CompareFunction.Greater:
                        func = GLStencilFunction.Greater;
                        break;
                    case CompareFunction.GreaterEqual:
                        func = GLStencilFunction.Gequal;
                        break;
                    case CompareFunction.Less:
                        func = GLStencilFunction.Less;
                        break;
                    case CompareFunction.LessEqual:
                        func = GLStencilFunction.Lequal;
                        break;
                    case CompareFunction.Never:
                        func = GLStencilFunction.Never;
                        break;
                    case CompareFunction.NotEqual:
                        func = GLStencilFunction.Notequal;
                        break;
                }

                GL.StencilFunc(func, ReferenceStencil, StencilMask);
                GraphicsExtensions.CheckGLError();

                GL.StencilOp(GetStencilOp(StencilFail),
                                GetStencilOp(StencilDepthBufferFail),
                                GetStencilOp(StencilPass));
                GraphicsExtensions.CheckGLError();
            }
        }

        private static StencilOp GetStencilOp(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Keep:
                    return StencilOp.Keep;
                case StencilOperation.Decrement:
                    return StencilOp.DecrWrap;
                case StencilOperation.DecrementSaturation:
                    return StencilOp.Decr;
                case StencilOperation.IncrementSaturation:
                    return StencilOp.Incr;
                case StencilOperation.Increment:
                    return StencilOp.IncrWrap;
                case StencilOperation.Invert:
                    return StencilOp.Invert;
                case StencilOperation.Replace:
                    return StencilOp.Replace;
                case StencilOperation.Zero:
                    return StencilOp.Zero;
                default:
                    return StencilOp.Keep;
            }
        }

#elif DIRECTX

        internal void ApplyState(GraphicsDevice device)
        {
            if (_state == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                GraphicsDevice = device;

                // Build the description.
                var desc = new SharpDX.Direct3D11.DepthStencilStateDescription();

                desc.IsDepthEnabled = DepthBufferEnable;
                desc.DepthComparison = GetComparison(DepthBufferFunction);

                if (DepthBufferWriteEnable)
                    desc.DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.All;
                else
                    desc.DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.Zero;

                desc.IsStencilEnabled = StencilEnable;
                desc.StencilReadMask = (byte)StencilMask; // TODO: Should this instead grab the upper 8bits?
                desc.StencilWriteMask = (byte)StencilWriteMask;

                if (TwoSidedStencilMode)
                {
                    desc.BackFace.Comparison = GetComparison(CounterClockwiseStencilFunction);
                    desc.BackFace.DepthFailOperation = GetStencilOp(CounterClockwiseStencilDepthBufferFail);
                    desc.BackFace.FailOperation = GetStencilOp(CounterClockwiseStencilFail);
                    desc.BackFace.PassOperation = GetStencilOp(CounterClockwiseStencilPass);
                }
                else
                {   //use same settings as frontFace 
                    desc.BackFace.Comparison = GetComparison(StencilFunction);
                    desc.BackFace.DepthFailOperation = GetStencilOp(StencilDepthBufferFail);
                    desc.BackFace.FailOperation = GetStencilOp(StencilFail);
                    desc.BackFace.PassOperation = GetStencilOp(StencilPass);
                }

                desc.FrontFace.Comparison = GetComparison(StencilFunction);
                desc.FrontFace.DepthFailOperation = GetStencilOp(StencilDepthBufferFail);
                desc.FrontFace.FailOperation = GetStencilOp(StencilFail);
                desc.FrontFace.PassOperation = GetStencilOp(StencilPass);

                // Create the state.
                _state = new SharpDX.Direct3D11.DepthStencilState(GraphicsDevice._d3dDevice, desc);
            }

            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Apply the state!
            device._d3dContext.OutputMerger.DepthStencilReference = ReferenceStencil;
            device._d3dContext.OutputMerger.DepthStencilState = _state;
        }

        static private SharpDX.Direct3D11.Comparison GetComparison( CompareFunction compare)
        {
            switch (compare)
            {
                case CompareFunction.Always:
                    return SharpDX.Direct3D11.Comparison.Always;

                case CompareFunction.Equal:
                    return SharpDX.Direct3D11.Comparison.Equal;

                case CompareFunction.Greater:
                    return SharpDX.Direct3D11.Comparison.Greater;

                case CompareFunction.GreaterEqual:
                    return SharpDX.Direct3D11.Comparison.GreaterEqual;

                case CompareFunction.Less:
                    return SharpDX.Direct3D11.Comparison.Less;

                case CompareFunction.LessEqual:
                    return SharpDX.Direct3D11.Comparison.LessEqual;

                case CompareFunction.Never:
                    return SharpDX.Direct3D11.Comparison.Never;

                case CompareFunction.NotEqual:
                    return SharpDX.Direct3D11.Comparison.NotEqual;

                default:
                    throw new NotImplementedException("Invalid comparison!");
            }
        }

        static private SharpDX.Direct3D11.StencilOperation GetStencilOp(StencilOperation op)
        {
            switch (op)
            {
                case StencilOperation.Decrement:
                    return SharpDX.Direct3D11.StencilOperation.Decrement;

                case StencilOperation.DecrementSaturation:
                    return SharpDX.Direct3D11.StencilOperation.DecrementAndClamp;

                case StencilOperation.Increment:
                    return SharpDX.Direct3D11.StencilOperation.Increment;

                case StencilOperation.IncrementSaturation:
                    return SharpDX.Direct3D11.StencilOperation.IncrementAndClamp;

                case StencilOperation.Invert:
                    return SharpDX.Direct3D11.StencilOperation.Invert;

                case StencilOperation.Keep:
                    return SharpDX.Direct3D11.StencilOperation.Keep;

                case StencilOperation.Replace:
                    return SharpDX.Direct3D11.StencilOperation.Replace;

                case StencilOperation.Zero:
                    return SharpDX.Direct3D11.StencilOperation.Zero;

                default:
                    throw new NotImplementedException("Invalid stencil operation!");
            }
        }

#endif // DIRECTX
#if PSM
        internal void ApplyState(GraphicsDevice device)
        {
            #warning Unimplemented
        }
#endif
	}
}

