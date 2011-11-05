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
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public class VertexDeclaration : GraphicsResource
    {
        // Fields
        internal VertexElement[] _elements;
        internal int _vertexStride;

        public VertexDeclaration(params VertexElement[] elements)
        {
            if ((elements == null) || (elements.Length == 0))
            {
                throw new ArgumentNullException("elements", "Elements cannot be empty");
            }
            else
            {
                VertexElement[] elementArray = (VertexElement[])elements.Clone();
                this._elements = elementArray;
                this._vertexStride = getVertexStride(elementArray);
            }
        }

        private static int getVertexStride(VertexElement[] elements)
        {
            int max = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                int start = elements[i].Offset + elements[i].VertexElementFormat.GetTypeSize();
                if (max < start)
                {
                    max = start;
                }
            }

            return max;
        }

        public VertexDeclaration(int vertexStride, params VertexElement[] elements)
        {
            if ((elements == null) || (elements.Length == 0))
            {
                throw new ArgumentNullException("elements", "Elements cannot be empty");
            }
            else
            {
                VertexElement[] elementArray = (VertexElement[])elements.Clone();
                this._elements = elementArray;
                this._vertexStride = vertexStride;
            }
        }

        internal static VertexDeclaration FromType(Type vertexType)
        {
            if (vertexType == null)
            {
                throw new ArgumentNullException("vertexType", "Cannot be null");
            }
            if (!vertexType.IsValueType)
            {
                object[] args = new object[] { vertexType };
                throw new ArgumentException("vertexType", "Must be value type");
            }
            IVertexType type = Activator.CreateInstance(vertexType) as IVertexType;
            if (type == null)
            {
                object[] objArray3 = new object[] { vertexType };
                throw new ArgumentException("vertexData does not inherit IVertexType");
            }
            VertexDeclaration vertexDeclaration = type.VertexDeclaration;
            if (vertexDeclaration == null)
            {
                object[] objArray2 = new object[] { vertexType };
                throw new Exception("VertexDeclaration cannot be null");
            }

            return vertexDeclaration;
        }

        public static void PrepareForUse(VertexDeclaration vd)
        {
            GLStateManager.VertexArray(true);

            bool normal = false;
            bool texcoord = false;

            foreach (var ve in vd.GetVertexElements())
            {
                switch (ve.VertexElementUsage)
                {
                    case VertexElementUsage.Position:
                        GL11.VertexPointer(
                            ve.VertexElementFormat.OpenGLNumberOfElements(),
#if WINDOWS
                            ve.VertexElementFormat.OpenGLVertexPointerType(),
#else
                            ve.VertexElementFormat.OpenGLValueType(),
#endif
                            vd.VertexStride,
                            //ve.Offset
                            (IntPtr)ve.Offset
                            );
                        break;
                    case VertexElementUsage.Color:
                        GL11.ColorPointer(
                            ve.VertexElementFormat.OpenGLNumberOfElements(),
#if WINDOWS
                            ve.VertexElementFormat.OpenGLColorPointerType(),
#else
                            ve.VertexElementFormat.OpenGLValueType(),
#endif
                            vd.VertexStride,
                            //ve.Offset
                            (IntPtr)ve.Offset
                            );
                        break;
                    case VertexElementUsage.Normal:
                        GL11.NormalPointer(
#if WINDOWS
                            ve.VertexElementFormat.OpenGLNormalPointerType(),
#else
                            ve.VertexElementFormat.OpenGLValueType(),
#endif
                            vd.VertexStride,
                            //ve.Offset
                            (IntPtr)ve.Offset
                            );
                        normal = true;
                        break;
                    case VertexElementUsage.TextureCoordinate:
                        GL11.TexCoordPointer(
                            ve.VertexElementFormat.OpenGLNumberOfElements(),
#if WINDOWS
                            ve.VertexElementFormat.OpenGLTexCoordPointerType(),
#else
                            ve.VertexElementFormat.OpenGLValueType(),
#endif
                            vd.VertexStride,
                            //ve.Offset
                            (IntPtr)ve.Offset
                            );
                        texcoord = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            GLStateManager.TextureCoordArray(texcoord);
            GLStateManager.NormalArray(normal);
        }

        public VertexElement[] GetVertexElements()
        {
            return (VertexElement[])this._elements.Clone();
        }

        // Properties
        public int VertexStride
        {
            get
            {
                return this._vertexStride;
            }
        }
    }
}