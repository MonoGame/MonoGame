// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

public sealed partial class SpriteFont
{
    private static unsafe SpriteFont PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream, int size, IEnumerable<CharacterRegion> characterRegions)
    {
        List<CharacterRegion> regions = new List<CharacterRegion>(characterRegions);
        CharacterRegion[] runtimeRegions = regions.ToArray();
        SpriteFontRuntimeState runtimeState = CreateRuntimeState(ReadFontData(stream), size);

        if (runtimeRegions.Length == 0)
        {
            return CreateEmptyRuntimeSpriteFont(graphicsDevice, runtimeState, size);
        }

        return CreateRuntimeSpriteFont(graphicsDevice, runtimeState, runtimeRegions, null);
    }

    private unsafe void PlatformEnsureGlyphs(ref CharacterSource text)
    {
        List<char> missingCharacters = GetMissingCharacters(ref text);
        if (missingCharacters.Count == 0)
        {
            return;
        }

        CharacterRegion[] characterRegions = BuildCharacterRegions(missingCharacters);
        EnsureRuntimeGlyphs(_runtimeState,
                           characterRegions,
                           out byte* atlasRgba,
                           out int atlasWidth,
                           out int atlasHeight,
                           out bool atlasRebuilt,
                           out MGF_Glyph* glyphs,
                           out int glyphCount,
                           out int lineSpacing);

        Texture2D texture = _texture;
        if (texture.Width != atlasWidth || texture.Height != atlasHeight)
        {
            texture = new Texture2D(_texture.GraphicsDevice, atlasWidth, atlasHeight, false, SurfaceFormat.Color);
        }

        if (atlasRebuilt || texture != _texture)
        {
            MGG.Texture_SetData(_texture.GraphicsDevice.Handle,
                                texture.Handle,
                                0,
                                0,
                                0,
                                0,
                                0,
                                atlasWidth,
                                atlasHeight,
                                1,
                                atlasRgba,
                                atlasWidth * atlasHeight * 4);
        }
        else
        {
            UploadChangedGlyphBounds(glyphs, glyphCount, atlasRgba, atlasWidth);
        }

        SpriteFont spriteFont = CreateSpriteFont(texture, glyphs, glyphCount, lineSpacing, _defaultCharacter, _runtimeState);
        SetRuntimeGlyphData(spriteFont);
    }

    private static byte[] ReadFontData(Stream stream)
    {
        byte[] fontData;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            fontData = memoryStream.ToArray();
        }

        return fontData;
    }

    private static unsafe SpriteFontRuntimeState CreateRuntimeState(byte[] fontData, int size)
    {
        GCHandle fontDataHandle = default;

        try
        {
            fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            MGF_RuntimeFont* runtimeFont = MGF.RuntimeFont_Create((byte*)fontDataHandle.AddrOfPinnedObject(), fontData.Length, size);
            if (runtimeFont == null)
            {
                throw new InvalidOperationException("Failed to create a runtime SpriteFont from the supplied font data.");
            }

            return new SpriteFontRuntimeState(runtimeFont);
        }
        finally
        {
            if (fontDataHandle.IsAllocated)
            {
                fontDataHandle.Free();
            }
        }
    }

    private static unsafe SpriteFont CreateRuntimeSpriteFont(GraphicsDevice graphicsDevice,
                                                             SpriteFontRuntimeState runtimeState,
                                                             CharacterRegion[] runtimeRegions,
                                                             char? defaultCharacter)
    {
        EnsureRuntimeGlyphs(runtimeState,
                           runtimeRegions,
                           out byte* atlasRgba,
                           out int atlasWidth,
                           out int atlasHeight,
                           out _,
                           out MGF_Glyph* glyphs,
                           out int glyphCount,
                           out int lineSpacing);

        Texture2D texture = new Texture2D(graphicsDevice, atlasWidth, atlasHeight, false, SurfaceFormat.Color);
        MGG.Texture_SetData(graphicsDevice.Handle,
                            texture.Handle,
                            0,
                            0,
                            0,
                            0,
                            0,
                            atlasWidth,
                            atlasHeight,
                            1,
                            atlasRgba,
                            atlasWidth * atlasHeight * 4);

        return CreateSpriteFont(texture, glyphs, glyphCount, lineSpacing, defaultCharacter, runtimeState);
    }

    private static unsafe void EnsureRuntimeGlyphs(SpriteFontRuntimeState runtimeState,
                                                   CharacterRegion[] runtimeRegions,
                                                   out byte* atlasRgba,
                                                   out int atlasWidth,
                                                   out int atlasHeight,
                                                   out bool atlasRebuilt,
                                                   out MGF_Glyph* glyphs,
                                                   out int glyphCount,
                                                   out int lineSpacing)
    {
        GCHandle regionHandle = default;

        try
        {
            MGF_CharacterRegion[] nativeRegions = new MGF_CharacterRegion[runtimeRegions.Length];
            for (int i = 0; i < runtimeRegions.Length; i++)
            {
                nativeRegions[i].Start = runtimeRegions[i].Start;
                nativeRegions[i].End = runtimeRegions[i].End;
            }

            regionHandle = GCHandle.Alloc(nativeRegions, GCHandleType.Pinned);

            if (!MGF.RuntimeFont_EnsureGlyphs(runtimeState.Handle,
                                             (MGF_CharacterRegion*)regionHandle.AddrOfPinnedObject(),
                                             nativeRegions.Length,
                                             out atlasRgba,
                                             out atlasWidth,
                                             out atlasHeight,
                                             out atlasRebuilt,
                                             out glyphs,
                                             out glyphCount,
                                             out lineSpacing))
            {
                throw new InvalidOperationException("Failed to update a runtime SpriteFont from the supplied font data.");
            }

            if (atlasRgba == null || glyphs == null || glyphCount <= 0)
            {
                throw new InvalidOperationException("Runtime SpriteFont update did not return any glyph data.");
            }
        }
        finally
        {
            if (regionHandle.IsAllocated)
            {
                regionHandle.Free();
            }
        }
    }

    private static SpriteFont CreateEmptyRuntimeSpriteFont(GraphicsDevice graphicsDevice,
                                                           SpriteFontRuntimeState runtimeState,
                                                           int size)
    {
        Texture2D texture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
        texture.SetData(new[] { Color.Transparent });

        return new SpriteFont(texture,
                              new List<Rectangle>(),
                              new List<Rectangle>(),
                              new List<char>(),
                              size,
                              0.0f,
                              new List<Vector3>(),
                              null,
                              runtimeState);
    }

    private static unsafe SpriteFont CreateSpriteFont(Texture2D texture,
                                                      MGF_Glyph* glyphs,
                                                      int glyphCount,
                                                      int lineSpacing,
                                                      char? defaultCharacter,
                                                      SpriteFontRuntimeState runtimeState)
    {
        List<Rectangle> glyphBounds = new List<Rectangle>(glyphCount);
        List<Rectangle> cropping = new List<Rectangle>(glyphCount);
        List<char> characters = new List<char>(glyphCount);
        List<Vector3> kerning = new List<Vector3>(glyphCount);

        for (int i = 0; i < glyphCount; i++)
        {
            MGF_Glyph glyph = glyphs[i];
            characters.Add(glyph.Character);
            glyphBounds.Add(new Rectangle(glyph.BoundsX, glyph.BoundsY, glyph.BoundsWidth, glyph.BoundsHeight));
            cropping.Add(new Rectangle(glyph.CroppingX, glyph.CroppingY, glyph.CroppingWidth, glyph.CroppingHeight));
            kerning.Add(new Vector3(glyph.LeftSideBearing, glyph.Width, glyph.RightSideBearing));
        }

        return new SpriteFont(texture, glyphBounds, cropping, characters, lineSpacing, 0.0f, kerning, defaultCharacter, runtimeState);
    }

    private List<char> GetMissingCharacters(ref CharacterSource text)
    {
        HashSet<char> missingCharacters = new HashSet<char>();
        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            if (character == '\r' || character == '\n')
            {
                continue;
            }

            if (!TryGetGlyphIndexExact(character, out _))
            {
                missingCharacters.Add(character);
            }
        }

        return new List<char>(missingCharacters);
    }

    private CharacterRegion[] BuildCharacterRegions(List<char> missingCharacters)
    {
        missingCharacters.Sort();

        List<CharacterRegion> regions = new List<CharacterRegion>();
        if (missingCharacters.Count == 0)
        {
            return regions.ToArray();
        }

        char regionStart = missingCharacters[0];
        char regionEnd = missingCharacters[0];

        for (int i = 1; i < missingCharacters.Count; i++)
        {
            char character = missingCharacters[i];
            if (character == regionEnd + 1)
            {
                regionEnd = character;
                continue;
            }

            regions.Add(new CharacterRegion(regionStart, regionEnd));
            regionStart = character;
            regionEnd = character;
        }

        regions.Add(new CharacterRegion(regionStart, regionEnd));
        return regions.ToArray();
    }

    private unsafe void UploadChangedGlyphBounds(MGF_Glyph* glyphs, int glyphCount, byte* atlasRgba, int atlasWidth)
    {
        Dictionary<char, Rectangle> currentGlyphBounds = new Dictionary<char, Rectangle>(_glyphs.Length);
        List<Rectangle> changedBounds = new List<Rectangle>();

        for (int i = 0; i < _glyphs.Length; i++)
        {
            currentGlyphBounds[_glyphs[i].Character] = _glyphs[i].BoundsInTexture;
        }

        for (int i = 0; i < glyphCount; i++)
        {
            MGF_Glyph glyph = glyphs[i];
            Rectangle nextBounds = new Rectangle(glyph.BoundsX, glyph.BoundsY, glyph.BoundsWidth, glyph.BoundsHeight);
            if (!ShouldUploadGlyph(currentGlyphBounds, glyph.Character, nextBounds))
            {
                continue;
            }

            if (nextBounds.Width <= 0 || nextBounds.Height <= 0)
            {
                continue;
            }

            changedBounds.Add(nextBounds);
        }

        if (changedBounds.Count == 0)
        {
            return;
        }

        Rectangle dirtyBounds = MergeGlyphBounds(changedBounds);
        UploadDirtyBounds(atlasRgba, atlasWidth, dirtyBounds);
    }

    private static bool ShouldUploadGlyph(Dictionary<char, Rectangle> currentGlyphBounds, char character, Rectangle nextBounds)
    {
        if (!currentGlyphBounds.TryGetValue(character, out Rectangle currentBounds))
        {
            return true;
        }

        return currentBounds.X != nextBounds.X ||
               currentBounds.Y != nextBounds.Y ||
               currentBounds.Width != nextBounds.Width ||
               currentBounds.Height != nextBounds.Height;
    }

    private static Rectangle MergeGlyphBounds(List<Rectangle> bounds)
    {
        Rectangle mergedBounds = bounds[0];

        for (int i = 1; i < bounds.Count; i++)
        {
            Rectangle nextBounds = bounds[i];
            int left = Math.Min(mergedBounds.Left, nextBounds.Left);
            int top = Math.Min(mergedBounds.Top, nextBounds.Top);
            int right = Math.Max(mergedBounds.Right, nextBounds.Right);
            int bottom = Math.Max(mergedBounds.Bottom, nextBounds.Bottom);
            mergedBounds = new Rectangle(left, top, right - left, bottom - top);
        }

        return mergedBounds;
    }

    private unsafe void UploadDirtyBounds(byte* atlasRgba, int atlasWidth, Rectangle bounds)
    {
        int rowStride = atlasWidth * 4;
        int uploadBufferBytes = checked(bounds.Width * bounds.Height * 4);
        byte[] uploadBuffer = new byte[uploadBufferBytes];

        // one bigger upload tends to be nicer on the driver than a pile of tiny row updates.
        // We spend a little CPU time packing this block so the GPU side only sees one copy
        fixed (byte* uploadData = uploadBuffer)
        {
            for (int  y = 0; y < bounds.Height; y++)
            {
                int sourceOffset = ((bounds.Y + y) * rowStride) + (bounds.X * 4);
                int destinationOffset = y * bounds.Width * 4;
                Buffer.MemoryCopy(atlasRgba + sourceOffset,
                                  uploadData + destinationOffset,
                                  bounds.Width * 4,
                                  bounds.Width * 4);
            }

            MGG.Texture_SetData(_texture.GraphicsDevice.Handle,
                                _texture.Handle,
                                0,
                                0,
                                bounds.X,
                                bounds.Y,
                                0,
                                bounds.Width,
                                bounds.Height,
                                1,
                                uploadData,
                                uploadBufferBytes);
        }
    }
}
