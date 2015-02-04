// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    class IndexBufferReader : ContentTypeReader<IndexBuffer>
    {
        protected internal override IndexBuffer Read(ContentReader input, IndexBuffer existingInstance)
        {
            IndexBuffer indexBuffer = existingInstance;

            bool sixteenBits = input.ReadBoolean();
            int dataSize = input.ReadInt32();
            byte[] data = MemoryPool.Current.GetPooledBuffer(dataSize);
            input.Read(data, 0, dataSize);

            if (indexBuffer == null)
            {
                indexBuffer = new IndexBuffer(input.GraphicsDevice,
                    sixteenBits ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits, 
                    dataSize / (sixteenBits ? 2 : 4), BufferUsage.None);
            }

            indexBuffer.SetData(data, 0, dataSize);
            MemoryPool.Current.PoolBuffer(data);
            return indexBuffer;
        }
    }
}
