using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public class DynamicVertexBuffer : VertexBuffer
    {
		internal int UserOffset;

		public bool IsContentLost { get { return false; } }

        public DynamicVertexBuffer(GraphicsDevice graphics, Type type, int vertexCount, BufferUsage bufferUsage)
            : base(graphics, type, vertexCount, bufferUsage)
        {
        }

        public DynamicVertexBuffer (GraphicsDevice graphics, VertexDeclaration vertexDecs, int vertexCount, BufferUsage bufferUsage)
            : base (graphics,vertexDecs.GetType(), vertexCount,bufferUsage)
        {
        }

		public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            base.SetData<T>(offsetInBytes, data, startIndex, elementCount, VertexDeclaration.VertexStride, options);
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
			base.SetData<T>(0, data, startIndex, elementCount, VertexDeclaration.VertexStride, options);
        }
    }
}