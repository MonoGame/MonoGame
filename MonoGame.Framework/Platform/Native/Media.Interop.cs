// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace MonoGame.Interop;


[MGHandle]
internal readonly struct MGM_Song { }

internal static unsafe partial class MGM
{
    const string MonoGameNativeDLL = "monogame.native";

    #region Song

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NativeFinishedCallback(nint callbackData);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGM_Song* Song_Create(string mediaFilePath);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_GetDuration", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong Song_GetDuration(MGM_Song* song);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_GetPosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong Song_GetPosition(MGM_Song* song);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_GetVolume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial float Song_GetVolume(MGM_Song* song);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_SetVolume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_SetVolume(MGM_Song* song, float volume);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_Pause", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Pause(MGM_Song* song);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_Play", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Play(MGM_Song* song, ulong startPositionMs, [MarshalAs(UnmanagedType.FunctionPtr)] NativeFinishedCallback callback, nint callbackData);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_Resume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Resume(MGM_Song* song);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_Stop", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Stop(MGM_Song* song);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGM_Song_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Destroy(MGM_Song* song);

    #endregion
}
