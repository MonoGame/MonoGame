// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    class VertexBufferReader : ContentTypeReader<VertexBuffer>
    {
        protected internal override VertexBuffer Read(ContentReader input, VertexBuffer existingInstance)
        {
            VertexBuffer vertexBuffer = existingInstance;

            var declaration = input.ReadRawObject<VertexDeclaration>();
            var vertexCount = (int)input.ReadUInt32();
            int dataSize = vertexCount * declaration.VertexStride;
            byte[] data = MemoryPool.Current.GetPooledBuffer(dataSize);
            input.Read(data, 0, dataSize);

            if(vertexBuffer == null)
            { 
                vertexBuffer = new VertexBuffer(input.GraphicsDevice, declaration, vertexCount, BufferUsage.None);
            }

            vertexBuffer.SetData(data, 0, dataSize);
            MemoryPool.Current.PoolBuffer(data);
            return vertexBuffer;
        }
    }
}
