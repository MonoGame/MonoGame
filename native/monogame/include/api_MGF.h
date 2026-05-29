// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "api_common.h"

struct MGF_RuntimeFont;

struct MGF_CharacterRegion
{
    mgchar Start;
    mgchar End;
};

struct MGF_Glyph
{
    mgchar Character;
    mgint Size;
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

MG_EXPORT MGF_RuntimeFont* MGF_RuntimeFont_Create(
    mgbyte* data,
    mgint dataBytes
);

MG_EXPORT void MGF_RuntimeFont_Destroy(
    MGF_RuntimeFont* runtimeFont
);

MG_EXPORT mgbool MGF_RuntimeFont_EnsureGlyphs(
    MGF_RuntimeFont* runtimeFont,
    mgint size,
    MGF_CharacterRegion* characterRegions,
    mgint characterRegionCount,
    mgbyte*& atlasRgba,
    mgint& atlasWidth,
    mgint& atlasHeight,
    mgbool& atlasRebuilt,
    MGF_Glyph*& glyphs,
    mgint& glyphCount,
    mgint& lineSpacing
);

MG_EXPORT void MGF_Free(void* resource);
