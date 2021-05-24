// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class Image2DArrayReader : ContentTypeReader<Image2DArray>
    {
        public Image2DArrayReader()
        {
        }

        protected internal override Image2DArray Read(ContentReader reader, Image2DArray existingInstance)
        {
            return Read(reader, existingInstance, int.MaxValue);
        }

        internal Image2DArray Read(ContentReader reader, Image2DArray existingInstance, int maxCount)
        {
            SurfaceFormat surfaceFormat = (SurfaceFormat)reader.ReadInt32();
            SurfaceFormat convertedFormat = Texture2DReader.GetConvertedFormat(surfaceFormat, false, false);

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int levelCount = reader.ReadInt32();
            int actualLevelCount = Math.Min(levelCount, maxCount);

            Image2DArray images = existingInstance ?? new Image2DArray(width, height, new Image2D[actualLevelCount]);

            for (int level = 0; level < actualLevelCount; level++)
            {
                int levelDataSizeInBytes = reader.ReadInt32();
                byte[] levelData = ContentManager.ScratchBufferPool.Get(levelDataSizeInBytes);
                reader.Read(levelData, 0, levelDataSizeInBytes);
                int levelWidth = Math.Max(width >> level, 1);
                int levelHeight = Math.Max(height >> level, 1);

                // Convert the image data if required
                byte[] actualLevelData = Texture2DReader.ConvertLevelData(
                    surfaceFormat, convertedFormat, false, false, false,
                    levelData, levelWidth, levelHeight, ref levelDataSizeInBytes);

                Image2D image = images[level];
                bool needsCopy = true;

                if (image == null)
                {
                    if (actualLevelData == levelData)
                    {
                        image = new Image2D(levelWidth, levelHeight);
                    }
                    else
                    {
                        image = new Image2D(actualLevelData, levelWidth, levelHeight);
                        needsCopy = false;
                    }
                    images[level] = image;
                }

                if (needsCopy)
                    Buffer.BlockCopy(actualLevelData, 0, image.PixelData, 0, levelDataSizeInBytes);

                ContentManager.ScratchBufferPool.Return(levelData);
            }

            return images;
        }
    }
}
