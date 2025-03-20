// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
                
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes

#pragma once

#include "api_common.h"
#include "api_enums.h"

struct MG_Asset;

MG_EXPORT mgbool MG_Asset_Open (const char* path, MG_Asset*& handle, mglong& length);
MG_EXPORT mgint MG_Asset_Read (MG_Asset* handle,  mgbyte* buffer, mglong count);
MG_EXPORT mglong MG_Asset_Seek (MG_Asset* handle, mglong offset, mgint whence);
MG_EXPORT void MG_Asset_Close (MG_Asset* handle);