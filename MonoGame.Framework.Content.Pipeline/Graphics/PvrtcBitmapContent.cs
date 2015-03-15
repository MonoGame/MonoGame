// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class PvrtcBitmapContent : BitmapContent
    {
        internal byte[] _bitmapData;
        internal int _bitsPerPixel;

        internal SurfaceFormat _format;

        public PvrtcBitmapContent(int bitsPerPixel)
        {
            System.Diagnostics.Debug.Assert(bitsPerPixel == 2 || bitsPerPixel == 4);

            _bitsPerPixel = bitsPerPixel;

            _format = _bitsPerPixel == 2 ? SurfaceFormat.RgbaPvrtc2Bpp : SurfaceFormat.RgbaPvrtc4Bpp;
        }

        public PvrtcBitmapContent(int bitsPerPixel, int width, int height) : this(bitsPerPixel)
        {
            Width = width;
            Height = height;

            int size;
            if (bitsPerPixel == 2)
                size = (Math.Max(width, 16) * Math.Max(height, 8) * 2 + 7) / 8;
            else
                size = (Math.Max(width, 8) * Math.Max(height, 8) * 4 + 7) / 8;

            _bitmapData = new byte[size];
        }

        public override byte[] GetPixelData()
        {
            return _bitmapData;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            _bitmapData = sourceData;
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
