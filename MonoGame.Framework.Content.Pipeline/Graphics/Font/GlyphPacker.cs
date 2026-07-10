// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    // Helper for arranging many small bitmaps onto a single larger surface.
    internal static class GlyphPacker
    {
        public static BitmapContent ArrangeGlyphs(GlyphData[] sourceGlyphs, bool requirePOT, bool requireSquare)
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
            int guessedWidth = GuessOutputWidth(sourceGlyphs);
            var rectPacker = new MaxRectsBin(guessedWidth, 1024 * 16, GrowRule.Height);

            for (int i = 0; i < glyphs.Count; i++)
            {
                ArrangedGlyph glyph = glyphs[i];
                Rectangle bounds = rectPacker.Insert(glyph.Width, glyph.Height, MaxRectsHeuristic.Bl);
                glyph.X = bounds.X;
                glyph.Y = bounds.Y;
            }

            // Create the merged output bitmap.
            int outputWidth = MakeValidTextureSize(rectPacker.UsedWidth, requirePOT);
            int outputHeight = MakeValidTextureSize(rectPacker.UsedHeight, requirePOT);

            if (requireSquare)
            {
                outputHeight = Math.Max(outputWidth, outputHeight);
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
            public GlyphData Source;

            public int X;
            public int Y;

            public int Width;
            public int Height;
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
                return a.Source.GlyphIndex.CompareTo(b.Source.GlyphIndex);
        }


        // Heuristic guesses what might be a good output width for a list of glyphs.
        static int GuessOutputWidth(GlyphData[] sourceGlyphs)
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

