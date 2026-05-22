// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using StbRectPackSharp;
using StbTrueTypeSharp;

using StbTrueTypeFontInfo = StbTrueTypeSharp.StbTrueType.stbtt_fontinfo;
using StbRectPackRect = StbRectPackSharp.StbRectPack.stbrp_rect;
using StbRectPackContext = StbRectPackSharp.StbRectPack.stbrp_context;

namespace Microsoft.Xna.Framework.Graphics;

internal static class SpriteFontBaker
{
    private const int Padding = 1;
    private const int MinimumAtlasSize = 256;
    private const int MaximumAtlasSize = 4096;

    public static unsafe SpriteFont Bake(GraphicsDevice graphicsDevice, byte[] data, int size, IEnumerable<CharacterRegion> characterRegions)
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

        List<char> characters = CollectCharacters(characterRegions);
        if (characters.Count == 0)
        {
            throw new ArgumentException("At least one character must be supplied.", nameof(characterRegions));
        }

        StbTrueTypeFontInfo fontInfo = StbTrueType.CreateFont(data, 0);
        if (fontInfo == null)
        {
            throw new InvalidOperationException("The supplied font data could not be parsed.");
        }

        using (fontInfo)
        {
            float scale = StbTrueType.stbtt_ScaleForMappingEmToPixels(fontInfo, size);

            int ascent;
            int descent;
            int lineGap;
            StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);

            int ascentPixels = (int)Math.Ceiling(ascent * scale);
            int descentPixels = (int)Math.Ceiling(-descent * scale);
            int lineGapPixels = (int)Math.Ceiling(lineGap * scale);
            int lineSpacing = ascentPixels + descentPixels + lineGapPixels;

            List<GlyphBuildInfo> glyphs = BuildGlyphs(fontInfo, characters, scale);
            if (glyphs.Count == 0)
            {
                throw new InvalidOperationException("The supplied character regions do not resolve to any glyphs in this font.");
            }

            int atlasSize = EstimateInitialAtlasSize(glyphs);
            while (!TryPackGlyphs(glyphs, atlasSize, atlasSize))
            {
                if (atlasSize >= MaximumAtlasSize)
                {
                    throw new InvalidOperationException("The supplied character regions could not be packed into a texture atlas.");
                }

                atlasSize *= 2;
            }

            byte[] atlasPixels = new byte[Math.Max(1, atlasSize * atlasSize)];
            RasterizeGlyphs(fontInfo, glyphs, atlasPixels, atlasSize, scale);

            Texture2D texture = new Texture2D(graphicsDevice, atlasSize, atlasSize, false, SurfaceFormat.Color);
            texture.SetData(ConvertAlphaMaskToRgba(atlasPixels));

            List<Rectangle> glyphBounds = new List<Rectangle>(glyphs.Count);
            List<Rectangle> cropping = new List<Rectangle>(glyphs.Count);
            List<char> bakedCharacters = new List<char>(glyphs.Count);
            List<Vector3> kerning = new List<Vector3>(glyphs.Count);

            foreach (GlyphBuildInfo glyph in glyphs)
            {
                bakedCharacters.Add(glyph.Character);
                glyphBounds.Add(new Rectangle(glyph.AtlasX, glyph.AtlasY, glyph.Width, glyph.Height));
                cropping.Add(new Rectangle(0, ascentPixels + glyph.BitmapY0, glyph.Width, lineSpacing));
                kerning.Add(new Vector3(glyph.LeftSideBearing, glyph.WidthValue, glyph.RightSideBearing));
            }

            return new SpriteFont(texture, glyphBounds, cropping, bakedCharacters, lineSpacing, 0.0f, kerning, null);
        }
    }

    private static List<char> CollectCharacters(IEnumerable<CharacterRegion> characterRegions)
    {
        SortedSet<char> characters = new SortedSet<char>();

        foreach (CharacterRegion region in characterRegions)
        {
            for (int value = region.Start; value <= region.End; value++)
            {
                characters.Add((char)value);
            }
        }

        return new List<char>(characters);
    }

    private static unsafe List<GlyphBuildInfo> BuildGlyphs(StbTrueTypeFontInfo fontInfo, List<char> characters, float scale)
    {
        List<GlyphBuildInfo> glyphs = new List<GlyphBuildInfo>(characters.Count);

        foreach (char character in characters)
        {
            int glyphIndex = StbTrueType.stbtt_FindGlyphIndex(fontInfo, character);
            if (glyphIndex == 0)
            {
                continue;
            }

            int bitmapX0;
            int bitmapY0;
            int bitmapX1;
            int bitmapY1;
            StbTrueType.stbtt_GetGlyphBitmapBox(fontInfo, glyphIndex, scale, scale, &bitmapX0, &bitmapY0, &bitmapX1, &bitmapY1);

            int advanceWidth;
            int leftSideBearing;
            StbTrueType.stbtt_GetGlyphHMetrics(fontInfo, glyphIndex, &advanceWidth, &leftSideBearing);

            int width = Math.Max(0, bitmapX1 - bitmapX0);
            int height = Math.Max(0, bitmapY1 - bitmapY0);
            float advancePixels = advanceWidth * scale;

            GlyphBuildInfo glyph = new GlyphBuildInfo()
            {
                Character = character,
                GlyphIndex = glyphIndex,
                BitmapX0 = bitmapX0,
                BitmapY0 = bitmapY0,
                Width = width,
                Height = height
            };

            if (width > 0)
            {
                glyph.LeftSideBearing = bitmapX0;
                glyph.WidthValue = width;
                glyph.RightSideBearing = advancePixels - bitmapX1;
            }
            else
            {
                glyph.LeftSideBearing = 0.0f;
                glyph.WidthValue = 0.0f;
                glyph.RightSideBearing = advancePixels;
            }

            glyphs.Add(glyph);
        }

        return glyphs;
    }

    private static int EstimateInitialAtlasSize(List<GlyphBuildInfo> glyphs)
    {
        int totalArea = 0;

        foreach (GlyphBuildInfo glyph in glyphs)
        {
            if (glyph.Width == 0 || glyph.Height == 0)
            {
                continue;
            }

            totalArea += (glyph.Width + Padding) * (glyph.Height + Padding);
        }

        if (totalArea == 0)
        {
            return 1;
        }

        int estimated = (int)Math.Ceiling(Math.Sqrt(totalArea * 1.5));
        estimated = Math.Max(MinimumAtlasSize, estimated);
        estimated = Math.Min(MaximumAtlasSize, estimated);
        return MathHelper.NextPowerOfTwo(estimated);
    }

    private static unsafe bool TryPackGlyphs(List<GlyphBuildInfo> glyphs, int atlasWidth, int atlasHeight)
    {
        int packedGlyphCount = 0;
        foreach (GlyphBuildInfo glyph in glyphs)
        {
            if (glyph.Width > 0 && glyph.Height > 0)
            {
                packedGlyphCount++;
            }
        }

        if (packedGlyphCount == 0)
        {
            return true;
        }

        StbRectPackRect[] rectangles = new StbRectPackRect[packedGlyphCount];
        int[] glyphIndexes = new int[packedGlyphCount];

        int rectangleIndex = 0;
        for (int i = 0; i < glyphs.Count; i++)
        {
            GlyphBuildInfo glyph = glyphs[i];
            if (glyph.Width == 0 || glyph.Height == 0)
            {
                continue;
            }

            StbRectPackRect rectangle = rectangles[rectangleIndex];
            rectangle.id = rectangleIndex;
            rectangle.w = glyph.Width + Padding;
            rectangle.h = glyph.Height + Padding;
            rectangles[rectangleIndex] = rectangle;

            glyphIndexes[rectangleIndex] = i;
            rectangleIndex++;
        }

        using (StbRectPackContext context = new StbRectPackContext(atlasWidth))
        {
            StbRectPackContext* contextPtr = &context;
            fixed (StbRectPackRect* rectanglesPtr = rectangles)
            {
                StbRectPack.stbrp_init_target(contextPtr, atlasWidth, atlasHeight, context.all_nodes, atlasWidth);
                StbRectPack.stbrp_pack_rects(contextPtr, rectanglesPtr, rectangles.Length);
            }
        }

        for (int i = 0; i < rectangles.Length; i++)
        {
            if (rectangles[i].was_packed == 0)
            {
                return false;
            }

            GlyphBuildInfo glyph = glyphs[glyphIndexes[i]];
            glyph.AtlasX = rectangles[i].x;
            glyph.AtlasY = rectangles[i].y;
            glyphs[glyphIndexes[i]] = glyph;
        }

        return true;
    }

    private static unsafe void RasterizeGlyphs(StbTrueTypeFontInfo fontInfo, List<GlyphBuildInfo> glyphs, byte[] atlasPixels, int atlasWidth, float scale)
    {
        fixed (byte* atlasPixelsPtr = atlasPixels)
        {
            foreach (GlyphBuildInfo glyph in glyphs)
            {
                if (glyph.Width == 0 || glyph.Height == 0)
                {
                    continue;
                }

                byte* destination = atlasPixelsPtr + (glyph.AtlasY * atlasWidth) + glyph.AtlasX;
                StbTrueType.stbtt_MakeGlyphBitmap(fontInfo, destination, glyph.Width, glyph.Height, atlasWidth, scale, scale, glyph.GlyphIndex);
            }
        }
    }

    private static byte[] ConvertAlphaMaskToRgba(byte[] atlasPixels)
    {
        byte[] texturePixels = new byte[atlasPixels.Length * 4];

        for (int i = 0; i < atlasPixels.Length; i++)
        {
            byte alpha = atlasPixels[i];
            int textureIndex = i * 4;
            texturePixels[textureIndex + 0] = alpha;
            texturePixels[textureIndex + 1] = alpha;
            texturePixels[textureIndex + 2] = alpha;
            texturePixels[textureIndex + 3] = alpha;
        }

        return texturePixels;
    }

    private struct GlyphBuildInfo
    {
        public char Character;
        public int GlyphIndex;
        public int BitmapX0;
        public int BitmapY0;
        public int Width;
        public int Height;
        public int AtlasX;
        public int AtlasY;
        public float LeftSideBearing;
        public float WidthValue;
        public float RightSideBearing;
    }
}
