// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
                
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes

#pragma once

#include "api_common.h"
#include "api_enums.h"
#include "api_structs.h"



MG_EXPORT void MGI_ReadRGBA(mgbyte* data, mgint dataBytes, mgbool zeroTransparentPixels, mgint& width, mgint& height, mgbyte*& rgba);
MG_EXPORT void MGI_WriteJpg(mgbyte* data, mgint dataBytes, mgint width, mgint height, mgint quality, mgbyte*& jpg, mgint& jpgBytes);
MG_EXPORT void MGI_WritePng(mgbyte* data, mgint dataBytes, mgint width, mgint height, mgbyte*& png, mgint& pngBytes);
