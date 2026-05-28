// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents a runtime loaded font face that can bake glyphs on demand.
/// </summary>
public sealed partial class DynamicSpriteFont
{
    private readonly CharacterRegion[] _characterRegions;
    private readonly byte[] _fontData;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Dictionary<int, DynamicSpriteFontSizeData> _sizeDataBySize;
    private float _size;

    /// <summary>
    /// Gets or sets the active font size used for measuring and drawing.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// THrown when the assigned value is zero, negative, <see cref="float.NaN"/> or infinite.
    /// </exception>
    public float Size
    {
        get => _size;
        set
        {
            ValidateSize(value, nameof(value));
            _size = value;
        }
    }

    /// <summary>
    /// Gets the current atlas texture for the active size.
    /// </summary>
    public Texture2D Texture
    {
        get
        {
            return GetCurrentSizeData().Texture;
        }
    }

    private DynamicSpriteFont(GraphicsDevice graphicsDevice, byte[] fontDta, float size, CharacterRegion[] characterRegions)
    {
        _graphicsDevice = graphicsDevice;
        _fontData = fontDta;
        _characterRegions = characterRegions;
        _sizeDataBySize = new Dictionary<int, DynamicSpriteFontSizeData>();
        _size = size;
    }

    /// <summary>
    /// Creates a <see cref="DynamicSpriteFont"/> from a font file.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device that will own runtime font resources.</param>
    /// <param name="path">The path to a TrueType or OpenType font file.</param>
    /// <param name="size">The initial active size for the dynamic font.</param>
    /// <param name="characterRegions">Optional character regions to warm at creation time.</param>
    /// <returns>A new <see cref="DynamicSpriteFont"/> for the supplied font face.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/>, <paramref name="path"/>, or 
    /// <paramref name="characterRegions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="path"/> is empty or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw when <paramref name="size"/> is zero, negative <see cref="float.NaN"/>, or infinite.
    /// </exception>
    public static DynamicSpriteFont FromFile(GraphicsDevice graphicsDevice, string path, float size, IEnumerable<CharacterRegion> characterRegions)
    {
        if (graphicsDevice == null)
        {
            throw new ArgumentNullException(nameof(graphicsDevice), $"{nameof(graphicsDevice)} must not be null.");
        }

        if (path == null)
        {
            throw new ArgumentNullException(nameof(path), $"{nameof(path)} must not be null.");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The font file path must not be empty or whitespace.", nameof(path));
        }

        if (characterRegions == null)
        {
            throw new ArgumentNullException(nameof(characterRegions), $"{nameof(characterRegions)} must not be null.");
        }

        ValidateSize(size, nameof(size));

        using (Stream stream = File.OpenRead(path))
        {
            return FromStream(graphicsDevice, stream, size, characterRegions);
        }
    }

    /// <summary>
    /// Creates a new <see cref="DynamicSpriteFont"/> from a font stream.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device that will own runtime font resources.</param>
    /// <param name="stream">The stream containing TrueType or OpenType font data.</param>
    /// <param name="size">The initial active size for the dynamic font.</param>
    /// <param name="characterRegions">Optional character regions to warm at creation time.</param>
    /// <returns>A new <see cref="DynamicSpriteFont"/> for the supplied font face.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/>, <paramref name="stream"/>, or 
    /// <paramref name="characterRegions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw when <paramref name="size"/> is zero, negative <see cref="float.NaN"/>, or infinite.
    /// </exception>
    public static DynamicSpriteFont FromStream(GraphicsDevice graphicsDevice, Stream stream, float size, IEnumerable<CharacterRegion> characterRegions)
    {
        if (graphicsDevice == null)
        {
            throw new ArgumentNullException(nameof(graphicsDevice), $"{nameof(graphicsDevice)} must not be null.");
        }

        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream), $"{nameof(stream)} must not be null.");
        }

        if (characterRegions == null)
        {
            throw new ArgumentNullException(nameof(characterRegions), $"{nameof(characterRegions)} must not be null.");
        }

        ValidateSize(size, nameof(size));

#if !NATIVE
        throw new PlatformNotSupportedException("Runtime SpriteFont baking is currently implemented only for MonoGame.Framework.Native.");
#else
        List<CharacterRegion> regions = new List<CharacterRegion>(characterRegions);
        byte[] fontData = ReadFontData(stream);
        return new DynamicSpriteFont(graphicsDevice, fontData, size, regions.ToArray());
#endif
    }

    /// <summary>
    /// Returns the size of a string when rendered using the current font state.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The size, in pixels, of <paramref name="text"/> when rendered in this font.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="text"/> is <see langword="null"/>.
    /// </exception>
    public Vector2 MeasureString(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text), $"{nameof(text)} must not be null.");
        }

        if (text.Length == 0)
        {
            return Vector2.Zero;
        }

        DynamicSpriteFontCharacterSource source = new DynamicSpriteFontCharacterSource(text);
        return MeasureString(ref source);
    }

    /// <summary>
    /// Returns the size of the contents of a <see cref="StringBuilder"/>when rendered using the current font state.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The size, in pixels, of <paramref name="text"/> when rendered in this font.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="text"/> is <see langword="null"/>.
    /// </exception>
    public Vector2 MeasureString(StringBuilder text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text), $"{nameof(text)} must not be null.");
        }

        if (text.Length == 0)
        {
            return Vector2.Zero;
        }

        DynamicSpriteFontCharacterSource source = new DynamicSpriteFontCharacterSource(text);
        return MeasureString(ref source);
    }

    internal void EnsureGlyphs(string text)
    {
        DynamicSpriteFontCharacterSource source = new DynamicSpriteFontCharacterSource(text);
        EnsureGlyphs(ref source);
    }

    internal void EnsureGlyphs(StringBuilder text)
    {
        DynamicSpriteFontCharacterSource source = new DynamicSpriteFontCharacterSource(text);
        EnsureGlyphs(ref source);
    }

    internal void EnsureGlyphs(ref DynamicSpriteFontCharacterSource text)
    {
        if (text.Length == 0)
        {
            return;
        }

        DynamicSpriteFontSizeData sizeData = GetCurrentSizeData();

#if NATIVE
        PlatformEnsureGlyphs(sizeData, ref text);
#endif
    }

    internal DynamicSpriteFontSizeData GetCurrentSizeData()
    {
        int rasterizedSize = GetRasterizedSize();
        DynamicSpriteFontSizeData sizeData;
        if (_sizeDataBySize.TryGetValue(rasterizedSize, out sizeData))
        {
            return sizeData;
        }

        sizeData = CreateSizeData(rasterizedSize);
        _sizeDataBySize[rasterizedSize] = sizeData;
        return sizeData;
    }

    internal Vector2 MeasureString(ref DynamicSpriteFontCharacterSource text)
    {
        EnsureGlyphs(ref text);
        return GetCurrentSizeData().MeasureString(ref text);
    }

    private static byte[] ReadFontData(Stream stream)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }

    private int GetRasterizedSize()
    {
        return (int)MathF.Ceiling(_size);
    }

    private static void ValidateSize(float size, string paramName)
    {
        if (float.IsNaN(size) || float.IsInfinity(size) || size <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be a positive finite value.");
        }
    }

    internal struct DynamicSpriteFontGlyph
    {
        public static readonly DynamicSpriteFontGlyph Empty = new DynamicSpriteFontGlyph();

        public Rectangle BoundsInTexture;
        public char Character;
        public Rectangle Cropping;
        public float LeftSideBearing;
        public float RightSideBearing;
        public float Width;
        public float WidthIncludingBearings;
    }

    internal struct DynamicSpriteFontCharacterSource
    {
        private readonly StringBuilder _builder;
        private readonly string _string;

        public readonly int Length;

        public char this[int index]
        {
            get
            {
                if (_string != null)
                {
                    return _string[index];
                }

                return _builder[index];
            }
        }

        public DynamicSpriteFontCharacterSource(string text)
        {
            _string = text;
            _builder = null;
            Length = text.Length;
        }

        public DynamicSpriteFontCharacterSource(StringBuilder text)
        {
            _builder = text;
            _string = null;
            Length = text.Length;
        }

    }

    internal sealed unsafe class DynamicSpriteFontRuntimeState
    {
        public readonly MGF_RuntimeFont* Handle;

        public DynamicSpriteFontRuntimeState(MGF_RuntimeFont* handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle), $"{nameof(handle)} must not be null.");
            }

            Handle = handle;
        }

        ~DynamicSpriteFontRuntimeState()
        {
            if (Handle != null)
            {
                MGF.RuntimeFont_Destroy(Handle);
            }
        }
    }

    internal sealed class DynamicSpriteFontSizeData
    {
        private const string TextContainsUnresolvableCharacters = 
            "Text contains characters that cannot be resolved by this DynamicSpriteFont.";

        private readonly Dictionary<char, int> _glyphIndices;

        public DynamicSpriteFontGlyph[] Glyphs { get; private set; }
        public int LineSpacing { get; private set; }
        public DynamicSpriteFontRuntimeState RuntimeState { get; }
        public float Spacing { get; }
        public Texture2D Texture { get; private set; }

        public DynamicSpriteFontSizeData(Texture2D texture,
                                         DynamicSpriteFontGlyph[] glyphs,
                                         int lineSpacing,
                                         float spacing,
                                         DynamicSpriteFontRuntimeState runtimeState)
        {
            Texture = texture;
            RuntimeState = runtimeState;
            Spacing = spacing;
            _glyphIndices = new Dictionary<char, int>(glyphs.Length);

            Update(texture, glyphs, lineSpacing);
        }

        public unsafe int GetGlyphIndexOrDefault(char c)
        {
            if (!TryGetGlyphIndex(c, out int glyphIndex))
            {
                throw new ArgumentException(TextContainsUnresolvableCharacters, nameof(c));
            }

            return glyphIndex;
        }

        public unsafe Vector2 MeasureString(ref DynamicSpriteFontCharacterSource text)
        {
            if (text.Length == 0)
            {
                return Vector2.Zero;
            }

            float width = 0.0f;
            float finalLineHeight = LineSpacing;
            Vector2 offset = Vector2.Zero;
            bool firstGlyphOfLine = true;

            fixed (DynamicSpriteFontGlyph* pGlyphs = Glyphs)
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
                    Debug.Assert(currentGlyphIndex >= 0 && currentGlyphIndex < Glyphs.Length, "currentGlyphIndex was outside the bounds of the array.");
                    DynamicSpriteFontGlyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;

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

        public void Update(Texture2D texture, DynamicSpriteFontGlyph[] glyphs, int lineSpacing)
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
    }
}