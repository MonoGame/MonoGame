// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MG_Storage.h"

#include <emscripten/emscripten.h>

#include <string>
#include <vector>

struct MG_StorageDevice
{
    std::string title;
    mgint player;
};

struct MG_StorageContainer
{
    mgint id;
    std::vector<std::string> paths;
    std::vector<mgbyte*> results;
    std::vector<mgbyte> loaded;
};

EM_JS(void, MG_Storage_Web_Initialize, (),
{
    if (!Module.monogameStorage)
    {
        Module.monogameStorage = globalThis.MonoGameWebHost.createStorage();
    }
});

EM_JS(mgint, MG_Storage_Web_Open, (const char* title, mgint player, const char* name, mgint canCreate),
{
    return Module.monogameStorage.open(UTF8ToString(title), player, UTF8ToString(name), canCreate !== 0);
});

EM_JS(void, MG_Storage_Web_Close, (mgint id),
{
    Module.monogameStorage.close(id);
});

EM_JS(mgint, MG_Storage_Web_Delete, (const char* title, mgint player, const char* name),
{
    return Module.monogameStorage.remove(UTF8ToString(title), player, UTF8ToString(name)) ? 1 : 0;
});

EM_JS(mgint, MG_Storage_Web_Enumerate, (mgint id, mgbyte* data, mgint capacity),
{
    const paths = Module.monogameStorage.enumerate(id);
    if (paths === null)
    {
        return 0;
    }

    const bytes = new TextEncoder().encode(paths + "\0");
    if (data)
    {
        if (bytes.length > capacity)
        {
            return 0;
        }

        HEAPU8.set(bytes, data);
    }

    return bytes.length;
});

EM_JS(mgint, MG_Storage_Web_Load, (mgint id, const char* name, mgbyte* data, mgint capacity),
{
    const bytes = Module.monogameStorage.load(id, UTF8ToString(name));
    if (bytes === null)
    {
        return -1;
    }

    if (data)
    {
        if (bytes.length > capacity)
        {
            return -1;
        }

        HEAPU8.set(bytes, data);
    }

    return bytes.length;
});

EM_JS(mgint, MG_Storage_Web_Mutate, (mgint id, const char* name, const char* operation, const mgbyte* data, mgint size),
{
    return Module.monogameStorage.mutate(id, UTF8ToString(name), UTF8ToString(operation), HEAPU8.subarray(data, data + size)) ? 1 : 0;
});

EM_JS(void, MG_Storage_Web_Commit, (mgint id),
{
    Module.monogameStorage.commit(id);
});

EM_JS(mgdouble, MG_Storage_Web_GetTotalSpace, (),
{
    return globalThis.MonoGameWebHost.storageTotalSpace;
});

EM_JS(mgdouble, MG_Storage_Web_GetFreeSpace, (),
{
    return globalThis.MonoGameWebHost.getStorageFreeSpace();
});

MG_StorageDevice* MG_Storage_OpenDevice(const char* title, mgint player)
{
    if (title == nullptr || title[0] == 0 || player < -1 || player > 3)
    {
        return nullptr;
    }

    MG_Storage_Web_Initialize();
    return new MG_StorageDevice { title, player };
}

void MG_Storage_CloseDevice(MG_StorageDevice* device)
{
    delete device;
}

mglong MG_Storage_GetTotalSpace(MG_StorageDevice*)
{
    return static_cast<mglong>(MG_Storage_Web_GetTotalSpace());
}

mglong MG_Storage_GetFreeSpace(MG_StorageDevice*)
{
    return static_cast<mglong>(MG_Storage_Web_GetFreeSpace());
}

MG_StorageContainer* MG_Storage_OpenContainer(MG_StorageDevice* device, const char* name, mglong requiredFreeBytes)
{
    if (device == nullptr || name == nullptr || requiredFreeBytes <= 0)
    {
        return nullptr;
    }

    mgint id = MG_Storage_Web_Open(device->title.c_str(), device->player, name, requiredFreeBytes <= MG_Storage_GetFreeSpace(device));
    if (id == 0)
    {
        return nullptr;
    }

    MG_StorageContainer* container = new MG_StorageContainer();
    container->id = id;
    return container;
}

mgbool MG_Storage_DeleteContainer(MG_StorageDevice* device, const char* name)
{
    if (device == nullptr || name == nullptr)
    {
        return false;
    }

    return MG_Storage_Web_Delete(device->title.c_str(), device->player, name);
}

void MG_Storage_CloseContainer(MG_StorageContainer* container)
{
    if (container == nullptr)
    {
        return;
    }

    MG_Storage_Web_Close(container->id);
    delete container;
}

void MG_Storage_EnumerateContent(MG_StorageContainer* container, mgbyte**& filesAndDirectories, mgint& size)
{
    filesAndDirectories = nullptr;
    size = 0;
    if (container == nullptr)
    {
        return;
    }

    mgint length = MG_Storage_Web_Enumerate(container->id, nullptr, 0);
    if (length <= 0)
    {
        return;
    }

    std::vector<mgbyte> text(length);
    if (MG_Storage_Web_Enumerate(container->id, text.data(), length) != length)
    {
        return;
    }

    container->paths.clear();
    container->results.clear();
    std::string paths(reinterpret_cast<const char*>(text.data()));
    size_t start = 0;
    while (start < paths.size())
    {
        size_t end = paths.find('\n', start);
        if (end == std::string::npos)
        {
            end = paths.size();
        }
        container->paths.push_back(paths.substr(start, end - start));
        start = end + 1;
    }

    for (std::string& path : container->paths)
    {
        container->results.push_back(reinterpret_cast<mgbyte*>(&path[0]));
    }

    size = static_cast<mgint>(container->results.size());
    filesAndDirectories = container->results.data();
}

mgbyte* MG_Storage_FileLoad(MG_StorageContainer* container, const char* name, mgint& size)
{
    size = 0;
    if (container == nullptr || name == nullptr)
    {
        return nullptr;
    }

    mgint length = MG_Storage_Web_Load(container->id, name, nullptr, 0);
    if (length < 0)
    {
        return nullptr;
    }

    container->loaded.resize(length > 0 ? length : 1);
    if (MG_Storage_Web_Load(container->id, name, container->loaded.data(), length) != length)
    {
        return nullptr;
    }

    size = length;
    return container->loaded.data();
}

mgbool MG_Storage_FileSave(MG_StorageContainer* container, const char* name, mgbyte* data, mgint size)
{
    if (container == nullptr || name == nullptr || size < 0 || (data == nullptr && size != 0))
    {
        return false;
    }

    return MG_Storage_Web_Mutate(container->id, name, "save", data, size);
}

mgbool MG_Storage_FileDelete(MG_StorageContainer* container, const char* name)
{
    if (container == nullptr || name == nullptr)
    {
        return false;
    }

    return MG_Storage_Web_Mutate(container->id, name, "delete", nullptr, 0);
}

mgbool MG_Storage_DirectoryDelete(MG_StorageContainer* container, const char* name)
{
    if (container == nullptr || name == nullptr)
    {
        return false;
    }

    return MG_Storage_Web_Mutate(container->id, name, "rmdir", nullptr, 0);
}

mgbool MG_Storage_DirectoryCreate(MG_StorageContainer* container, const char* name)
{
    if (container == nullptr || name == nullptr)
    {
        return false;
    }

    return MG_Storage_Web_Mutate(container->id, name, "mkdir", nullptr, 0);
}

void MG_Storage_CommitContainer(MG_StorageContainer* container)
{
    if (container != nullptr)
    {
        MG_Storage_Web_Commit(container->id);
    }
}
