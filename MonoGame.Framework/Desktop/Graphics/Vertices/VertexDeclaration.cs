using System;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
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
                VertexElement[] elementArray = (VertexElement[]) elements.Clone();
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
                VertexElement[] elementArray = (VertexElement[]) elements.Clone();
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
            bool color = false;
            bool texcoord = false;
			
            foreach (var ve in vd.GetVertexElements())
            {
                    switch (ve.VertexElementUsage)
                    {
                        case VertexElementUsage.Position:
                            GL.VertexPointer(
                                ve.VertexElementFormat.OpenGLNumberOfElements(),
                                ve.VertexElementFormat.OpenGLVertexPointerType(),
                                vd.VertexStride,
                                (IntPtr)ve.Offset
                                );

                            break;
                        case VertexElementUsage.Color:
                            GL.ColorPointer(
                                ve.VertexElementFormat.OpenGLNumberOfElements(),
                                ve.VertexElementFormat.OpenGLColorPointerType(),
                                vd.VertexStride,
                                (IntPtr)ve.Offset
                                );
                            color = true;
                            break;
                        case VertexElementUsage.Normal:
                            GL.NormalPointer(
                            ve.VertexElementFormat.OpenGLNormalPointerType(),
                            vd.VertexStride,
                            (IntPtr)ve.Offset
                            );
					normal = true;
                            break;
                        case VertexElementUsage.TextureCoordinate:
                            GL.TexCoordPointer(
                            ve.VertexElementFormat.OpenGLNumberOfElements(),
                            ve.VertexElementFormat.OpenGLTexCoordPointerType(),
                            vd.VertexStride,
                            (IntPtr)ve.Offset
                            );
                            texcoord = true;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
            }

            GLStateManager.TextureCoordArray(texcoord);
            GLStateManager.ColorArray(color);
            GLStateManager.NormalArray(normal);
        }

        public VertexElement[] GetVertexElements()
        {
            return (VertexElement[]) this._elements.Clone();
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