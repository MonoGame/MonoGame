using System;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif PSS
using Sce.Pss.Core.Graphics;
#elif DIRECTX
using System.Reflection;
using System.Collections.Generic;
#else
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class VertexDeclaration : GraphicsResource
	{
#if PSS
        private VertexFormat[] _vertexFormat;
#endif

		private VertexElement[] _elements;
        private int _vertexStride;

        /// <summary>
        /// A hash value which can be used to compare declarations.
        /// </summary>
        internal int HashKey { get; private set; }


		public VertexDeclaration(params VertexElement[] elements)
            : this( GetVertexStride(elements), elements)
		{
		}

        public VertexDeclaration(int vertexStride, params VertexElement[] elements)
        {
            if ((elements == null) || (elements.Length == 0))
                throw new ArgumentNullException("elements", "Elements cannot be empty");

            var elementArray = (VertexElement[])elements.Clone();
            _elements = elementArray;
            _vertexStride = vertexStride;

            // TODO: Is there a faster/better way to generate a
            // unique hashkey for the same vertex layouts?
            {
                var signature = string.Empty;
                foreach (var element in _elements)
                    signature += element;

                var bytes = System.Text.Encoding.UTF8.GetBytes(signature);
                HashKey = Effect.ComputeHash(bytes);
            }
        }

		private static int GetVertexStride(VertexElement[] elements)
		{
			int max = 0;
			for (var i = 0; i < elements.Length; i++)
			{
                var start = elements[i].Offset + elements[i].VertexElementFormat.GetTypeSize();
				if (max < start)
					max = start;
			}

			return max;
		}

        /// <summary>
        /// Returns the VertexDeclaration for Type.
        /// </summary>
        /// <param name="vertexType">A value type which implements the IVertexType interface.</param>
        /// <returns>The VertexDeclaration.</returns>
        /// <remarks>
        /// Prefer to use VertexDeclarationCache when the declaration lookup
        /// can be performed with a templated type.
        /// </remarks>
		internal static VertexDeclaration FromType(Type vertexType)
		{
			if (vertexType == null)
				throw new ArgumentNullException("vertexType", "Cannot be null");

#if WINRT
            if (!vertexType.GetTypeInfo().IsValueType)
#else
            if (!vertexType.IsValueType)
#endif
            {
                var args = new object[] { vertexType };
				throw new ArgumentException("vertexType", "Must be value type");
			}

            var type = Activator.CreateInstance(vertexType) as IVertexType;
			if (type == null)
			{
                var objArray3 = new object[] { vertexType };
				throw new ArgumentException("vertexData does not inherit IVertexType");
			}

            var vertexDeclaration = type.VertexDeclaration;
			if (vertexDeclaration == null)
			{
                var objArray2 = new object[] { vertexType };
				throw new Exception("VertexDeclaration cannot be null");
			}

			return vertexDeclaration;
		}
        
        public VertexElement[] GetVertexElements()
		{
			return (VertexElement[])_elements.Clone();
		}

		public int VertexStride
		{
			get
			{
				return _vertexStride;
			}
		}

#if OPENGL
		internal void Apply()
		{
            Apply(IntPtr.Zero);
        }

		internal void Apply(IntPtr offset)
		{

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
		}
#endif // OPENGL

#if DIRECTX

        internal SharpDX.Direct3D11.InputElement[] GetInputLayout()
        {
            var inputs = new SharpDX.Direct3D11.InputElement[_elements.Length];
            for (var i = 0; i < _elements.Length; i++)
                inputs[i] = _elements[i].GetInputElement();

            return inputs;
        }

#endif
        
#if PSS
        internal VertexFormat[] GetVertexFormat()
        {
            if (_vertexFormat == null)
            {
                _vertexFormat = new VertexFormat[_elements.Length];
                for (int i = 0; i < _vertexFormat.Length; i++)
                    _vertexFormat[i] = PSSHelper.ToVertexFormat(_elements[i].VertexElementFormat);
            }
            
            return _vertexFormat;
        }
#endif
	}
}