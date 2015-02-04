// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class TextureCubeReader : ContentTypeReader<TextureCube>
    {
		
		protected internal override TextureCube Read(ContentReader input, TextureCube existingInstance)
        {
            TextureCube textureCube = null;

			SurfaceFormat surfaceFormat = (SurfaceFormat)input.ReadInt32();
			int size = input.ReadInt32();
			int levels = input.ReadInt32();

            if (existingInstance == null)
                textureCube = new TextureCube(input.GraphicsDevice, size, levels > 1, surfaceFormat);
            else
                textureCube = existingInstance;

            for (int face = 0; face < 6; face++) 
            {
                for (int i=0; i<levels; i++) 
                {
					int faceSize = input.ReadInt32();
                    byte[] faceData = MemoryPool.Current.GetPooledBuffer(faceSize);
                    input.Read(faceData, 0, faceSize);
                    textureCube.SetData<byte>((CubeMapFace)face, i, null, faceData, 0, faceSize);
                    MemoryPool.Current.PoolBuffer(faceData);
                }
            }
            
            return textureCube;
        }
    }
}
