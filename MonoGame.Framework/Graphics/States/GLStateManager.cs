using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WINDOWS
using GL11 = OpenTK.Graphics.OpenGL.GL;
using All11 = OpenTK.Graphics.OpenGL.All;
using ArrayCap11 = OpenTK.Graphics.OpenGL.ArrayCap;
using EnableCap11 = OpenTK.Graphics.OpenGL.EnableCap;
using MatrixMode11 = OpenTK.Graphics.OpenGL.MatrixMode;
using BlendingFactorSrc11 = OpenTK.Graphics.OpenGL.BlendingFactorSrc;
using BlendingFactorDest11 = OpenTK.Graphics.OpenGL.BlendingFactorDest;
using VertexPointerType = OpenTK.Graphics.OpenGL.VertexPointerType;
#else
using OpenTK.Graphics.ES20;
using OpenTK.Graphics.ES11;
using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;
using ArrayCap11 = OpenTK.Graphics.ES11.All;
using EnableCap11 = OpenTK.Graphics.ES11.All;
using MatrixMode11 = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc11 = OpenTK.Graphics.ES11.All;
using BlendingFactorDest11 = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
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
        private static BlendingFactorSrc11 _blendFuncSource;
        private static BlendingFactorDest11 _blendFuncDest;
#if WINDOWS
        private static EnableCap11 _cull = EnableCap11.AlphaTest; // default
#else
        private static EnableCap11 _cull = All11.Ccw; // default
#endif

        public static void TextureCoordArray(bool enable)
        {
            if (enable && (_textureCoordArray != GLStateEnabled.True))
                GL11.EnableClientState(ArrayCap11.TextureCoordArray);
            else
                GL11.EnableClientState(ArrayCap11.TextureCoordArray);
        }

        public static void VertexArray(bool enable)
        {
            if (enable && (_vertextArray != GLStateEnabled.True))
                GL11.EnableClientState(ArrayCap11.VertexArray);
            else
                GL11.EnableClientState(ArrayCap11.VertexArray);
        }

        public static void ColorArray(bool enable)
        {
            if (enable && (_colorArray != GLStateEnabled.True))
                GL11.EnableClientState(ArrayCap11.ColorArray);
            else
                GL11.EnableClientState(ArrayCap11.ColorArray);
        }

        public static void NormalArray(bool enable)
        {
            if (enable && (_normalArray != GLStateEnabled.True))
                GL11.EnableClientState(ArrayCap11.NormalArray);
            else
                GL11.EnableClientState(ArrayCap11.NormalArray);
        }

        public static void Textures2D(bool enable)
        {
            if (enable && (_textures2D != GLStateEnabled.True))
                GL11.Enable(EnableCap11.Texture2D);
            else
                GL11.Disable(EnableCap11.Texture2D);
        }

        public static void DepthTest(bool enable)
        {
            if (enable && (_depthTest != GLStateEnabled.True))
                GL11.Enable(EnableCap11.DepthTest);
            else
                GL11.Disable(EnableCap11.DepthTest);
        }

        public static void Blend(bool enable)
        {
            GL11.Enable(EnableCap11.Blend);
        }

        public static void Projection(Matrix projection)
        {
            GL11.MatrixMode(MatrixMode11.Projection);
            GL11.LoadIdentity();
            GL11.LoadMatrix(Matrix.ToFloatArray(projection));
            //GL11.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);
        }

        public static void View(Matrix view)
        {
#if WINDOWS
            GL11.MatrixMode(MatrixMode11.Modelview);
#else
            GL11.MatrixMode(MatrixMode11.Viewport);
#endif
            GL11.LoadIdentity();
            GL11.LoadMatrix(Matrix.ToFloatArray(view));
            //GL11.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);
        }

        public static void World(Matrix world)
        {
            GL11.MatrixMode(MatrixMode11.Modelview);
            GL11.LoadIdentity();
            GL11.LoadMatrix(Matrix.ToFloatArray(world));
            //GL11.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);
        }

        public static void Cull(EnableCap11 cullMode)
        {
            if (_cull != cullMode)
            {
                _cull = cullMode;
                GL11.Enable(_cull);
            }
        }

        public static void BlendFunc(BlendingFactorSrc11 source, BlendingFactorDest11 dest)
        {
            if (source != _blendFuncSource && dest != _blendFuncDest)
            {
                source = _blendFuncSource;
                dest = _blendFuncDest;

                GL11.BlendFunc(source, dest);
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
