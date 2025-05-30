// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "mg_common.h"
#include "api_MG_Asset.h"
#include <stdio.h>

struct MG_Asset
{
    FILE* file;
};

mgbool MG_Asset_Open(const char* path, MG_Asset*& handle, mglong& length)
{
    handle = new MG_Asset();
    handle->file = fopen(path, "rb");
    if (handle->file == nullptr)
    {
        delete handle;
        return false;
    }

    if (fseek(handle->file, 0, SEEK_END) != 0)
    {
        //unable to seek file for some reason
        delete handle;
        return false;
    }

    length = ftell(handle->file);

    if (fseek(handle->file, 0, SEEK_SET) != 0)
    {
        //unable to seek back to file start for some reason
        delete handle;
        return false;
    }

    return true;
}

mgint MG_Asset_Read(MG_Asset* handle,  mgbyte* buffer, mglong count)
{
    return fread(buffer, 1, count, handle->file);
}

mglong MG_Asset_Seek(MG_Asset* handle, mglong offset, mgint whence)
{
    return fseek(handle->file, offset, whence);
}

void MG_Asset_Close(MG_Asset* handle)
{
    fclose(handle->file);
    delete handle;
}
