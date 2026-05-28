// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Runtime.InteropServices;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

public sealed partial class DynamicSpriteFont
{
#if NATIVE
    private unsafe DynamicSpriteFontSizeData CreateSizeData(int rasterizedSize)
    {
        DynamicSpriteFontRuntimeState runtimeState = CreateRuntimeState(_fontData, rasterizedSize);
        if (_characterRegions.Length == 0)
        {
            return CreateEmptyRuntimeSizeData(_graphicsDevice, runtimeState, rasterizedSize);
        }

        return CreateRuntimeSizeData(_graphicsDevice, runtimeState, _characterRegions);
    }

    private unsafe void PlatformEnsureGlyphs(DynamicSpriteFontSizeData sizeData, ref DynamicSpriteFontCharacterSource text)
    {
        List<char> missingCharacters = GetMissingCharacters(sizeData, ref text);
        if (missingCharacters.Count == 0)
        {
            return;
        }

        CharacterRegion[] characterRegions = BuildCharacterRegions(missingCharacters);
        EnsureRuntimeGlyphs(sizeData.RuntimeState,
                            characterRegions,
                            out byte* atlasRgba,
                            out int atlasWidth,
                            out int atlasHeight,
                            out bool atlasRebuilt,
                            out MGF_Glyph* glyphs,
                            out int glyphCount,
                            out int lineSpacing);

        Texture2D currentTexture = sizeData.Texture;
        Texture2D nextTexture = currentTexture;
        if (currentTexture.width != atlasWidth || currentTexture.height != atlasHeight)
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
            UploadChangedGlyphBounds(sizeData, glyphs, glyphCount, atlasRgba, atlasWidth);
        }

        DynamicSpriteFontGlyph[] updatedGlyphs = CreateGlyphs(glyphs, glyphCount);
        sizeData.Update(nextTexture, updatedGlyphs, lineSpacing);

        if (nextTexture != currentTexture)
        {
            currentTexture.Dispose();
        }
    }

    private static unsafe CharacterRegion[] BuildCharacterRegions(List<char> missingCharacters)
    {
        if (missingCharacters.Count == 0)
        {
            return Array.Empty<CharacterRegion>();
        }

        missingCharacters.Sort();

        List<CharacterRegion> regions = new List<CharacterRegion>();

        char regionStart = missingCharacters[0];
        char regionEnd = missingCharacters[0];

        for (int i = 0; i < missingCharacters.Count; i++)
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

    private static unsafe DynamicSpriteFontGlyph[] CreateGlyphs(MGF_Glyph* glyphs, int glyphCount)
    {
        DynamicSpriteFontGlyph[] spriteFontGlyphs = new DynamicSpriteFontGlyph[glyphCount];

        for (int i = 0; i < glyphCount; i++)
        {
            MGF_Glyph glyph = glyphs[i];
            spriteFontGlyphs[i] = new DynamicSpriteFontGlyph
            {
                Character = glyph.Character,
                BoundsInTexture = new Rectangle(glyph.BoundsX, glyph.BoundsY, glyph.BoundsWidth, glyph.BoundsHeight),
                Cropping = new Rectangle(glyph.CroppingX, glyph.CroppingY, glyph.CroppingWidth, glyph.CroppingHeight),
                LeftSideBearing = glyph.LeftSideBearing,
                Width = glyph.Width,
                RightSideBearing = glyph.RightSideBearing,
                WidthIncludingBearings = glyph.LeftSideBearing + glyph.Width + glyph.RightSideBearing
            };
        }

        return spriteFontGlyphs;
    }

    private static unsafe DynamicSpriteFontSizeData CreateEmptyRuntimeSizeData(GraphicsDevice graphicsDevice,
                                                                               DynamicSpriteFontRuntimeState runtimeState,
                                                                               int size)
    {
        Texture2D texture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
        texture.SetData(new Color[] { Color.Transparent });

        return new DynamicSpriteFontSizeData(texture, Array.Empty<DynamicSpriteFontGlyph>(), size, 0.0f, runtimeState);
    }

    private static unsafe DynamicSpriteFontRuntimeState CreateRuntimeState(byte[] fontData, int size)
    {
        GCHandle fontDataHandle = default;

        try
        {
            fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            MGF_RuntimeFont* runtimeFont = MGF.RuntimeFont_Create((byte*)fontDataHandle.AddrOfPinnedObject(), fontData.Length, size);
            if (runtimeFont == null)
            {
                throw new InvalidOperationException("Failed to create a runtime DynamicSpriteFont from the supplied font data");
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

    private static unsafe DynamicSpriteFontSizeData CreateRuntimeSizeData(GraphicsDevice graphicsDevice,
                                                                          DynamicSpriteFontRuntimeState runtimeState,
                                                                          CharacterRegion[] runtimeRegions)
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

        DynamicSpriteFontGlyph[] spriteFontGlyphs = CreateGlyphs(glyphs, glyphCount);
        return new DynamicSpriteFontSizeData(texture, spriteFontGlyphs, lineSpacing, 0.0f, runtimeState);
    }

    private static unsafe void EnsureRuntimeGlyphs(DynamicSpriteFontRuntimeState runtimeState,
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

    private static List<char> GetMissingCharacters(DynamicSpriteFontSizeData sizeData, ref DynamicSpriteFontCharacterSource text)
    {
        HashSet<char> missingCharacters = new HashSet<char>();
        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            if (character == '\r' || character == '\n')
            {
                continue;
            }

            if (!sizeData.TryGetGlyphIndexExact(character, out _))
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

    private static bool ShouldUploadGlyph(Dictionary<char, Rectangle> currentGlyphBounds, char character, Rectangle nextBounds)
    {
        if (!currentGlyphBounds.TryGetValue(character, out Rectangle currentBounds))
        {
            return true;
        }

        return currentBounds != nextBounds;
    }

    private static unsafe void UploadChangedGlyphBounds(DynamicSpriteFontSizeData sizeData,
                                                       MGF_Glyph* glyphs,
                                                       int glyphCount,
                                                       byte* atlasRgba,
                                                       int atlasWidth)
    {
        Dictionary<char, Rectangle> currentGlyphBounds = new Dictionary<char, Rectangle>(sizeData.Glyphs.Length);
        List<Rectangle> changedBounds = new List<Rectangle>();

        for (int i = 0; i < sizeData.Glyphs.Length; i++)
        {
            currentGlyphBounds[sizeData.Glyphs[i].Character] = sizeData.Glyphs[i].BoundsInTexture;
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
        UploadDirtyBounds(sizeData.Texture, atlasRgba, atlasWidth, dirtyBounds);
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
    private DynamicSpriteFontSizeData CreateSizeData(int rasterizedSize)
    {
        throw new PlatformNotSupportedException("Runtime SpriteFont baking is currently implemented only for MonoGame.Framework.Native.");
    }

    private void PlatformEnsureGlyphs(DynamicSpriteFontSizeData sizeData, ref DynamicSpriteFontCharacterSource text)
    {
        throw new PlatformNotSupportedException("Runtime SpriteFont baking is currently implemented only for MonoGame.Framework.Native.");
    }
#endif
}