// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGF.h"
#include "mg_common.h"
#include <cmath>

#define STB_TRUETYPE_IMPLEMENTATION
#include "stb_truetype.h"

namespace
{
    constexpr mgint Padding = 1;
    constexpr mgint MinimumAtlasSize = 256;
    constexpr mgint MaximumAtlasSize = 4096;

    struct GlyphBuildInfo
    {
        mgchar Character;
        mgint Width;
        mgint Height;
        mgint BitmapTop;
        mgint AtlasX;
        mgint AtlasY;
        mgfloat LeftSideBearing;
        mgfloat WidthValue;
        mgfloat RightSideBearing;
        std::vector<mgbyte> Pixels;
    };

    mgint next_power_of_two(mgint value)
    {
        mgint result = 1;
        while (result < value)
            result <<= 1;
        return result;
    }

    std::vector<mgchar> collect_characters(MGF_CharacterRegion* characterRegions, mgint characterRegionCount)
    {
        std::vector<mgchar> characters;

        for (mgint i = 0; i < characterRegionCount; ++i)
        {
            const auto start = static_cast<mgint>(characterRegions[i].Start);
            const auto end = static_cast<mgint>(characterRegions[i].End);

            for (mgint value = start; value <= end; ++value)
                characters.push_back(static_cast<mgchar>(value));
        }

        std::sort(characters.begin(), characters.end());
        characters.erase(std::unique(characters.begin(), characters.end()), characters.end());
        return characters;
    }

    std::vector<GlyphBuildInfo> build_glyphs(const stbtt_fontinfo& font,
                                             float scale,
                                             const std::vector<mgchar>& characters)
    {
        std::vector<GlyphBuildInfo> glyphs;
        glyphs.reserve(characters.size());

        for (mgchar character : characters)
        {
            if (stbtt_FindGlyphIndex(&font, character) == 0)
                continue;

            int advanceWidth;
            int leftSideBearing;
            int x0;
            int y0;
            int x1;
            int y1;

            stbtt_GetCodepointHMetrics(&font, character, &advanceWidth, &leftSideBearing);
            stbtt_GetCodepointBitmapBox(&font, character, scale, scale, &x0, &y0, &x1, &y1);

            const mgint width = x1 - x0;
            const mgint height = y1 - y0;
            const mgint bitmapTop = -y0;
            const mgfloat advancePixels = advanceWidth * scale;

            GlyphBuildInfo glyph = {};
            glyph.Character = character;
            glyph.Width = width;
            glyph.Height = height;
            glyph.BitmapTop = bitmapTop;

            if (width > 0 && height > 0)
            {
                glyph.Pixels.resize(static_cast<size_t>(width) * static_cast<size_t>(height));
                stbtt_MakeCodepointBitmap(&font, glyph.Pixels.data(), width, height, width, scale, scale, character);
            }

            if (width > 0)
            {
                glyph.LeftSideBearing = static_cast<mgfloat>(x0);
                glyph.WidthValue = static_cast<mgfloat>(width);
                glyph.RightSideBearing = advancePixels - (static_cast<mgfloat>(x0) + static_cast<mgfloat>(width));
            }
            else
            {
                glyph.LeftSideBearing = 0.0f;
                glyph.WidthValue = 0.0f;
                glyph.RightSideBearing = advancePixels;
            }

            glyphs.push_back(std::move(glyph));
        }

        return glyphs;
    }

    mgint estimate_initial_atlas_size(const std::vector<GlyphBuildInfo>& glyphs)
    {
        mgint totalArea = 0;

        for (const auto& glyph : glyphs)
        {
            if (glyph.Width == 0 || glyph.Height == 0)
                continue;

            totalArea += (glyph.Width + Padding) * (glyph.Height + Padding);
        }

        if (totalArea == 0)
            return 1;

        auto estimated = static_cast<mgint>(std::ceil(std::sqrt(static_cast<double>(totalArea) * 1.5)));
        estimated = std::max(MinimumAtlasSize, estimated);
        estimated = std::min(MaximumAtlasSize, estimated);
        return next_power_of_two(estimated);
    }

    mgbool try_pack_glyphs(std::vector<GlyphBuildInfo>& glyphs,
                           mgint atlasWidth,
                           mgint atlasHeight)
    {
        std::vector<size_t> packOrder;
        packOrder.reserve(glyphs.size());

        for (size_t i = 0; i < glyphs.size(); ++i)
        {
            if (glyphs[i].Width > 0 && glyphs[i].Height > 0)
                packOrder.push_back(i);
        }

        std::sort(packOrder.begin(), packOrder.end(), [&glyphs](size_t left, size_t right)
        {
                      const auto& leftGlyph = glyphs[left];
                      const auto& rightGlyph = glyphs[right];

                      if (leftGlyph.Height != rightGlyph.Height)
                          return rightGlyph.Height < leftGlyph.Height;

                      return rightGlyph.Width < leftGlyph.Width; 
        });

        mgint x = 0;
        mgint y = 0;
        mgint rowHeight = 0;

        for (size_t glyphIndex : packOrder)
        {
            auto& glyph = glyphs[glyphIndex];
            const mgint packedWidth = glyph.Width + Padding;
            const mgint packedHeight = glyph.Height + Padding;

            if (packedWidth > atlasWidth || packedHeight > atlasHeight)
                return false;

            if (x + packedWidth > atlasWidth)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            if (y + packedHeight > atlasHeight)
                return false;

            glyph.AtlasX = x;
            glyph.AtlasY = y;

            x += packedWidth;
            rowHeight = std::max(rowHeight, packedHeight);
        }

        return true;
    }

    std::vector<mgbyte> rasterize_glyphs_to_rgba(const std::vector<GlyphBuildInfo>& glyphs,
                                                 mgint atlasWidth,
                                                 mgint atlasHeight)
    {
        std::vector<mgbyte> atlas(static_cast<size_t>(atlasWidth) * static_cast<size_t>(atlasHeight) * 4, 0);

        for (const auto& glyph : glyphs)
        {
            if (glyph.Width == 0 || glyph.Height == 0 || glyph.Pixels.empty())
                continue;

            for (mgint row = 0; row < glyph.Height; ++row)
            {
                for (mgint column = 0; column < glyph.Width; ++column)
                {
                    const auto alpha = glyph.Pixels[static_cast<size_t>(row) * glyph.Width + column];
                    const auto atlasIndex = static_cast<size_t>(((glyph.AtlasY + row) * atlasWidth) + glyph.AtlasX + column) * 4;

                    atlas[atlasIndex + 0] = alpha;
                    atlas[atlasIndex + 1] = alpha;
                    atlas[atlasIndex + 2] = alpha;
                    atlas[atlasIndex + 3] = alpha;
                }
            }
        }

        return atlas;
    }
}

mgbool MGF_BakeSpriteFont(mgbyte* data,
                          mgint dataBytes,
                          mgint size,
                          MGF_CharacterRegion* characterRegions,
                          mgint characterRegionCount,
                          mgbyte*& atlasRgba,
                          mgint& atlasWidth,
                          mgint& atlasHeight,
                          MGF_Glyph*& glyphs,
                          mgint& glyphCount,
                          mgint& lineSpacing)
{
    atlasRgba = nullptr;
    atlasWidth = 0;
    atlasHeight = 0;
    glyphs = nullptr;
    glyphCount = 0;
    lineSpacing = 0;

    if (data == nullptr || dataBytes <= 0 || size <= 0 || characterRegions == nullptr || characterRegionCount <= 0)
        return false;

    const auto fontOffset = stbtt_GetFontOffsetForIndex(data, 0);
    if (fontOffset < 0)
        return false;

    stbtt_fontinfo font = {};
    if (!stbtt_InitFont(&font, data, fontOffset))
        return false;

    const auto characters = collect_characters(characterRegions, characterRegionCount);
    if (characters.empty())
        return false;

    const auto scale = stbtt_ScaleForPixelHeight(&font, static_cast<float>(size));

    int ascent;
    int descent;
    int lineGap;
    stbtt_GetFontVMetrics(&font, &ascent, &descent, &lineGap);

    const auto ascentPixels = static_cast<mgint>(std::ceil(ascent * scale));
    lineSpacing = static_cast<mgint>(std::ceil((ascent - descent + lineGap) * scale));
    if (lineSpacing <= 0)
        lineSpacing = size;

    auto glyphBuilds = build_glyphs(font, scale, characters);
    if (glyphBuilds.empty())
        return false;

    auto atlasSize = estimate_initial_atlas_size(glyphBuilds);
    while (!try_pack_glyphs(glyphBuilds, atlasSize, atlasSize))
    {
        if (atlasSize >= MaximumAtlasSize)
            return false;

        atlasSize *= 2;
    }

    auto atlas = rasterize_glyphs_to_rgba(glyphBuilds, atlasSize, atlasSize);

    auto glyphBuffer = static_cast<MGF_Glyph*>(malloc(sizeof(MGF_Glyph) * glyphBuilds.size()));
    auto atlasBuffer = static_cast<mgbyte*>(malloc(atlas.size()));
    if (glyphBuffer == nullptr || atlasBuffer == nullptr)
    {
        free(glyphBuffer);
        free(atlasBuffer);
        return false;
    }

    memcpy(atlasBuffer, atlas.data(), atlas.size());

    for (size_t i = 0; i < glyphBuilds.size(); ++i)
    {
        const auto& glyph = glyphBuilds[i];
        auto& result = glyphBuffer[i];

        result.Character = glyph.Character;
        result.BoundsX = glyph.AtlasX;
        result.BoundsY = glyph.AtlasY;
        result.BoundsWidth = glyph.Width;
        result.BoundsHeight = glyph.Height;
        result.CroppingX = 0;
        result.CroppingY = ascentPixels - glyph.BitmapTop;
        result.CroppingWidth = glyph.Width;
        result.CroppingHeight = lineSpacing;
        result.LeftSideBearing = glyph.LeftSideBearing;
        result.Width = glyph.WidthValue;
        result.RightSideBearing = glyph.RightSideBearing;
    }

    atlasRgba = atlasBuffer;
    atlasWidth = atlasSize;
    atlasHeight = atlasSize;
    glyphs = glyphBuffer;
    glyphCount = static_cast<mgint>(glyphBuilds.size());
    return true;
}

void MGF_Free(void* resource)
{
    if (resource != nullptr)
        free(resource);
}
