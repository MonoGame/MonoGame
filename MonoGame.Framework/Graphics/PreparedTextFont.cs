// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents the prepared glyph data that a text draw path needs from a font implementation.
/// </summary>
internal sealed class PreparedTextFont
{
    private int _defaultGlyphIndex;
    private readonly Dictionary<char, int> _glyphIndices;
    private readonly string _textContainsUnresolvableCharacters;

    /// <summary>
    /// Gets the texture that contains the prepared glyph images.
    /// </summary>
    public Texture2D Texture { get; private set; }

    /// <summary>
    /// Gets the glyph data available for drawing.
    /// </summary>
    public FontGlyph[] Glyphs { get; private set; }

    /// <summary>
    /// Gets the line spacing used when advancing between lines, in pixels.
    /// </summary>
    public int LineSpacing { get; private set; }

    /// <summary>
    /// Gets the additional spacing applied between glyphs, in pixels.
    /// </summary>
    public float Spacing { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PreparedTextFont"/> class.
    /// </summary>
    /// <param name="texture">The texture that contains the prepared glyph images.</param>
    /// <param name="glyphs">The glyph data available for drawing.</param>
    /// <param name="lineSpacing">The line spacing used when advancing between lines, in pixels.</param>
    /// <param name="spacing">The additional spacing applied between glyphs, in pixels.</param>
    /// <param name="defaultGlyphIndex">
    /// The fallback glyph index used when a character cannot be resolved, or <c>-1</c> when no fallback glyph exists.
    /// </param>
    /// <param name="textContainsUnresolvableCharacters">
    /// The exception message used when a glyph cannot be resolved.
    /// </param>
    public PreparedTextFont(Texture2D texture,
                            FontGlyph[] glyphs,
                            int lineSpacing,
                            float spacing,
                            int defaultGlyphIndex,
                            string textContainsUnresolvableCharacters)
    {
        _defaultGlyphIndex = defaultGlyphIndex;
        _glyphIndices = new Dictionary<char, int>(glyphs.Length);
        _textContainsUnresolvableCharacters = textContainsUnresolvableCharacters;
        Spacing = spacing;

        Update(texture, glyphs, lineSpacing);
    }

    public int GetGlyphIndexOrDefault(char c)
    {
        int glyphIndex;
        if (!TryGetGlyphIndex(c, out glyphIndex))
        {
            if (_defaultGlyphIndex == -1)
            {
                throw new ArgumentException(_textContainsUnresolvableCharacters, nameof(c));
            }

            return _defaultGlyphIndex;
        }

        return glyphIndex;
    }

    public unsafe Vector2 MeasureString(ref FontCharacterSource text)
    {
        if (text.Length == 0)
        {
            return Vector2.Zero;
        }

        float width = 0.0f;
        float finalLineHeight = LineSpacing;
        Vector2 offset = Vector2.Zero;
        bool firstGlyphOfLine = true;

        fixed (FontGlyph* pGlyphs = Glyphs)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\r')
                {
                    continue;
                }

                if (c == '\n')
                {
                    finalLineHeight = LineSpacing;
                    offset.X = 0;
                    offset.Y += LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }
                int currentGlyphIndex = GetGlyphIndexOrDefault(c);
                Debug.Assert(currentGlyphIndex >= 0 && currentGlyphIndex < Glyphs.Length, $"{nameof(currentGlyphIndex)} was outside the bounds of the array");
                FontGlyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                // so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += Spacing + pCurrentGlyph->LeftSideBearing;
                }

                offset.X += pCurrentGlyph->Width;

                float proposedWidth = offset.X + Math.Max(pCurrentGlyph->RightSideBearing, 0);
                if (proposedWidth > width)
                {
                    width = proposedWidth;
                }

                offset.X += pCurrentGlyph->RightSideBearing;

                if (pCurrentGlyph->Cropping.Height > finalLineHeight)
                {
                    finalLineHeight = pCurrentGlyph->Cropping.Height;
                }
            }
        }

        return new Vector2(width, offset.Y + finalLineHeight);
    }

    public void Update(Texture2D texture, FontGlyph[] glyphs, int lineSpacing)
    {
        Texture = texture;
        Glyphs = glyphs;
        LineSpacing = lineSpacing;

        _glyphIndices.Clear();

        for (int i = 0; i < glyphs.Length; i++)
        {
            _glyphIndices[glyphs[i].Character] = i;
        }
    }

    public void UpdateDefaultGlyphIndex(int defaultGlyphIndex)
    {
        _defaultGlyphIndex = defaultGlyphIndex;
    }

    private bool TryGetGlyphIndex(char c, out int index)
    {
        if (_glyphIndices.TryGetValue(c, out index))
        {
            return true;
        }

        char alternate = char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c);
        return _glyphIndices.TryGetValue(alternate, out index);
    }

    public bool TryGetGlyphIndexExact(char c, out int index)
    {
        return _glyphIndices.TryGetValue(c, out index);
    }
}
