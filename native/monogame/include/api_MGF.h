// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "api_common.h"

// Opaque font handle used by incremental glyph baking API.
struct MGF_Font;

// Stable result codes returned by the explicit font contract.
enum MGF_ResultCode
{
    MGF_ResultCode_Success = 0,
    MGF_ResultCode_InvalidArgument = 1,
    MGF_ResultCode_InvalidFontData = 2,
    MGF_ResultCode_OutOfMemory = 3,
    MGF_ResultCode_BackendInitializationFailed = 4,
    MGF_ResultCode_FontSizeSetupFailed = 5,
    MGF_ResultCode_GlyphLoadFailed = 6,
    MGF_ResultCode_GlyphRenderFailed = 7,
    MGF_ResultCode_UnsupportedGlyphBitmapFormat = 8,
    MGF_ResultCode_AtlasCapacityExceeded = 9,
    MGF_ResultCode_NoGlyphData = 10,
    MGF_ResultCode_InternalError = 11
};


// Inclusive Unicode character range.
struct MGF_CharacterRegion
{
    mgchar Start;
    mgchar End;
};

// Glyph metrics and atlas placement returned to managed code.
struct MGF_Glyph
{
    mgchar Character;
    mgint Size;
    mgint PageIndex;
    mgint BoundsX;
    mgint BoundsY;
    mgint BoundsWidth;
    mgint BoundsHeight;
    mgint CroppingX;
    mgint CroppingY;
    mgint CroppingWidth;
    mgint CroppingHeight;
    mgfloat LeftSideBearing;
    mgfloat Width;
    mgfloat RightSideBearing;
};

// Atlas data returned for each page touched by the current glyph update.
struct MGF_PageUpdate
{
    mgint PageIndex;
    mgbyte* AtlasRgba;
    mgint AtlasWidth;
    mgint AtlasHeight;
};

struct MGF_BakeSpriteFontRequest
{
    const mgbyte* Data;
    mgint DataBytes;
    mgint Size;
    const MGF_CharacterRegion* CharacterRegions;
    mgint CharacterRegionCount;
};

struct MGF_BakeSpriteFontResult
{
    mgbyte* AtlasRgba;
    mgint AtlasWidth;
    mgint AtlasHeight;
    MGF_Glyph* Glyphs;
    mgint GlyphCount;
    mgint LineSpacing;
};

struct MGF_FontEnsureGlyphsRequest
{
    MGF_Font* Font;
    mgint Size;
    const MGF_CharacterRegion* CharacterRegions;
    mgint CharacterRegionCount;
};

struct MGF_FontEnsureGlyphsResult
{
    MGF_PageUpdate* PageUpdates;
    mgint PageUpdateCount;
    MGF_Glyph* Glyphs;
    mgint GlyphCount;
    mgint LineSpacing;
};

/**
 * Bakes a traditional SpriteFont atlas in one call
 *
 * This is a convenience wrapper over runtime font API for callers that only need a single
 * atlas page.  On success `atlasRgba` and `glyphs` are newly allocated buffers owned by the
 * caller and must be released with MGF_Free.  The function fails if the requested glyph set would
 * require more than one atlas page.
 *
 * @param request Bake inputs. Must not be `nullptr`
 * @param result Receives the baked atlas and glyph outputs on success.  Must not be `nullptr`.
 * @return `MGF_ResultCode_Success` when the font data was valid and the baked glyph set fit
 * within one atlas page.
 */
MG_EXPORT MGF_ResultCode MGF_BakeSpriteFont(const MGF_BakeSpriteFontRequest* request,
                                            MGF_BakeSpriteFontResult* result);

/**
 * Creates a font handle for incremental glyph baking.
 *
 * The function copies the provided font bytes so the caller may release the original buffer after
 * this call returns.  The returned handle must be destroyed with MGF_Font_Destroy.
 *
 * @param data Pointer to the font file bytes.
 * @param dataBytes Number of byte available in `data`.
 * @param font Receives a fully initialized runtime font handle on success.
 * @return `MGF_ResultCode_Success` on success; otherwise returns  non-success result and leaves
 * `font` as `nullptr`.
 */
MG_EXPORT MGF_ResultCode MGF_Font_Create(mgbyte* data,
                                         mgint dataBytes,
                                         MGF_Font*& font);

// Releases a handle created by MGF_Font_Create. Passing `nullptr` is allowed.
MG_EXPORT void MGF_Font_Destroy(MGF_Font* font);

/**
 * Ensures that the requested characters exist in the font atlas for a given size.
 *
 * Duplicate and overlapping character regions are merged before glyph generation.  On success,
 * `pageUpdates` points to the atlas pages touched by this call and `glyphs` points to the full
 * baked glyph set currently owned by `font`.  Those pointers remain valid until the next
 * call that mutates `font` or until MGF_Font_Destroy is called.
 *
 * @param request Glyph request inputs.  Must not be `nullptr`.
 * @param result Receives the touched atlas pages and glyph outputs. Must not be `nullptr`.
 * @return `MGF_Result_Code_Success` when the glyph request completed successfully;
 * otherwise returns a non-success result and leaves all output data clear.
 */
MG_EXPORT MGF_ResultCode MGF_Font_EnsureGlyphs(const MGF_FontEnsureGlyphsRequest* request,
                                                      MGF_FontEnsureGlyphsResult* result);

// Releases buffers returned by MGF_BakeSpriteFont.  Passing `nullptr` is allowed.
MG_EXPORT void MGF_Free(void* resource);
