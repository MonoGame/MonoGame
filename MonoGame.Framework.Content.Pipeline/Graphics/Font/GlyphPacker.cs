// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            // Keep a record of glyphs in a dictionary so we can look them up based on
            // their index later for some optimisations
            Dictionary<int, ArrangedGlyph> glyphDict = new Dictionary<int, ArrangedGlyph>();
            for (int i = 0; i < glyphs.Count; i++)
            {
                glyphDict.Add(i, glyphs[i]);
            }

			for (int i = 0; i < glyphs.Count; i++)
			{
				PositionGlyph(glyphDict, i, outputWidth);

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
		static void PositionGlyph(Dictionary<int, ArrangedGlyph> glyphs, int index, int outputWidth)
		{
            int x = 0;
            int y = 0;

            bool incrementSet = false;
            int nextYIncrement = int.MaxValue;

            // Find a starting Y value
            int startY = 0;
            for (int i = 0; i < index; i++)
            {
                // If a glyph thats smaller than us is placed here, we know there's no point
                // searching before it, as there will be no spaces suitable for us
                // So we iterate over all the glyphs present to confirm where we should start
                // This saves countless comparisons later on
                if (glyphs[i].Width <= glyphs[index].Width &&
                    glyphs[i].Height <= glyphs[index].Height)
                {
                    startY = Math.Max(startY, glyphs[i].Y);
                }
            }

            y = startY;
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

                // With every glyph we hit, calculate its bottom position, keep track of the smallest
                // Y movement required to hit the bottom of a glyph during our checks along this row.
                // If we fail to find a spot on this row, we know there wont be any space before the new
                // bottom value of the smallest glyph we found, so we increment our Y by that amount
                // instead of by 1 to save on watsed checks
                int intersectsBottom = glyphs[intersects].Y + glyphs[intersects].Height;
                int incrementToHitBottom = intersectsBottom - y;
                if (incrementToHitBottom < nextYIncrement)
                {
                    nextYIncrement = incrementToHitBottom;
                    incrementSet = true;
                }

				// Skip past the existing glyph that we collided with.
				x = glyphs[intersects].X + glyphs[intersects].Width;

				// If we ran out of room to move to the right, try the next line down instead.
				if (x + glyphs[index].Width > outputWidth)
				{
					x = 0;
					y += incrementSet ? nextYIncrement : 1;
                    nextYIncrement = int.MaxValue;
                    incrementSet = false;
				}
			}
		}


		// Checks if a proposed glyph position collides with anything that we already arranged.
		static int FindIntersectingGlyph(Dictionary<int, ArrangedGlyph> glyphs, int index, int x, int y)
		{
			int w = glyphs[index].Width;
			int h = glyphs[index].Height;

			for (int i = 0; i < index; i++)
			{
                var targetGlyph = glyphs[i];
				if (targetGlyph.X >= x + w)
					continue;

				if (targetGlyph.X + targetGlyph.Width <= x)
					continue;

				if (targetGlyph.Y >= y + h)
					continue;

				if (targetGlyph.Y + targetGlyph.Height <= y)
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

