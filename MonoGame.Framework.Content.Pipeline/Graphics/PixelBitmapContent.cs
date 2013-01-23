// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class PixelBitmapContentBase<T> : BitmapContent where T : struct, IEquatable<T>
    {
        internal T[,] _pixelData;
        protected SurfaceFormat _format;

        public virtual T GetPixel(int x, int y)
        {
            if (x * y == 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException("x or y");

            return _pixelData[y, x];
        }

        public override byte[] GetPixelData()
        {
            throw new NotImplementedException();
        }

        public virtual T[] GetRow(int y)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public virtual void SetPixel(int x, int y, T value)
        {
            throw new NotImplementedException();
        }

        public virtual void ReplaceColor(T originalColor, T newColor)
        {
            throw new NotImplementedException();
        }
    }
}
