// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

public sealed partial class DynamicSpriteFont : GraphicsResource
{
#if NATIVE
    private unsafe void PlatformEnsureGlyphs(int rasterizedSize,
                                             PreparedTextFont preparedTextFont,
                                             ref FontCharacterSource text)
    {
        List<char> missingCharacters = GetMissingCharacters(preparedTextFont, ref text);
        if (_defaultCharacter.HasValue && !preparedTextFont.TryGetGlyphIndex(_defaultCharacter.Value, out _))
        {
            missingCharacters.Add(_defaultCharacter.Value);
        }

        if (missingCharacters.Count == 0)
        {
            return;
        }

        CharacterRegion[] characterRegions = BuildCharacterRegions(missingCharacters);
        EnsureNativeGlyphs(_fontHandle,
                           rasterizedSize,
                           characterRegions,
                           out MGF_PageUpdate* pageUpdates,
                           out int pageUpdateCount,
                           out MGF_Glyph* glyphs,
                           out int glyphCount,
                           out int lineSpacing);

        ApplyGlyphUpdateResults(rasterizedSize,
                                pageUpdates,
                                pageUpdateCount,
                                glyphs,
                                glyphCount,
                                lineSpacing);
    }

    private unsafe void PlatformWarmGlyphs(int rasterizedSize, CharacterRegion[] characterRegions)
    {
        EnsureNativeGlyphs(_fontHandle,
                           rasterizedSize,
                           characterRegions,
                           out MGF_PageUpdate* pageUpdates,
                           out int pageUpdateCount,
                           out MGF_Glyph* glyphs,
                           out int glyphCount,
                           out int lineSpacing);

        ApplyGlyphUpdateResults(rasterizedSize,
                                pageUpdates,
                                pageUpdateCount,
                                glyphs,
                                glyphCount,
                                lineSpacing);
    }

    private unsafe void ApplyGlyphUpdateResults(int rasterizedSize,
                                                MGF_PageUpdate* pageUpdates,
                                                int pageUpdateCount,
                                                MGF_Glyph* glyphs,
                                                int glyphCount,
                                                int lineSpacing)
    {

        int currentPageIndex = _currentPageIndex;
        for (int i = 0; i < pageUpdateCount; i++)
        {
            MGF_PageUpdate pageUpdate = pageUpdates[i];
            currentPageIndex = pageUpdate.PageIndex;
            ApplyPageUpdate(glyphs, glyphCount, pageUpdate);
        }

        // Native returns the full glyph list because atlas rebuilds can repack previously baked glyphs onto
        // different bounds or pages, so the managed caches must refresh from the complete glyph state.
        FontGlyph[] updatedGlyphs = CreateGlyphs(glyphs, glyphCount);
        _currentPageIndex = currentPageIndex;
        UpdateGlyphBoundsByPage(updatedGlyphs);
        UpdatePreparedTextFontCaches(updatedGlyphs, rasterizedSize, lineSpacing);
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
                PageIndex = glyph.PageIndex,
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

    private static unsafe FontHandle CreateFontHandle(byte[] fontData)
    {
        GCHandle fontDataHandle = default;

        try
        {
            fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            MGF_ResultCode resultCode = MGF.MGF_Font_Create((byte*)fontDataHandle.AddrOfPinnedObject(),
                                                        fontData.Length,
                                                        out MGF_Font* font);

            if (resultCode != MGF_ResultCode.Success)
            {
                throw resultCode switch
                {
                    MGF_ResultCode.BackendInitializationFailed => new InvalidOperationException($"Failed to create {nameof(DynamicSpriteFont)} because the font system could not be initialized."),
                    MGF_ResultCode.InternalError => new InvalidOperationException($"Failed to create {nameof(DynamicSpriteFont)} because an internal font error occurred."),
                    MGF_ResultCode.InvalidArgument => new InvalidOperationException($"Failed to create {nameof(DynamicSpriteFont)} because invalid arguments were passed to the font system."),
                    MGF_ResultCode.InvalidFontData => new InvalidOperationException($"Failed to create {nameof(DynamicSpriteFont)} because the supplied font data is invalid."),
                    MGF_ResultCode.OutOfMemory => new OutOfMemoryException($"Failed to create {nameof(DynamicSpriteFont)} because the font loader ran out of memory while initializing the font face."),
                    _ => new InvalidOperationException($"Failed to create {nameof(DynamicSpriteFont)} because the font system return {resultCode}")
                };
            }

            // It should be impossible for a result code to be Success AND the font handle to be null
            if (font == null)
            {
                throw new InvalidOperationException($"Failed to create {nameof(DynamicSpriteFont)} because the font system reported success without returning a font handle.");
            }

            return new FontHandle(font);
        }
        finally
        {
            if (fontDataHandle.IsAllocated)
            {
                fontDataHandle.Free();
            }
        }
    }

    private static unsafe void EnsureNativeGlyphs(FontHandle fontHandle,
                                                  int size,
                                                  CharacterRegion[] characterRegions,
                                                  out MGF_PageUpdate* pageUpdates,
                                                  out int pageUpdateCount,
                                                  out MGF_Glyph* glyphs,
                                                  out int glyphCount,
                                                  out int lineSpacing)
    {
        GCHandle regionHandle = default;

        try
        {
            MGF_CharacterRegion[] nativeRegions = new MGF_CharacterRegion[characterRegions.Length];
            for (int i = 0; i < characterRegions.Length; i++)
            {
                nativeRegions[i].Start = characterRegions[i].Start;
                nativeRegions[i].End = characterRegions[i].End;
            }

            regionHandle = GCHandle.Alloc(nativeRegions, GCHandleType.Pinned);

            MGF_FontEnsureGlyphsRequest request = new MGF_FontEnsureGlyphsRequest();
            request.Font = fontHandle.Handle;
            request.Size = size;
            request.CharacterRegions = (MGF_CharacterRegion*)regionHandle.AddrOfPinnedObject();
            request.CharacterRegionCount = nativeRegions.Length;

            MGF_FontEnsureGlyphsResult result = default;
            MGF_ResultCode resultCode = MGF.MGF_Font_EnsureGlyphs(&request, &result);

            pageUpdates = result.PageUpdates;
            pageUpdateCount = result.PageUpdateCount;
            glyphs = result.Glyphs;
            glyphCount = result.GlyphCount;
            lineSpacing = result.LineSpacing;

            if (resultCode != MGF_ResultCode.Success)
            {
                throw resultCode switch
                {
                    MGF_ResultCode.AtlasCapacityExceeded => new InvalidOperationException($"Failed to update {nameof(DynamicSpriteFont)}because the requested glyphs could not fit within the maximum atlas size."),
                    MGF_ResultCode.FontSizeSetupFailed => new InvalidOperationException($"Failed to update {nameof(DynamicSpriteFont)} because the requested font size could not be applied."),
                    MGF_ResultCode.GlyphLoadFailed => new InvalidOperationException($"Failed to update {nameof(DynamicSpriteFont)} because one or more glyphs could not be loaded."),
                    MGF_ResultCode.GlyphRenderFailed => new InvalidOperationException($"Failed to update {nameof(DynamicSpriteFont)} because one or more glyphs could not be rasterized."),
                    MGF_ResultCode.InternalError => new InvalidOperationException($"Failed to update {nameof(DynamicSpriteFont)} because an internal font error occurred."),
                    MGF_ResultCode.InvalidArgument => new InvalidOperationException($"Failed to update {nameof(DynamicSpriteFont)} because invalid glyph update arguments were passed to the font system."),
                    MGF_ResultCode.NoGlyphData => new InvalidOperationException($"Failed to update {nameof(DynamicSpriteFont)} because the glyph request did not produce any glyph data."),
                    MGF_ResultCode.OutOfMemory => new OutOfMemoryException($"Failed to update {nameof(DynamicSpriteFont)} because glyph baking ran out of memory."),
                    MGF_ResultCode.UnsupportedGlyphBitmapFormat => new InvalidOperationException($"Failed to update {nameof(DynamicSpriteFont)} because the glyph bitmap format is not supported."),
                    _ => new InvalidOperationException($"Failed to update {nameof(DynamicSpriteFont)} because the font system returned {resultCode}.")
                };
            }

            if (pageUpdateCount < 0 || glyphCount <= 0)
            {
                throw new InvalidOperationException($"{nameof(DynamicSpriteFont)} update did not return any glyph data.");
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

    private static List<char> GetMissingCharacters(PreparedTextFont preparedTextFont, ref FontCharacterSource text)
    {
        HashSet<char> missingCharacters = new HashSet<char>();

        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            if (character == '\r' || character == '\n')
            {
                continue;
            }

            if (!preparedTextFont.TryGetGlyphIndexExact(character, out _))
            {
                missingCharacters.Add(character);
            }
        }

        return new List<char>(missingCharacters);
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

    private static unsafe void UploadChangedGlyphBounds(Dictionary<long, Rectangle> currentGlyphBounds,
                                                        Texture2D texture,
                                                        MGF_Glyph* glyphs,
                                                        int glyphCount,
                                                        int pageIndex,
                                                        byte* atlasRgba,
                                                        int atlasWidth)
    {
        List<Rectangle> changedBounds = new List<Rectangle>();

        for (int i = 0; i < glyphCount; i++)
        {
            MGF_Glyph glyph = glyphs[i];
            if (glyph.PageIndex != pageIndex)
            {
                continue;
            }

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
        UploadDirtyBounds(texture, atlasRgba, atlasWidth, dirtyBounds);
    }

    private unsafe void ApplyPageUpdate(MGF_Glyph* glyphs,
                                        int glyphCount,
                                        MGF_PageUpdate pageUpdate)
    {
        _texturesByPage.TryGetValue(pageUpdate.PageIndex, out Texture2D currentTexture);

        Texture2D nextTexture = currentTexture;
        if (currentTexture == null ||
            currentTexture.Width != pageUpdate.AtlasWidth ||
            currentTexture.Height != pageUpdate.AtlasHeight)
        {
            nextTexture = new Texture2D(GraphicsDevice, pageUpdate.AtlasWidth, pageUpdate.AtlasHeight, false, SurfaceFormat.Color);
        }

        if (nextTexture != currentTexture)
        {
            MGG.Texture_SetData(GraphicsDevice.Handle,
                                nextTexture.Handle,
                                0,
                                0,
                                0,
                                0,
                                0,
                                pageUpdate.AtlasWidth,
                                pageUpdate.AtlasHeight,
                                1,
                                pageUpdate.AtlasRgba,
                                pageUpdate.AtlasWidth * pageUpdate.AtlasHeight * 4);
        }
        else
        {
            UploadChangedGlyphBounds(GetGlyphBoundsForPage(pageUpdate.PageIndex),
                                     nextTexture,
                                     glyphs,
                                     glyphCount,
                                     pageUpdate.PageIndex,
                                     pageUpdate.AtlasRgba,
                                     pageUpdate.AtlasWidth);
        }

        _texturesByPage[pageUpdate.PageIndex] = nextTexture;

        if (currentTexture != null && nextTexture != currentTexture)
        {
            currentTexture.Dispose();
        }
    }

    private void UpdatePreparedTextFontCaches(FontGlyph[] glyphs, int currentSize, int currentLineSpacing)
    {
        Dictionary<int, List<FontGlyph>> glyphsBySize = new Dictionary<int, List<FontGlyph>>();

        for (int i = 0; i < glyphs.Length; i++)
        {
            FontGlyph glyph = glyphs[i];

            if (!glyphsBySize.TryGetValue(glyph.Size, out List<FontGlyph> glyphList))
            {
                glyphList = new List<FontGlyph>();
                glyphsBySize.Add(glyph.Size, glyphList);
            }

            glyphList.Add(glyph);
        }

        foreach (KeyValuePair<int, PreparedTextFont> pair in _preparedTextFontsBySize)
        {
            int lineSpacing = pair.Key == currentSize ? currentLineSpacing : pair.Value.LineSpacing;

            if (glyphsBySize.TryGetValue(pair.Key, out List<FontGlyph> existingGlyphs))
            {
                pair.Value.Update(_texturesByPage, _currentPageIndex, existingGlyphs.ToArray(), lineSpacing);
                glyphsBySize.Remove(pair.Key);
            }
            else
            {
                pair.Value.Update(_texturesByPage, _currentPageIndex, Array.Empty<FontGlyph>(), lineSpacing);
            }

            int defaultGlyphIndex = -1;

            if (_defaultCharacter.HasValue)
            {
                pair.Value.TryGetGlyphIndex(_defaultCharacter.Value, out defaultGlyphIndex);
            }

            pair.Value.UpdateDefaultGlyphIndex(defaultGlyphIndex);
        }

        foreach (KeyValuePair<int, List<FontGlyph>> pair in glyphsBySize)
        {
            int lineSpacing = pair.Key == currentSize ? currentLineSpacing : 0;
            _preparedTextFontsBySize[pair.Key] = CreatePreparedTextFont(_currentPageIndex, pair.Value.ToArray(), lineSpacing, 0.0f);
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
                                      PreparedTextFont preparedTextFont,
                                      ref FontCharacterSource text)
    {
        throw new PlatformNotSupportedException("Runtime SpriteFont baking is currently implemented only for MonoGame.Framework.Native.");
    }

    private void PlatformWarmGlyphs(int rasterizedSize, CharacterRegion[] characterRegions)
    {
        throw new PlatformNotSupportedException("Runtime SpriteFont baking is currently implemented only for MonoGame.Framework.Native.");
    }

    private static FontHandle CreateFontHandle(byte[] fontData)
    {
        throw new PlatformNotSupportedException("Runtime SpriteFont baking is currently implemented only for MonoGame.Framework.Native.");
    }
#endif
}
