// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class PixelBitmapContent<T> : BitmapContent where T : struct, IEquatable<T>
    {
        internal T[][] _pixelData;

        internal SurfaceFormat _format = SurfaceFormat.Color;

        public PixelBitmapContent(int width, int height)
        {
            Height = height;
            Width = width;

            _pixelData = new T[height][];

            for (int y = 0; y < height; y++)
                _pixelData[y] = new T[width];
        }

        public override byte[] GetPixelData()
        {
            if (_format != SurfaceFormat.Color)
                throw new NotImplementedException();

            var formatSize = _format.GetSize();
            var dataSize = Width * Height * formatSize;
            var outputData = new byte[dataSize];

            for (var x = 0; x < Height; x++)
            {
                var dataHandle = GCHandle.Alloc(_pixelData[x], GCHandleType.Pinned);
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64());

                Marshal.Copy(dataPtr, outputData, (formatSize * x * Width), (Width * formatSize));

                dataHandle.Free();
            }

            return outputData;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            var size = _format.GetSize();

            for(var x = 0; x < Height; x++)
            {
                var dataHandle = GCHandle.Alloc(_pixelData[x], GCHandleType.Pinned);
                var dataPtr = (IntPtr)dataHandle.AddrOfPinnedObject().ToInt64();

                Marshal.Copy(sourceData, (x * Width * size), dataPtr, Width * size);

                dataHandle.Free();
            }
        }

        public T[] GetRow(int y)
        {
            if (y >= Height)
                throw new ArgumentOutOfRangeException("y");

            return _pixelData[y];
        }

        /// <summary>
        /// Gets the corresponding GPU texture format for the specified bitmap type.
        /// </summary>
        /// <param name="format">Format being retrieved.</param>
        /// <returns>The GPU texture format of the bitmap type.</returns>
        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = _format;
            return true;
        }

        public T GetPixel(int x, int y)
        {
            return _pixelData[y][x];
        }

        public void SetPixel(int x, int y, T value)
        {
            _pixelData[y][x] = value;
        }

        public void ReplaceColor(T originalColor, T newColor)
        {
            for (var y = 0; y < Height; y++ )
            {
                for (var x = 0; x < Width; x++)
                {
                    if (_pixelData[y][x].Equals(originalColor))
                        _pixelData[y][x] = newColor;
                }
            }
        }

        protected override bool TryCopyFrom(BitmapContent srcBitmap, Rectangle srcRect, Rectangle dstRect)
        {
            return false;
        }

        protected override bool TryCopyTo(BitmapContent dstBitmap, Rectangle srcRect, Rectangle dstRect)
        {
            SurfaceFormat format;
            if (!dstBitmap.TryGetFormat(out format) || format != _format)
                return false;

            var dst = dstBitmap as PixelBitmapContent<T>;
            for (var i = 0; i < dstRect.Height; i++)
            {
                var dy = dstRect.Y + i;
                for (var j = 0; j < dstRect.Width; j++)
                {
                    var dx = dstRect.X + j;

                    var uv = new Vector2()
                    {
                        X = j / (float)dstRect.Width,
                        Y = i / (float)dstRect.Height,
                    };

                    var sx = MathHelper.Clamp((int)Math.Round(uv.X * srcRect.Width) + srcRect.X, 0, Width - 1);
                    var sy = MathHelper.Clamp((int)Math.Round(uv.Y * srcRect.Height) + srcRect.Y, 0, Height - 1);
                    var pixel = GetPixel(sx, sy);
                    dst.SetPixel(dx, dy, pixel);
                }
            }

            return true;
        }
    }
}
