// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Media;

namespace MonoGame.Interop;


[MGHandle]
internal readonly struct MGM_Song { }

[MGHandle]
internal readonly struct MGM_Video { }


internal static unsafe partial class MGM
{
    #region Song

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NativeFinishedCallback(nint callbackData);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGM_Song* Song_Create(string mediaFilePath);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Destroy(MGM_Song* song);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_GetDuration", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong Song_GetDuration(MGM_Song* song);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_GetPosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong Song_GetPosition(MGM_Song* song);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_GetVolume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial float Song_GetVolume(MGM_Song* song);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_SetVolume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_SetVolume(MGM_Song* song, float volume);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_Play", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Play(MGM_Song* song, ulong startPositionMs, [MarshalAs(UnmanagedType.FunctionPtr)] NativeFinishedCallback callback, nint callbackData);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_Pause", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Pause(MGM_Song* song);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_Resume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Resume(MGM_Song* song);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Song_Stop", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Song_Stop(MGM_Song* song);

    #endregion


    #region Video

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGM_Video* Video_Create(string mediaFilePath, int cachedFrameNum, out int width, out int height, out float fps, out ulong duration);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Video_Destroy(MGM_Video* video);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_GetState", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MediaState Video_GetState(MGM_Video* video);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_GetPosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong Video_GetPosition(MGM_Video* video);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_SetVolume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Video_SetVolume(MGM_Video* video, float volume);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_SetLooped", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Video_SetLooped(MGM_Video* video, [MarshalAs(UnmanagedType.U1)] bool looped);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_Play", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Video_Play(MGM_Video* video);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_Pause", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Video_Pause(MGM_Video* video);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_Resume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Video_Resume(MGM_Video* video);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_Stop", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Video_Stop(MGM_Video* video);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_Video_GetFrame", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Video_GetFrame(MGM_Video* video, out uint frame, out MGG_Texture* handle);


    #endregion
}
