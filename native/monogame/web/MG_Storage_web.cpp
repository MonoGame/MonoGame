// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MG_Storage.h"

// TODO: These are stub implementations
//       Need to implement storage for browser.

MG_StorageDevice* MG_Storage_OpenDevice(const char*, mgint)
{
    return nullptr;
}

void MG_Storage_CloseDevice(MG_StorageDevice*)
{
}

mglong MG_Storage_GetTotalSpace(MG_StorageDevice*)
{
    return 0;
}

mglong MG_Storage_GetFreeSpace(MG_StorageDevice*)
{
    return 0;
}

MG_StorageContainer* MG_Storage_OpenContainer(MG_StorageDevice*, const char*, mglong)
{
    return nullptr;
}

mgbool MG_Storage_DeleteContainer(MG_StorageDevice*, const char*)
{
    return false;
}

void MG_Storage_CloseContainer(MG_StorageContainer*)
{
}

void MG_Storage_EnumerateContent(MG_StorageContainer*, mgbyte**& filesAndDirectories, mgint& size)
{
    filesAndDirectories = nullptr;
    size = 0;
}

mgbyte* MG_Storage_FileLoad(MG_StorageContainer*, const char*, mgint& size)
{
    size = 0;
    return nullptr;
}

mgbool MG_Storage_FileSave(MG_StorageContainer*, const char*, mgbyte*, mgint)
{
    return false;
}

mgbool MG_Storage_FileDelete(MG_StorageContainer*, const char*)
{
    return false;
}

mgbool MG_Storage_DirectoryDelete(MG_StorageContainer*, const char*)
{
    return false;
}

mgbool MG_Storage_DirectoryCreate(MG_StorageContainer*, const char*)
{
    return false;
}

void MG_Storage_CommitContainer(MG_StorageContainer*)
{
}
