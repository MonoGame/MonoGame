// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "mg_common.h"
#include "api_MG_Storage.h"

#include <stdio.h>

#if defined(_WIN32)
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shellapi.h>
#include <shlobj.h>
#else

#endif


struct MG_StorageDevice
{
    std::string root;
};


struct MG_StorageContainer
{
    std::string root;
    std::string name;

    // The last enumerated files and folders.
    std::vector<std::string> content;

    // Last results returned from MG_EnumerateContent.
    std::vector<const char*> results;

    // Last file returned from load_file.
    std::vector<uint8_t> load_file;
};


static void MG_CreateDirectory(const char* directory)
{
#if defined(_WIN32)
    CreateDirectoryA(directory, NULL);
#else

#endif
}

static bool _MG_Storage_DeleteDirectory(char* path)
{
#if _WIN32

    // SHFileOperationA does not like the trailing slash.
    int len = strlen(path);
    if (path[len - 1] == '/')
        path[len - 1] = 0;

    SHFILEOPSTRUCTA op = {};
    op.wFunc = FO_DELETE;
    op.pFrom = path;
    op.fFlags = //FOF_ALLOWUNDO |
        FOF_NO_UI;

    int ok = SHFileOperationA(&op);
    if (ok == 0)
        return true;

#else

#error Fix me!

#endif

    return false;
}

MG_StorageDevice* MG_Storage_OpenDevice(const char* titleName, mgint playerIndex)
{
    std::string root;

#if defined(_WIN32)

    PWSTR pszMyDocuments;
    HRESULT hr = SHGetKnownFolderPath(FOLDERID_Documents, KF_FLAG_DEFAULT, NULL, &pszMyDocuments);
    if (!SUCCEEDED(hr))
        return nullptr;

    char temp[MAX_PATH];
    WideCharToMultiByte(CP_UTF8, 0, pszMyDocuments, -1, temp, MAX_PATH, NULL, NULL);

    root = temp;
#else

#endif

    MG_StorageDevice* device = new MG_StorageDevice();

    // We always return forward slashes on all platforms.
    std::replace(root.begin(), root.end(), '\\', '/');

    // Make sure the SaveGames folder exists on Windows.
    device->root = root;
    device->root += "/SavedGames/";
    MG_CreateDirectory(device->root.c_str());

    // Now add the title name and create that directory.
    device->root += titleName;
    device->root += "/";
    MG_CreateDirectory(device->root.c_str());

    return device;
}

void MG_Storage_CloseDevice(MG_StorageDevice* device)
{
    if (device)
        delete device;
}

mglong MG_Storage_GetTotalSpace(MG_StorageDevice* device)
{
    assert(device != nullptr);

#if defined(_WIN32)
    DISK_SPACE_INFORMATION info;
    HRESULT ok = GetDiskSpaceInformationA(device->root.c_str(), &info);
    if (FAILED(ok))
        return 0;

    mglong total = (mglong)(
        info.CallerTotalAllocationUnits *
        info.BytesPerSector *
        info.SectorsPerAllocationUnit);

    return total;
#else
    return 0;
#endif
}

mglong MG_Storage_GetFreeSpace(MG_StorageDevice* device)
{
    assert(device != nullptr);

#if defined(_WIN32)
    DISK_SPACE_INFORMATION info;
    HRESULT ok = GetDiskSpaceInformationA(device->root.c_str(), &info);
    if (FAILED(ok))
        return 0;

    mglong free = (mglong)(
        info.CallerAvailableAllocationUnits *
        info.BytesPerSector *
        info.SectorsPerAllocationUnit);

    return free;
#else
    return 0;
#endif
}

MG_StorageContainer* MG_Storage_OpenContainer(MG_StorageDevice* device, const char* name, mglong requiredFreeBytes)
{
    assert(device != nullptr);

    auto container = new MG_StorageContainer();
    container->name = name;

    // Make a folder off the device root.
    container->root = device->root;
    container->root += name;
    container->root += "/";

    // TODO: Good or bad idea?
    // 
    // We specifically don't create the directory here
    // this is something that commit will do if needed.   

    return container;
}

mgbool MG_Storage_DeleteContainer(MG_StorageDevice* device, const char* name)
{
    assert(device != nullptr);

    std::string root = device->root;
    root += name;

    return _MG_Storage_DeleteDirectory(root.data());
}

void MG_Storage_CloseContainer(MG_StorageContainer* container)
{
    if (!container)
        return;

    delete container;
}

static void _MG_Storage_CreateCommitDirectory(MG_StorageContainer* container)
{
    SHCreateDirectoryExA(nullptr, container->root.c_str(), nullptr);
}


#if defined(_WIN32)
static bool MG_EnumerateContent(MG_StorageContainer* container, const char* directory)
{
    std::string path;
    path = directory;
    path += "*";

    WIN32_FIND_DATAA found;
    HANDLE h = ::FindFirstFileA(path.c_str(), &found);
    if (h == INVALID_HANDLE_VALUE)
        return false;

    do
    {
        std::string name = found.cFileName;

        if (name == "." || name == "..")
            continue;

        std::string fullpath;
        fullpath = directory;
        fullpath += name;

        if (found.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
        {
            fullpath += "/";

            std::string relative = fullpath.substr(container->root.size());
            container->content.push_back(relative);

            MG_EnumerateContent(container, fullpath.c_str());
            continue;
        }

        // Just a file.
        std::string relative = fullpath.substr(container->root.size());
        container->content.push_back(relative);
    }
    while (FindNextFileA(h, &found) == TRUE);

    FindClose(h);

    return true;
}
#else

#endif

void MG_Storage_EnumerateContent(MG_StorageContainer* container, mgbyte**& filesAndDirectories, mgint& size)
{
    filesAndDirectories = nullptr;
    size = 0;

    if (!container)
        return;
    
    container->content.clear();
    container->results.clear();

#if defined(_WIN32)
    MG_EnumerateContent(container, container->root.c_str());
#else

#endif

    size = container->content.size();
    container->results.resize(size);
    for (int i=0; i < size; i++)
        container->results[i] = container->content[i].c_str();

    filesAndDirectories = (mgbyte**)container->results.data();
}

static void _MG_Storage_MakePath(MG_StorageContainer* container, char* path, const char* name)
{
    // TODO: Protect against long paths or disallow them!
    strcpy(path, container->root.c_str());
    strcat(path, name);
}

mgbool MG_Storage_FileExists(MG_StorageContainer* container, const char* name)
{
    if (!container)
        return false;

    char path[MAX_PATH];
    _MG_Storage_MakePath(container, path, name);

    return false;
}

mgbyte* MG_Storage_FileLoad(MG_StorageContainer* container, const char* name, mgint& size)
{
    if (!container)
    {
        size = 0;
        return nullptr;
    }

    char path[MAX_PATH];
    _MG_Storage_MakePath(container, path, name);

    FILE* file = fopen(path, "rb");
    if (file == nullptr)
    {
        size = 0;
        return nullptr;
    }

    fseek(file, 0, SEEK_END);
    size = ftell(file);
    fseek(file, 0, SEEK_SET);
    container->load_file.resize(size);

    fread(container->load_file.data(), 1, size, file);
    fclose(file);

    return container->load_file.data();
}

mgbool MG_Storage_FileSave(MG_StorageContainer* container, const char* name, mgbyte* data, mgint size)
{
    if (!container)
        return false;

    _MG_Storage_CreateCommitDirectory(container);

    char path[MAX_PATH];
    _MG_Storage_MakePath(container, path, name);

    FILE* file = fopen(path, "wb");
    if (file == nullptr)
        return false;

    int written = fwrite(data, 1, size, file);    
    fclose(file);

    if (written != size)
        return false;

    return true;
}

mgbool MG_Storage_FileDelete(MG_StorageContainer* container, const char* name)
{
    if (!container)
        return false;

    char path[MAX_PATH];
    _MG_Storage_MakePath(container, path, name);

#if _WIN32

    return ::DeleteFileA(path);

#else

#error FIX ME!

    return false;
#endif
}

mgbool MG_Storage_DirectoryExists(MG_StorageContainer* container, const char* name)
{
    if (!container)
        return false;

    char path[MAX_PATH];
    _MG_Storage_MakePath(container, path, name);

    return false;
}

mgbool MG_Storage_DirectoryDelete(MG_StorageContainer* container, const char* name)
{
    if (!container)
        return false;

    char path[MAX_PATH];
    _MG_Storage_MakePath(container, path, name);

    return _MG_Storage_DeleteDirectory(path);
}

mgbool MG_Storage_DirectoryCreate(MG_StorageContainer* container, const char* name)
{
    if (!container)
        return false;

    _MG_Storage_CreateCommitDirectory(container);

    char path[MAX_PATH];
    _MG_Storage_MakePath(container, path, name);

    int ok = SHCreateDirectoryExA(nullptr, path, nullptr);
    if (ok == ERROR_SUCCESS)
        return true;

    if (ok == ERROR_FILE_EXISTS || ok == ERROR_ALREADY_EXISTS)
        return true;

    return false;
}

void MG_Storage_CommitContainer(MG_StorageContainer* container)
{
    if (!container)
        return;

    // TODO!
}
