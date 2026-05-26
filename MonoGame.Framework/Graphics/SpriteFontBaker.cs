// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if NATIVE
using MonoGame.Interop;
#endif

namespace Microsoft.Xna.Framework.Graphics;

internal static class SpriteFontBaker
{
    public static SpriteFont Bake(GraphicsDevice graphicsDevice, byte[] data, int size, IEnumerable<CharacterRegion> characterRegions)
    {
        if (graphicsDevice == null)
        {
            throw new ArgumentNullException(nameof(graphicsDevice), $"{nameof(graphicsDevice)} must not be null.");
        }

        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), $"{nameof(data)} must not be null.");
        }

        if (characterRegions == null)
        {
            throw new ArgumentNullException(nameof(characterRegions), $"{nameof(characterRegions)} must not be null.");
        }

        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), $"{nameof(size)} must be greater than zero.");
        }

        List<CharacterRegion> regions = new List<CharacterRegion>(characterRegions);
        if (regions.Count == 0)
        {
            throw new ArgumentException("At least on character must be supplied.", nameof(characterRegions));
        }

#if NATIVE
        return BakeNative(graphicsDevice, data, size, regions);
#else
        throw new PlatformNotSupportedException("Runtime SpriteFont baking is currently implemented only for MonoGame.Framework.Native.");
#endif
    }

#if NATIVE
    private static unsafe SpriteFont BakeNative(GraphicsDevice graphicsDevice, byte[] data, int size, List<CharacterRegion> characterRegions)
    {
        GCHandle fontDataHandle = default;
        GCHandle regionHandle = default;
        byte* atlasRgba = null;
        MGF_Glyph* glyphs = null;

        try
        {
            fontDataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

            MGF_CharacterRegion[] nativeRegions = new MGF_CharacterRegion[characterRegions.Count];
            for (int i = 0; i < characterRegions.Count; i++)
            {
                nativeRegions[i].Start = characterRegions[i].Start;
                nativeRegions[i].End = characterRegions[i].End;
            }

            regionHandle = GCHandle.Alloc(nativeRegions, GCHandleType.Pinned);

            if (!MGF.BakeSpriteFont((byte*)fontDataHandle.AddrOfPinnedObject(),
                                    data.Length,
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
                throw new InvalidOperationException("Failed to bake a runtime SpriteFont from the supplied font data");
            }

            if (atlasRgba == null || glyphs == null || glyphCount <= 0)
            {
                throw new InvalidOperationException("Runtime SpriteFont baking did not return any glyph data.");
            }

            byte[] atlasPixels = new byte[atlasWidth * atlasHeight * 4];
            Marshal.Copy((IntPtr)atlasRgba, atlasPixels, 0, atlasPixels.Length);

            Texture2D texture = new Texture2D(graphicsDevice, atlasWidth, atlasHeight, false, SurfaceFormat.Color);
            texture.SetData(atlasPixels);

            List<Rectangle> glyphBounds = new List<Rectangle>(glyphCount);
            List<Rectangle> cropping = new List<Rectangle>(glyphCount);
            List<char> characters = new List<char>(glyphCount);
            List<Vector3> kerning = new List<Vector3>(glyphCount);

            
            for(int i = 0; i < glyphCount; i++)
            {
                MGF_Glyph glyph = glyphs[i];
                characters.Add(glyph.Character);
                glyphBounds.Add(new Rectangle(glyph.BoundsX, glyph.BoundsY, glyph.BoundsWidth, glyph.BoundsHeight));
                cropping.Add(new Rectangle(glyph.CroppingX, glyph.CroppingY, glyph.CroppingWidth, glyph.CroppingHeight));
                kerning.Add(new Vector3(glyph.LeftSideBearing, glyph.Width, glyph.RightSideBearing));
            }

            return new SpriteFont(texture, glyphBounds, cropping, characters, lineSpacing, 0.0f, kerning, null);
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
#endif
}
