// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
                
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes

#pragma once

#include "api_common.h"
#include "api_enums.h"
#include "api_structs.h"


struct MG_StorageDevice;
struct MG_StorageContainer;

MG_EXPORT MG_StorageDevice* MG_Storage_OpenDevice(const char* titleName, mgint playerIndex);
MG_EXPORT void MG_Storage_CloseDevice(MG_StorageDevice* device);
MG_EXPORT mglong MG_Storage_GetTotalSpace(MG_StorageDevice* device);
MG_EXPORT mglong MG_Storage_GetFreeSpace(MG_StorageDevice* device);
MG_EXPORT MG_StorageContainer* MG_Storage_OpenContainer(MG_StorageDevice* device, const char* name, mglong requiredFreeBytes);
MG_EXPORT mgbool MG_Storage_DeleteContainer(MG_StorageDevice* device, const char* name);
MG_EXPORT void MG_Storage_CloseContainer(MG_StorageContainer* container);
MG_EXPORT void MG_Storage_EnumerateContent(MG_StorageContainer* container, mgbyte**& filesAndDirectories, mgint& size);
MG_EXPORT mgbyte* MG_Storage_FileLoad(MG_StorageContainer* container, const char* name, mgint& size);
MG_EXPORT mgbool MG_Storage_FileSave(MG_StorageContainer* container, const char* name, mgbyte* data, mgint size);
MG_EXPORT mgbool MG_Storage_FileDelete(MG_StorageContainer* container, const char* name);
MG_EXPORT mgbool MG_Storage_DirectoryDelete(MG_StorageContainer* container, const char* name);
MG_EXPORT mgbool MG_Storage_DirectoryCreate(MG_StorageContainer* container, const char* name);
MG_EXPORT void MG_Storage_CommitContainer(MG_StorageContainer* container);
