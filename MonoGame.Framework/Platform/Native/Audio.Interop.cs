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

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_System_Create", ExactSpelling = true)]
    public static extern MGA_System* System_Create();

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_System_Destroy", ExactSpelling = true)]
    public static extern void System_Destroy(MGA_System* system);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_System_GetMaxInstances", ExactSpelling = true)]
    public static extern int System_GetMaxInstances();

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_System_SetReverbSettings", ExactSpelling = true)]
    public static extern void System_SetReverbSettings(MGA_System* system, in ReverbSettings settings);

    #endregion

    #region Buffer

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_Create", ExactSpelling = true)]
    public static extern MGA_Buffer* Buffer_Create(MGA_System* system);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_Destroy", ExactSpelling = true)]
    public static extern void Buffer_Destroy(MGA_Buffer* buffer);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_InitializeFormat", ExactSpelling = true)]
    public static extern void Buffer_InitializeFormat(
        MGA_Buffer* buffer,
        byte* waveHeader,
        byte* waveData,
        int length,
        int loopStart,
        int loopLength);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_InitializePCM", ExactSpelling = true)]
    public static extern void Buffer_InitializePCM(
        MGA_Buffer* buffer,
        byte* waveData,
        int offset,
        int length,
        int sampleBits,
        int sampleRate,
        int channels,
        int loopStart,
        int loopLength);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_InitializeXact", ExactSpelling = true)]
    public static extern void Buffer_InitializeXact(
        MGA_Buffer* buffer,
        uint codec,
        byte* waveData,
        int length,
        int sampleRate,
        int blockAlignment,
        int channels,
        int loopStart,
        int loopLength);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Buffer_GetDuration", ExactSpelling = true)]
    public static extern ulong Buffer_GetDuration(MGA_Buffer* buffer);

    #endregion

    #region Voice

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Create", ExactSpelling = true)]
    public static extern MGA_Voice* Voice_Create(MGA_System* system, int sampleRate = 0, int channels = 0);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Destroy", ExactSpelling = true)]
    public static extern void Voice_Destroy(MGA_Voice* voice);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_GetBufferCount", ExactSpelling = true)]
    public static extern int Voice_GetBufferCount(MGA_Voice* voice);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetBuffer", ExactSpelling = true)]
    public static extern void Voice_SetBuffer(MGA_Voice* voice, MGA_Buffer* buffer);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_AppendBuffer", ExactSpelling = true)]
    public static extern void Voice_AppendBuffer(MGA_Voice* voice, byte* buffer, uint size);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Play", ExactSpelling = true)]
    public static extern void Voice_Play(MGA_Voice* voice, byte looped);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Pause", ExactSpelling = true)]
    public static extern void Voice_Pause(MGA_Voice* voice);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Resume", ExactSpelling = true)]
    public static extern void Voice_Resume(MGA_Voice* voice);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Stop", ExactSpelling = true)]
    public static extern void Voice_Stop(MGA_Voice* voice, byte immediate);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_GetState", ExactSpelling = true)]
    public static extern SoundState Voice_GetState(MGA_Voice* voice);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_GetPosition", ExactSpelling = true)]
    public static extern ulong Voice_GetPosition(MGA_Voice* voice);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetPan", ExactSpelling = true)]
    public static extern void Voice_SetPan(MGA_Voice* voice, float pan);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetPitch", ExactSpelling = true)]
    public static extern void Voice_SetPitch(MGA_Voice* voice, float pitch);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetVolume", ExactSpelling = true)]
    public static extern void Voice_SetVolume(MGA_Voice* voice, float volume);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetReverbMix", ExactSpelling = true)]
    public static extern void Voice_SetReverbMix(MGA_Voice* voice, float mix);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_SetFilterMode", ExactSpelling = true)]
    public static extern void Voice_SetFilterMode(MGA_Voice* voice, FilterMode mode, float filterQ, float frequency);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_ClearFilterMode", ExactSpelling = true)]
    public static extern void Voice_ClearFilterMode(MGA_Voice* voice);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGA_Voice_Apply3D", ExactSpelling = true)]
    public static extern void Voice_Apply3D(MGA_Voice* voice, in Listener listener, in Emitter emitter, float distanceScale);

    #endregion
}


