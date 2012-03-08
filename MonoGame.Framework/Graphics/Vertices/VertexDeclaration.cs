using System;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif WINRT

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
		
		internal void Apply()
		{
			Apply (IntPtr.Zero);
		}

		internal void Apply(IntPtr offset)
		{
#if ES11
			GLStateManager.VertexArray(true);

			bool normal = false;
			bool texcoord = false;
			bool color = false;
			
			foreach (var ve in this.GetVertexElements())
			{
				IntPtr elementOffset = (IntPtr)(offset.ToInt64 () + ve.Offset);
				switch (ve.VertexElementUsage)
				{
				case VertexElementUsage.Position:
					GL.VertexPointer(
						ve.VertexElementFormat.OpenGLNumberOfElements(),
						ve.VertexElementFormat.OpenGLVertexPointerType(),
						this.VertexStride,
						elementOffset);
					GL.VertexAttribPointer(GraphicsDevice.attributePosition,
						ve.VertexElementFormat.OpenGLNumberOfElements(),
						ve.VertexElementFormat.OpenGLVertexAttribPointerType(),
						false,
						this.VertexStride,
						elementOffset);
					break;
				case VertexElementUsage.Color:
					GL.ColorPointer(
						ve.VertexElementFormat.OpenGLNumberOfElements(),
						ve.VertexElementFormat.OpenGLColorPointerType(),
						this.VertexStride,
						elementOffset);
					color = true;
					break;
				case VertexElementUsage.Normal:
					GL.NormalPointer(
						ve.VertexElementFormat.OpenGLNormalPointerType(),
						this.VertexStride,
						elementOffset);
					normal = true;
					break;
				case VertexElementUsage.TextureCoordinate:
					GL.TexCoordPointer(
						ve.VertexElementFormat.OpenGLNumberOfElements(),
						ve.VertexElementFormat.OpenGLTexCoordPointerType(),
						this.VertexStride,
						elementOffset
					);
					texcoord = true;
					break;
				default:
					throw new NotImplementedException();
				}
			}
			GLStateManager.ColorArray (color);
			GLStateManager.NormalArray(normal);
			GLStateManager.TextureCoordArray(texcoord);
#else

            // TODO: This is executed on every draw call... can we not
            // allocate a vertex declaration once and just re-apply it?

			bool[] enabledAttributes = new bool[16];
			foreach (var ve in this.GetVertexElements())
			{
				IntPtr elementOffset = (IntPtr)(offset.ToInt64 () + ve.Offset);
				int attributeLocation = -1;
				
				switch (ve.VertexElementUsage) {
				case VertexElementUsage.Position: attributeLocation = GraphicsDevice.attributePosition + ve.UsageIndex; break;
				case VertexElementUsage.Normal: attributeLocation = GraphicsDevice.attributeNormal; break;
				case VertexElementUsage.Color: attributeLocation = GraphicsDevice.attributeColor; break;
				case VertexElementUsage.BlendIndices: attributeLocation = GraphicsDevice.attributeBlendIndicies; break;
				case VertexElementUsage.BlendWeight: attributeLocation = GraphicsDevice.attributeBlendWeight; break;
				case VertexElementUsage.TextureCoordinate: attributeLocation = GraphicsDevice.attributeTexCoord + ve.UsageIndex; break;
				default:
					throw new NotImplementedException();
				}
				GL.VertexAttribPointer(attributeLocation,
				                       ve.VertexElementFormat.OpenGLNumberOfElements(),
				                       ve.VertexElementFormat.OpenGLVertexAttribPointerType(),
                                       ve.OpenGLVertexAttribNormalized(),
				                       this.VertexStride,
				                       elementOffset);
				enabledAttributes[attributeLocation] = true;
			}
			
			for (int i=0; i<16; i++) {
				GLStateManager.VertexAttribArray(i, enabledAttributes[i]);
			}
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