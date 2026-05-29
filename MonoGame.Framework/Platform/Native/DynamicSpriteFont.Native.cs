// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

public sealed partial class DynamicSpriteFont
{
#if NATIVE
    private unsafe void PlatformEnsureGlyphs(int rasterizedSize,
                                             PreparedTextFontData preparedTextFontData,
                                             ref FontCharacterSource text)
    {
        List<char> missingCharacters = GetMissingCharacters(preparedTextFontData, ref text);
        if (missingCharacters.Count == 0)
        {
            return;
        }

        CharacterRegion[] characterRegions = BuildCharacterRegions(missingCharacters);
        EnsureRuntimeGlyphs(_runtimeState,
                            rasterizedSize,
                            characterRegions,
                            out byte* atlasRgba,
                            out int atlasWidth,
                            out int atlasHeight,
                            out bool atlasRebuilt,
                            out MGF_Glyph* glyphs,
                            out int glyphCount,
                            out int lineSpacing);

        Texture2D currentTexture = _texture;
        Texture2D nextTexture = currentTexture;
        if (currentTexture.Width != atlasWidth || currentTexture.Height != atlasHeight)
        {
            nextTexture = new Texture2D(currentTexture.GraphicsDevice, atlasWidth, atlasHeight, false, SurfaceFormat.Color);
        }

        if (atlasRebuilt || nextTexture != currentTexture)
        {
            MGG.Texture_SetData(currentTexture.GraphicsDevice.Handle,
                                nextTexture.Handle,
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
            UploadChangedGlyphBounds(preparedTextFontData, glyphs, glyphCount, atlasRgba, atlasWidth);
        }

        FontGlyph[] updatedGlyphs = CreateGlyphs(glyphs, glyphCount);
        UpdatePreparedTextFontDataCaches(nextTexture, updatedGlyphs, rasterizedSize, lineSpacing);

        _texture = nextTexture;

        if (nextTexture != currentTexture)
        {
            currentTexture.Dispose();
        }
    }

    private static CharacterRegion[] BuildCharacterRegions(List<char> missingCharacters)
    {
        if (missingCharacters.Count == 0)
        {
            return Array.Empty<CharacterRegion>();
        }

        missingCharacters.Sort();

        List<CharacterRegion> regions = new List<CharacterRegion>();
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

    private static unsafe FontGlyph[] CreateGlyphs(MGF_Glyph* glyphs, int glyphCount)
    {
        FontGlyph[] dynamicGlyphs = new FontGlyph[glyphCount];

        for (int i = 0; i < glyphCount; i++)
        {
            MGF_Glyph glyph = glyphs[i];
            dynamicGlyphs[i] = new FontGlyph
            {
                Character = glyph.Character,
                Size = glyph.Size,
                BoundsInTexture = new Rectangle(glyph.BoundsX, glyph.BoundsY, glyph.BoundsWidth, glyph.BoundsHeight),
                Cropping = new Rectangle(glyph.CroppingX, glyph.CroppingY, glyph.CroppingWidth, glyph.CroppingHeight),
                LeftSideBearing = glyph.LeftSideBearing,
                Width = glyph.Width,
                RightSideBearing = glyph.RightSideBearing,
                WidthIncludingBearings = glyph.LeftSideBearing + glyph.Width + glyph.RightSideBearing
            };
        }

        return dynamicGlyphs;
    }

    private static unsafe DynamicSpriteFontRuntimeState CreateRuntimeState(byte[] fontData)
    {
        GCHandle fontDataHandle = default;

        try
        {
            fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            MGF_RuntimeFont* runtimeFont = MGF.RuntimeFont_Create((byte*)fontDataHandle.AddrOfPinnedObject(), fontData.Length);
            if (runtimeFont == null)
            {
                throw new InvalidOperationException("Failed to create a runtime DynamicSpriteFont from the supplied font data.");
            }

            return new DynamicSpriteFontRuntimeState(runtimeFont);
        }
        finally
        {
            if (fontDataHandle.IsAllocated)
            {
                fontDataHandle.Free();
            }
        }
    }

    private static unsafe void EnsureRuntimeGlyphs(DynamicSpriteFontRuntimeState runtimeState,
                                                   int size,
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
                                             size,
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
                throw new InvalidOperationException("Failed to update a runtime DynamicSpriteFont from the supplied font data.");
            }

            if (atlasRgba == null || glyphs == null || glyphCount <= 0)
            {
                throw new InvalidOperationException("Runtime DynamicSpriteFont update did not return any glyph data.");
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

    private static List<char> GetMissingCharacters(PreparedTextFontData preparedTextFontData, ref FontCharacterSource text)
    {
        HashSet<char> missingCharacters = new HashSet<char>();

        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            if (character == '\r' || character == '\n')
            {
                continue;
            }

            if (!preparedTextFontData.TryGetGlyphIndexExact(character, out _))
            {
                missingCharacters.Add(character);
            }
        }

        return new List<char>(missingCharacters);
    }

    private static long GetGlyphLookupKey(char character, int size)
    {
        return ((long)size << 16) | character;
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

    private static bool ShouldUploadGlyph(Dictionary<long, Rectangle> currentGlyphBounds, long glyphKey, Rectangle nextBounds)
    {
        if (!currentGlyphBounds.TryGetValue(glyphKey, out Rectangle currentBounds))
        {
            return true;
        }

        return currentBounds != nextBounds;
    }

    private static unsafe void UploadChangedGlyphBounds(PreparedTextFontData preparedFontTextData,
                                                        MGF_Glyph* glyphs,
                                                        int glyphCount,
                                                        byte* atlasRgba,
                                                        int atlasWidth)
    {
        Dictionary<long, Rectangle> currentGlyphBounds = new Dictionary<long, Rectangle>(preparedFontTextData.Glyphs.Length);
        List<Rectangle> changedBounds = new List<Rectangle>();

        for (int i = 0; i < preparedFontTextData.Glyphs.Length; i++)
        {
            FontGlyph glyph = preparedFontTextData.Glyphs[i];
            currentGlyphBounds[GetGlyphLookupKey(glyph.Character, glyph.Size)] =glyph.BoundsInTexture;
        }

        for (int i = 0; i < glyphCount; i++)
        {
            MGF_Glyph glyph = glyphs[i];
            Rectangle nextBounds = new Rectangle(glyph.BoundsX, glyph.BoundsY, glyph.BoundsWidth, glyph.BoundsHeight);
            if (!ShouldUploadGlyph(currentGlyphBounds, GetGlyphLookupKey(glyph.Character, glyph.Size), nextBounds))
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
        UploadDirtyBounds(preparedFontTextData.Texture, atlasRgba, atlasWidth, dirtyBounds);
    }

    private void UpdatePreparedTextFontDataCaches(Texture2D texture, FontGlyph[] glyphs, int currentSize, int currentLineSpacing)
    {
        Dictionary<int, List<FontGlyph>> glyphsBySize = new Dictionary<int, List<FontGlyph>>();

        for(int i = 0; i < glyphs.Length; i++)
        {
            FontGlyph glyph = glyphs[i];
            List<FontGlyph> glyphList;
            if (!glyphsBySize.TryGetValue(glyph.Size, out glyphList))
            {
                glyphList = new List<FontGlyph>();
                glyphsBySize.Add(glyph.Size, glyphList);
            }

            glyphList.Add(glyph);
        }

        foreach(KeyValuePair<int, PreparedTextFontData> pair in _preparedTextFontDataBySize)
        {
            int lineSpacing = pair.Key == currentSize ? currentLineSpacing : pair.Value.LineSpacing;

            if (glyphsBySize.TryGetValue(pair.Key, out List<FontGlyph> existingGlyphs))
            {
                pair.Value.Update(texture, existingGlyphs.ToArray(), lineSpacing);
                glyphsBySize.Remove(pair.Key);
            }
            else
            {
                pair.Value.Update(texture, Array.Empty<FontGlyph>(), lineSpacing);
            }
        }

        foreach (KeyValuePair<int, List<FontGlyph>> pair in glyphsBySize)
        {
            int lineSpacing = pair.Key == currentSize ? currentLineSpacing : 0;
            _preparedTextFontDataBySize[pair.Key] = new PreparedTextFontData(texture, pair.Value.ToArray(), lineSpacing, 0.0f);
        }
    }

    private static unsafe void UploadDirtyBounds(Texture2D texture,
                                                 byte* atlasRgba,
                                                 int atlasWidth,
                                                 Rectangle bounds)
    {
        int rowStride = atlasWidth * 4;
        int uploadBufferBytes = checked(bounds.Width * bounds.Height * 4);
        byte[] uploadBuffer = new byte[uploadBufferBytes];

        fixed (byte* uploadData = uploadBuffer)
        {
            for (int y = 0; y < bounds.Height; y++)
            {
                int sourceOffset = ((bounds.Y + y) * rowStride) + (bounds.X * 4);
                int destinationOffset = y * bounds.Width * 4;
                Buffer.MemoryCopy(atlasRgba + sourceOffset,
                                  uploadData + destinationOffset,
                                  bounds.Width * 4,
                                  bounds.Width * 4);
            }

            MGG.Texture_SetData(texture.GraphicsDevice.Handle,
                                texture.Handle,
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
#else
    private void PlatformEnsureGlyphs(int rasterizedSize,
                                      PreparedTextFontData preparedTextFontData,
                                      ref FontCharacterSource text)
    {
        throw new PlatformNotSupportedException("Runtime SpriteFont baking is currently implemented only for MonoGame.Framework.Native.");
    }

    private static DynamicSpriteFontRuntimeState CreateRuntimeState(byte[] fontData)
    {
        throw new PlatformNotSupportedException("Runtime SpriteFont baking is currently implemented only for MonoGame.Framework.Native.");
    }
#endif
}
