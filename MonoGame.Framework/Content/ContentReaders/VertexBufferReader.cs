﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    class VertexBufferReader : ContentTypeReader<VertexBuffer>
    {
        protected internal override VertexBuffer Read(ContentReader input, VertexBuffer existingInstance)
        {
            var declaration = input.ReadRawObject<VertexDeclaration>();
            var vertexCount = (int)input.ReadUInt32();
            int dataSize = vertexCount * declaration.VertexStride;
            byte[] data = ContentManager.ScratchBufferPool.Get(dataSize);
            input.Read(data, 0, dataSize);

            var buffer = new VertexBuffer(input.GetGraphicsDevice(), declaration, vertexCount, BufferUsage.None);
            buffer.SetData(data, 0, dataSize);
            ContentManager.ScratchBufferPool.Return(data);
            return buffer;
        }
    }
}
