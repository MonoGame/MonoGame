// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Imaging;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class PixelBitmapContent<T> : PixelBitmapContentBase<Color> where T : struct, IEquatable<T>
    {
        protected PixelBitmapContent()
        {
        }

        public PixelBitmapContent(int width, int height)
        {
            Height = height;
            Width = width;

            _pixelData = new Color[height, width];

            for (int y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    _pixelData[y, x] = new Color();
                }
            }
        }

        public override byte[] GetPixelData()
        {
            var pixelSize = _format.Size();
            var outputData = new byte[Width * Height * pixelSize];

            var index = 0;
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    outputData[index++] = _pixelData[y, x].R;
                    outputData[index++] = _pixelData[y, x].G;
                    outputData[index++] = _pixelData[y, x].B;
                    outputData[index++] = _pixelData[y, x].A;
                }
            }
            return outputData;
        }

        // TODO: Docs say the value for this needs to be modifiable?
        public override Color[] GetRow(int y)
        {
            var output = new Color[Width];

            for (var x = 0; x < Width; x++)
                output[x] = _pixelData[y, x];

            return output;
        }

        /// <summary>
        /// Gets the corresponding GPU texture format for the specified bitmap type.
        /// </summary>
        /// <param name="format">Format being retrieved.</param>
        /// <returns>The GPU texture format of the bitmap type.</returns>
        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = SurfaceFormat.Color;
            return true;
        }

        internal void SetPixelData(byte[] sourceData, PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    _format = SurfaceFormat.Color;
                    SetPixelData(sourceData);
                    break;
                
                default:
                    throw new NotImplementedException();
            }
        }

        public override void SetPixelData(byte[] sourceData)
        {
            int counter = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _pixelData[y, x].B = sourceData[counter++];
                    _pixelData[y, x].G = sourceData[counter++];
                    _pixelData[y, x].R = sourceData[counter++];
                    _pixelData[y, x].A = sourceData[counter++];
                }
            }
        }
    }
}
