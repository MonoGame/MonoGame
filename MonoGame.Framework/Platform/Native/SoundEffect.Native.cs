// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoGame.Interop;


namespace Microsoft.Xna.Framework.Audio;

public sealed partial class SoundEffect
{
    internal static readonly int MAX_PLAYING_INSTANCES = MGA.System_GetMaxInstances();

    internal static unsafe MGA_System* System;

    internal unsafe MGA_Buffer* Buffer;

    private unsafe static void PlatformInitialize()
    {
        System = MGA.System_Create();
    }

    internal unsafe static void PlatformShutdown()
    {
        if (System != null)
        {
            MGA.System_Destroy(System);
            System = null;
        }
    }

    private void PlatformLoadAudioStream(Stream s, out TimeSpan duration)
    {
        using (var reader = new BinaryReader(s))
        {
            var riff = reader.ReadBytes(4);
            reader.ReadBytes(8);

            byte[] waveData = null;
            byte[] headerData = null;
            byte[] dpdsData = null;

            // Read chunks.
            for (;;)
            {
                var name = reader.ReadBytes(4);
                var len = reader.ReadInt32();
                if (len == -1)
                    break;

                var isData = name[0] == 'd' && name[1] == 'a' && name[2] == 't' && name[3] == 'a';
                var isFormat = name[0] == 'f' && name[1] == 'm' && name[2] == 't' && name[3] == ' ';
                var isDpDs = name[0] == 'd' && name[1] == 'p' && name[2] == 'd' && name[3] == 's';

                if (isData)
                    waveData = reader.ReadBytes(len);
                else if (isFormat)
                    headerData = reader.ReadBytes(len);
                else if (isDpDs)
                    dpdsData = reader.ReadBytes(len);
                else
                    reader.ReadBytes(len);

                if (waveData != null && headerData != null)
                    break;
            }

            unsafe
            {
                Buffer = MGA.Buffer_Create(System);
                MGA.Buffer_InitializeFormat(Buffer, headerData, waveData, waveData.Length, 0, 0);

                var milliseconds = MGA.Buffer_GetDuration(Buffer);
                duration = TimeSpan.FromMilliseconds(milliseconds);
            }
        }
    }

    private unsafe void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
    {
        Buffer = MGA.Buffer_Create(System);
        MGA.Buffer_InitializePCM(Buffer, buffer, offset, count, sampleBits, sampleRate, (int)channels, loopStart, loopLength);
    }

    private unsafe void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
    {
        Buffer = MGA.Buffer_Create(System);
        MGA.Buffer_InitializeFormat(Buffer, header, buffer, bufferSize, loopStart, loopLength);
    }

    // TODO: This should go away after we move to FAudio's Xact implementation.
    private unsafe void PlatformInitializeXact(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength, out TimeSpan duration)
    {
        // This is only the platform specific non-streaming
        // Xact sound handling as PCM is already handled.

        Buffer = MGA.Buffer_Create(System);
        MGA.Buffer_InitializeXact(Buffer, (uint)codec, buffer, buffer.Length, sampleRate, blockAlignment, channels, loopStart, loopLength);

        var milliseconds = MGA.Buffer_GetDuration(Buffer);
        duration = TimeSpan.FromMilliseconds(milliseconds);
    }

    private unsafe void PlatformSetupInstance(SoundEffectInstance instance)
    {
        // If the instance came from the pool then it could
        // already have a valid voice assigned.

        if (instance.Voice == null)
            instance.Voice = MGA.Voice_Create(System);

        MGA.Voice_SetBuffer(instance.Voice, Buffer);
    }

    private unsafe void PlatformDispose(bool disposing)
    {
        if (disposing)
        {
            if (Buffer != null)
            {
                MGA.Buffer_Destroy(Buffer);
                Buffer = null;
            }
        }
    }

    internal unsafe static void PlatformSetReverbSettings(ReverbSettings reverbSettings)
    {
        var settings = new MonoGame.Interop.ReverbSettings
        {
            ReflectionsDelayMs = reverbSettings.ReflectionsDelayMs,
            ReverbDelayMs = reverbSettings.ReverbDelayMs,
            PositionLeft = reverbSettings.PositionLeft,
            PositionRight = reverbSettings.PositionRight,
            PositionLeftMatrix = reverbSettings.PositionLeftMatrix,
            PositionRightMatrix = reverbSettings.PositionRightMatrix,
            EarlyDiffusion = reverbSettings.EarlyDiffusion,
            LateDiffusion = reverbSettings.LateDiffusion,
            LowEqGain = reverbSettings.LowEqGain,
            LowEqCutoff = reverbSettings.LowEqCutoff,
            HighEqGain = reverbSettings.HighEqGain,
            HighEqCutoff = reverbSettings.HighEqCutoff,
            RearDelayMs = reverbSettings.RearDelayMs,
            RoomFilterFrequencyHz = reverbSettings.RoomFilterFrequencyHz,
            RoomFilterMainDb = reverbSettings.RoomFilterMainDb,
            RoomFilterHighFrequencyDb = reverbSettings.RoomFilterHighFrequencyDb,
            ReflectionsGainDb = reverbSettings.ReflectionsGainDb,
            ReverbGainDb = reverbSettings.ReverbGainDb,
            DecayTimeSec = reverbSettings.DecayTimeSec,
            DensityPct = reverbSettings.DensityPct,
            RoomSizeFeet = reverbSettings.RoomSizeFeet,
            WetDryMixPct = reverbSettings.WetDryMixPct
        };

        MGA.System_SetReverbSettings(System, in settings);
    }
}
