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
        internal T[] _pixelData;
        protected SurfaceFormat _format = SurfaceFormat.Color;

        public PixelBitmapContent(int width, int height)
        {
            Height = height;
            Width = width;

            _pixelData = new T[height * width];

            var counter = 0;
            for (int y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    _pixelData[counter++] = new T();
                }
            }
        }

        public override byte[] GetPixelData()
        {
            var dataSize = Width * Height * _format.Size();
            int dataSize2 = Marshal.SizeOf(typeof(T));
            var outputData = new byte[dataSize];

            var dataHandle = GCHandle.Alloc(_pixelData, GCHandleType.Pinned);
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64());

            Marshal.Copy(dataPtr, outputData, 0, dataSize);

            dataHandle.Free();

            return outputData;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            var dataHandle = GCHandle.Alloc(_pixelData, GCHandleType.Pinned);
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64());

            // Copy from the temporary buffer to the destination array
            int dataSize = Marshal.SizeOf(typeof(T));

            Marshal.Copy(sourceData, 0, dataPtr, sourceData.Length);

            dataHandle.Free();
        }

        // TODO: Docs say the value for this needs to be modifiable?
        public T[] GetRow(int y)
        {
            var output = new T[Width];

            for (var x = 0; x < Width; x++)
                output[x] = _pixelData[(y * Height) + x];

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

        public T GetPixel(int x, int y)
        {
            return _pixelData[(y * Width) + x];
        }

        public void SetPixel(int x, int y, T value)
        {
            _pixelData[(y * Width) + x] = value;
        }

        public void ReplaceColor(T originalColor, T newColor)
        {
            for (var x = 0; x < _pixelData.Length; x++)
            {
                if (_pixelData[x].Equals(originalColor))
                    _pixelData[x] = newColor;
            }
        }

        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destRegion)
        {
            throw new NotImplementedException();
        }

        protected override bool TryCopyTo(BitmapContent destBitmap, Rectangle sourceRegion, Rectangle destRegion)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
