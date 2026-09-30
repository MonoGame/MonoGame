// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Structure used to store float values sequentially
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ABCFloat
    {
        /// <summary/>
        public float A;
        /// <summary/>
        public float B;
        /// <summary/>
        public float C;
    }

    // Represents a single character within a font.
    internal class Glyph(char character, GlyphData data)
    {
        // Unicode codepoint.
        public char Character { get;  } = character;

        // Image and layout data
        public GlyphData Data { get; } = data;
    }

    internal class GlyphData(uint glyphIndex, BitmapContent bitmap, Rectangle? subrect = null)
    {
        // Font-specific index of glyph
        public uint GlyphIndex = glyphIndex;

        // Glyph image data (may only use a portion of a larger bitmap).
        public BitmapContent Bitmap = bitmap;
        public Rectangle SubRect  = subrect.GetValueOrDefault(new Rectangle(0, 0, bitmap.Width, bitmap.Height));

        // Layout information.
        public float XOffset;
        public float YOffset;
        public int Width = bitmap.Width;
        public int Height = bitmap.Height;

        public float XAdvance;

        public ABCFloat CharacterWidths;
    }
}
