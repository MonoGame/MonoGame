// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGF.h"
#include "mg_common.h"

#include <algorithm>
#include <exception>
#include <cmath>
#include <cstring>
#include <iterator>
#include <new>
#include <string>
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
    mgint PageIndex;
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

struct RuntimeGlyphLocation
{
    size_t PageIndex;
    size_t GlyphIndex;
};

struct RuntimeAtlasPage
{
    mgint Index;
    mgint AtlasWidth;
    mgint AtlasHeight;
    stbrp_context PackContext;
    std::vector<stbrp_node> PackNodes;
    std::vector<GlyphBuildInfo> Glyphs;
    std::vector<mgbyte> Atlas;
};

struct MGF_RuntimeFont
{
    std::vector<mgbyte> FontData;
    stbtt_fontinfo Font;
    std::vector<RuntimeAtlasPage> Pages;
    std::vector<MGF_PageUpdate> PageUpdates;
    std::vector<MGF_Glyph> GlyphResults;
    std::unordered_map<GlyphKey, RuntimeGlyphLocation, GlyphKeyHash> GlyphLookup;
    std::unordered_map<mgint, RuntimeSizeInfo> SizeLookup;
    mgint LastErrorCode;
    std::string LastErrorMessage;
};

namespace
{
    constexpr mgint Padding = 1;
    constexpr mgint MinimumAtlasSize = 256;
    constexpr mgint MaximumAtlasSize = 4096;

    void clear_error(MGF_RuntimeFont& runtimeFont)
    {
        runtimeFont.LastErrorCode = MGF_RuntimeFontErrorCode_None;
        runtimeFont.LastErrorMessage.clear();
    }

    mgbool fail(MGF_RuntimeFont& runtimeFont, mgint errorCode, const char* message)
    {
        runtimeFont.LastErrorCode = errorCode;
        runtimeFont.LastErrorMessage = message;
        return false;
    }

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

        // The managed caller can send overlapping ranges.  Sorting and deduplicating keeps the
        // bake idempotent and avoids repacking the same codepoint more than once per size.
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
                                             const std::unordered_map<GlyphKey, RuntimeGlyphLocation, GlyphKeyHash>& glyphLookup)
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

    mgint estimate_initial_atlas_size(const std::vector<GlyphBuildInfo>& requiredGlyphs,
                                      const std::vector<GlyphBuildInfo>& optionalGlyphs)
    {
        mgint totalArea = 0;

        for (const auto& glyph : requiredGlyphs)
        {
            if (glyph.Width == 0 || glyph.Height == 0)
                continue;

            totalArea += (glyph.Width + Padding) * (glyph.Height + Padding);
        }

        for (const auto& glyph : optionalGlyphs)
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

    struct PackResult
    {
        std::vector<GlyphBuildInfo> PackedGlyphs;
        std::vector<GlyphBuildInfo> UnpackedGlyphs;
    };

    struct RuntimePageUpdateInfo
    {
        size_t PageIndex;
        mgbool AtlasRebuilt;
    };

    RuntimeAtlasPage* try_get_current_page(MGF_RuntimeFont& runtimeFont)
    {
        if (runtimeFont.Pages.empty())
            return nullptr;

        return &runtimeFont.Pages.back();
    }

    const RuntimeAtlasPage* try_get_current_page(const MGF_RuntimeFont& runtimeFont)
    {
        if (runtimeFont.Pages.empty())
            return nullptr;

        return &runtimeFont.Pages.back();
    }

    void initialize_packer(RuntimeAtlasPage& page)
    {
        page.PackNodes.resize(static_cast<size_t>(page.AtlasWidth));
        stbrp_init_target(&page.PackContext,
                          page.AtlasWidth,
                          page.AtlasHeight,
                          page.PackNodes.data(),
                          page.AtlasWidth);
    }

    void set_page_index(std::vector<GlyphBuildInfo>& glyphs, mgint pageIndex)
    {
        for (auto& glyph : glyphs)
            glyph.PageIndex = pageIndex;
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

    PackResult pack_glyph_group(stbrp_context* packContext, const std::vector<GlyphBuildInfo>& glyphs)
    {
        PackResult result = {};
        std::vector<GlyphBuildInfo> mutableGlyphs = glyphs;
        std::vector<stbrp_rect> rects;
        std::vector<size_t> glyphIndices;
        std::vector<mgbool> packedFlags(mutableGlyphs.size(), true);

        rects.reserve(mutableGlyphs.size());
        glyphIndices.reserve(mutableGlyphs.size());

        for (size_t i = 0; i < mutableGlyphs.size(); ++i)
        {
            auto& glyph = mutableGlyphs[i];
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

        if (!rects.empty())
        {
            stbrp_pack_rects(packContext, rects.data(), static_cast<int>(rects.size()));

            for (size_t rectIndex = 0; rectIndex < rects.size(); ++rectIndex)
            {
                const auto& rect = rects[rectIndex];
                auto& glyph = mutableGlyphs[glyphIndices[rectIndex]];
                if (rect.was_packed == 0)
                {
                    packedFlags[glyphIndices[rectIndex]] = false;
                    continue;
                }

                glyph.AtlasX = rect.x;
                glyph.AtlasY = rect.y;
            }
        }

        result.PackedGlyphs.reserve(mutableGlyphs.size());
        result.UnpackedGlyphs.reserve(mutableGlyphs.size());

        for (size_t i = 0; i < mutableGlyphs.size(); ++i)
        {
            if (packedFlags[i])
                result.PackedGlyphs.push_back(std::move(mutableGlyphs[i]));
            else
                result.UnpackedGlyphs.push_back(std::move(mutableGlyphs[i]));
        }

        return result;
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

                // The atlas is exported as RGBA even though stb_truetype gives us a single channel
                // coverage bitmap.  Replicating coverage into every channel keeps the native output
                // upload ready for MonoGame's existing texture path without an extra swizzle step.
                atlas[atlasIndex + 0] = alpha;
                atlas[atlasIndex + 1] = alpha;
                atlas[atlasIndex + 2] = alpha;
                atlas[atlasIndex + 3] = alpha;
            }
        }
    }

    void rasterize_page(RuntimeAtlasPage& page)
    {
        page.Atlas.assign(static_cast<size_t>(page.AtlasWidth) * static_cast<size_t>(page.AtlasHeight) * 4, 0);

        for (const auto& glyph : page.Glyphs)
            rasterize_glyph(page.Atlas, page.AtlasWidth, glyph);
    }

    mgbool rebuild_page(RuntimeAtlasPage& page,
                        const std::vector<GlyphBuildInfo>& requiredGlyphs,
                        const std::vector<GlyphBuildInfo>& optionalGlyphs,
                        mgint startingAtlasSize,
                        std::vector<GlyphBuildInfo>& remainingOptionalGlyphs)
    {
        mgint atlasSize = startingAtlasSize > 0 ?
            startingAtlasSize :
            estimate_initial_atlas_size(requiredGlyphs, optionalGlyphs);

        if (atlasSize < MinimumAtlasSize)
            atlasSize = MinimumAtlasSize;

        while (atlasSize <= MaximumAtlasSize)
        {
            stbrp_context requiredContext = {};
            std::vector<stbrp_node> requiredNodes(static_cast<size_t>(atlasSize));
            stbrp_init_target(&requiredContext,
                              atlasSize,
                              atlasSize,
                              requiredNodes.data(),
                              atlasSize);

            // Existing glyphs are treated as required so a rebuild never drops glyphs that were
            // already visible to managed code.  Only overflow from the newly requested set spills to
            // another page.
            PackResult requiredResult = pack_glyph_group(&requiredContext, requiredGlyphs);
            if (!requiredResult.UnpackedGlyphs.empty())
            {
                atlasSize *= 2;
                continue;
            }

            PackResult optionalResult = pack_glyph_group(&requiredContext, optionalGlyphs);
            if (!optionalResult.UnpackedGlyphs.empty() && atlasSize < MaximumAtlasSize)
            {
                atlasSize *= 2;
                continue;
            }

            page.AtlasWidth = atlasSize;
            page.AtlasHeight = atlasSize;
            page.Glyphs = std::move(requiredResult.PackedGlyphs);
            page.Glyphs.insert(page.Glyphs.end(),
                               std::make_move_iterator(optionalResult.PackedGlyphs.begin()),
                               std::make_move_iterator(optionalResult.PackedGlyphs.end()));

            set_page_index(page.Glyphs, page.Index);

            initialize_packer(page);
            if (!pack_glyphs(&page.PackContext, page.Glyphs))
            {
                if (atlasSize == MaximumAtlasSize)
                    return false;

                atlasSize *= 2;
                continue;
            }

            rasterize_page(page);
            remainingOptionalGlyphs = std::move(optionalResult.UnpackedGlyphs);
            return true;
        }

        return false;
    }

    void rebuild_lookup(MGF_RuntimeFont& runtimeFont)
    {
        runtimeFont.GlyphLookup.clear();

        for (size_t pageIndex = 0; pageIndex < runtimeFont.Pages.size(); ++pageIndex)
        {
            const auto& page = runtimeFont.Pages[pageIndex];
            for (size_t glyphIndex = 0; glyphIndex < page.Glyphs.size(); ++glyphIndex)
            {
                const auto& glyph = page.Glyphs[glyphIndex];
                runtimeFont.GlyphLookup[{ glyph.Character, glyph.Size }] = { pageIndex, glyphIndex };
            }
        }
    }

    void update_glyph_results(MGF_RuntimeFont& runtimeFont)
    {
        std::vector<const GlyphBuildInfo*> glyphs;
        size_t glyphCount = 0;
        for (const auto& page : runtimeFont.Pages)
            glyphCount += page.Glyphs.size();

        glyphs.reserve(glyphCount);
        for (const auto& page : runtimeFont.Pages)
        {
            for (const auto& glyph : page.Glyphs)
                glyphs.push_back(&glyph);
        }

        std::sort(glyphs.begin(), glyphs.end(), [](const GlyphBuildInfo* left, const GlyphBuildInfo* right)
        {
            if (left->Size != right->Size)
                return left->Size < right->Size;

            if (left->Character != right->Character)
                return left->Character < right->Character;

            return left->PageIndex < right->PageIndex;
        });

        rebuild_lookup(runtimeFont);

        runtimeFont.GlyphResults.resize(glyphs.size());
        for (size_t i = 0; i < glyphs.size(); ++i)
        {
            const auto& glyph = *glyphs[i];
            auto& result = runtimeFont.GlyphResults[i];
            const auto& sizeInfo = runtimeFont.SizeLookup[glyph.Size];

            result.Character = glyph.Character;
            result.Size = glyph.Size;
            result.PageIndex = glyph.PageIndex;
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

    void update_page_results(MGF_RuntimeFont& runtimeFont, const std::vector<RuntimePageUpdateInfo>& pageUpdates)
    {
        std::vector<RuntimePageUpdateInfo> uniqueUpdates;
        uniqueUpdates.reserve(pageUpdates.size());

        // A single ensure call can append to a page and later rebuild that same page.  The public
        // result only needs the final page snapshot plus whether any rebuild happened along the way.
        for (const RuntimePageUpdateInfo& pageUpdate : pageUpdates)
        {
            bool merged = false;
            for (RuntimePageUpdateInfo& existing : uniqueUpdates)
            {
                if (existing.PageIndex != pageUpdate.PageIndex)
                    continue;

                existing.AtlasRebuilt = existing.AtlasRebuilt || pageUpdate.AtlasRebuilt;
                merged = true;
                break;
            }

            if (!merged)
                uniqueUpdates.push_back(pageUpdate);
        }

        runtimeFont.PageUpdates.resize(uniqueUpdates.size());
        for (size_t i = 0; i < uniqueUpdates.size(); ++i)
        {
            const RuntimePageUpdateInfo& pageUpdate = uniqueUpdates[i];
            const RuntimeAtlasPage& page = runtimeFont.Pages[pageUpdate.PageIndex];
            MGF_PageUpdate& result = runtimeFont.PageUpdates[i];
            result.PageIndex = page.Index;
            result.AtlasRgba = page.Atlas.empty() ? nullptr : const_cast<mgbyte*>(page.Atlas.data());
            result.AtlasWidth = page.AtlasWidth;
            result.AtlasHeight = page.AtlasHeight;
            result.AtlasRebuilt = pageUpdate.AtlasRebuilt;
        }
    }

    mgbool ensure_glyphs(MGF_RuntimeFont& runtimeFont,
                         mgint size,
                         MGF_CharacterRegion* characterRegions,
                         mgint characterRegionCount,
                         MGF_PageUpdate*& pageUpdates,
                         mgint& pageUpdateCount,
                         MGF_Glyph*& glyphs,
                         mgint& glyphCount,
                         mgint& lineSpacing)
    {
        try
        {
            clear_error(runtimeFont);
            pageUpdates = nullptr;
            pageUpdateCount = 0;
            glyphs = nullptr;
            glyphCount = static_cast<mgint>(runtimeFont.GlyphResults.size());
            lineSpacing = 0;

            if (size <= 0 || characterRegions == nullptr || characterRegionCount <= 0)
            {
                return fail(runtimeFont,
                            MGF_RuntimeFontErrorCode_InvalidArgument,
                            "DynamicSpriteFont glyph update received invalid native arguments.");
            }

            const auto& sizeInfo = ensure_size_info(runtimeFont, size);
            lineSpacing = sizeInfo.LineSpacing;

            const auto characters = collect_characters(characterRegions, characterRegionCount);
            if (characters.empty())
            {
                return fail(runtimeFont,
                            MGF_RuntimeFontErrorCode_NoGlyphData,
                            "DynamicSpriteFont glyph update did not receive any characters to bake.");
            }

            auto newGlyphs = build_glyphs(runtimeFont.Font, sizeInfo, characters, runtimeFont.GlyphLookup);

            if (newGlyphs.empty())
            {
                glyphs = runtimeFont.GlyphResults.empty() ? nullptr : runtimeFont.GlyphResults.data();
                glyphCount = static_cast<mgint>(runtimeFont.GlyphResults.size());
                return true;
            }

            RuntimeAtlasPage* updatedPage = nullptr;
            std::vector<RuntimePageUpdateInfo> touchedPages;
            std::vector<GlyphBuildInfo> remainingGlyphs = std::move(newGlyphs);
            while (!remainingGlyphs.empty())
            {
                RuntimeAtlasPage* writablePage = try_get_current_page(runtimeFont);
                if (writablePage == nullptr)
                {
                    RuntimeAtlasPage page = {};
                    page.Index = static_cast<mgint>(runtimeFont.Pages.size());
                    runtimeFont.Pages.push_back(std::move(page));
                    writablePage = &runtimeFont.Pages.back();
                }

                auto glyphsToAppend = remainingGlyphs;
                set_page_index(glyphsToAppend, writablePage->Index);

                // Appending into the existing packer is the cheapest path because it preserves the
                // current atlas image and existing glyph coordinates.  We only rebuild when the new
                // rectangles no longer fit in the current page layout.
                if (writablePage->AtlasWidth > 0 &&
                    writablePage->AtlasHeight > 0 &&
                    pack_glyphs(&writablePage->PackContext, glyphsToAppend))
                {
                    for (const auto& glyph : glyphsToAppend)
                        rasterize_glyph(writablePage->Atlas, writablePage->AtlasWidth, glyph);

                    writablePage->Glyphs.insert(writablePage->Glyphs.end(),
                                                std::make_move_iterator(glyphsToAppend.begin()),
                                                std::make_move_iterator(glyphsToAppend.end()));

                    updatedPage = writablePage;
                    touchedPages.push_back({ static_cast<size_t>(writablePage->Index), false });
                    remainingGlyphs.clear();
                }
                else
                {
                    if (writablePage->AtlasWidth >= MaximumAtlasSize &&
                        writablePage->AtlasHeight >= MaximumAtlasSize)
                    {
                        RuntimeAtlasPage page = {};
                        page.Index = static_cast<mgint>(runtimeFont.Pages.size());
                        runtimeFont.Pages.push_back(std::move(page));
                        continue;
                    }

                    std::vector<GlyphBuildInfo> remainingAfterPage;
                    mgint startingAtlasSize = writablePage->AtlasWidth > 0 ?
                        writablePage->AtlasWidth :
                        estimate_initial_atlas_size(writablePage->Glyphs, remainingGlyphs);

                    if (!rebuild_page(*writablePage,
                                      writablePage->Glyphs,
                                      remainingGlyphs,
                                      startingAtlasSize,
                                      remainingAfterPage))
                    {
                        if (!writablePage->Glyphs.empty())
                        {
                            RuntimeAtlasPage page = {};
                            page.Index = static_cast<mgint>(runtimeFont.Pages.size());
                            runtimeFont.Pages.push_back(std::move(page));
                            continue;
                        }

                        return fail(runtimeFont,
                                    MGF_RuntimeFontErrorCode_AtlasCapacityExceeded,
                                    "DynamicSpriteFont glyphs could not fit within the maximum atlas page size.");
                    }

                    updatedPage = writablePage;
                    touchedPages.push_back({ static_cast<size_t>(writablePage->Index), true });
                    bool pageAcceptedNewGlyphs = remainingAfterPage.size() < remainingGlyphs.size();
                    remainingGlyphs = std::move(remainingAfterPage);

                    if (!remainingGlyphs.empty() && !pageAcceptedNewGlyphs)
                    {
                        RuntimeAtlasPage page = {};
                        page.Index = static_cast<mgint>(runtimeFont.Pages.size());
                        runtimeFont.Pages.push_back(std::move(page));
                    }
                }
            }

            update_glyph_results(runtimeFont);
            update_page_results(runtimeFont, touchedPages);

            if (updatedPage == nullptr)
            {
                return fail(runtimeFont,
                            MGF_RuntimeFontErrorCode_Unknown,
                            "DynamicSpriteFont glyph update completed without touching any atlas page.");
            }

            pageUpdates = runtimeFont.PageUpdates.empty() ? nullptr : runtimeFont.PageUpdates.data();
            pageUpdateCount = static_cast<mgint>(runtimeFont.PageUpdates.size());
            glyphs = runtimeFont.GlyphResults.empty() ? nullptr : runtimeFont.GlyphResults.data();
            glyphCount = static_cast<mgint>(runtimeFont.GlyphResults.size());
            lineSpacing = sizeInfo.LineSpacing;

            if (pageUpdates == nullptr || pageUpdateCount <= 0 || glyphs == nullptr || glyphCount <= 0)
            {
                return fail(runtimeFont,
                            MGF_RuntimeFontErrorCode_NoGlyphData,
                            "DynamicSpriteFont glyph update did not return any page or glyph results.");
            }

            return true;
        }
        catch (const std::bad_alloc&)
        {
            return fail(runtimeFont,
                        MGF_RuntimeFontErrorCode_OutOfMemory,
                        "DynamicSpriteFont glyph update ran out of native memory while growing atlas pages.");
        }
        catch (const std::exception& exception)
        {
            return fail(runtimeFont,
                        MGF_RuntimeFontErrorCode_Unknown,
                        exception.what());
        }
        catch (...)
        {
            return fail(runtimeFont,
                        MGF_RuntimeFontErrorCode_Unknown,
                        "DynamicSpriteFont glyph update failed for an unknown native reason.");
        }
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
                                    MGF_PageUpdate*& pageUpdates,
                                    mgint& pageUpdateCount,
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
                         pageUpdates,
                         pageUpdateCount,
                         glyphs,
                         glyphCount,
                         lineSpacing);
}

mgint MGF_RuntimeFont_GetLastErrorCode(MGF_RuntimeFont* runtimeFont)
{
    if (runtimeFont == nullptr)
        return MGF_RuntimeFontErrorCode_InvalidArgument;

    return runtimeFont->LastErrorCode;
}

const char* MGF_RuntimeFont_GetLastErrorMessage(MGF_RuntimeFont* runtimeFont)
{
    if (runtimeFont == nullptr)
        return "DynamicSpriteFont runtime font handle was null.";

    return runtimeFont->LastErrorMessage.c_str();
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

    MGF_PageUpdate* runtimePageUpdates = nullptr;
    mgint runtimePageUpdateCount = 0;
    MGF_Glyph* runtimeGlyphs = nullptr;
    mgint runtimeGlyphCount = 0;
    mgint runtimeLineSpacing = 0;

    const auto result = MGF_RuntimeFont_EnsureGlyphs(runtimeFont,
                                                     size,
                                                     characterRegions,
                                                     characterRegionCount,
                                                     runtimePageUpdates,
                                                     runtimePageUpdateCount,
                                                     runtimeGlyphs,
                                                     runtimeGlyphCount,
                                                     runtimeLineSpacing);

    // SpriteFont still expects a single atlas texture, so the one shot helper stays strict even
    // though the runtime font path can span multiple pages.
    if (!result || runtimePageUpdates == nullptr || runtimePageUpdateCount != 1 || runtimeGlyphs == nullptr || runtimeGlyphCount <= 0)
    {
        MGF_RuntimeFont_Destroy(runtimeFont);
        return false;
    }

    if (runtimeFont->Pages.size() != 1)
    {
        MGF_RuntimeFont_Destroy(runtimeFont);
        return false;
    }

    const MGF_PageUpdate& runtimePageUpdate = runtimePageUpdates[0];
    if (runtimePageUpdate.AtlasRgba == nullptr)
    {
        MGF_RuntimeFont_Destroy(runtimeFont);
        return false;
    }

    const auto atlasBytes = static_cast<size_t>(runtimePageUpdate.AtlasWidth) * static_cast<size_t>(runtimePageUpdate.AtlasHeight) * 4;
    auto atlasBuffer = static_cast<mgbyte*>(malloc(atlasBytes));
    auto glyphBuffer = static_cast<MGF_Glyph*>(malloc(sizeof(MGF_Glyph) * runtimeGlyphCount));
    if (atlasBuffer == nullptr || glyphBuffer == nullptr)
    {
        free(atlasBuffer);
        free(glyphBuffer);
        MGF_RuntimeFont_Destroy(runtimeFont);
        return false;
    }

    memcpy(atlasBuffer, runtimePageUpdate.AtlasRgba, atlasBytes);
    memcpy(glyphBuffer, runtimeGlyphs, sizeof(MGF_Glyph) * runtimeGlyphCount);

    atlasRgba = atlasBuffer;
    atlasWidth = runtimePageUpdate.AtlasWidth;
    atlasHeight = runtimePageUpdate.AtlasHeight;
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
