// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Helper for arranging many small bitmaps onto a single larger surface.
	internal static class GlyphPacker
	{
		public static BitmapContent ArrangeGlyphs(Glyph[] sourceGlyphs, bool requirePOT, bool requireSquare)
		{
			// Build up a list of all the glyphs needing to be arranged.
			var glyphs = new List<ArrangedGlyph>();

			for (int i = 0; i < sourceGlyphs.Length; i++)
			{
				var glyph = new ArrangedGlyph();

				glyph.Source = sourceGlyphs[i];

				// Leave a one pixel border around every glyph in the output bitmap.
				glyph.Width = sourceGlyphs[i].Subrect.Width + 2;
				glyph.Height = sourceGlyphs[i].Subrect.Height + 2;

				glyphs.Add(glyph);
			}

			// Sort so the largest glyphs get arranged first.
			glyphs.Sort(CompareGlyphSizes);

			// Work out how big the output bitmap should be.
			int outputWidth = GuessOutputWidth(sourceGlyphs);
			int outputHeight = 0;

			// Choose positions for each glyph, one at a time.
			for (int i = 0; i < glyphs.Count; i++)
			{
				PositionGlyph(glyphs, i, outputWidth);

				outputHeight = Math.Max(outputHeight, glyphs[i].Y + glyphs[i].Height);
			}

			// Create the merged output bitmap.
			outputHeight = MakeValidTextureSize(outputHeight, requirePOT);

			if (requireSquare)
            {
				outputHeight = Math.Max (outputWidth, outputHeight);
				outputWidth = outputHeight;
			}

			return CopyGlyphsToOutput(glyphs, outputWidth, outputHeight);
		}

		// Once arranging is complete, copies each glyph to its chosen position in the single larger output bitmap.
		static BitmapContent CopyGlyphsToOutput(List<ArrangedGlyph> glyphs, int width, int height)
		{
            var output = new PixelBitmapContent<Color>(width, height);

			foreach (var glyph in glyphs)
			{
				var sourceGlyph = glyph.Source;
				var sourceRegion = sourceGlyph.Subrect;
				var destinationRegion = new Rectangle(glyph.X + 1, glyph.Y + 1, sourceRegion.Width, sourceRegion.Height);

				BitmapContent.Copy(sourceGlyph.Bitmap, sourceRegion, output, destinationRegion);

				sourceGlyph.Bitmap = output;
				sourceGlyph.Subrect = destinationRegion;
			}

			return output;
		}


		// Internal helper class keeps track of a glyph while it is being arranged.
		class ArrangedGlyph
		{
			public Glyph Source;

			public int X;
			public int Y;

			public int Width;
			public int Height;
		}


		// Works out where to position a single glyph.
		static void PositionGlyph(List<ArrangedGlyph> glyphs, int index, int outputWidth)
		{
			int x = 0;
			int y = 0;

			while (true)
			{
				// Is this position free for us to use?
				int intersects = FindIntersectingGlyph(glyphs, index, x, y);

				if (intersects < 0)
				{
					glyphs[index].X = x;
					glyphs[index].Y = y;

					return;
				}

				// Skip past the existing glyph that we collided with.
				x = glyphs[intersects].X + glyphs[intersects].Width;

				// If we ran out of room to move to the right, try the next line down instead.
				if (x + glyphs[index].Width > outputWidth)
				{
					x = 0;
					y++;
				}
			}
		}


		// Checks if a proposed glyph position collides with anything that we already arranged.
		static int FindIntersectingGlyph(List<ArrangedGlyph> glyphs, int index, int x, int y)
		{
			int w = glyphs[index].Width;
			int h = glyphs[index].Height;

			for (int i = 0; i < index; i++)
			{
				if (glyphs[i].X >= x + w)
					continue;

				if (glyphs[i].X + glyphs[i].Width <= x)
					continue;

				if (glyphs[i].Y >= y + h)
					continue;

				if (glyphs[i].Y + glyphs[i].Height <= y)
					continue;

				return i;
			}

			return -1;
		}


		// Comparison function for sorting glyphs by size.
		static int CompareGlyphSizes(ArrangedGlyph a, ArrangedGlyph b)
		{
			const int heightWeight = 1024;

			int aSize = a.Height * heightWeight + a.Width;
			int bSize = b.Height * heightWeight + b.Width;

			if (aSize != bSize)
				return bSize.CompareTo(aSize);
			else
				return a.Source.Character.CompareTo(b.Source.Character);
		}


		// Heuristic guesses what might be a good output width for a list of glyphs.
		static int GuessOutputWidth(Glyph[] sourceGlyphs)
		{
			int maxWidth = 0;
			int totalSize = 0;

			foreach (var glyph in sourceGlyphs)
			{
				maxWidth = Math.Max(maxWidth, glyph.Bitmap.Width);
				totalSize += glyph.Bitmap.Width * glyph.Bitmap.Height;
			}

			int width = Math.Max((int)Math.Sqrt(totalSize), maxWidth);

			return MakeValidTextureSize(width, true);
		}


		// Rounds a value up to the next larger valid texture size.
		static int MakeValidTextureSize(int value, bool requirePowerOfTwo)
		{
			// In case we want to compress the texture, make sure the size is a multiple of 4.
			const int blockSize = 4;

			if (requirePowerOfTwo)
			{
				// Round up to a power of two.
				int powerOfTwo = blockSize;

				while (powerOfTwo < value)
					powerOfTwo <<= 1;

				return powerOfTwo;
			}
			else
			{
				// Round up to the specified block size.
				return (value + blockSize - 1) & ~(blockSize - 1);
			}
		}
	}

}

