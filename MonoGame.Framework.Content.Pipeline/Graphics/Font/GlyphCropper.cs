// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics;

// Crops unused space from around the edge of a glyph bitmap.
internal static class GlyphCropper
{
    public static void Crop(GlyphData glyph)
    {
        // Crop the top.
        while ((glyph.SubRect.Height > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.SubRect.X, glyph.SubRect.Y, glyph.SubRect.Width, 1)))
        {
            glyph.SubRect.Y++;
            glyph.SubRect.Height--;

            glyph.YOffset++;
        }

        // Crop the bottom.
        while ((glyph.SubRect.Height > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.SubRect.X, glyph.SubRect.Bottom - 1, glyph.SubRect.Width, 1)))
        {
            glyph.SubRect.Height--;
        }

        // Crop the left.
        while ((glyph.SubRect.Width > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.SubRect.X, glyph.SubRect.Y, 1, glyph.SubRect.Height)))
        {
            glyph.SubRect.X++;
            glyph.SubRect.Width--;

            glyph.XOffset++;
        }

        // Crop the right.
        while ((glyph.SubRect.Width > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.SubRect.Right - 1, glyph.SubRect.Y, 1, glyph.SubRect.Height)))
        {
            glyph.SubRect.Width--;

            glyph.XAdvance++;
        }
    }
}
