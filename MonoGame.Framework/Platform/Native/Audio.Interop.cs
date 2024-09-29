// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Framework.Utilities;


namespace MonoGame.Interop;

[MGHandle]
internal readonly struct MGA_System { }

[MGHandle]
internal readonly struct MGA_Buffer { }

[MGHandle]
internal readonly struct MGA_Voice { }


internal struct ReverbSettings
{
    public float ReflectionsDelayMs;
    public float ReverbDelayMs;
    public float PositionLeft;
    public float PositionRight;
    public float PositionLeftMatrix;
    public float PositionRightMatrix;
    public float EarlyDiffusion;
    public float LateDiffusion;
    public float LowEqGain;
    public float LowEqCutoff;
    public float HighEqGain;
    public float HighEqCutoff;
    public float RearDelayMs;
    public float RoomFilterFrequencyHz;
    public float RoomFilterMainDb;
    public float RoomFilterHighFrequencyDb;
    public float ReflectionsGainDb;
    public float ReverbGainDb;
    public float DecayTimeSec;
    public float DensityPct;
    public float RoomSizeFeet;
    public float WetDryMixPct;
}

internal struct Listener
{
    public Vector3 Position;
    public Vector3 Forward;
    public Vector3 Up;
    public Vector3 Velocity;
};

internal struct Emitter
{
    public Vector3 Position;
    public Vector3 Forward;
    public Vector3 Up;
    public Vector3 Velocity;
    public float DopplerScale;
};



/// <summary>
/// MonoGame native calls for platform audio features.
/// </summary>
internal static unsafe partial class MGA
{
    #region System

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NativeFinishedCallback(nint callbackData);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_System_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGA_System* System_Create();

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_System_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void System_Destroy(MGA_System* system);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_System_GetMaxInstances", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int System_GetMaxInstances();

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_System_SetReverbSettings", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void System_SetReverbSettings(MGA_System* system, in ReverbSettings settings);

    #endregion

    #region Buffer

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGA_Buffer* Buffer_Create(MGA_System* system);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Buffer_Destroy(MGA_Buffer* buffer);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_InitializeFormat", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Buffer_InitializeFormat(
        MGA_Buffer* buffer,
        byte[] waveHeader,
        byte[] waveData,
        int length,
        int loopStart,
        int loopLength);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_InitializePCM", StringMarshalling = StringMarshalling.Utf8)]
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

    // TODO: This should go away after we move to FAudio's Xact implementation.
    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_InitializeXact", StringMarshalling = StringMarshalling.Utf8)]
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

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_GetDuration", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong Buffer_GetDuration(MGA_Buffer* buffer);

    #endregion

    #region Voice

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGA_Voice* Voice_Create(MGA_System* system, int sampleRate = 0, int channels = 0);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Destroy(MGA_Voice* voice);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_GetBufferCount", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int Voice_GetBufferCount(MGA_Voice* voice);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetBuffer", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_SetBuffer(MGA_Voice* voice, MGA_Buffer* buffer);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_AppendBuffer", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_AppendBuffer(MGA_Voice* voice, byte* buffer, uint size);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Play", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Play(MGA_Voice* voice, [MarshalAs(UnmanagedType.U1)] bool looped);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Pause", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Pause(MGA_Voice* voice);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Resume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Resume(MGA_Voice* voice);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Stop", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Stop(MGA_Voice* voice, [MarshalAs(UnmanagedType.U1)] bool immediate);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_GetState", StringMarshalling = StringMarshalling.Utf8)]
    public static partial SoundState Voice_GetState(MGA_Voice* voice);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_GetPosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong Voice_GetPosition(MGA_Voice* voice);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetPan", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_SetPan(MGA_Voice* voice, float pan);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetPitch", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_SetPitch(MGA_Voice* voice, float pitch);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetVolume", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_SetVolume(MGA_Voice* voice, float volume);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetReverbMix", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_SetReverbMix(MGA_Voice* voice, float mix);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetFilterMode", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_SetFilterMode(MGA_Voice* voice, FilterMode mode, float filterQ, float frequency);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_ClearFilterMode", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_ClearFilterMode(MGA_Voice* voice);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Apply3D", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Voice_Apply3D(MGA_Voice* voice, in Listener listener, in Emitter emitter, float distanceScale);

    #endregion
}


