// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Horizontal layout values for glyph bitmap
    /// </summary>
    internal struct ABCFloat
    {
        /// <summary>
        /// Horizontal space to start of glyph bitmap
        /// </summary>
        public float A;

        /// <summary>
        /// Width of glyph bitmap
        /// </summary>
        public float B;

        /// <summary>
        /// Horizontal space after glyph bitmap
        /// </summary>
        public float C;
    }

    // Represents a single character within a font.
    internal class Glyph
    {
        // Constructor.
        public Glyph(char character, GlyphData data)
        {
            Character = character;
            Data = data;
        }

        // Unicode codepoint.
        public char Character;

        // Image and layout data
        public GlyphData Data;
    }

    internal class GlyphData
    {
        // Constructor.
        public GlyphData(uint glyphIndex, BitmapContent bitmap, Rectangle? subrect = null)
        {
            GlyphIndex = glyphIndex;
            Bitmap = bitmap;
            Subrect = subrect.GetValueOrDefault(new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            Width = bitmap.Width;
            Height = bitmap.Height;
        }

        // Font-specific index of glyph
        public uint GlyphIndex;

        // Glyph image data (may only use a portion of a larger bitmap).
        public BitmapContent Bitmap;
        public Rectangle Subrect;

        // Layout information.
        public float XOffset;
        public float YOffset;
        public int Width;
        public int Height;

        public float XAdvance;

        public ABCFloat CharacterWidths;
    }
}
