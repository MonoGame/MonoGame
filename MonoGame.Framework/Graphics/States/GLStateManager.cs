using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.ES20;
using OpenTK.Graphics.ES11;
using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;


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
        private static All11 _blendFuncSource;
        private static All11 _blendFuncDest;
        private static All11 _cull = All11.Ccw; // default

        public static void TextureCoordArray(bool enable)
        {
            if (enable && (_textureCoordArray != GLStateEnabled.True))
                GL11.EnableClientState(All11.TextureCoordArray);
            else
                GL11.EnableClientState(All11.TextureCoordArray);
        }

        public static void VertexArray(bool enable)
        {
            if (enable && (_vertextArray != GLStateEnabled.True))
                GL11.EnableClientState(All11.VertexArray);
            else
                GL11.EnableClientState(All11.VertexArray);
        }

        public static void ColorArray(bool enable)
        {
            if (enable && (_colorArray != GLStateEnabled.True))
                GL11.EnableClientState(All11.ColorArray);
            else
                GL11.EnableClientState(All11.ColorArray);
        }

        public static void NormalArray(bool enable)
        {
            if (enable && (_normalArray != GLStateEnabled.True))
                GL11.EnableClientState(All11.NormalArray);
            else
                GL11.EnableClientState(All11.NormalArray);
        }

        public static void Textures2D(bool enable)
        {
            if (enable && (_textures2D != GLStateEnabled.True))
                GL11.Enable(All11.Texture2D);
            else
                GL11.Disable(All11.Texture2D);
        }

        public static void DepthTest(bool enable)
        {
            if (enable && (_depthTest != GLStateEnabled.True))
                GL11.Enable(All11.DepthTest);
            else
                GL11.Disable(All11.DepthTest);
        }

        public static void Blend(bool enable)
        {
            GL11.Enable(All11.Blend);
        }

        public static void Projection(Matrix projection)
        {
            GL11.MatrixMode(All11.Projection);
            GL11.LoadIdentity();
            GL11.LoadMatrix(Matrix.ToFloatArray(projection));
            //GL11.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);
        }

        public static void View(Matrix view)
        {
            GL11.MatrixMode(All11.Viewport);
            GL11.LoadIdentity();
            GL11.LoadMatrix(Matrix.ToFloatArray(view));
            //GL11.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);
        }

        public static void World(Matrix world)
        {
            GL11.MatrixMode(All11.Modelview);
            GL11.LoadIdentity();
            GL11.LoadMatrix(Matrix.ToFloatArray(world));
            //GL11.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);
        }

        public static void Cull(All11 cullMode)
        {
            if (_cull != cullMode)
            {
                _cull = cullMode;
                GL11.Enable(_cull);
            }
        }

        public static void BlendFunc(All11 source, All11 dest)
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
