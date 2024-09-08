// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Audio;
using System;
using System.Runtime.InteropServices;


namespace MonoGame.Interop;



[MGHandle]
internal readonly struct MGA_System { }

[MGHandle]
internal readonly struct MGA_Buffer { }

[MGHandle]
internal readonly struct MGA_Voice { }


/// <summary>
/// MonoGame native calls for platform audio features.
/// </summary>
internal static unsafe partial class MGA
{
    const string MonoGameNativeDLL = "monogame.native";

    #region System

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NativeFinishedCallback(nint callbackData);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_System_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGA_System* System_Create();

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_System_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void System_Destroy(MGA_System* system);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_System_GetMaxInstances", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int System_GetMaxInstances();

    #endregion

    #region Buffer

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Buffer_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGA_Buffer* Buffer_Create();

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Buffer_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Buffer_Destroy(MGA_Buffer* buffer);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Buffer_InitializeFormat", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Buffer_InitializeFormat(
        MGA_Buffer* buffer,
        byte[] waveHeader,
        byte[] waveData,
        int length,
        int loopStart,
        int loopLength);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Buffer_InitializePCM", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Buffer_InitializePCM(
        MGA_Buffer* buffer,
        byte[] waveData,
        int offset,
        int length,
        int sampleBits,
        int sampleRate,
        int channels,
        int loopStart,
        int loopLength);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Buffer_InitializeXact", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Buffer_InitializeXact(
        MGA_Buffer* buffer,
        uint codec,
        byte[] waveData,
        int length,
        int sampleRate,
        int blockAlignment,
        int channels,
        int loopStart,
        int loopLength);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Buffer_GetDuration", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong Buffer_GetDuration(MGA_Buffer* buffer);

    #endregion

    #region Voice

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGA_Voice* Voice_Create();

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Destroy(MGA_Voice* voice);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_AppendBuffer", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_AppendBuffer(MGA_Voice* voice, MGA_Buffer* buffer, [MarshalAs(UnmanagedType.U1)] bool clear);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_Play", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Play(MGA_Voice* voice, [MarshalAs(UnmanagedType.U1)] bool looped);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_Pause", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Pause(MGA_Voice* voice);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_Resume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Resume(MGA_Voice* voice);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_Stop", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Stop(MGA_Voice* voice, [MarshalAs(UnmanagedType.U1)] bool immediate);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_GetState", StringMarshalling = StringMarshalling.Utf8)]
    public static partial SoundState Voice_GetState(MGA_Voice* voice);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetPan", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_SetPan(MGA_Voice* voice, float pan);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetPitch", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_SetPitch(MGA_Voice* voice, float pitch);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetVolume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_SetVolume(MGA_Voice* voice, float volume);

    #endregion
}


