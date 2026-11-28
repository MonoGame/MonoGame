// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents the prepared glyph data that a text draw path needs from a font implementation.
/// </summary>
internal sealed class PreparedTextFont
{
    private readonly Dictionary<char, int> _glyphIndices;
    private readonly string _textContainsUnresolvableCharacters;
    private readonly Dictionary<int, Texture2D> _textureByPage;
    private int _defaultGlyphIndex;

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

    internal bool UsesMultipleTextures => _textureByPage.Count > 1;

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
        _textureByPage = new Dictionary<int, Texture2D>();
        Spacing = spacing;

        Update(texture, glyphs, lineSpacing);
    }

    internal PreparedTextFont(Dictionary<int, Texture2D> texturesByPage,
                              int currentPageIndex,
                              FontGlyph[] glyphs,
                              int lineSpacing,
                              float spacing,
                              int defaultGlyphIndex,
                              string textContainsUnresolvableCharacters)
    {
        if (texturesByPage == null)
        {
            throw new ArgumentNullException(nameof(texturesByPage));
        }

        _defaultGlyphIndex = defaultGlyphIndex;
        _glyphIndices = new Dictionary<char, int>(glyphs.Length);
        _textContainsUnresolvableCharacters = textContainsUnresolvableCharacters;
        _textureByPage = new Dictionary<int, Texture2D>();
        Spacing = spacing;

        Update(texturesByPage, currentPageIndex, glyphs, lineSpacing);
    }

    public int GetGlyphIndexOrDefault(char c)
    {
        if (!TryGetGlyphIndex(c, out int glyphIndex))
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

        _textureByPage.Clear();
        _glyphIndices.Clear();

        for (int i = 0; i < glyphs.Length; i++)
        {
            _glyphIndices[glyphs[i].Character] = i;
        }
    }

    internal Texture2D GetTexture(int pageIndex)
    {
        if(_textureByPage.Count == 0)
        {
            return Texture;
        }

        if (_textureByPage.TryGetValue(pageIndex, out Texture2D texture))
        {
            return texture;
        }

        throw new InvalidOperationException($"PreparedTextFont does not have a texture for page {pageIndex}.");
    }

    internal void Update(Dictionary<int, Texture2D> texturesByPage,
                         int currentPageIndex,
                         FontGlyph[] glyphs,
                         int lineSpacing)
    {
        if (texturesByPage == null)
        {
            throw new ArgumentNullException(nameof(texturesByPage));
        }

        Update(GetPrimaryTexture(texturesByPage, currentPageIndex, glyphs), glyphs, lineSpacing);

        if (glyphs.Length == 0)
        {
            _textureByPage[currentPageIndex] = Texture;
            return;
        }

        for (int i = 0; i < glyphs.Length; i++)
        {
            int pageIndex = glyphs[i].PageIndex;
            if (_textureByPage.ContainsKey(pageIndex))
            {
                continue;
            }

            if (!texturesByPage.TryGetValue(pageIndex, out Texture2D texture))
            {
                throw new InvalidOperationException($"PreparedTextFont does not have a texture for page {pageIndex}.");
            }

            _textureByPage.Add(pageIndex, texture);
        }
    }

    public void UpdateDefaultGlyphIndex(int defaultGlyphIndex)
    {
        _defaultGlyphIndex = defaultGlyphIndex;
    }

    internal bool TryGetGlyphIndex(char c, out int index)
    {
        if (_glyphIndices.TryGetValue(c, out index))
        {
            return true;
        }

        char alternate = char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c);
        return _glyphIndices.TryGetValue(alternate, out index);
    }

    // Runtime glyph baking needs to know whether this exact codepoint is already preset,
    // not whether SpriteFont-style alternate-case fallback could resolve it for drawing
    // like TryGetGlyphIndex does above
    public bool TryGetGlyphIndexExact(char c, out int index)
    {
        return _glyphIndices.TryGetValue(c, out index);
    }

    private static Texture2D GetPrimaryTexture(Dictionary<int, Texture2D> texturesByPage,
                                               int currentPageIndex,
                                               FontGlyph[] glyphs)
    {
        if (glyphs.Length > 0)
        {
            if (texturesByPage.TryGetValue(glyphs[0].PageIndex, out Texture2D glyphTexture))
            {
                return glyphTexture;
            }
        }

        if (texturesByPage.TryGetValue(currentPageIndex, out Texture2D currentTexture))
        {
            return currentTexture;
        }

        foreach (KeyValuePair<int, Texture2D> pair in texturesByPage)
        {
            return pair.Value;
        }

        throw new InvalidOperationException("PreparedTextFont requires at least one texture.");
    }
}
