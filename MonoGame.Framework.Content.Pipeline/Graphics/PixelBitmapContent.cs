// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class PixelBitmapContent<T> : BitmapContent where T : struct, IEquatable<T>
    {
        internal T[][] _pixelData;

        protected PixelBitmapContent()
        {
        }

        public PixelBitmapContent(int width, int height) : base(width, height)
        {
            _pixelData = new T[height][];

            for (int y = 0; y < height; y++)
                _pixelData[y] = new T[width];
        }

        public T GetPixel(int x, int y)
        {
            checkPixelRange(y, x);

            return _pixelData[y][x];
        }

        public override byte[] GetPixelData()
        {
            SurfaceFormat format;
            if (!TryGetFormat(out format))
                throw new Exception(string.Format("Tried to get pixel Data for PixedBitmapContent<{0}> with an invalid surfaceformat", 
                                                    typeof(T)));

            if (typeof(T) != typeof(Color))
                throw new NotImplementedException("GetPixelData is not supported for Non-Color formats.");

            var pixelSize = format.Size();
            var outputData = new byte[Width * Height * pixelSize];

            for (int i = 0; i < Width; i++) 
            {
                var row = GetRow(i);
                for (int j = 0; j < row.Length; j++) 
                {
                    var col = (row[j] as Color?).Value;

                    outputData[(i * row.Length) + j] = col.R;
                    outputData[(i * row.Length) + j + 1] = col.G;
                    outputData[(i * row.Length) + j + 2] = col.B;
                    outputData[(i * row.Length) + j + 3] = col.A;
                }
            }

            return outputData;
        }

        public T[] GetRow(int y)
        {
            if ((y < 0) || (y >= Height))
                throw new ArgumentOutOfRangeException("y");

            return _pixelData[y];
        }

        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destRegion)
        {
            throw new NotImplementedException();
        }

        protected override bool TryCopyTo(BitmapContent destBitmap, Rectangle sourceRegion, Rectangle destRegion)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetFormat(out SurfaceFormat format)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override void SetPixelData(byte[] sourceData)
        {
            if (typeof(T) != typeof(Color))
                throw new NotImplementedException("SetPixelData is not supported for Non-Color formats.");

            SurfaceFormat format;
            if (!TryGetFormat(out format))
                throw new Exception(string.Format("Tried to get pixel Data for PixedBitmapContent<{0}> with an invalid surfaceformat",
                                                    typeof(T)));

            sourceData.SetPixelData(this, 0, format);
            /*var formatSize = format.Size();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    sourceData.GetPixel((y * formatSize) + (x * formatSize), format, out _pixelData[y][x]);
                }
            }*/
        }

        private void SetPixelData()
        {

        }

        public void SetPixel(int x, int y, T value)
        {
            checkPixelRange(y, x);

            _pixelData[y][x] = value;
        }

        public void ReplaceColor(T originalColor, T newColor)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (_pixelData[y][x].Equals(originalColor))
                    {
                        _pixelData[y][x] = newColor;
                    }
                }
            }
        }

        private void checkPixelRange(int y, int x)
        {
            if (x * y == 0 || x >= Height || y >= Width)
                throw new ArgumentOutOfRangeException("x or y");
        }
    }
}
