// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGF.h"
#include "mg_common.h"

#include <algorithm>
#include <cmath>
#include <cstdio>
#include <cstring>
#include <exception>
#include <iterator>
#include <new>
#include <string>
#include <unordered_map>
#include <utility>
#include <vector>

#define STB_RECT_PACK_IMPLEMENTATION
#include "stb_rect_pack.h"

#include <ft2build.h>
#include FT_FREETYPE_H

#if defined(_WIN32) && defined(_DEBUG)
// Prevent Windows headers from defining min/max macros 
// that break std::min/std::max in debug builds.
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <Windows.h>
#endif

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

struct FontSizeInfo
{
    mgint Size;
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
        const size_t character = static_cast<size_t>(key.Character);
        const size_t size = static_cast<size_t>(key.Size);
        return (character << 16) ^ size;
    }
};

struct GlyphLocation
{
    size_t PageIndex;
    size_t GlyphIndex;
};

struct FontAtlasPage
{
    mgint Index;
    mgint AtlasWidth;
    mgint AtlasHeight;
    stbrp_context PackContext;
    std::vector<stbrp_node> PackNodes;
    std::vector<GlyphBuildInfo> Glyphs;
    std::vector<mgbyte> Atlas;
};

struct MGF_Font
{
    std::vector<mgbyte> FontData;
    FT_Library FreeTypeLibrary;
    FT_Face FreeTypeFace;
    std::vector<FontAtlasPage> Pages;
    std::vector<MGF_PageUpdate> PageUpdates;
    std::vector<MGF_Glyph> GlyphResults;
    std::unordered_map<GlyphKey, GlyphLocation, GlyphKeyHash> GlyphLookup;
    std::unordered_map<mgint, FontSizeInfo> SizeLookup;
};

namespace
{
    constexpr mgint Padding = 1;
    constexpr mgint FixedAtlasSize = 1024;

    const char* mgf_result_code_name(MGF_ResultCode resultCode)
    {
        switch (resultCode)
        {
            case MGF_ResultCode_Success:                        return "Success";
            case MGF_ResultCode_InvalidArgument:                return "InvalidArgument";
            case MGF_ResultCode_InvalidFontData:                return "InvalidFontData";
            case MGF_ResultCode_OutOfMemory:                    return "OutOfMemory";
            case MGF_ResultCode_BackendInitializationFailed:    return "BackendInitializationFailed";
            case MGF_ResultCode_FontSizeSetupFailed:            return "FontSizeSetupFailed";
            case MGF_ResultCode_GlyphLoadFailed:                return "GlyphLoadFailed";
            case MGF_ResultCode_GlyphRenderFailed:              return "GlyphRenderFailed";
            case MGF_ResultCode_UnsupportedGlyphBitmapFormat:   return "UnsupportedGlyphBitmapFormat";
            case MGF_ResultCode_AtlasCapacityExceeded:          return "AtlasCapacityExceeded";
            case MGF_ResultCode_NoGlyphData:                    return "NoGlyphData";
            case MGF_ResultCode_InternalError:                  return "InternalError";
            default:                                            return "Unknown";
        }
    }

#if defined(_DEBUG)
    void mgf_debug_write(const char* message)
    {
        if (message == nullptr || message[0] == '\0')
            return;

#if defined(_WIN32)
        OutputDebugStringA(message);
#else
        fputs(message, stderr);
        fflush(stderr);
#endif
    }

    void mgf_debug_log_result(const char* context, MGF_ResultCode resultCode, const char* message)
    {
        char buffer[512] = {};
        snprintf(buffer,
                 sizeof(buffer),
                 "[MGF] %s: result=%s message=\"%s\"\n",
                 context != nullptr ? context : "<unknown>",
                 mgf_result_code_name(resultCode),
                 message != nullptr ? message : "");

        mgf_debug_write(buffer);
    }

    void mgf_debug_log_message(const char* context, const char* message)
    {
        char buffer[512] = {};
        snprintf(buffer,
                 sizeof(buffer),
                 "[MGF] %s: %s\n",
                 context != nullptr ? context : "<unknown>",
                 message != nullptr ? message : "");

        mgf_debug_write(buffer);
    }
#else
    void mgf_debug_log_result(const char*, MGF_ResultCode, const char*) {}
    void mgf_debug_log_message(const char*, const char*) {}
#endif

    mgint ceil_pixels_from_26dot6(FT_Pos value)
    {
        return static_cast<mgint>((value + 63) >> 6);
    }

    mgfloat pixels_from_26dot6(FT_Pos value)
    {
        return static_cast<mgfloat>(value) / 64.0f;
    }

    MGF_ResultCode copy_glyph_bitmap(const FT_Bitmap& bitmap, std::vector<mgbyte>& pixels)
    {
        const mgint width = static_cast<mgint>(bitmap.width);
        const mgint height = static_cast<mgint>(bitmap.rows);
        if (width <= 0 || height <= 0)
        {
            pixels.clear();
            return MGF_ResultCode_Success;
        }

        if (bitmap.buffer == nullptr)
            return MGF_ResultCode_InternalError;

        pixels.resize(static_cast<size_t>(width) * static_cast<size_t>(height));

        const mgint sourcePitch = bitmap.pitch >= 0 ? bitmap.pitch : -bitmap.pitch;
        for (mgint row = 0; row < height; ++row)
        {
            const mgint sourceRowIndex = bitmap.pitch >= 0 ? row : (height - 1 - row);
            const mgbyte* sourceRow = bitmap.buffer + static_cast<size_t>(sourceRowIndex) * sourcePitch;
            mgbyte* destinationRow = pixels.data() + static_cast<size_t>(row) * width;

            if (bitmap.pixel_mode == FT_PIXEL_MODE_GRAY)
            {
                memcpy(destinationRow, sourceRow, static_cast<size_t>(width));
                continue;
            }

            if (bitmap.pixel_mode == FT_PIXEL_MODE_MONO)
            {
                for (mgint column = 0; column < width; ++column)
                {
                    const mgbyte mask = static_cast<mgbyte>(0x80 >> (column & 7));
                    destinationRow[column] = (sourceRow[column >> 3] & mask) != 0 ? 255 : 0;
                }

                continue;
            }

            return MGF_ResultCode_UnsupportedGlyphBitmapFormat;
        }

        return MGF_ResultCode_Success;
    }

    std::vector<mgchar> collect_characters(const MGF_CharacterRegion* characterRegions, mgint characterRegionCount)
    {
        std::vector<mgchar> characters;

        for (mgint i = 0; i < characterRegionCount; ++i)
        {
            const mgint start = static_cast<mgint>(characterRegions[i].Start);
            const mgint end = static_cast<mgint>(characterRegions[i].End);

            for (mgint value = start; value <= end; ++value)
                characters.push_back(static_cast<mgchar>(value));
        }

        // The managed caller can send overlapping ranges.  Sorting and deduplicating keeps the
        // bake idempotent and avoids repacking the same codepoint more than once per size.
        std::sort(characters.begin(), characters.end());
        characters.erase(std::unique(characters.begin(), characters.end()), characters.end());
        return characters;
    }

    void clear_font_ensure_glyphs_result(MGF_FontEnsureGlyphsResult& result)
    {
        result.PageUpdates = nullptr;
        result.PageUpdateCount = 0;
        result.Glyphs = nullptr;
        result.GlyphCount = 0;
        result.LineSpacing = 0;
    }

    void clear_bake_sprite_font_result(MGF_BakeSpriteFontResult& result)
    {
        result.AtlasRgba = nullptr;
        result.AtlasWidth = 0;
        result.AtlasHeight = 0;
        result.Glyphs = nullptr;
        result.GlyphCount = 0;
        result.LineSpacing = 0;
    }

    MGF_ResultCode ensure_size_info(MGF_Font& font, mgint size, FontSizeInfo*& sizeInfo)
    {
        sizeInfo = nullptr;

        if (font.FreeTypeFace == nullptr)
            return MGF_ResultCode_InternalError;

        if (FT_Set_Pixel_Sizes(font.FreeTypeFace, 0, static_cast<FT_UInt>(size)) != FT_Err_Ok)
            return MGF_ResultCode_FontSizeSetupFailed;

        std::unordered_map<mgint, FontSizeInfo>::iterator sizeIterator = font.SizeLookup.find(size);
        if (sizeIterator != font.SizeLookup.end())
        {
            sizeInfo = &sizeIterator->second;
            return MGF_ResultCode_Success;
        }

        FontSizeInfo newSizeInfo = {};
        newSizeInfo.Size = size;
        if (font.FreeTypeFace->size == nullptr)
            return MGF_ResultCode_InternalError;

        const FT_Size_Metrics& sizeMetrics = font.FreeTypeFace->size->metrics;
        newSizeInfo.AscentPixels = ceil_pixels_from_26dot6(sizeMetrics.ascender);
        newSizeInfo.LineSpacing = ceil_pixels_from_26dot6(sizeMetrics.height);
        if (newSizeInfo.LineSpacing <= 0)
            newSizeInfo.LineSpacing = size;

        const std::pair<std::unordered_map<mgint, FontSizeInfo>::iterator, bool> result = font.SizeLookup.emplace(size, newSizeInfo);
        sizeInfo = &result.first->second;
        return MGF_ResultCode_Success;
    }

    MGF_ResultCode build_glyph(FT_Face freeTypeFace,
                               const FontSizeInfo& sizeInfo,
                               mgchar character,
                               GlyphBuildInfo& glyph,
                               mgbool& glyphBuilt)
    {
        glyphBuilt = false;

        if (freeTypeFace == nullptr)
            return MGF_ResultCode_InternalError;

        const FT_UInt glyphIndex = FT_Get_Char_Index(freeTypeFace, static_cast<FT_ULong>(character));
        if (glyphIndex == 0)
            return MGF_ResultCode_Success;

        if (FT_Load_Glyph(freeTypeFace, glyphIndex, FT_LOAD_DEFAULT) != FT_Err_Ok)
            return MGF_ResultCode_GlyphLoadFailed;

        FT_GlyphSlot glyphSlot = freeTypeFace->glyph;
        if (glyphSlot == nullptr)
            return MGF_ResultCode_InternalError;

        if (glyphSlot->format != FT_GLYPH_FORMAT_BITMAP &&
            FT_Render_Glyph(glyphSlot, FT_RENDER_MODE_NORMAL) != FT_Err_Ok)
        {
            return MGF_ResultCode_GlyphRenderFailed;
        }

        const mgint width = static_cast<mgint>(glyphSlot->bitmap.width);
        const mgint height = static_cast<mgint>(glyphSlot->bitmap.rows);
        const mgint bitmapTop = glyphSlot->bitmap_top;
        const mgfloat leftSideBearing = static_cast<mgfloat>(glyphSlot->bitmap_left);
        const mgfloat widthValue = static_cast<mgfloat>(width);
        const mgfloat advancePixels = pixels_from_26dot6(glyphSlot->advance.x);

        glyph = {};
        glyph.Character = character;
        glyph.Size = sizeInfo.Size;
        glyph.Width = width;
        glyph.Height = height;
        glyph.BitmapTop = bitmapTop;
        glyph.LeftSideBearing = leftSideBearing;
        glyph.WidthValue = widthValue;
        glyph.RightSideBearing = advancePixels - (leftSideBearing + widthValue);

        const MGF_ResultCode copyResult = copy_glyph_bitmap(glyphSlot->bitmap, glyph.Pixels);
        if (copyResult != MGF_ResultCode_Success)
            return copyResult;

        glyphBuilt = true;
        return MGF_ResultCode_Success;
    }

    MGF_ResultCode build_glyphs(FT_Face freeTypeFace,
                                const FontSizeInfo& sizeInfo,
                                const std::vector<mgchar>& characters,
                                const std::unordered_map<GlyphKey, GlyphLocation, GlyphKeyHash>& glyphLookup,
                                std::vector<GlyphBuildInfo>& glyphs)
    {
        glyphs.clear();
        glyphs.reserve(characters.size());

        for (mgchar character : characters)
        {
            if (glyphLookup.find({ character, sizeInfo.Size }) != glyphLookup.end())
                continue;

            GlyphBuildInfo glyph = {};
            mgbool glyphBuilt = false;
            const MGF_ResultCode buildResult = build_glyph(freeTypeFace, sizeInfo, character, glyph, glyphBuilt);

            if (buildResult != MGF_ResultCode_Success)
                return buildResult;

            if (glyphBuilt)
                glyphs.push_back(std::move(glyph));
        }

        return MGF_ResultCode_Success;
    }

    struct PackResult
    {
        std::vector<GlyphBuildInfo> PackedGlyphs;
        std::vector<GlyphBuildInfo> UnpackedGlyphs;
    };

    FontAtlasPage* try_get_current_page(MGF_Font& font)
    {
        if (font.Pages.empty())
            return nullptr;

        return &font.Pages.back();
    }

    const FontAtlasPage* try_get_current_page(const MGF_Font& font)
    {
        if (font.Pages.empty())
            return nullptr;

        return &font.Pages.back();
    }

    void initialize_packer(FontAtlasPage& page)
    {
        page.PackNodes.resize(static_cast<size_t>(page.AtlasWidth));
        stbrp_init_target(&page.PackContext,
                          page.AtlasWidth,
                          page.AtlasHeight,
                          page.PackNodes.data(),
                          page.AtlasWidth);
    }

    void initialize_page(FontAtlasPage& page, mgint pageIndex)
    {
        page.Index = pageIndex;
        page.AtlasWidth = FixedAtlasSize;
        page.AtlasHeight = FixedAtlasSize;
        page.Atlas.assign(static_cast<size_t>(page.AtlasWidth) * static_cast<size_t>(page.AtlasHeight) * 4, 0);
        initialize_packer(page);
    }

    FontAtlasPage& add_page(MGF_Font& font)
    {
        font.Pages.emplace_back();
        FontAtlasPage& page = font.Pages.back();
        initialize_page(page, static_cast<mgint>(font.Pages.size() - 1));
        return page;
    }

    void set_page_index(std::vector<GlyphBuildInfo>& glyphs, mgint pageIndex)
    {
        for (GlyphBuildInfo& glyph : glyphs)
            glyph.PageIndex = pageIndex;
    }

    PackResult pack_glyph_group(stbrp_context* packContext, const std::vector<GlyphBuildInfo>& glyphs)
    {
        PackResult result = {};
        std::vector<GlyphBuildInfo> mutableGlyphs = glyphs;
        std::vector<stbrp_rect> rects;
        std::vector<mgbool> packedFlags(mutableGlyphs.size(), true);

        rects.reserve(mutableGlyphs.size());

        for (size_t i = 0; i < mutableGlyphs.size(); ++i)
        {
            GlyphBuildInfo& glyph = mutableGlyphs[i];
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
        }

        if (!rects.empty())
        {
            stbrp_pack_rects(packContext, rects.data(), static_cast<int>(rects.size()));

            for (size_t rectIndex = 0; rectIndex < rects.size(); ++rectIndex)
            {
                const stbrp_rect& rect = rects[rectIndex];
                const size_t glyphIndex = static_cast<size_t>(rect.id);
                GlyphBuildInfo& glyph = mutableGlyphs[glyphIndex];
                if (rect.was_packed == 0)
                {
                    packedFlags[glyphIndex] = false;
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
                const mgbyte alpha = glyph.Pixels[static_cast<size_t>(row) * glyph.Width + column];
                const size_t atlasIndex = static_cast<size_t>(((glyph.AtlasY + row) * atlasWidth) + glyph.AtlasX + column) * 4;

                // The atlas is exported as RGBA even though the glyph rasterizer gives us a single
                // channel coverage bitmap. Replicating coverage into every channel keeps the native
                // output upload ready for MonoGame's existing texture path without an extra swizzle
                // step.
                atlas[atlasIndex + 0] = alpha;
                atlas[atlasIndex + 1] = alpha;
                atlas[atlasIndex + 2] = alpha;
                atlas[atlasIndex + 3] = alpha;
            }
        }
    }

    void rasterize_page(FontAtlasPage& page)
    {
        page.Atlas.assign(static_cast<size_t>(page.AtlasWidth) * static_cast<size_t>(page.AtlasHeight) * 4, 0);

        for (const GlyphBuildInfo& glyph : page.Glyphs)
            rasterize_glyph(page.Atlas, page.AtlasWidth, glyph);
    }

    void rebuild_lookup(MGF_Font& font)
    {
        font.GlyphLookup.clear();

        for (size_t pageIndex = 0; pageIndex < font.Pages.size(); ++pageIndex)
        {
            const FontAtlasPage& page = font.Pages[pageIndex];
            for (size_t glyphIndex = 0; glyphIndex < page.Glyphs.size(); ++glyphIndex)
            {
                const GlyphBuildInfo& glyph = page.Glyphs[glyphIndex];
                font.GlyphLookup[{ glyph.Character, glyph.Size }] = { pageIndex, glyphIndex };
            }
        }
    }

    void update_glyph_results(MGF_Font& font)
    {
        std::vector<const GlyphBuildInfo*> glyphs;
        size_t glyphCount = 0;
        for (const FontAtlasPage& page : font.Pages)
            glyphCount += page.Glyphs.size();

        glyphs.reserve(glyphCount);
        for (const FontAtlasPage& page : font.Pages)
        {
            for (const GlyphBuildInfo& glyph : page.Glyphs)
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

        rebuild_lookup(font);

        font.GlyphResults.resize(glyphs.size());
        for (size_t i = 0; i < glyphs.size(); ++i)
        {
            const GlyphBuildInfo& glyph = *glyphs[i];
            MGF_Glyph& result = font.GlyphResults[i];
            const FontSizeInfo& sizeInfo = font.SizeLookup[glyph.Size];

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

    void update_page_results(MGF_Font& font, const std::vector<size_t>& pageUpdates)
    {
        std::vector<size_t> uniqueUpdates;
        uniqueUpdates.reserve(pageUpdates.size());

        for (size_t pageIndex : pageUpdates)
        {
            if (std::find(uniqueUpdates.begin(), uniqueUpdates.end(), pageIndex) == uniqueUpdates.end())
                uniqueUpdates.push_back(pageIndex);
        }

        font.PageUpdates.resize(uniqueUpdates.size());
        for (size_t i = 0; i < uniqueUpdates.size(); ++i)
        {
            const FontAtlasPage& page = font.Pages[uniqueUpdates[i]];
            MGF_PageUpdate& result = font.PageUpdates[i];
            result.PageIndex = page.Index;
            result.AtlasRgba = page.Atlas.empty() ? nullptr : const_cast<mgbyte*>(page.Atlas.data());
            result.AtlasWidth = page.AtlasWidth;
            result.AtlasHeight = page.AtlasHeight;
        }
    }

    MGF_ResultCode ensure_glyphs(const MGF_FontEnsureGlyphsRequest& request,
                                 MGF_FontEnsureGlyphsResult& result)
    {
        try
        {
            clear_font_ensure_glyphs_result(result);

            if (request.Font == nullptr ||
                request.Size <= 0 ||
                request.CharacterRegions == nullptr ||
                request.CharacterRegionCount <= 0)
            {
                return MGF_ResultCode_InvalidArgument;
            }

            MGF_Font& font = *request.Font;

            FontSizeInfo* sizeInfo = nullptr;
            const MGF_ResultCode sizeInfoResult = ensure_size_info(font, request.Size, sizeInfo);
            if (sizeInfoResult != MGF_ResultCode_Success)
                return sizeInfoResult;

            result.LineSpacing = sizeInfo->LineSpacing;

            const std::vector<mgchar> characters = collect_characters(request.CharacterRegions, request.CharacterRegionCount);
            if (characters.empty())
                return MGF_ResultCode_NoGlyphData;

            std::vector<GlyphBuildInfo> newGlyphs;
            const MGF_ResultCode buildResult = build_glyphs(font.FreeTypeFace,
                                                            *sizeInfo,
                                                            characters,
                                                            font.GlyphLookup,
                                                            newGlyphs);

            if (buildResult != MGF_ResultCode_Success)
                return buildResult;

            if (newGlyphs.empty())
            {
                result.Glyphs = font.GlyphResults.empty() ? nullptr : font.GlyphResults.data();
                result.GlyphCount = static_cast<mgint>(font.GlyphResults.size());
                return MGF_ResultCode_Success;
            }

            FontAtlasPage* updatedPage = nullptr;
            std::vector<size_t> touchedPages;
            std::vector<GlyphBuildInfo> remainingGlyphs = std::move(newGlyphs);
            while (!remainingGlyphs.empty())
            {
                FontAtlasPage* writablePage = try_get_current_page(font);
                if (writablePage == nullptr)
                {
                    writablePage = &add_page(font);
                }

                set_page_index(remainingGlyphs, writablePage->Index);
                PackResult packResult = pack_glyph_group(&writablePage->PackContext, remainingGlyphs);
                if (!packResult.PackedGlyphs.empty())
                {
                    for (const GlyphBuildInfo& glyph : packResult.PackedGlyphs)
                        rasterize_glyph(writablePage->Atlas, writablePage->AtlasWidth, glyph);

                    writablePage->Glyphs.insert(writablePage->Glyphs.end(),
                                                std::make_move_iterator(packResult.PackedGlyphs.begin()),
                                                std::make_move_iterator(packResult.PackedGlyphs.end()));

                    updatedPage = writablePage;
                    touchedPages.push_back(static_cast<size_t>(writablePage->Index));
                    remainingGlyphs = std::move(packResult.UnpackedGlyphs);
                }

                if (remainingGlyphs.empty())
                {
                    continue;
                }

                if (writablePage->Glyphs.empty())
                {
                    return MGF_ResultCode_AtlasCapacityExceeded;
                }

                // When the current fixed-size atlas page fills up, spill remaining glyphs into a new page
                add_page(font);
            }

            update_glyph_results(font);
            update_page_results(font, touchedPages);

            if (updatedPage == nullptr)
            {
                mgf_debug_log_result("ensure_glyphs",
                                     MGF_ResultCode_InternalError,
                                     "glyph generation completed without tracking an updated atlas page.");
                return MGF_ResultCode_InternalError;
            }

            result.PageUpdates = font.PageUpdates.empty() ? nullptr : font.PageUpdates.data();
            result.PageUpdateCount = static_cast<mgint>(font.PageUpdates.size());
            result.Glyphs = font.GlyphResults.empty() ? nullptr : font.GlyphResults.data();
            result.GlyphCount = static_cast<mgint>(font.GlyphResults.size());
            result.LineSpacing = sizeInfo->LineSpacing;

            if (result.PageUpdates == nullptr ||
                result.PageUpdateCount <= 0 ||
                result.Glyphs == nullptr ||
                result.GlyphCount <= 0)
            {
                mgf_debug_log_result("ensure_glyphs",
                                     MGF_ResultCode_NoGlyphData,
                                     "glyph generation completed but the final output snapshot was incomplete.");
                return MGF_ResultCode_NoGlyphData;
            }

            return MGF_ResultCode_Success;
        }
        catch (const std::bad_alloc&)
        {
            mgf_debug_log_result("ensure_glyphs",
                                 MGF_ResultCode_OutOfMemory,
                                 "glyph generation ran out of memory.");
            return MGF_ResultCode_OutOfMemory;
        }
        catch (...)
        {
            mgf_debug_log_result("ensure_glyphs",
                                 MGF_ResultCode_InternalError,
                                 "glyph generation caught a unknown exception.");
            return MGF_ResultCode_InternalError;
        }
    }
}

MGF_ResultCode MGF_Font_Create(mgbyte* data,
                               mgint dataBytes,
                               MGF_Font*& font)
{
    font = nullptr;

    if (data == nullptr || dataBytes <= 0)
    {
        mgf_debug_log_result("MGF_Font_Create",
                             MGF_ResultCode_InvalidArgument,
                             "create was called with null font data or a non-positive data length");
        return MGF_ResultCode_InvalidArgument;
    }

    MGF_Font* createdFont = nullptr;

    try
    {
        createdFont = new MGF_Font();
        createdFont->FreeTypeLibrary = nullptr;
        createdFont->FreeTypeFace = nullptr;
        createdFont->FontData.assign(data, data + dataBytes);

        const FT_Error initResult = FT_Init_FreeType(&createdFont->FreeTypeLibrary);
        if (initResult != FT_Err_Ok)
        {
            delete createdFont;
            createdFont = nullptr;
            mgf_debug_log_result("MGF_Font_Create",
                                 MGF_ResultCode_BackendInitializationFailed,
                                 "the font system could not be initialized");
            return MGF_ResultCode_BackendInitializationFailed;
        }

        const FT_Error faceResult = FT_New_Memory_Face(createdFont->FreeTypeLibrary,
                                                       createdFont->FontData.data(),
                                                       static_cast<FT_Long>(createdFont->FontData.size()),
                                                       0,
                                                       &createdFont->FreeTypeFace);

        if (faceResult != FT_Err_Ok)
        {
            FT_Done_FreeType(createdFont->FreeTypeLibrary);
            delete createdFont;
            createdFont = nullptr;
            mgf_debug_log_result("MGF_Font_Create",
                                 MGF_ResultCode_InvalidFontData,
                                 "the supplied font data could not be opened");
            return MGF_ResultCode_InvalidFontData;
        }

        font = createdFont;
        return MGF_ResultCode_Success;
    }
    catch (const std::bad_alloc&)
    {
        if (createdFont != nullptr)
        {
            if (createdFont->FreeTypeFace != nullptr)
                FT_Done_Face(createdFont->FreeTypeFace);

            if (createdFont->FreeTypeLibrary != nullptr)
                FT_Done_FreeType(createdFont->FreeTypeLibrary);

            delete createdFont;
        }

        mgf_debug_log_result("MGF_Font_Create",
                             MGF_ResultCode_OutOfMemory,
                             "create ran out of memory while initializing the font face.");
        return MGF_ResultCode_OutOfMemory;
    }
    catch (const std::exception&)
    {
        if (createdFont != nullptr)
        {
            if (createdFont->FreeTypeFace != nullptr)
                FT_Done_Face(createdFont->FreeTypeFace);

            if (createdFont->FreeTypeLibrary != nullptr)
                FT_Done_FreeType(createdFont->FreeTypeLibrary);

            delete createdFont;
        }

        mgf_debug_log_result("MGF_Font_Create",
                             MGF_ResultCode_InternalError,
                             "create caught a standard exception.");
        return MGF_ResultCode_InternalError;
    }
    catch (...)
    {
        if (createdFont != nullptr)
        {
            if (createdFont->FreeTypeFace != nullptr)
                FT_Done_Face(createdFont->FreeTypeFace);

            if (createdFont->FreeTypeLibrary != nullptr)
                FT_Done_FreeType(createdFont->FreeTypeLibrary);

            delete createdFont;
        }

        mgf_debug_log_result("MGF_Font_Create",
                             MGF_ResultCode_InternalError,
                             "create caught an unknown exception.");
        return MGF_ResultCode_InternalError;
    }
}

void MGF_Font_Destroy(MGF_Font* font)
{
    if (font != nullptr)
    {
        if (font->FreeTypeFace != nullptr)
            FT_Done_Face(font->FreeTypeFace);

        if (font->FreeTypeLibrary != nullptr)
            FT_Done_FreeType(font->FreeTypeLibrary);
    }

    delete font;
}

MGF_ResultCode MGF_Font_EnsureGlyphs(const MGF_FontEnsureGlyphsRequest* request,
                                     MGF_FontEnsureGlyphsResult* result)
{
    if (result != nullptr)
        clear_font_ensure_glyphs_result(*result);

    if (request == nullptr || result == nullptr)
    {
        mgf_debug_log_result("MGF_Font_EnsureGlyphs",
                             MGF_ResultCode_InvalidArgument,
                             "ensure glyphs was called with a null request or result.");
        return MGF_ResultCode_InvalidArgument;
    }

    if (request->Font == nullptr)
    {
        mgf_debug_log_result("MGF_Font_EnsureGlyphs",
                             MGF_ResultCode_InvalidArgument,
                             "ensure glyphs was called with a null font handle.");
        return MGF_ResultCode_InvalidArgument;
    }

    const MGF_ResultCode resultCode = ensure_glyphs(*request, *result);
    if (resultCode != MGF_ResultCode_Success)
    {
        mgf_debug_log_result("MGF_Font_EnsureGlyphs",
                             resultCode,
                             "ensure glyphs returned a non-success result.");
    }

    return resultCode;
}

MGF_ResultCode MGF_BakeSpriteFont(const MGF_BakeSpriteFontRequest* request,
                                  MGF_BakeSpriteFontResult* result)
{
    if (result != nullptr)
        clear_bake_sprite_font_result(*result);

    if (request == nullptr || result == nullptr)
    {
        mgf_debug_log_result("MGF_BakeSpriteFont",
                             MGF_ResultCode_InvalidArgument,
                             "bake was called with a null request or result.");
        return MGF_ResultCode_InvalidArgument;
    }

    MGF_Font* font = nullptr;
    const MGF_ResultCode createResult = MGF_Font_Create(const_cast<mgbyte*>(request->Data),
                                                        request->DataBytes,
                                                        font);
    if (createResult != MGF_ResultCode_Success)
    {
        mgf_debug_log_result("MGF_BakeSpriteFont",
                             createResult,
                             "bake failed while creating the font handle.");
        return createResult;
    }

    MGF_FontEnsureGlyphsRequest ensureRequest = {};
    ensureRequest.Font = font;
    ensureRequest.Size = request->Size;
    ensureRequest.CharacterRegions = request->CharacterRegions;
    ensureRequest.CharacterRegionCount = request->CharacterRegionCount;

    MGF_FontEnsureGlyphsResult ensureResult = {};
    const MGF_ResultCode updateResult = MGF_Font_EnsureGlyphs(&ensureRequest, &ensureResult);


    // SpriteFont still expects a single atlas texture, so the one shot helper stays strict even
    // though the incremental font path can span multiple pages.
    if (updateResult != MGF_ResultCode_Success)
    {
        MGF_Font_Destroy(font);
        mgf_debug_log_result("MGF_BakeSpriteFont",
                             updateResult,
                             "baked failed while ensuring glyphs.");
        return updateResult;
    }

    if (ensureResult.PageUpdates == nullptr || ensureResult.PageUpdateCount != 1 || ensureResult.Glyphs == nullptr || ensureResult.GlyphCount <= 0)
    {
        MGF_Font_Destroy(font);
        mgf_debug_log_result("MGF_BakeSpriteFont",
                             MGF_ResultCode_NoGlyphData,
                             "bake did not produce a single valid atlas page and glyph set.");
        return MGF_ResultCode_NoGlyphData;
    }

    if (font->Pages.size() != 1)
    {
        MGF_Font_Destroy(font);
        mgf_debug_log_result("MGF_BakeSpriteFont",
                             MGF_ResultCode_AtlasCapacityExceeded,
                             "bake required more than one atlas page.");
        return MGF_ResultCode_AtlasCapacityExceeded;
    }

    const MGF_PageUpdate& runtimePageUpdate = ensureResult.PageUpdates[0];
    if (runtimePageUpdate.AtlasRgba == nullptr)
    {
        MGF_Font_Destroy(font);
        mgf_debug_log_result("MGF_BakeSpriteFont",
                             MGF_ResultCode_NoGlyphData,
                             "bake completed without atlas pixel data.");
        return MGF_ResultCode_NoGlyphData;
    }

    const size_t atlasBytes = static_cast<size_t>(runtimePageUpdate.AtlasWidth) * static_cast<size_t>(runtimePageUpdate.AtlasHeight) * 4;
    mgbyte* atlasBuffer = static_cast<mgbyte*>(malloc(atlasBytes));
    MGF_Glyph* glyphBuffer = static_cast<MGF_Glyph*>(malloc(sizeof(MGF_Glyph) * ensureResult.GlyphCount));
    if (atlasBuffer == nullptr || glyphBuffer == nullptr)
    {
        free(atlasBuffer);
        free(glyphBuffer);
        MGF_Font_Destroy(font);
        mgf_debug_log_result("MGF_BakeSpriteFont",
                             MGF_ResultCode_OutOfMemory,
                             "bake ran out of memory while copying atlas or glyph results.");
        return MGF_ResultCode_OutOfMemory;
    }

    memcpy(atlasBuffer, runtimePageUpdate.AtlasRgba, atlasBytes);
    memcpy(glyphBuffer, ensureResult.Glyphs, sizeof(MGF_Glyph) * ensureResult.GlyphCount);

    result->AtlasRgba = atlasBuffer;
    result->AtlasWidth = runtimePageUpdate.AtlasWidth;
    result->AtlasHeight = runtimePageUpdate.AtlasHeight;
    result->Glyphs = glyphBuffer;
    result->GlyphCount = ensureResult.GlyphCount;
    result->LineSpacing = ensureResult.LineSpacing;

    MGF_Font_Destroy(font);
    return MGF_ResultCode_Success;
}

void MGF_Free(void* resource)
{
    if (resource != nullptr)
        free(resource);
}
