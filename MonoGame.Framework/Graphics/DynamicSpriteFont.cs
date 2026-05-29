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
public sealed partial class DynamicSpriteFont
{
    private readonly CharacterRegion[] _characterRegions;
    private readonly byte[] _fontData;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Dictionary<int, PreparedTextFontData> _preparedTextFontDataBySize;
    private readonly DynamicSpriteFontRuntimeState _runtimeState;
    private Texture2D _texture;
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
    /// Gets the current atlas texture for this font face.
    /// </summary>
    public Texture2D Texture
    {
        get
        {
            return _texture;
        }
    }

    private DynamicSpriteFont(GraphicsDevice graphicsDevice,
                              byte[] fontData,
                              DynamicSpriteFontRuntimeState runtimeState,
                              float size,
                              CharacterRegion[] characterRegions)
    {
        _graphicsDevice = graphicsDevice;
        _fontData = fontData;
        _runtimeState = runtimeState;
        _characterRegions = characterRegions;
        _preparedTextFontDataBySize = new Dictionary<int, PreparedTextFontData>();
        _texture = CreateInitialTexture(graphicsDevice);
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
        DynamicSpriteFontRuntimeState runtimeState = CreateRuntimeState(fontData);
        return new DynamicSpriteFont(graphicsDevice, fontData, runtimeState, size, regions.ToArray());
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

        FontCharacterSource source = new FontCharacterSource(text);
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
        PreparedTextFontData preparedTextFontData = GetCurrentPreparedTextFontData();

#if NATIVE
        PlatformEnsureGlyphs(rasterizedSize, preparedTextFontData, ref text);
#endif
    }

    internal PreparedTextFontData GetCurrentPreparedTextFontData()
    {
        int rasterizedSize = GetRasterizedSize();
        PreparedTextFontData preparedTextFontData;
        if (_preparedTextFontDataBySize.TryGetValue(rasterizedSize, out preparedTextFontData))
        {
            return preparedTextFontData;
        }

        preparedTextFontData = new PreparedTextFontData(_texture, Array.Empty<FontGlyph>(), 0, 0.0f);
        _preparedTextFontDataBySize[rasterizedSize] = preparedTextFontData;
        return preparedTextFontData;
    }

    internal Vector2 MeasureString(ref FontCharacterSource text)
    {
        EnsureGlyphs(ref text);
        return GetCurrentPreparedTextFontData().MeasureString(ref text);
    }

    internal PreparedTextFont GetPreparedTextFont(ref FontCharacterSource text)
    {
        EnsureGlyphs(ref text);
        return GetCurrentPreparedTextFontData().GetPreparedTextFont();
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

    private static Texture2D CreateInitialTexture(GraphicsDevice graphicsDevice)
    {
        Texture2D texture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
        texture.SetData(new Color[] { Color.Transparent });
        return texture;
    }

    private static void ValidateSize(float size, string paramName)
    {
        if (float.IsNaN(size) || float.IsInfinity(size) || size <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be a positive finite value.");
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

    internal sealed class PreparedTextFontData
    {
        private const string TextContainsUnresolvableCharacters = 
            "Text contains characters that cannot be resolved by this DynamicSpriteFont.";

        private readonly PreparedTextFont _preparedTextFont;

        public FontGlyph[] Glyphs => _preparedTextFont.Glyphs;
        public int LineSpacing => _preparedTextFont.LineSpacing;
        public float Spacing { get; }
        public Texture2D Texture => _preparedTextFont.Texture;

        public PreparedTextFontData(Texture2D texture,
                                    FontGlyph[] glyphs,
                                    int lineSpacing,
                                    float spacing)
        {
            Spacing = spacing;
            _preparedTextFont = new PreparedTextFont(texture, glyphs, lineSpacing, spacing, -1, TextContainsUnresolvableCharacters);
        }

        public int GetGlyphIndexOrDefault(char c)
        {
            return _preparedTextFont.GetGlyphIndexOrDefault(c);
        }

        public Vector2 MeasureString(ref FontCharacterSource text)
        {
            return _preparedTextFont.MeasureString(ref text);
        }

        public bool TryGetGlyphIndexExact(char c, out int index)
        {
            return _preparedTextFont.TryGetGlyphIndexExact(c, out index);
        }

        public void Update(Texture2D texture, FontGlyph[] glyphs, int lineSpacing)
        {
            _preparedTextFont.Update(texture, glyphs, lineSpacing);
        }

        public PreparedTextFont GetPreparedTextFont()
        {
            return _preparedTextFont;
        }
    }
}
