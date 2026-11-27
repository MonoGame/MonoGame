// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MonoGame.Interop;

[MGHandle]
internal readonly struct MG_StorageDevice { }

[MGHandle]
internal readonly struct MG_StorageContainer { }


internal static unsafe partial class MG_Storage
{
    #region StorageDevice

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_OpenDevice", ExactSpelling = true)]
    public static extern MG_StorageDevice* OpenDevice([MarshalAs(UnmanagedType.LPUTF8Str)] string titleName, int playerIndex);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_CloseDevice", ExactSpelling = true)]
    public static extern void CloseDevice(MG_StorageDevice* device);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_GetTotalSpace", ExactSpelling = true)]
    public static extern long GetTotalSpace(MG_StorageDevice* device);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_GetFreeSpace", ExactSpelling = true)]
    public static extern long GetFreeSpace(MG_StorageDevice* device);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_OpenContainer", ExactSpelling = true)]
    public static extern MG_StorageContainer* OpenContainer(MG_StorageDevice* device, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, long requiredFreeBytes);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_DeleteContainer", ExactSpelling = true)]
    public static extern bool DeleteContainer(MG_StorageDevice* device, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    #endregion

    #region StorageContainer


    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_CloseContainer", ExactSpelling = true)]
    public static extern void CloseContainer(MG_StorageContainer* container);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_EnumerateContent", ExactSpelling = true)]
    public static extern void EnumerateContent(MG_StorageContainer* container, out byte** filesAndDirectories, out int size);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_FileLoad", ExactSpelling = true)]
    public static extern byte* FileLoad(MG_StorageContainer* container, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, out int size);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_FileSave", ExactSpelling = true)]
    public static extern bool FileSave(MG_StorageContainer* container, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, byte* data, int size);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_FileDelete", ExactSpelling = true)]
    public static extern bool FileDelete(MG_StorageContainer* container, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_DirectoryDelete", ExactSpelling = true)]
    public static extern bool DirectoryDelete(MG_StorageContainer* container, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_DirectoryCreate", ExactSpelling = true)]
    public static extern bool DirectoryCreate(MG_StorageContainer* container, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);


    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MG_Storage_CommitContainer", ExactSpelling = true)]
    public static extern void CommitContainer(MG_StorageContainer* container);

    #endregion
}
