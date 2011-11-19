﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoMac.OpenGL;

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
				GL.CullFace(CullFaceMode.FrontAndBack);
				GL.FrontFace(FrontFaceDirection.Cw);
				break;
			case CullMode.CullCounterClockwiseFace:
				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.FrontAndBack);
				GL.FrontFace(FrontFaceDirection.Ccw);
				break;
			}
        }

		public static void SetRasterizerStates (RasterizerState state) {
			Cull(state);
			FillMode(state);
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
