using System;

using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class MonoGameGraphicsHelper
	{
		internal static void SetDepthStencilState ( DepthStencilState state )
		{

			if (state.DepthBufferEnable) {
				// enable Depth Buffer
				GL.Enable( EnableCap.DepthTest);
			}
			else {
				GL.Disable (EnableCap.DepthTest);
			}


			if (state.StencilEnable) {

				// enable Stencil
				GL.Enable( EnableCap.StencilTest);

				// Set color mask - not needed
				//GL.ColorMask(false, false, false, false); //Disable drawing colors to the screen
				// set function
				StencilFunction func = StencilFunction.Always;
				switch (state.StencilFunction) {
				case CompareFunction.Always:
					func = StencilFunction.Always;
					break;
				case CompareFunction.Equal:
					func = StencilFunction.Equal;
					break;
				case CompareFunction.Greater:
					func = StencilFunction.Greater;
					break;
				case CompareFunction.GreaterEqual:
					func = StencilFunction.Gequal;
					break;
				case CompareFunction.Less:
					func = StencilFunction.Less;
					break;
				case CompareFunction.LessEqual:
					func = StencilFunction.Lequal;
					break;
				case CompareFunction.Never:
					func = StencilFunction.Never;
					break;
				case CompareFunction.NotEqual:
					func = StencilFunction.Notequal;
					break;
				}

				GL.StencilFunc (func, state.ReferenceStencil, state.StencilMask);

				GL.StencilOp (GetStencilOp(state.StencilFail) , GetStencilOp (state.StencilDepthBufferFail)
					, GetStencilOp ( state.StencilPass));
			}
			else {
				// Set color mask - not needed
				//GL.ColorMask(true, true, true, true); // Enable drawing colors to the screen
				GL.Disable (EnableCap.StencilTest);
			}

		}

		static StencilOp GetStencilOp (StencilOperation operation) {

			switch (operation) {
			case StencilOperation.Keep:
				return StencilOp.Keep;
			case StencilOperation.Decrement:
				return StencilOp.Decr;
			case StencilOperation.DecrementSaturation:
				return StencilOp.DecrWrap;
			case StencilOperation.Increment:
				return StencilOp.Incr;
			case StencilOperation.IncrementSaturation:
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
	}
}

