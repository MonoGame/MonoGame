using System;
#if WINDOWS
using OpenTK.Graphics.OpenGL;
using GL11 = OpenTK.Graphics.OpenGL.GL;
using All11 = OpenTK.Graphics.OpenGL.All;
using ArrayCap11 = OpenTK.Graphics.OpenGL.ArrayCap;
using EnableCap11 = OpenTK.Graphics.OpenGL.EnableCap;
using MatrixMode11 = OpenTK.Graphics.OpenGL.MatrixMode;
using BlendingFactorSrc11 = OpenTK.Graphics.OpenGL.BlendingFactorSrc;
using BlendingFactorDest11 = OpenTK.Graphics.OpenGL.BlendingFactorDest;
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
using VertexPointerType11 = OpenTK.Graphics.ES11.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public static class GraphicsExtensions
    {
        public static All11 OpenGL11(this CullMode cull)
        {
            switch (cull)
            {
                case CullMode.CullClockwiseFace:
                    return All11.Cw;
                case CullMode.CullCounterClockwiseFace:
                    return All11.Ccw;
                default:
                    throw new NotImplementedException();
            }
        }

        public static int OpenGLNumberOfElements(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    throw new NotImplementedException();

                case VertexElementFormat.Vector2:
                    return 2;

                case VertexElementFormat.Vector3:
                    return 3;

                case VertexElementFormat.Vector4:
                    return 4;

                case VertexElementFormat.Color:
                    return 4;

                case VertexElementFormat.Byte4:
                    return 4;

                case VertexElementFormat.Short2:
                    return 2;

                case VertexElementFormat.Short4:
                    return 2;

                case VertexElementFormat.NormalizedShort2:
                    return 2;

                case VertexElementFormat.NormalizedShort4:
                    return 4;

                case VertexElementFormat.HalfVector2:
                    return 2;

                case VertexElementFormat.HalfVector4:
                    return 4;
            }

            throw new NotImplementedException();
        }

#if WINDOWS
        public static VertexPointerType OpenGLVertexPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    throw new NotImplementedException();

                case VertexElementFormat.Vector2:
                    return VertexPointerType.Float;

                case VertexElementFormat.Vector3:
                    return VertexPointerType.Float;

                case VertexElementFormat.Vector4:
                    return VertexPointerType.Float;
            }

            throw new NotImplementedException();
        }

        public static ColorPointerType OpenGLColorPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    throw new NotImplementedException();

                case VertexElementFormat.Vector2:
                    return ColorPointerType.Float;

                case VertexElementFormat.Vector3:
                    return ColorPointerType.Float;

                case VertexElementFormat.Vector4:
                    return ColorPointerType.Float;

                case VertexElementFormat.Color:
                    return ColorPointerType.UnsignedByte;

                case VertexElementFormat.Byte4:
                    return ColorPointerType.UnsignedByte;

                case VertexElementFormat.Short2:
                    return ColorPointerType.UnsignedShort;

                case VertexElementFormat.Short4:
                    return ColorPointerType.UnsignedShort;

                case VertexElementFormat.NormalizedShort2:
                    return ColorPointerType.UnsignedShort;

                case VertexElementFormat.NormalizedShort4:
                    return ColorPointerType.UnsignedShort;

                case VertexElementFormat.HalfVector2:
                    return ColorPointerType.Float;

                case VertexElementFormat.HalfVector4:
                    return ColorPointerType.Float;
            }

            throw new NotImplementedException();
        }

        public static NormalPointerType OpenGLNormalPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    throw new NotImplementedException();

                case VertexElementFormat.Vector2:
                    return NormalPointerType.Float;

                case VertexElementFormat.Vector3:
                    return NormalPointerType.Float;

                case VertexElementFormat.Vector4:
                    return NormalPointerType.Float;

                case VertexElementFormat.HalfVector2:
                    return NormalPointerType.Float;

                case VertexElementFormat.HalfVector4:
                    return NormalPointerType.Float;
            }

            throw new NotImplementedException();
        }

        public static TexCoordPointerType OpenGLTexCoordPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    throw new NotImplementedException();

                case VertexElementFormat.Vector2:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.Vector3:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.Vector4:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.HalfVector2:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.HalfVector4:
                    return TexCoordPointerType.Float;
            }

            throw new NotImplementedException();
        }
#else
        public static All11 OpenGLValueType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    throw new NotImplementedException();

                case VertexElementFormat.Vector2:
                    return All11.Float;

                case VertexElementFormat.Vector3:
                    return All11.Float;

                case VertexElementFormat.Vector4:
                    return All11.Float;

                case VertexElementFormat.Color:
                    return All11.UnsignedByte;

                case VertexElementFormat.Byte4:
                    return All11.UnsignedByte;

                case VertexElementFormat.Short2:
                    return All11.UnsignedShort;

                case VertexElementFormat.Short4:
                    return All11.UnsignedShort;

                case VertexElementFormat.NormalizedShort2:
                    return All11.UnsignedShort;

                case VertexElementFormat.NormalizedShort4:
                    return All11.UnsignedShort;

                case VertexElementFormat.HalfVector2:
                    return All11.Float;

                case VertexElementFormat.HalfVector4:
                    return All11.Float;
            }

            throw new NotImplementedException();
        }
#endif

        public static int Size(this SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Color:
                    return 0;
                case SurfaceFormat.Dxt3:
                    return 4;
                case SurfaceFormat.Bgra4444:
                    return 2;
                case SurfaceFormat.Bgra5551:
                    return 2;
                case SurfaceFormat.Alpha8:
                    return 1;
                default:
                    throw new NotImplementedException();
            }
        }

        public static int GetTypeSize(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 4;

                case VertexElementFormat.Vector2:
                    return 8;

                case VertexElementFormat.Vector3:
                    return 12;

                case VertexElementFormat.Vector4:
                    return 0x10;

                case VertexElementFormat.Color:
                    return 4;

                case VertexElementFormat.Byte4:
                    return 4;

                case VertexElementFormat.Short2:
                    return 4;

                case VertexElementFormat.Short4:
                    return 8;

                case VertexElementFormat.NormalizedShort2:
                    return 4;

                case VertexElementFormat.NormalizedShort4:
                    return 8;

                case VertexElementFormat.HalfVector2:
                    return 4;

                case VertexElementFormat.HalfVector4:
                    return 8;
            }
            return 0;
        }
    }
}
