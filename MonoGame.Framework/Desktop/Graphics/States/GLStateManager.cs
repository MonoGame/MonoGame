using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MACOSX
using MonoMac.OpenGL;
#else
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
#endif

using Microsoft.Xna.Framework;

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

        public static void TextureCoordArray(bool enable)
        {
            if (enable && (_textureCoordArray != GLStateEnabled.True))
                GL.EnableClientState(ArrayCap.TextureCoordArray);
            else
                GL.DisableClientState(ArrayCap.TextureCoordArray);
        }

        public static void VertexArray(bool enable)
        {
            if (enable && (_vertextArray != GLStateEnabled.True))
                GL.EnableClientState(ArrayCap.VertexArray);
            else
                GL.DisableClientState(ArrayCap.VertexArray);
        }

        public static void ColorArray(bool enable)
        {
            if (enable && (_colorArray != GLStateEnabled.True))
                GL.EnableClientState(ArrayCap.ColorArray);
            else
                GL.DisableClientState(ArrayCap.ColorArray);
			//GL.Enable(EnableCap.ColorArray);
        }

        public static void NormalArray(bool enable)
        {
            if (enable && (_normalArray != GLStateEnabled.True))
                GL.EnableClientState(ArrayCap.NormalArray);
            else
                GL.DisableClientState(ArrayCap.NormalArray);
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

        public static void Viewport(Rectangle viewport)
        {
            GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        } 

        public static void Projection(Matrix projection)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(Matrix.ToFloatArray(projection));
            //GL11.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);
        }

        public static void View(Matrix view)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(Matrix.ToFloatArray(view));
            //GL11.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);
        }

        public static void World(Matrix world)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(Matrix.ToFloatArray(world));
            //GL11.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);
        }

		public static void WorldView (Matrix world, Matrix view)
		{
			Matrix worldView;
			Matrix.Multiply(ref world, ref view, out worldView);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
			GL.LoadMatrix(Matrix.ToFloatArray(worldView));
		}

		public static void SetBlendStates (BlendState state)
		{
			// Set blending mode
			BlendEquationMode blendMode = state.ColorBlendFunction.GetBlendEquationMode();
			GL.BlendEquation (blendMode);

			// Set blending function
			BlendingFactorSrc bfs = state.ColorSourceBlend.GetBlendFactorSrc();
			BlendingFactorDest bfd = state.ColorDestinationBlend.GetBlendFactorDest();
			GL.BlendFunc (bfs, bfd);

			GL.Enable (EnableCap.Blend);
		}

		public static void FillMode (RasterizerState state)
		{
			switch (state.FillMode) {
			case Microsoft.Xna.Framework.Graphics.FillMode.Solid:
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				break;
			case Microsoft.Xna.Framework.Graphics.FillMode.WireFrame:
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				break;
			}
		}

		public static void Cull(RasterizerState state)
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
				GL.FrontFace(FrontFaceDirection.Ccw);
				break;
			case CullMode.CullCounterClockwiseFace:
				GL.Enable(EnableCap.CullFace);
				// set it to Back
				GL.CullFace(CullFaceMode.Back);
				// I know this seems weird and maybe it is but based
				//  on the samples these seem to be reversed in OpenGL and DirectX
				GL.FrontFace(FrontFaceDirection.Cw);
				break;
			}
		}

		public static void SetRasterizerStates (RasterizerState state) {
			Cull(state);
			FillMode(state);
		}

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
