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
        return CreateRuntimeSpriteFont(graphicsDevice,
                                       ReadFontData(stream),
                                       size,
                                       runtimeRegions,
                                       null);
    }

    private static unsafe SpriteFont CreateRuntimeSpriteFont(GraphicsDevice graphicsDevice,
                                                             byte[] fontData,
                                                             int size,
                                                             CharacterRegion[] runtimeRegions,
                                                             char? defaultCharacter)
    {
        SpriteFontRuntimeState runtimeState = new SpriteFontRuntimeState(fontData, size, runtimeRegions);
        if(runtimeRegions.Length == 0)
        {
            return CreateEmptyRuntimeSpriteFont(graphicsDevice, size, runtimeState);
        }

        GCHandle fontDataHandle = default;
        GCHandle regionHandle = default;
        byte* atlasRgba = null;
        MGF_Glyph* glyphs = null;

        try
        {
            fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);

            MGF_CharacterRegion[] nativeRegions = new MGF_CharacterRegion[runtimeRegions.Length];
            for (int i = 0; i < runtimeRegions.Length; i++)
            {
                nativeRegions[i].Start = runtimeRegions[i].Start;
                nativeRegions[i].End = runtimeRegions[i].End;
            }

            regionHandle = GCHandle.Alloc(nativeRegions, GCHandleType.Pinned);

            if (!MGF.BakeSpriteFont((byte*)fontDataHandle.AddrOfPinnedObject(),
                                   fontData.Length,
                                   size,
                                   (MGF_CharacterRegion*)regionHandle.AddrOfPinnedObject(),
                                   nativeRegions.Length,
                                   out atlasRgba,
                                   out int atlasWidth,
                                   out int atlasHeight,
                                   out glyphs,
                                   out int glyphCount,
                                   out int lineSpacing))
            {
                throw new InvalidOperationException("Failed to bake a runtime SpriteFont from the supplied font data.");
            }

            if (atlasRgba == null || glyphs == null || glyphCount <= 0)
            {
                throw new InvalidOperationException("Runtime SpriteFont baking did not return any glyph data.");
            }

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
        finally
        {
            if (atlasRgba != null)
            {
                MGF.Free(atlasRgba);
            }

            if (glyphs != null)
            {
                MGF.Free(glyphs);
            }

            if (regionHandle.IsAllocated)
            {
                regionHandle.Free();
            }

            if (fontDataHandle.IsAllocated)
            {
                fontDataHandle.Free();
            }
        }
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

    private void PlatformEnsureGlyphs(ref CharacterSource text)
    {
        List<char> missingCharacters = GetMissingCharacters(ref text);
        if (missingCharacters.Count == 0)
        {
            return;
        }

        CharacterRegion[] characterRegions = BuildCharacterRegions(missingCharacters);
        SpriteFont spriteFont = CreateRuntimeSpriteFont(_texture.GraphicsDevice,
                                                        _runtimeState.FontData,
                                                        _runtimeState.Size,
                                                        characterRegions,
                                                        _defaultCharacter);
        _runtimeState.CharacterRegions = characterRegions;
        SetRuntimeGlyphData(spriteFont);
    }

    private List<char> GetMissingCharacters(ref CharacterSource text)
    {
        HashSet<char> missingCharacters = new HashSet<char>();
        for(int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            if(character == '\r' || character == '\n')
            {
                continue;
            }

            int glyphIndex;
            if(!TryGetGlyphIndexExact(character, out _))
            {
                missingCharacters.Add(character);
            }            
        }

        return new List<char>(missingCharacters);
    }

    private CharacterRegion[] BuildCharacterRegions(List<char> missingCharacters)
    {
        HashSet<char> allCharacters = new HashSet<char>(Characters);
        for(int i = 0; i < missingCharacters.Count; i++)
        {
            allCharacters.Add(missingCharacters[i]);
        }

        List<char> sortedCharacters = new List<char>(allCharacters);
        sortedCharacters.Sort();

        List<CharacterRegion> regions = new List<CharacterRegion>();
        if(sortedCharacters.Count == 0)
        {
            return regions.ToArray();
        }

        char regionStart = sortedCharacters[0];
        char regionEnd = sortedCharacters[0];

        for(int i = 0; i < sortedCharacters.Count; i++)
        {
            char character = sortedCharacters[i];
            if(character == regionEnd + 1)
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

    private static SpriteFont CreateEmptyRuntimeSpriteFont(GraphicsDevice graphicsDevice, int size, SpriteFontRuntimeState runtimeState)
    {
        Texture2D texture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
        texture.SetData(new Color[] { Color.Transparent });

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
}
