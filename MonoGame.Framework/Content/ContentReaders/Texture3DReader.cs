// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class Texture3DReader : ContentTypeReader<Texture3D>
    {
        protected internal override Texture3D Read(ContentReader input, Texture3D existingInstance)
        {
            Texture3D texture = null;

            SurfaceFormat format = (SurfaceFormat)input.ReadInt32();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            int depth = input.ReadInt32();
            int levelCount = input.ReadInt32();

            if (existingInstance == null)
                texture = new Texture3D(input.GraphicsDevice, width, height, depth, levelCount > 1, format);
            else
                texture = existingInstance;
            
            for (int i = 0; i < levelCount; i++)
            {
                int dataSize = input.ReadInt32();
                byte[] data = MemoryPool.Current.GetPooledBuffer(dataSize);
                input.Read(data, 0, dataSize);
                texture.SetData(i, 0, 0, width, height, 0, depth, data, 0, dataSize);
                MemoryPool.Current.PoolBuffer(data);

                // Calculate dimensions of next mip level.
                width = Math.Max(width >> 1, 1);
                height = Math.Max(height >> 1, 1);
                depth = Math.Max(depth >> 1, 1);
            }

            return texture;
        }
    }
}
