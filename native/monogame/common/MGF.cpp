// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGF.h"
#include "mg_common.h"

#include <algorithm>
#include <cmath>
#include <cstring>
#include <unordered_map>
#include <utility>
#include <vector>

#define STB_RECT_PACK_IMPLEMENTATION
#include "stb_rect_pack.h"

#define STB_TRUETYPE_IMPLEMENTATION
#include "stb_truetype.h"

struct GlyphBuildInfo
{
    mgchar Character;
    mgint Size;
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

struct RuntimeSizeInfo
{
    mgint Size;
    float Scale;
    mgint AscentPixels;
    mgint LineSpacing;
};

struct GlyphKey
{
    mgchar Character;
    mgint Size;

    bool operator==(const GlyphKey& other) const
    {
        return Character == other.Character && Size == other.Size;
    }
};

struct GlyphKeyHash
{
    size_t operator()(const GlyphKey& key) const
    {
        const auto character = static_cast<size_t>(key.Character);
        const auto size = static_cast<size_t>(key.Size);
        return (character << 16) ^ size;
    }
};

struct MGF_RuntimeFont
{
    std::vector<mgbyte> FontData;
    stbtt_fontinfo Font;
    mgint AtlasWidth;
    mgint AtlasHeight;
    stbrp_context PackContext;
    std::vector<stbrp_node> PackNodes;
    std::vector<GlyphBuildInfo> Glyphs;
    std::vector<MGF_Glyph> GlyphResults;
    std::unordered_map<GlyphKey, size_t, GlyphKeyHash> GlyphLookup;
    std::unordered_map<mgint, RuntimeSizeInfo> SizeLookup;
    std::vector<mgbyte> Atlas;
};

namespace
{
    constexpr mgint Padding = 1;
    constexpr mgint MinimumAtlasSize = 256;
    constexpr mgint MaximumAtlasSize = 4096;

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

    RuntimeSizeInfo& ensure_size_info(MGF_RuntimeFont& runtimeFont, mgint size)
    {
        auto sizeIterator = runtimeFont.SizeLookup.find(size);
        if (sizeIterator != runtimeFont.SizeLookup.end())
            return sizeIterator->second;

        RuntimeSizeInfo sizeInfo = {};
        sizeInfo.Size = size;
        sizeInfo.Scale = stbtt_ScaleForPixelHeight(&runtimeFont.Font, static_cast<float>(size));

        int ascent;
        int descent;
        int lineGap;
        stbtt_GetFontVMetrics(&runtimeFont.Font, &ascent, &descent, &lineGap);

        sizeInfo.AscentPixels = static_cast<mgint>(std::ceil(ascent * sizeInfo.Scale));
        sizeInfo.LineSpacing = static_cast<mgint>(std::ceil((ascent - descent + lineGap) * sizeInfo.Scale));
        if (sizeInfo.LineSpacing <= 0)
            sizeInfo.LineSpacing = size;

        const auto result = runtimeFont.SizeLookup.emplace(size, sizeInfo);
        return result.first->second;
    }

    mgbool build_glyph(const stbtt_fontinfo& font, const RuntimeSizeInfo& sizeInfo, mgchar character, GlyphBuildInfo& glyph)
    {
        if (stbtt_FindGlyphIndex(&font, character) == 0)
            return false;

        int advanceWidth;
        int leftSideBearing;
        int x0;
        int y0;
        int x1;
        int y1;

        stbtt_GetCodepointHMetrics(&font, character, &advanceWidth, &leftSideBearing);
        stbtt_GetCodepointBitmapBox(&font, character, sizeInfo.Scale, sizeInfo.Scale, &x0, &y0, &x1, &y1);

        const mgint width = x1 - x0;
        const mgint height = y1 - y0;
        const mgint bitmapTop = -y0;
        const mgfloat advancePixels = advanceWidth * sizeInfo.Scale;

        glyph = {};
        glyph.Character = character;
        glyph.Size = sizeInfo.Size;
        glyph.Width = width;
        glyph.Height = height;
        glyph.BitmapTop = bitmapTop;

        if (width > 0 && height > 0)
        {
            glyph.Pixels.resize(static_cast<size_t>(width) * static_cast<size_t>(height));
            stbtt_MakeCodepointBitmap(&font,
                                      glyph.Pixels.data(),
                                      width,
                                      height,
                                      width,
                                      sizeInfo.Scale,
                                      sizeInfo.Scale,
                                      character);
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

        return true;
    }

    std::vector<GlyphBuildInfo> build_glyphs(const stbtt_fontinfo& font,
                                             const RuntimeSizeInfo& sizeInfo,
                                             const std::vector<mgchar>& characters,
                                             const std::unordered_map<GlyphKey, size_t, GlyphKeyHash>& glyphLookup)
    {
        std::vector<GlyphBuildInfo> glyphs;
        glyphs.reserve(characters.size());

        for (mgchar character : characters)
        {
            if (glyphLookup.find({ character, sizeInfo.Size }) != glyphLookup.end())
                continue;

            GlyphBuildInfo glyph = {};
            if (build_glyph(font, sizeInfo, character, glyph))
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

    void initialize_pakcer(MGF_RuntimeFont& runtimeFont)
    {
        runtimeFont.PackNodes.resize(static_cast<size_t>(runtimeFont.AtlasWidth));
        stbrp_init_target(&runtimeFont.PackContext,
                          runtimeFont.AtlasWidth,
                          runtimeFont.AtlasHeight,
                          runtimeFont.PackNodes.data(),
                          runtimeFont.AtlasWidth);
    }

    mgbool pack_glyphs(stbrp_context* packContext, std::vector<GlyphBuildInfo>& glyphs)
    {
        std::vector<stbrp_rect> rects;
        std::vector<size_t> glyphIndices;
        rects.reserve(glyphs.size());
        glyphIndices.reserve(glyphs.size());

        for (size_t i = 0; i < glyphs.size(); ++i)
        {
            auto& glyph = glyphs[i];
            if (glyph.Width <= 0 || glyph.Height <= 0)
            {
                glyph.AtlasX = 0;
                glyph.AtlasY = 0;
                continue;
            }

            stbrp_rect rect = {};
            rect.id = static_cast<int>(i);
            rect.w = static_cast<stbrp_coord>(glyph.Width + Padding);
            rect.h = static_cast<stbrp_coord>(glyph.Height + Padding);
            rects.push_back(rect);
            glyphIndices.push_back(i);
        }

        if (rects.empty())
            return true;

        stbrp_pack_rects(packContext, rects.data(), static_cast<int>(rects.size()));

        for (size_t rectIndex = 0; rectIndex < rects.size(); ++rectIndex)
        {
            const auto& rect = rects[rectIndex];
            if (rect.was_packed == 0)
                return false;

            auto& glyph = glyphs[glyphIndices[rectIndex]];
            glyph.AtlasX = rect.x;
            glyph.AtlasY = rect.y;
        }

        return true;
    }

    void rasterize_glyph(std::vector<mgbyte>& atlas,
                         mgint atlasWidth,
                         const GlyphBuildInfo& glyph)
    {
        if (glyph.Width == 0 || glyph.Height == 0 || glyph.Pixels.empty())
            return;

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

    void rebuild_lookup(MGF_RuntimeFont& runtimeFont)
    {
        runtimeFont.GlyphLookup.clear();

        for (size_t i = 0; i < runtimeFont.Glyphs.size(); ++i)
            runtimeFont.GlyphLookup[{ runtimeFont.Glyphs[i].Character, runtimeFont.Glyphs[i].Size }] = i;
    }

    void update_glyph_results(MGF_RuntimeFont& runtimeFont)
    {
        std::sort(runtimeFont.Glyphs.begin(), runtimeFont.Glyphs.end(), [](const GlyphBuildInfo& left, const GlyphBuildInfo& right)
        {
            if (left.Size != right.Size)
                return left.Size < right.Size;

            return left.Character < right.Character;
        });

        rebuild_lookup(runtimeFont);

        runtimeFont.GlyphResults.resize(runtimeFont.Glyphs.size());
        for (size_t i = 0; i < runtimeFont.Glyphs.size(); ++i)
        {
            const auto& glyph = runtimeFont.Glyphs[i];
            auto& result = runtimeFont.GlyphResults[i];
            const auto& sizeInfo = runtimeFont.SizeLookup[glyph.Size];

            result.Character = glyph.Character;
            result.Size = glyph.Size;
            result.BoundsX = glyph.AtlasX;
            result.BoundsY = glyph.AtlasY;
            result.BoundsWidth = glyph.Width;
            result.BoundsHeight = glyph.Height;
            result.CroppingX = 0;
            result.CroppingY = sizeInfo.AscentPixels - glyph.BitmapTop;
            result.CroppingWidth = glyph.Width;
            result.CroppingHeight = sizeInfo.LineSpacing;
            result.LeftSideBearing = glyph.LeftSideBearing;
            result.Width = glyph.WidthValue;
            result.RightSideBearing = glyph.RightSideBearing;
        }
    }

    mgbool rebuild_full_atlas(MGF_RuntimeFont& runtimeFont)
    {
        initialize_pakcer(runtimeFont);
        if (!pack_glyphs(&runtimeFont.PackContext, runtimeFont.Glyphs))
            return false;

        runtimeFont.Atlas.assign(static_cast<size_t>(runtimeFont.AtlasWidth) * static_cast<size_t>(runtimeFont.AtlasHeight) * 4, 0);

        for (const auto& glyph : runtimeFont.Glyphs)
            rasterize_glyph(runtimeFont.Atlas, runtimeFont.AtlasWidth, glyph);

        update_glyph_results(runtimeFont);
        return true;
    }

    mgbool rebuild_runtime_font(MGF_RuntimeFont& runtimeFont,
                                const std::vector<GlyphBuildInfo> glyphs,
                                mgint startingAtlasSize)
    {
        mgint atlasSize = startingAtlasSize > 0 ? startingAtlasSize : estimate_initial_atlas_size(glyphs);
        while (atlasSize <= MaximumAtlasSize)
        {
            runtimeFont.AtlasWidth = atlasSize;
            runtimeFont.AtlasHeight = atlasSize;
            runtimeFont.Glyphs = glyphs;

            if (rebuild_full_atlas(runtimeFont))
                return true;

            atlasSize *= 2;
        }

        return false;
    }

    mgbool ensure_glyphs(MGF_RuntimeFont& runtimeFont,
                         mgint size,
                         MGF_CharacterRegion* characterRegions,
                         mgint characterRegionCount,
                         mgbyte*& atlasRgba,
                         mgint& atlasWidth,
                         mgint& atlasHeight,
                         mgbool& atlasRebuilt,
                         MGF_Glyph*& glyphs,
                         mgint& glyphCount,
                         mgint& lineSpacing)
    {
        atlasRgba = nullptr;
        atlasWidth = runtimeFont.AtlasWidth;
        atlasHeight = runtimeFont.AtlasHeight;
        atlasRebuilt = false;
        glyphs = nullptr;
        glyphCount = static_cast<mgint>(runtimeFont.GlyphResults.size());
        lineSpacing = 0;

        if (size <= 0 || characterRegions == nullptr || characterRegionCount <= 0)
            return false;

        const auto& sizeInfo = ensure_size_info(runtimeFont, size);
        lineSpacing = sizeInfo.LineSpacing;

        const auto characters = collect_characters(characterRegions, characterRegionCount);
        if (characters.empty())
            return false;

        auto newGlyphs = build_glyphs(runtimeFont.Font, sizeInfo, characters, runtimeFont.GlyphLookup);

        if (newGlyphs.empty())
        {
            atlasRgba = runtimeFont.Atlas.empty() ? nullptr : runtimeFont.Atlas.data();
            glyphs = runtimeFont.GlyphResults.empty() ? nullptr : runtimeFont.GlyphResults.data();
            return glyphs != nullptr;
        }

        if (runtimeFont.AtlasWidth == 0 || runtimeFont.AtlasHeight == 0)
        {
            atlasRebuilt = true;

            if (!rebuild_runtime_font(runtimeFont, newGlyphs, estimate_initial_atlas_size(newGlyphs)))
                return false;
        }
        else
        {
            auto glyphsToAppend = newGlyphs;

            // Keep the packer alive across calls so we can keep filling the same atlas
            // instead of starting over very time a new glyph shows up
            if (!pack_glyphs(&runtimeFont.PackContext, glyphsToAppend))
            {
                auto allGlyphs = runtimeFont.Glyphs;
                allGlyphs.insert(allGlyphs.end(), newGlyphs.begin(), newGlyphs.end());

                // If the live packer paints itself into a corner, fall back to a full
                // repack/grow pass instead of giving up on the atlas entirely
                atlasRebuilt = true;
                if (!rebuild_runtime_font(runtimeFont,
                                          allGlyphs,
                                          runtimeFont.AtlasWidth > 0 ? runtimeFont.AtlasWidth : estimate_initial_atlas_size(allGlyphs)))
                    return false;

            }
            else
            {
                for (const auto& glyph : glyphsToAppend)
                    rasterize_glyph(runtimeFont.Atlas, runtimeFont.AtlasWidth, glyph);

                runtimeFont.Glyphs.insert(runtimeFont.Glyphs.end(), glyphsToAppend.begin(), glyphsToAppend.end());
                update_glyph_results(runtimeFont);
            }
        }

        atlasRgba = runtimeFont.Atlas.empty() ? nullptr : runtimeFont.Atlas.data();
        atlasWidth = runtimeFont.AtlasWidth;
        atlasHeight = runtimeFont.AtlasHeight;
        glyphs = runtimeFont.GlyphResults.empty() ? nullptr : runtimeFont.GlyphResults.data();
        glyphCount = static_cast<mgint>(runtimeFont.GlyphResults.size());
        lineSpacing = sizeInfo.LineSpacing;
        return atlasRgba != nullptr && glyphs != nullptr && glyphCount > 0;
    }
}

MGF_RuntimeFont* MGF_RuntimeFont_Create(mgbyte* data, mgint dataBytes)
{
    if (data == nullptr || dataBytes <= 0)
        return nullptr;

    auto runtimeFont = new MGF_RuntimeFont();
    runtimeFont->FontData.assign(data, data + dataBytes);

    const auto fontOffset = stbtt_GetFontOffsetForIndex(runtimeFont->FontData.data(), 0);
    if (fontOffset < 0)
    {
        delete runtimeFont;
        return nullptr;
    }

    runtimeFont->Font = {};
    if (!stbtt_InitFont(&runtimeFont->Font, runtimeFont->FontData.data(), fontOffset))
    {
        delete runtimeFont;
        return nullptr;
    }

    runtimeFont->AtlasWidth = 0;
    runtimeFont->AtlasHeight = 0;
    return runtimeFont;
}

void MGF_RuntimeFont_Destroy(MGF_RuntimeFont* runtimeFont)
{
    delete runtimeFont;
}

mgbool MGF_RuntimeFont_EnsureGlyphs(MGF_RuntimeFont* runtimeFont,
                                    mgint size,
                                    MGF_CharacterRegion* characterRegions,
                                    mgint characterRegionCount,
                                    mgbyte*& atlasRgba,
                                    mgint& atlasWidth,
                                    mgint& atlasHeight,
                                    mgbool& atlasRebuilt,
                                    MGF_Glyph*& glyphs,
                                    mgint& glyphCount,
                                    mgint& lineSpacing)
{
    if (runtimeFont == nullptr)
        return false;

    return ensure_glyphs(*runtimeFont,
                         size,
                         characterRegions,
                         characterRegionCount,
                         atlasRgba,
                         atlasWidth,
                         atlasHeight,
                         atlasRebuilt,
                         glyphs,
                         glyphCount,
                         lineSpacing);
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

    auto runtimeFont = MGF_RuntimeFont_Create(data, dataBytes);
    if (runtimeFont == nullptr)
        return false;

    mgbool atlasRebuilt = false;
    mgbyte* runtimeAtlas = nullptr;
    MGF_Glyph* runtimeGlyphs = nullptr;
    mgint runtimeAtlasWidth = 0;
    mgint runtimeAtlasHeight = 0;
    mgint runtimeGlyphCount = 0;
    mgint runtimeLineSpacing = 0;

    const auto result = MGF_RuntimeFont_EnsureGlyphs(runtimeFont,
                                                     size,
                                                     characterRegions,
                                                     characterRegionCount,
                                                     runtimeAtlas,
                                                     runtimeAtlasWidth,
                                                     runtimeAtlasHeight,
                                                     atlasRebuilt,
                                                     runtimeGlyphs,
                                                     runtimeGlyphCount,
                                                     runtimeLineSpacing);

    if (!result || runtimeAtlas == nullptr || runtimeGlyphs == nullptr || runtimeGlyphCount <= 0)
    {
        MGF_RuntimeFont_Destroy(runtimeFont);
        return false;
    }

    const auto atlasBytes = static_cast<size_t>(runtimeAtlasWidth) * static_cast<size_t>(runtimeAtlasHeight) * 4;
    auto atlasBuffer = static_cast<mgbyte*>(malloc(atlasBytes));
    auto glyphBuffer = static_cast<MGF_Glyph*>(malloc(sizeof(MGF_Glyph) * runtimeGlyphCount));
    if (atlasBuffer == nullptr || glyphBuffer == nullptr)
    {
        free(atlasBuffer);
        free(glyphBuffer);
        MGF_RuntimeFont_Destroy(runtimeFont);
        return false;
    }

    memcpy(atlasBuffer, runtimeAtlas, atlasBytes);
    memcpy(glyphBuffer, runtimeGlyphs, sizeof(MGF_Glyph) * runtimeGlyphCount);

    atlasRgba = atlasBuffer;
    atlasWidth = runtimeAtlasWidth;
    atlasHeight = runtimeAtlasHeight;
    glyphs = glyphBuffer;
    glyphCount = runtimeGlyphCount;
    lineSpacing = runtimeLineSpacing;

    MGF_RuntimeFont_Destroy(runtimeFont);
    return true;
}

void MGF_Free(void* resource)
{
    if (resource != nullptr)
        free(resource);
}
