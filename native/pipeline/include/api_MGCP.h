// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
                
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes

#pragma once

#include "api_common.h"
#include "api_enums.h"
#include "api_structs.h"



MG_EXPORT void* MP_ImportBitmap(const char* importPath, MGCP_Bitmap& bitmap);
MG_EXPORT void MP_FreeBitmap(MGCP_Bitmap& bitmap);
MG_EXPORT void* MP_ResizeBitmap(MGCP_Bitmap& srcBitmap, MGCP_Bitmap& dstBitmap);
MG_EXPORT void* MP_ExportBitmap(MGCP_Bitmap& bitmap, const char* exportPath);
