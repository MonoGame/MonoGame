// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents a runtime loaded font face that can bake glyphs on demand.
/// </summary>
public sealed partial class DynamicSpriteFont : GraphicsResource
{
    private const string TextContainsUnresolvableCharacters = $"Text contains characters that cannot be resolved by this {nameof(DynamicSpriteFont)}.";
    private const string UnresolvableCharacter = $"Character cannot be resolved by this {nameof(DynamicSpriteFont)}.";

    private static readonly Dictionary<long, Rectangle> EmptyGlyphBounds = new Dictionary<long, Rectangle>();

    // Incremental atlas uploads need the last known glyph bounds across every baked size,
    // not just the current prepared size
    private readonly Dictionary<int, Dictionary<long, Rectangle>> _glyphBoundsByPage;

    private readonly Dictionary<int, PreparedTextFont> _preparedTextFontsBySize;
    private readonly Dictionary<int, Texture2D> _texturesByPage;
    private readonly FontHandle _fontHandle;
    private char? _defaultCharacter;
    private int _currentPageIndex;
    private float _size;

    /// <summary>
    /// Gets or sets the active font size used for measuring and drawing.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the assigned value is zero, negative, <see cref="float.NaN"/> or infinite.
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
    /// Gets or sets the character that will be substituted when a given character
    /// cannot be resolved by this font.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the assigned character cannot be resolved by this font.
    /// </exception>
    public char? DefaultCharacter
    {
        get => _defaultCharacter;
        set
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DynamicSpriteFont));
            }

            if (value.HasValue)
            {
                EnsureDefaultCharacterAvailable(value.Value);
            }

            _defaultCharacter = value;
            UpdateDefaultGlyphIndices();
        }
    }

    /// <summary>
    /// Gets the total number of atlas pages currently allocated for this font face.
    /// </summary>
    public int TotalPages
    {
        get
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DynamicSpriteFont));
            }

            return _texturesByPage.Count;
        }
    }

    /// <summary>
    /// Gets the index of the atlas page currently receiving new glyphs.
    /// </summary>
    public int CurrentAtlasPageIndex
    {
        get
        {
            return _currentPageIndex;
        }
    }

    private DynamicSpriteFont(GraphicsDevice graphicsDevice, FontHandle fontHandle, float size)
    {
        if (fontHandle == null)
        {
            throw new ArgumentNullException(nameof(fontHandle));
        }

        GraphicsDevice = graphicsDevice;
        _glyphBoundsByPage = new Dictionary<int, Dictionary<long, Rectangle>>();
        _fontHandle = fontHandle;
        _preparedTextFontsBySize = new Dictionary<int, PreparedTextFont>();
        _texturesByPage = new Dictionary<int, Texture2D>();
        _texturesByPage[0] = CreateInitialTexture(graphicsDevice);
        _currentPageIndex = 0;
        _size = size;
    }

    /// <summary>
    /// Creates a <see cref="DynamicSpriteFont"/> from a font file.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device that will own the dynamic font resources.</param>
    /// <param name="path">The path to a TrueType or OpenType font file.</param>
    /// <param name="size">The initial active size for the dynamic font.</param>
    /// <returns>A new <see cref="DynamicSpriteFont"/> for the supplied font face.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> or <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="path"/> is empty or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw when <paramref name="size"/> is zero, negative, <see cref="float.NaN"/>, or infinite.
    /// </exception>
    public static DynamicSpriteFont FromFile(GraphicsDevice graphicsDevice, string path, float size)
    {
        return FromFile(graphicsDevice, path, size, Array.Empty<CharacterRegion>());
    }

    /// <summary>
    /// Creates a <see cref="DynamicSpriteFont"/> from a font file.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device that will own dynamic font resources.</param>
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
    /// Throw when <paramref name="size"/> is zero, negative, <see cref="float.NaN"/>, or infinite.
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
    /// <param name="graphicsDevice">The graphics device that will own dynamic font resources.</param>
    /// <param name="stream">The stream containing TrueType or OpenType font data.</param>
    /// <param name="size">The initial active size for the dynamic font.</param>
    /// <returns>A new <see cref="DynamicSpriteFont"/> for the supplied font face.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> or <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw when <paramref name="size"/> is zero, negative, <see cref="float.NaN"/>, or infinite.
    /// </exception>
    public static DynamicSpriteFont FromStream(GraphicsDevice graphicsDevice, Stream stream, float size)
    {
        return FromStream(graphicsDevice, stream, size, Array.Empty<CharacterRegion>());
    }

    /// <summary>
    /// Creates a new <see cref="DynamicSpriteFont"/> from a font stream.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device that will own dynamic font resources.</param>
    /// <param name="stream">The stream containing TrueType or OpenType font data.</param>
    /// <param name="size">The initial active size for the dynamic font.</param>
    /// <param name="characterRegions">Optional character regions to warm at creation time.</param>
    /// <returns>A new <see cref="DynamicSpriteFont"/> for the supplied font face.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/>, <paramref name="stream"/>, or
    /// <paramref name="characterRegions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw when <paramref name="size"/> is zero, negative, <see cref="float.NaN"/>, or infinite.
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
        throw new PlatformNotSupportedException($"{nameof(DynamicSpriteFont)} is currently implemented only for MonoGame.Framework.Native.");
#else
        List<CharacterRegion> regions = new List<CharacterRegion>(characterRegions);

        byte[] fontData;
        using(MemoryStream memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            fontData = memoryStream.ToArray();
        }

        FontHandle fontHandle = CreateFontHandle(fontData);
        DynamicSpriteFont dynamicSpriteFont = new DynamicSpriteFont(graphicsDevice, fontHandle, size);

        try
        {
            CharacterRegion[] initialCharacterRegions = regions.ToArray();
            dynamicSpriteFont.WarmCharacterRegions(initialCharacterRegions);
            return dynamicSpriteFont;
        }
        catch
        {
            dynamicSpriteFont.Dispose();
            throw;
        }
#endif
    }

    /// <summary>
    /// Gets the atlas texture for a specific page index.
    /// </summary>
    /// <param name="pageIndex">The atlas page index to retrieve.</param>
    /// <returns>The texture backing the requested atlas page.</returns>
    public Texture2D GetTexture(int pageIndex)
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(DynamicSpriteFont));
        }

        if (pageIndex < 0 || pageIndex >= _texturesByPage.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex), $"{nameof(pageIndex)} must be greater than or equal to zero and less than {TotalPages}.");
        }

        return _texturesByPage[pageIndex];
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

        FontCharacterSource source = new FontCharacterSource(text);
        return MeasureString(ref source);
    }

    /// <summary>
    /// Returns the size of the contents of a <see cref="StringBuilder"/> when rendered using the current font state.
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

        FontCharacterSource source = new FontCharacterSource(text);
        return MeasureString(ref source);
    }

    internal void EnsureGlyphs(string text)
    {
        FontCharacterSource source = new FontCharacterSource(text);
        EnsureGlyphs(ref source);
    }

    internal void EnsureGlyphs(StringBuilder text)
    {
        FontCharacterSource source = new FontCharacterSource(text);
        EnsureGlyphs(ref source);
    }

    internal void EnsureGlyphs(ref FontCharacterSource text)
    {
        if (text.Length == 0)
        {
            return;
        }

        int rasterizedSize = GetRasterizedSize();
        PreparedTextFont preparedTextFont = GetCurrentPreparedTextFont();

#if NATIVE
        PlatformEnsureGlyphs(rasterizedSize, preparedTextFont, ref text);
#endif
    }

    internal PreparedTextFont GetCurrentPreparedTextFont()
    {
        int rasterizedSize = GetRasterizedSize();

        if (_preparedTextFontsBySize.TryGetValue(rasterizedSize, out PreparedTextFont preparedTextFont))
        {
            return preparedTextFont;
        }

        preparedTextFont = CreatePreparedTextFont(_currentPageIndex, Array.Empty<FontGlyph>(), 0, 0.0f);
        _preparedTextFontsBySize[rasterizedSize] = preparedTextFont;
        return preparedTextFont;
    }

    internal Vector2 MeasureString(ref FontCharacterSource text)
    {
        EnsureGlyphs(ref text);
        return GetCurrentPreparedTextFont().MeasureString(ref text);
    }

    private void WarmCharacterRegions(CharacterRegion[] characterRegions)
    {
        if (characterRegions.Length == 0)
        {
            return;
        }

#if NATIVE
        PlatformWarmGlyphs(GetRasterizedSize(), characterRegions);
#endif
    }

    private PreparedTextFont CreatePreparedTextFont(int currentPageIndex,
                                                    FontGlyph[] glyphs,
                                                    int lineSpacing,
                                                    float spacing)
    {
        return new PreparedTextFont(_texturesByPage,
                                    currentPageIndex,
                                    glyphs,
                                    lineSpacing,
                                    spacing,
                                    ResolveDefaultGlyphIndex(glyphs),
                                    TextContainsUnresolvableCharacters);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (Texture2D texture in _texturesByPage.Values)
            {
                texture.Dispose();
            }

            _texturesByPage.Clear();
            _preparedTextFontsBySize.Clear();
            _glyphBoundsByPage.Clear();
            _fontHandle.Dispose();
        }

        base.Dispose(disposing);
    }

    private static long GetGlyphLookupKey(char character, int size)
    {
        return ((long)size << 16) | character;
    }

    private Dictionary<long, Rectangle> GetGlyphBoundsForPage(int pageIndex)
    {
        if (_glyphBoundsByPage.TryGetValue(pageIndex, out Dictionary<long, Rectangle> glyphBounds))
        {
            return glyphBounds;
        }

        return EmptyGlyphBounds;
    }

    private void UpdateGlyphBoundsByPage(FontGlyph[] glyphs)
    {
        // Atlas rebuilds can move already baked glyphs between pages or replace old bounds entirely,
        // so rebuilding from the full glyph snapshot avoids leaving stale per-page entries behind
        _glyphBoundsByPage.Clear();

        for (int i = 0; i < glyphs.Length; i++)
        {
            FontGlyph glyph = glyphs[i];

            if (!_glyphBoundsByPage.TryGetValue(glyph.PageIndex, out Dictionary<long, Rectangle> glyphBounds))
            {
                glyphBounds = new Dictionary<long, Rectangle>();
                _glyphBoundsByPage.Add(glyph.PageIndex, glyphBounds);
            }

            glyphBounds[GetGlyphLookupKey(glyph.Character, glyph.Size)] = glyph.BoundsInTexture;
        }
    }

    private static Texture2D CreateInitialTexture(GraphicsDevice graphicsDevice)
    {
        Texture2D texture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
        texture.SetData(new Color[] { Color.Transparent });
        return texture;
    }

    private void EnsureDefaultCharacterAvailable(char defaultCharacter)
    {
        FontCharacterSource source = new FontCharacterSource(defaultCharacter.ToString());

        try
        {
            EnsureGlyphs(ref source);
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException(UnresolvableCharacter);
        }

        if (!GetCurrentPreparedTextFont().TryGetGlyphIndex(defaultCharacter, out _))
        {
            throw new ArgumentException(UnresolvableCharacter);
        }
    }

    private int GetRasterizedSize()
    {
        return (int)MathF.Ceiling(_size);
    }

    private void UpdateDefaultGlyphIndices()
    {
        foreach (PreparedTextFont preparedTextFont in _preparedTextFontsBySize.Values)
        {
            int defaultGlyphIndex = -1;

            if (_defaultCharacter.HasValue)
            {
                preparedTextFont.TryGetGlyphIndex(_defaultCharacter.Value, out defaultGlyphIndex);
            }

            preparedTextFont.UpdateDefaultGlyphIndex(defaultGlyphIndex);
        }
    }

    private int ResolveDefaultGlyphIndex(FontGlyph[] glyphs)
    {
        if (!_defaultCharacter.HasValue)
        {
            return -1;
        }

        char defaultCharacter = _defaultCharacter.Value;
        char alternate = char.IsUpper(defaultCharacter) ?
                         char.ToLower(defaultCharacter) :
                         char.ToUpper(defaultCharacter);

        bool checkAlternate = alternate != defaultCharacter;
        int alternateIndex = -1;

        for (int i = 0; i < glyphs.Length; i++)
        {
            char character = glyphs[i].Character;

            if (character == defaultCharacter)
            {
                return i;
            }

            if (checkAlternate && alternateIndex == -1 && character == alternate)
            {
                alternateIndex = i;
            }
        }

        return alternateIndex;
    }

    private static void ValidateSize(float size, string paramName)
    {
        if (float.IsNaN(size) || float.IsInfinity(size) || size <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be a positive finite value.");
        }
    }

    private sealed unsafe class FontHandle : IDisposable
    {
        public readonly MGF_Font* Handle;
        private bool _isDisposed;

        public FontHandle(MGF_Font* handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle), $"{nameof(handle)} must not be null.");
            }

            Handle = handle;
        }

        ~FontHandle() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (Handle != null)
            {
                MGF.MGF_Font_Destroy(Handle);
            }

            _isDisposed = true;
        }
    }
}
