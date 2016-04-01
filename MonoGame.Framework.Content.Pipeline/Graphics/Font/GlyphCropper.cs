// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Crops unused space from around the edge of a glyph bitmap.
	internal static class GlyphCropper
	{
		public static void Crop(Glyph glyph)
		{
			// Crop the top.
			while ((glyph.Subrect.Height > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.Subrect.X, glyph.Subrect.Y, glyph.Subrect.Width, 1)))
			{
				glyph.Subrect.Y++;
				glyph.Subrect.Height--;

				glyph.YOffset++;
			}

			// Crop the bottom.
			while ((glyph.Subrect.Height > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.Subrect.X, glyph.Subrect.Bottom - 1, glyph.Subrect.Width, 1)))
			{
				glyph.Subrect.Height--;
			}

			// Crop the left.
			while ((glyph.Subrect.Width > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.Subrect.X, glyph.Subrect.Y, 1, glyph.Subrect.Height)))
			{
				glyph.Subrect.X++;
				glyph.Subrect.Width--;

				glyph.XOffset++;
			}

			// Crop the right.
			while ((glyph.Subrect.Width > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.Subrect.Right - 1, glyph.Subrect.Y, 1, glyph.Subrect.Height)))
			{
				glyph.Subrect.Width--;

				glyph.XAdvance++;
			}
		}
	}

}
