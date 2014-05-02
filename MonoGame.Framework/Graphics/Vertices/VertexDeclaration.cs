// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if WINRT
using System.Reflection;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class VertexDeclaration : GraphicsResource
	{
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
                HashKey = MonoGame.Utilities.Hash.ComputeHash(bytes);
            }
        }

		private static int GetVertexStride(VertexElement[] elements)
		{
			int max = 0;
			for (var i = 0; i < elements.Length; i++)
			{
                var start = elements[i].Offset + elements[i].VertexElementFormat.GetSize();
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
				throw new ArgumentException("vertexType", "Must be value type");
			}

            var type = Activator.CreateInstance(vertexType) as IVertexType;
			if (type == null)
			{
				throw new ArgumentException("vertexData does not inherit IVertexType");
			}

            var vertexDeclaration = type.VertexDeclaration;
			if (vertexDeclaration == null)
			{
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
    }
}