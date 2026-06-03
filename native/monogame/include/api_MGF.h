// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "api_common.h"

// Opaque runtime font handle used by incremental glyph baking API.
struct MGF_RuntimeFont;

// Error codes reported by MGG_RuntimeFont_GetLastErrorCode after a runtime call fails.
enum MGF_RuntimeFontErrorCode
{
    MGF_RuntimeFontErrorCode_None = 0,
    MGF_RuntimeFontErrorCode_InvalidArgument = 1,
    MGF_RuntimeFontErrorCode_OutOfMemory = 2,
    MGF_RuntimeFontErrorCode_AtlasCapacityExceeded = 3,
    MGF_RuntimeFontErrorCode_NoGlyphData = 4,
    MGF_RuntimeFontErrorCode_Unknown = 5
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

// Atlas data returned for each page touched by the current runtime glyph update.
struct MGF_PageUpdate
{
    mgint PageIndex;
    mgbyte* AtlasRgba;
    mgint AtlasWidth;
    mgint AtlasHeight;
    mgbool AtlasRebuilt;
};

/**
 * Bakes a traditional SpriteFont atlas in one call
 *
 * This is a convenience wrapper over the runtime font API for callers that only need a single
 * atlas page.  On success `atlasRgba` and `glyphs` are newly allocated buffers owned by the
 * caller and must be released with MGF_Free.  The function fails if the requested glyph set would
 * require more than one atlas page.
 *
 * @param data Pointer to the font file bytes.
 * @param dataBytes Number of bytes available at `data`.
 * @param size Requested glyph height in pixels. Must be greater than zero.
 * @param characterRegions Inclusive character ranges to bake.
 * @param characterRegionCount Number of entries in `characterRegions`.
 * @param atlasRgba Receives a tightly packed RGBA atlas buffer on success.
 * @param atlasWidth Receives the atlas width in pixels.
 * @param atlasHeight Receives the atlas height in pixels.
 * @param glyphs Receives the baked glyph metrics on success.
 * @param glyphCount Receives the number of entries written to `glyphs`.
 * @param lineSpacing Receives the font line spacing in pixels for `size`.
 * @return `true` when the font data was valid and the baked glyph set fit within one atlas page.
 */
MG_EXPORT mgbool MGF_BakeSpriteFont(
    mgbyte* data,
    mgint dataBytes,
    mgint size,
    MGF_CharacterRegion* characterRegions,
    mgint characterRegionCount,
    mgbyte*& atlasRgba,
    mgint& atlasWidth,
    mgint& atlasHeight,
    MGF_Glyph*& glyphs,
    mgint& glyphCount,
    mgint& lineSpacing
);

/**
 * Creates a runtime font handle for incremental glyph baking.
 *
 * The function copies the provided font bytes so the caller may release the original buffer after
 * this call returns.  The returned handle must be destroyed with MGF_RuntimeFont_Destroy.
 *
 * @param data Pointer to the font file bytes.
 * @param dataBytes Number of byte available in `data`.
 * @return A runtime font handle, or `nullptr` when the arguments are invalid or the font cannot
 * be parsed.
 */
MG_EXPORT MGF_RuntimeFont* MGF_RuntimeFont_Create(
    mgbyte* data,
    mgint dataBytes
);

// Releases a handle created by MGF_RuntimeFont_Create. Passing `nullptr` is allowed.
MG_EXPORT void MGF_RuntimeFont_Destroy(
    MGF_RuntimeFont* runtimeFont
);

/**
 * Ensures that the requested characters exist in the runtime atlas for a given size.
 *
 * Duplicate and overlapping character regions are merged before glyph generation.  On success,
 * `pageUpdates` points to the atlas pages touched by this call and `glyphs` points to the full
 * baked glyph set currently owned by `runtimeFont`.  Those pointers remain valid until the next
 * call that mutates `runtimeFont` or until MGF_RuntimeFont_Destroy is called.
 *
 * @param runtimeFont Runtime font handle created by MGF_RuntimeFont_Create.
 * @param size Requested glyph height in pixels. Must be greater than zero.
 * @param characterRegions Inclusive character ranges to bake.
 * @param characterRegionCount Number of entries in `characterRegions`.
 * @param pageUpdates Receives the touched atlas pages for this call.
 * @param glyphs Receives the complete glyph table currently cached by `runtimeFont`.
 * @param glyphCount Receives the number of entries written to `glyphs`.
 * @param lineSpacing Receives the font line spacing in pixels for `size`.
 * @return `true` when the glyph request completed successfully.
 */
MG_EXPORT mgbool MGF_RuntimeFont_EnsureGlyphs(
    MGF_RuntimeFont* runtimeFont,
    mgint size,
    MGF_CharacterRegion* characterRegions,
    mgint characterRegionCount,
    MGF_PageUpdate*& pageUpdates,
    mgint& pageUpdateCount,
    MGF_Glyph*& glyphs,
    mgint& glyphCount,
    mgint& lineSpacing
);

// Returns the last error code recorded on `runtimeFont`, or InvalidArgument for a null handle.
MG_EXPORT mgint MGF_RuntimeFont_GetLastErrorCode(MGF_RuntimeFont* runtimeFont);

// Returns the last error message recorded on `runtimeFont`, or a static message for a null handle.
MG_EXPORT const char* MGF_RuntimeFont_GetLastErrorMessage(MGF_RuntimeFont* runtimeFont);

// Releases buffers returned by MGF_BakeSpriteFont.  Passing `nullptr` is allowed.
MG_EXPORT void MGF_Free(void* resource);
