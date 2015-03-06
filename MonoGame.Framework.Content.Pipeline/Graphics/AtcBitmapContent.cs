// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using ATI.TextureConverter;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class AtcBitmapContent : BitmapContent
    {
        internal byte[] _bitmapData;

        public AtcBitmapContent()
            : base()
        {
        }

        public AtcBitmapContent(int width, int height)
            : base(width, height)
        {
        }

        public override byte[] GetPixelData()
        {
            return _bitmapData;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            _bitmapData = sourceData;
        }

		protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
			// Region copy is not supported.
			if (destinationRegion.Top != 0 ||
				destinationRegion.Left != 0 ||
				destinationRegion.Width != Width ||
				destinationRegion.Height != Height)
				return false;
			if (sourceRegion.Top != 0 ||
				sourceRegion.Left != 0 ||
				sourceRegion.Width != sourceBitmap.Width ||
				sourceRegion.Height != sourceBitmap.Height)
				return false;

			// If needed, convert to floating point format
			if (!(sourceBitmap is PixelBitmapContent<Color>))
			{
				var colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
				BitmapContent.Copy(sourceBitmap, colorBitmap);
				sourceBitmap = colorBitmap;
			}

			SurfaceFormat format;
			TryGetFormat(out format);

			ATICompressor.CompressionFormat targetFormat;
			switch (format) {
				case SurfaceFormat.RgbaATCExplicitAlpha:
					targetFormat = ATICompressor.CompressionFormat.AtcRgbaExplicitAlpha;
					break;
				case SurfaceFormat.RgbaATCInterpolatedAlpha:
					targetFormat = ATICompressor.CompressionFormat.AtcRgbaInterpolatedAlpha;
					break;
				default:
					return false;
			}

			var sourceData = sourceBitmap.GetPixelData();
			var compressedData = ATICompressor.Compress(sourceData, Width, Height, targetFormat);
			SetPixelData(compressedData);

			return true;
        }

        protected override bool TryCopyTo(BitmapContent destBitmap, Rectangle sourceRegion, Rectangle destRegion)
        {
            // No support for copying from a ATC texture yet
            return false;
        }
    }
}
