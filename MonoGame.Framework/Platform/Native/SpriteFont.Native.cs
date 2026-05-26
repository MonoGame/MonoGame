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
        if (regions.Count == 0)
        {
            throw new ArgumentException("At least one character must be supplied.", nameof(characterRegions));
        }

        byte[] fontData;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            fontData = memoryStream.ToArray();
        }

        GCHandle fontDataHandle = default;
        GCHandle regionHandle = default;
        byte* atlasRgba = null;
        MGF_Glyph* glyphs = null;

        try
        {
            fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);

            MGF_CharacterRegion[] nativeRegions = new MGF_CharacterRegion[regions.Count];
            for (int i = 0; i < regions.Count; i++)
            {
                nativeRegions[i].Start = regions[i].Start;
                nativeRegions[i].End = regions[i].End;
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
            if(atlasRgba != null)
            {
                MGF.Free(atlasRgba);
            }

            if(glyphs != null)
            {
                MGF.Free(glyphs);
            }

            if(regionHandle.IsAllocated)
            {
                regionHandle.Free();
            }

            if(fontDataHandle.IsAllocated)
            {
                fontDataHandle.Free();
            }
        }
    }
}
