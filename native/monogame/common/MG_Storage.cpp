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
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/statvfs.h>
#include <dirent.h>
#include <unistd.h>
#include <errno.h>
#include <fts.h>
#include <ftw.h>
#define FILE_PERMISSIONS 0755
#endif


struct MG_StorageDevice
{
    std::string root;
};


struct MG_StorageContainer
{
    std::string storage_root;

    std::string root;
    std::string name;

    // The last enumerated files and folders.
    std::vector<std::string> content;

    // Last results returned from MG_EnumerateContent.
    std::vector<const char*> results;

    // Last file returned from load_file.
    std::vector<uint8_t> load_file;

    bool do_commit = false;
};


#if _WIN32

static void _MakeFileOperationPath(char* path, const char* source)
{
    strcpy(path, source);

    // We need forward slashes here.
    char* slash = path;
    while (slash[0] != 0)
    {
        if (slash[0] == '/')
            slash[0] = '\\';
        ++slash;
    }

    // SHFileOperationA does not like the trailing slash.
    int len = strlen(path);
    if (path[len - 1] == '\\')
        path[len - 1] = 0;

    // SHFileOperationA wants a double null terminator.
    path[strlen(path) + 1] = 0;
}

#endif


static bool _MG_CreateDirectory(const char* directory)
{
#if defined(_WIN32)
    int ok = SHCreateDirectoryExA(nullptr, directory, NULL);
    if (ok == ERROR_SUCCESS)
        return true;

    // These should probably also be valid create directory cases.
    if (ok == ERROR_FILE_EXISTS || ok == ERROR_ALREADY_EXISTS)
        return true;

    return false;
#else

    std::string path = directory;
    for (size_t i = 1; i <= path.size(); ++i)
    {
        if (i != path.size() && path[i] != '/')
            continue;

        std::string part = path.substr(0, i);
        int ok = mkdir(part.c_str(), FILE_PERMISSIONS);
        if (ok != 0)
        {
            if (errno != EEXIST)
                return false;
        }
    }

    return true;
#endif
}

static bool _MG_Storage_DeleteDirectory(const char* path)
{
    // This is a recursive delete operation.

#if _WIN32
    char tmp[MAX_PATH];
    _MakeFileOperationPath(tmp, path);

    SHFILEOPSTRUCTA op = {};
    op.wFunc = FO_DELETE;
    op.pFrom = tmp;
    op.fFlags = FOF_NO_UI;

    int ok = SHFileOperationA(&op);
    if (ok == 0)
        return true;
#else
    DIR* h = opendir(path);
    if (h == nullptr)
        return false;

    while (true)
    {
        struct dirent* e;
        e = readdir(h);
        if (e == nullptr)
            break;

        std::string name = e->d_name;
        if (name == "." || name == "..")
            continue;

        std::string entry = path;
        entry += name;

        if (e->d_type == DT_DIR)
        {
            entry += "/";

            _MG_Storage_DeleteDirectory(entry.c_str());
            continue;
        }

        // Delete the file.
        unlink(entry.c_str());
    }

    closedir(h);

    int err = remove(path);
    if (err == 0)
        return true;
#endif

    return false;
}

static bool _MG_Storage_CopyDirectory(const char* source, const char* dest)
{
#if _WIN32
    char src[MAX_PATH];
    _MakeFileOperationPath(src, source);

    char dst[MAX_PATH];
    _MakeFileOperationPath(dst, dest);

    SHFILEOPSTRUCTA op = {};
    op.wFunc = FO_COPY;
    op.pFrom = src;
    op.pTo = dst;
    op.fFlags = FOF_NO_UI;
    int ok = SHFileOperationA(&op);
    if (ok == 0)
        return true;

    return false;
#else
    if (!_MG_CreateDirectory(dest))
        return false;

    DIR* h = opendir(source);
    if (h == nullptr)
        return false;

    while (true)
    {
        struct dirent* e;
        e = readdir(h);
        if (e == nullptr)
            break;

        std::string name = e->d_name;
        if (name == "." || name == "..")
            continue;

        std::string spath = source;
        spath += name;

        std::string dpath = dest;
        dpath += name;

        if (e->d_type == DT_DIR)
        {
            spath += "/";
            dpath += "/";

            _MG_Storage_CopyDirectory(spath.c_str(), dpath.c_str());
            continue;
        }

        // Copy the source file to the des

        const int BUFFER_SIZE = 1024 * 1024;
        char* buffer = new char[BUFFER_SIZE];
        FILE* sf = fopen(spath.c_str(), "rb");
        if (sf == nullptr)
        {
            // Copy failed!
            delete [] buffer;
            closedir(h);
            return false;
        }

        FILE* df = fopen(dpath.c_str(), "wb");
        if (df == nullptr)
        {
            // Copy failed!
            delete [] buffer;
            fclose(sf);
            closedir(h);
            return false;
        }

        while (true)
        {
            int read = fread(buffer, 1, BUFFER_SIZE, sf);
            if (read <= 0)
                break;

            fwrite(buffer, 1, read, df);
        }

        delete [] buffer;
        fclose(df);
        fclose(sf);
    }

    closedir(h);

    return true;
#endif
}

static bool _MG_Storage_RenameDirectory(const char* source, const char* dest)
{
#if _WIN32
    char src[MAX_PATH];
    _MakeFileOperationPath(src, source);

    char dst[MAX_PATH];
    _MakeFileOperationPath(dst, dest);

    SHFILEOPSTRUCTA op = {};
    op.wFunc = FO_RENAME;
    op.pFrom = src;
    op.pTo = dst;
    op.fFlags = FOF_NO_UI;
    int ok = SHFileOperationA(&op);
    if (ok == 0)
        return true;

#else
    int ok = rename(source, dest);
    if (ok == 0)
        return true;
#endif

    return false;
}

static bool _MG_Storage_DirectoryExists(const char* path)
{
#if _WIN32
    char tmp[MAX_PATH];
    _MakeFileOperationPath(tmp, path);

    DWORD att = GetFileAttributesA(tmp);
    if (att == INVALID_FILE_ATTRIBUTES)
        return false;

    if ((att & FILE_ATTRIBUTE_DIRECTORY) != 0)
        return true;
#else
    struct stat s;
    int ok = stat(path, &s);
    if (ok == 0)
        return S_ISDIR(s.st_mode);
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

    const char* home = getenv("XDG_DATA_HOME");
    if (home != nullptr && home[0] != 0)
        root = home;
    else
    {
        home = getenv("HOME");
        if (home == nullptr || home[0] == 0)
            root = ".";
        else
        {
            root = home;
            root += "/.local/share";
        }
    }

#endif

    MG_StorageDevice* device = new MG_StorageDevice();

    // We always return forward slashes on all platforms.
    std::replace(root.begin(), root.end(), '\\', '/');

    // Make sure the SaveGames folder exists.
    device->root = root;
    device->root += "/SavedGames/";
    _MG_CreateDirectory(device->root.c_str());

    // Now add the title name and create that directory.
    device->root += titleName;
    device->root += "/";
    _MG_CreateDirectory(device->root.c_str());

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

    struct statvfs s;
    int ok = statvfs(device->root.c_str(), &s);
    if (ok != 0)
        return 0;

    uint64_t total_space = (uint64_t)s.f_blocks * s.f_frsize;
    return total_space;

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
    struct statvfs s;
    int ok = statvfs(device->root.c_str(), &s);
    if (ok != 0)
        return 0;

    uint64_t free_space = (uint64_t)s.f_bavail * s.f_frsize;
    return free_space;
#endif
}

MG_StorageContainer* MG_Storage_OpenContainer(MG_StorageDevice* device, const char* name, mglong requiredFreeBytes)
{
    assert(device != nullptr);

    auto container = new MG_StorageContainer();
    container->name = name;
    container->storage_root = device->root;

    // Make a folder off the device root.
    container->root = device->root;
    container->root += name;
    container->root += "/";

    // Check to see if we exist.
    if (_MG_Storage_DirectoryExists(container->root.c_str()))
    {
        // Nothing to do... we're good!
    }
    else
    {
        // Check to see if we crashed/powered off in the middle
        // of the save commit... try to restore it.      
        std::string last_container;
        last_container = container->storage_root;
        last_container += "~";
        last_container += container->name;

        if (_MG_Storage_DirectoryExists(last_container.c_str()))
        {
            // Restore the last container!
            _MG_Storage_RenameDirectory(last_container.c_str(), container->root.c_str());
        }
    }

    return container;
}

mgbool MG_Storage_DeleteContainer(MG_StorageDevice* device, const char* name)
{
    assert(device != nullptr);

    std::string root = device->root;
    root += name;
    root += "/";

    // This is an atomic operation, so just do it.

    return _MG_Storage_DeleteDirectory(root.data());
}

void MG_Storage_CloseContainer(MG_StorageContainer* container)
{
    if (!container)
        return;

    delete container;
}

static void _MG_Storage_MakeCommitPath(MG_StorageContainer* container, std::string& path, const char* name)
{
    // The commit occurs in the game's save path next
    // to the original container with the name ~.
    std::string scratch;
    scratch = container->storage_root;
    scratch += "~~/";

    if (!container->do_commit)
    {
        // TODO: Need to handle failure and bubble it up!

        // Make sure any old work is cleared.
        _MG_Storage_DeleteDirectory(scratch.c_str());
    
        // If the container source folder doesn't exist this is new storage
        // and we just need to create a empty scratch folder.
        if (!_MG_Storage_DirectoryExists(container->root.c_str()))
            _MG_CreateDirectory(scratch.c_str());
        else
        {
            // Copy the content of the container on disk to
            // the scratch container folder.
            _MG_Storage_CopyDirectory(container->root.c_str(), scratch.c_str());
        }

        container->do_commit = true;
    }

    // TODO: Protect against long paths or disallow them!
    path = scratch;
    if (name)
        path += name;
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

static bool MG_EnumerateContent(MG_StorageContainer* container, const char* directory)
{
    DIR* h = opendir(directory);
    if (h == nullptr)
        return false;

    while (true)
    {
        struct dirent* e;
        e = readdir(h);
        if (e == nullptr)
            break;

        std::string name = e->d_name;

        if (name == "." || name == "..")
            continue;

        std::string fullpath;
        fullpath = directory;
        fullpath += name;

        if (e->d_type == DT_DIR)
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

    closedir(h);

    return true;
}

#endif

void MG_Storage_EnumerateContent(MG_StorageContainer* container, mgbyte**& filesAndDirectories, mgint& size)
{
    filesAndDirectories = nullptr;
    size = 0;

    if (!container)
        return;
    
    container->content.clear();
    container->results.clear();

    MG_EnumerateContent(container, container->root.c_str());

    size = container->content.size();
    container->results.resize(size);
    for (int i=0; i < size; i++)
        container->results[i] = container->content[i].c_str();

    filesAndDirectories = (mgbyte**)container->results.data();
}

static void _MG_Storage_MakePath(MG_StorageContainer* container, std::string& path, const char* name)
{
    path = container->root.c_str();
    path += name;
}

mgbool MG_Storage_FileExists(MG_StorageContainer* container, const char* name)
{
    if (!container)
        return false;

    std::string path;
    _MG_Storage_MakePath(container, path, name);

#if _WIN32
    char tmp[MAX_PATH];
    _MakeFileOperationPath(tmp, path.c_str());

    DWORD att = GetFileAttributesA(tmp);
    if (att == INVALID_FILE_ATTRIBUTES)
        return false;

    if ((att & FILE_ATTRIBUTE_DIRECTORY) != 0)
        return false;
#else
    struct stat s;
    int ok = stat(path.c_str(), &s);
    if (ok != 0)
        return false;
    if (!S_ISREG(s.st_mode))
        return false;
#endif

    return true;
}

mgbyte* MG_Storage_FileLoad(MG_StorageContainer* container, const char* name, mgint& size)
{
    if (!container)
    {
        size = 0;
        return nullptr;
    }

    std::string path;
    _MG_Storage_MakePath(container, path, name);

    FILE* file = fopen(path.c_str(), "rb");
    if (file == nullptr)
    {
        size = 0;
        return nullptr;
    }

    fseek(file, 0, SEEK_END);
    size = ftell(file);
    if (size < 0 || size > INT_MAX)
    {
        size = 0;
        container->load_file.resize(size);
    }
    else
    {
        fseek(file, 0, SEEK_SET);

        container->load_file.resize(size);
        fread(container->load_file.data(), 1, size, file);
    }

    fclose(file);

    return container->load_file.data();
}

mgbool MG_Storage_FileSave(MG_StorageContainer* container, const char* name, mgbyte* data, mgint size)
{
    if (!container)
        return false;

    std::string path;
    _MG_Storage_MakeCommitPath(container, path, name);

    FILE* file = fopen(path.c_str(), "wb");
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

    std::string path;
    _MG_Storage_MakeCommitPath(container, path, name);

#if _WIN32
    bool result = ::DeleteFileA(path.c_str());
    if (result)
        return true;

    DWORD err = GetLastError();
    if (err == ERROR_FILE_NOT_FOUND)
        return true;
#else
    int err = unlink(path.c_str());
    if (err == 0)
        return true;
    if (errno == ENOENT)
        return true;
#endif

    return false;
}

mgbool MG_Storage_DirectoryExists(MG_StorageContainer* container, const char* name)
{
    if (!container)
        return false;

    std::string path;
    _MG_Storage_MakePath(container, path, name);

    return _MG_Storage_DirectoryExists(path.c_str());
}

mgbool MG_Storage_DirectoryDelete(MG_StorageContainer* container, const char* name)
{
    if (!container)
        return false;

    std::string path;
    _MG_Storage_MakeCommitPath(container, path, name);
    path += "/";

    return _MG_Storage_DeleteDirectory(path.c_str());
}

mgbool MG_Storage_DirectoryCreate(MG_StorageContainer* container, const char* name)
{
    if (!container)
        return false;

    std::string path;
    _MG_Storage_MakeCommitPath(container, path, name);

    return _MG_CreateDirectory(path.c_str());
}

void MG_Storage_CommitContainer(MG_StorageContainer* container)
{
    if (!container)
        return;

    // TODO: Bubble up failure cases!
    
    // We didn't make any changes...
    if (!container->do_commit)
        return;

    // Get the scratch container path.
    std::string scratch;
    _MG_Storage_MakeCommitPath(container, scratch, nullptr);

    // Rename the last container to ~name.
    // 
    // If we crash or lose power after this the next time
    // we start up we restore this folder.
    //
    std::string last_container;
    last_container = container->storage_root;
    last_container += "~";
    last_container += container->name;
    _MG_Storage_RenameDirectory(container->root.c_str(), last_container.c_str());

    // Rename the scratch to be the new container.
    _MG_Storage_RenameDirectory(scratch.c_str(), container->root.c_str());

    // Finally delete the original/last container.
    last_container += "/";
    _MG_Storage_DeleteDirectory(last_container.c_str());

    container->do_commit = false;
}
