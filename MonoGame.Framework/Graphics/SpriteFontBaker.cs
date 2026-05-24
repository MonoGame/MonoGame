// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

        FreeType.FT_LibraryRec* library = null;
        FreeType.FT_FaceRec* face = null;
        GCHandle fontDataHandle = default;

        try
        {
            FreeType.CheckError(FreeType.FT_Init_FreeType(out library));

            fontDataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            byte* fileBase = (byte*)fontDataHandle.AddrOfPinnedObject();
            FreeType.CheckError(FreeType.FT_New_Memory_Face(library, fileBase, data.Length, 0, out face));
            FreeType.CheckError(FreeType.FT_Set_Pixel_Sizes(face, 0, (uint)size));

            int ascentPixels = FreeTypeHelper.GetAscender(face);
            int lineSpace = FreeTypeHelper.GetLineSpacing(face);

            if (lineSpace <= 0)
            {
                lineSpace = size;
            }

            List<GlyphBuildInfo> glyphs = BuildGlyphs(face, characters);
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
            RasterizeGlyphs(glyphs, atlasPixels, atlasSize);

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
                cropping.Add(new Rectangle(0, ascentPixels - glyph.BitmapTop, glyph.Width, lineSpace));
                kerning.Add(new Vector3(glyph.LeftSideBearing, glyph.WidthValue, glyph.RightSideBearing));
            }

            return new SpriteFont(texture, glyphBounds, cropping, bakedCharacters, lineSpace, 0.0f, kerning, null);
        }
        finally
        {
            if (face != null)
            {
                FreeType.FT_Done_Face(face);
            }

            if (library != null)
            {
                FreeType.FT_Done_FreeType(library);
            }

            if (fontDataHandle.IsAllocated)
            {
                fontDataHandle.Free();
            }
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

    private static unsafe List<GlyphBuildInfo> BuildGlyphs(FreeType.FT_FaceRec* face, List<char> characters)
    {
        List<GlyphBuildInfo> glyphs = new List<GlyphBuildInfo>(characters.Count);

        foreach (char character in characters)
        {
            uint glyphIndex = FreeType.FT_Get_Char_Index(face, character);
            if (glyphIndex == 0)
            {
                continue;
            }

            FreeType.CheckError(FreeType.FT_Load_Glyph(face, glyphIndex, 0));

            FreeType.FT_GlyphSlotRec* glyphSlot = FreeTypeHelper.GetGlyphSlot(face);
            FreeType.CheckError(FreeType.FT_Render_Glyph(glyphSlot, FreeType.RenderMode.Normal));

            FreeType.FT_Bitmap bitmap = FreeTypeHelper.GetGlyphBitmap(glyphSlot);
            int width = checked((int)bitmap.width);
            int height = checked((int)bitmap.rows);
            int bitmapLeft = FreeTypeHelper.GetGlyphBitmapLeft(glyphSlot);
            int bitmapTop = FreeTypeHelper.GetGlyphBitmapTop(glyphSlot);
            float advancePixels = FreeTypeHelper.GetGlyphHorizontalAdvance(glyphSlot);


            GlyphBuildInfo glyph = new GlyphBuildInfo()
            {
                Character = character,
                Width = width,
                Height = height,
                BitmapTop = bitmapTop,
                Pixels = ExtractBitmap(bitmap)
            };

            if (width > 0)
            {
                glyph.LeftSideBearing = bitmapLeft;
                glyph.WidthValue = width;
                glyph.RightSideBearing = advancePixels - (bitmapLeft + width);
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

    private static unsafe byte[] ExtractBitmap(FreeType.FT_Bitmap bitmap)
    {
        int width = checked((int)bitmap.width);
        int rows = checked((int)bitmap.rows);

        if (width == 0 || rows == 0 || bitmap.buffer == null)
        {
            return Array.Empty<byte>();
        }

        byte[] pixels = new byte[width * rows];
        int sourceRowPitch = Math.Abs(bitmap.pitch);
        byte[] rowBuffer = new byte[sourceRowPitch];

        for (int row = 0; row < bitmap.rows; row++)
        {
            int sourceRow = bitmap.pitch >= 0 ? row : (rows - 1 - row);
            byte* source = bitmap.buffer + (sourceRow * sourceRowPitch);
            Marshal.Copy((IntPtr)source, rowBuffer, 0, sourceRowPitch);

            int destinationIndex = row * width;
            switch ((FreeType.PixelMode)bitmap.pixel_mode)
            {
                case FreeType.PixelMode.Gray:
                    Buffer.BlockCopy(rowBuffer, 0, pixels, destinationIndex, width);
                    break;

                case FreeType.PixelMode.Mono:
                    ExpandMonochromeRow(rowBuffer, width, pixels, destinationIndex);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported FreeType pixel mode '{bitmap.pixel_mode}'.");
            }
        }

        return pixels;
    }

    private static void ExpandMonochromeRow(byte[] source, int width, byte[] destination, int destinationIndex)
    {
        for (int column = 0; column < width; column++)
        {
            int sourceByte = column >> 3;
            int bitIndex = 7 - (column & 7);

            destination[destinationIndex + column] = (source[sourceByte] & (1 << bitIndex)) != 0 ?
                                                     byte.MaxValue :
                                                     byte.MinValue;
        }
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
        List<int> packOrder = new List<int>(glyphs.Count);
        for (int i = 0; i < glyphs.Count; i++)
        {
            if (glyphs[i].Width > 0 && glyphs[i].Height > 0)
            {
                packOrder.Add(i);
            }
        }

        packOrder.Sort((left, right) =>
        {
            GlyphBuildInfo leftGlyph = glyphs[left];
            GlyphBuildInfo rightGlyph = glyphs[right];

            int heightComparison = rightGlyph.Height.CompareTo(leftGlyph.Height);
            if (heightComparison != 0)
            {
                return heightComparison;
            }

            return rightGlyph.Width.CompareTo(leftGlyph.Width);
        });

        int x = 0;
        int y = 0;
        int rowHeight = 0;

        foreach (int glyphIndex in packOrder)
        {
            GlyphBuildInfo glyph = glyphs[glyphIndex];
            int packedWidth = glyph.Width + Padding;
            int packedHeight = glyph.Height + Padding;

            if (packedWidth > atlasHeight || packedHeight > atlasHeight)
            {
                return false;
            }

            if (x + packedWidth > atlasWidth)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            if (y + packedHeight > atlasHeight)
            {
                return false;
            }

            glyph.AtlasX = x;
            glyph.AtlasY = y;
            glyphs[glyphIndex] = glyph;

            x += packedWidth;
            rowHeight = Math.Max(rowHeight, packedHeight);
        }

        return true;
    }

    private static void RasterizeGlyphs(List<GlyphBuildInfo> glyphs, byte[] atlasPixels, int atlasWidth)
    {
        foreach (GlyphBuildInfo glyph in glyphs)
        {
            if (glyph.Width == 0 || glyph.Height == 0 || glyph.Pixels.Length == 0)
            {
                continue;
            }

            for (int row = 0; row < glyph.Height; row++)
            {
                int sourceIndex = row * glyph.Width;
                int destinationIndex = ((glyph.AtlasY + row) * atlasWidth) + glyph.AtlasX;
                Buffer.BlockCopy(glyph.Pixels, sourceIndex, atlasPixels, destinationIndex, glyph.Width);
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
        public int Width;
        public int Height;
        public int BitmapTop;
        public int AtlasX;
        public int AtlasY;
        public float LeftSideBearing;
        public float WidthValue;
        public float RightSideBearing;
        public byte[] Pixels;
    }
}
