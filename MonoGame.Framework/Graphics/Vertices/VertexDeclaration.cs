using System;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS
using OpenTK.Graphics.OpenGL;
#else

#if ES11
using OpenTK.Graphics.ES11;
#else
using OpenTK.Graphics.ES20;
#endif

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
//            if (!vertexType.IsValueType)
//            {
//                object[] args = new object[] { vertexType };
//                throw new ArgumentException("vertexType", "Must be value type");
//            }
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
#if ES11
		GLStateManager.VertexArray(true);
#else
			GLStateManager.VertexAttribArray(GraphicsDevice.attributePosition, true);
#endif
            bool normal = false;
            bool texcoord = false;
			bool color = false;
			
            foreach (var ve in vd.GetVertexElements())
            {
                    switch (ve.VertexElementUsage)
                    {
                        case VertexElementUsage.Position:
#if ES11
                            GL.VertexPointer(
                                ve.VertexElementFormat.OpenGLNumberOfElements(),
                                ve.VertexElementFormat.OpenGLVertexPointerType(),
                                vd.VertexStride,
                                (IntPtr)ve.Offset
                                );
#else
							GL.VertexAttribPointer(GraphicsDevice.attributePosition,
					            ve.VertexElementFormat.OpenGLNumberOfElements(),
					            ve.VertexElementFormat.OpenGLVertexAttribPointerType(),
					            false,
					            vd.VertexStride,
					            (IntPtr)ve.Offset);
#endif
                            break;
                        case VertexElementUsage.Color:
#if ES11
                            GL.ColorPointer(
                                ve.VertexElementFormat.OpenGLNumberOfElements(),
                                ve.VertexElementFormat.OpenGLColorPointerType(),
                                vd.VertexStride,
                                (IntPtr)ve.Offset
                                );
#else
							GL.VertexAttribPointer(GraphicsDevice.attributeColor,
					            ve.VertexElementFormat.OpenGLNumberOfElements(),
					            ve.VertexElementFormat.OpenGLVertexAttribPointerType(),
					            true,
					            vd.VertexStride,
					            (IntPtr)ve.Offset);
#endif
                            color = true;
                            break;
                        case VertexElementUsage.Normal:
#if ES11
                            GL.NormalPointer(
                            ve.VertexElementFormat.OpenGLNormalPointerType(),
                            vd.VertexStride,
                            (IntPtr)ve.Offset
                            );
#else
							GL.VertexAttribPointer(GraphicsDevice.attributeNormal,
					            ve.VertexElementFormat.OpenGLNumberOfElements(),
					            ve.VertexElementFormat.OpenGLVertexAttribPointerType(),
					            false,
					            vd.VertexStride,
					            (IntPtr)ve.Offset);
#endif
					normal = true;
                            break;
                        case VertexElementUsage.TextureCoordinate:
#if ES11
                            GL.TexCoordPointer(
                            ve.VertexElementFormat.OpenGLNumberOfElements(),
                            ve.VertexElementFormat.OpenGLTexCoordPointerType(),
                            vd.VertexStride,
                            (IntPtr)ve.Offset
                            );
#else
							GL.VertexAttribPointer(GraphicsDevice.attributeTexCoord,
					            ve.VertexElementFormat.OpenGLNumberOfElements(),
					            ve.VertexElementFormat.OpenGLVertexAttribPointerType(),
					            false,
					            vd.VertexStride,
					            (IntPtr)ve.Offset);
#endif
                            texcoord = true;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
            }
#if ES11
            GLStateManager.ColorArray (color);
            GLStateManager.NormalArray(normal);
		GLStateManager.TextureCoordArray(texcoord);
#else
			GLStateManager.VertexAttribArray(GraphicsDevice.attributeColor, color);
			GLStateManager.VertexAttribArray(GraphicsDevice.attributeTexCoord, texcoord);
			GLStateManager.VertexAttribArray(GraphicsDevice.attributeNormal, normal);
#endif
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