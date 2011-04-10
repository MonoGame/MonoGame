using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;

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
