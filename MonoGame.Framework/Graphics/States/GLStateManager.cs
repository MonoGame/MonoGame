using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
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
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public static class GLStateManager
    {
        private static GLStateEnabled _textureCoordArray;
        private static GLStateEnabled _textures2D;
        private static GLStateEnabled _vertextArray;
        private static GLStateEnabled _colorArray;
        private static GLStateEnabled _normalArray;
        private static GLStateEnabled _depthTest;
        private static BlendingFactorSrc _blendFuncSource;
        private static BlendingFactorDest _blendFuncDest;
        private static All _cull = All.Ccw; // default
			
		public static void VertexAttribArray(int index, bool enable) {
			if (enable) {
				GL.EnableVertexAttribArray(index);
			} else {
				GL.DisableVertexAttribArray(index);
			}
		}
		
		public static void Textures2D(bool enable)
        {
            if (enable && (_textures2D != GLStateEnabled.True))
                GL.Enable(EnableCap.Texture2D);
            else
                GL.Disable(EnableCap.Texture2D);
        }

        public static void DepthTest(bool enable)
        {
            if (enable && (_depthTest != GLStateEnabled.True))
                GL.Enable(EnableCap.DepthTest);
            else
                GL.Disable(EnableCap.DepthTest);
        }

        public static void Blend(bool enable)
        {
            GL.Enable(EnableCap.Blend);
        }

		public static void SetBlendStates (BlendState state)
		{
			// Set blending mode
			BlendEquationMode blendMode = state.ColorBlendFunction.GetBlendEquationMode();

			GL.BlendEquation (blendMode);	
			
			// Set blending function
			BlendingFactorSrc bfs = state.ColorSourceBlend.GetBlendFactorSrc();
			BlendingFactorDest bfd = state.ColorDestinationBlend.GetBlendFactorDest();
#if IPHONE
			GL.BlendFunc ((All)bfs, (All)bfd);
#else
			GL.BlendFunc (bfs, bfd);
#endif
			
			GL.Enable (EnableCap.Blend);
		}

        public static void FillMode(RasterizerState state)
        {
#if MONOMAC || WINDOWS || LINUX
			switch (state.FillMode) {
			case Microsoft.Xna.Framework.Graphics.FillMode.Solid:
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				break;
			case Microsoft.Xna.Framework.Graphics.FillMode.WireFrame:
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				break;
			}
#else
            if (state.FillMode != Microsoft.Xna.Framework.Graphics.FillMode.Solid)
            {
                throw new NotImplementedException();
            }
#endif
        }


		public static void Cull(RasterizerState state, bool offscreen)
		{
			switch (state.CullMode) {
			case CullMode.None:
				GL.Disable(EnableCap.CullFace);
				break;
			case CullMode.CullClockwiseFace:
				GL.Enable(EnableCap.CullFace);
				// set it to Back
				GL.CullFace(CullFaceMode.Back);
				// Set our direction
				// I know this seems weird and maybe it is but based
				//  on the samples these seem to be reversed in OpenGL and DirectX

				//Also reversed again if we render offscreen, since we flip all the verticies
				if (offscreen) {
					GL.FrontFace(FrontFaceDirection.Cw);
				} else {
					GL.FrontFace(FrontFaceDirection.Ccw);
				}
				break;
			case CullMode.CullCounterClockwiseFace:
				GL.Enable(EnableCap.CullFace);
				// set it to Back
				GL.CullFace(CullFaceMode.Back);
				// I know this seems weird and maybe it is but based
				//  on the samples these seem to be reversed in OpenGL and DirectX

				//Also reversed again if we render offscreen, since we flip all the verticies
				if (offscreen) {
					GL.FrontFace(FrontFaceDirection.Ccw);
				} else {
					GL.FrontFace(FrontFaceDirection.Cw);
				}
				break;
			}
		}

		public static void SetRasterizerStates (RasterizerState state, bool offscreen) {
			Cull(state, offscreen);
			FillMode(state);
			if (state.ScissorTestEnable)
			{
				GL.Enable (EnableCap.ScissorTest);
			} else {
				GL.Disable(EnableCap.ScissorTest);
			}
		}
		
		public static void SetScissor (Rectangle scissor) {
			GL.Scissor(scissor.X, scissor.Y, scissor.Width, scissor.Height );
		}

		internal static void SetDepthStencilState ( DepthStencilState state )
		{

			if (state.DepthBufferEnable) {
				// enable Depth Buffer
				GL.Enable( EnableCap.DepthTest);

				DepthFunction func = DepthFunction.Always;
				switch (state.DepthBufferFunction) {
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

				GL.DepthMask (state.DepthBufferWriteEnable);
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
			case StencilOperation.IncrementSaturation:
				return StencilOp.IncrWrap;
			case StencilOperation.Increment:
				return StencilOp.Incr;
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

        public static void BlendFunc(BlendingFactorSrc source, BlendingFactorDest dest)
        {
            if (source != _blendFuncSource && dest != _blendFuncDest)
            {
                source = _blendFuncSource;
                dest = _blendFuncDest;
				
                GL.BlendFunc(source, dest);
			}
        }
    }

    public enum GLStateEnabled
    {
        False,
        True,
        NotSet
    }
}
