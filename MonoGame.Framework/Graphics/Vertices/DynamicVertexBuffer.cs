using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public class DynamicVertexBuffer : VertexBuffer
    {
        public DynamicVertexBuffer(GraphicsDevice graphics, Type type, int vertexCount, BufferUsage bufferUsage)
            : base(graphics, type, vertexCount, bufferUsage)
        {
        }

        public DynamicVertexBuffer (GraphicsDevice graphics, VertexDeclaration vertexDecs, int vertexCount, BufferUsage bufferUsage)
            : base (graphics,vertexDecs.GetType(), vertexCount,bufferUsage)
        {
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            throw new NotImplementedException();
        }
    }
}