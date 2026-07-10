// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Assorted helpers for doing useful things with bitmaps.
	internal static class BitmapUtils
	{
        // Checks whether an area of a bitmap contains entirely the specified alpha value.
        public static bool IsAlphaEntirely(byte expectedAlpha, BitmapContent bitmap, Rectangle? region = null)
		{
            var bitmapRegion = region.HasValue ? region.Value : new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            // Works with PixelBitmapContent<byte> at this stage
            if (bitmap is PixelBitmapContent<byte>)
            {
                var bmp = bitmap as PixelBitmapContent<byte>;
                for (int y = 0; y < bitmapRegion.Height; y++)
                {
                    for (int x = 0; x < bitmapRegion.Width; x++)
                    {
                        var alpha = bmp.GetPixel(bitmapRegion.X + x, bitmapRegion.Y + y);
                        if (alpha != expectedAlpha)
                            return false;
                    }
                }
                return true;
            }
            else if (bitmap is PixelBitmapContent<Color>)
            {
                var bmp = bitmap as PixelBitmapContent<Color>;
                for (int y = 0; y < bitmapRegion.Height; y++)
                {
                    for (int x = 0; x < bitmapRegion.Width; x++)
                    {
                        var alpha = bmp.GetPixel(bitmapRegion.X + x, bitmapRegion.Y + y).A;
                        if (alpha != expectedAlpha)
                            return false;
                    }
                }
                return true;
            }
            throw new ArgumentException("Expected PixelBitmapContent<byte> or PixelBitmapContent<Color>, got " + bitmap.GetType().Name, "bitmap");
		}
	}
}
