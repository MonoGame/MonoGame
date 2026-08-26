// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio;

public sealed partial class SoundEffect : IDisposable
{
    internal const int MAX_PLAYING_INSTANCES = 0;

    private static readonly PlatformNotSupportedException s_notSupportedException =
        new PlatformNotSupportedException("Audio playback is not implemented for MonoGame.Web yet.");

    private static void ThrowNotSupported()
    {
        throw s_notSupportedException;
    }

    private void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;
        ThrowNotSupported();
    }

    private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
    {
        ThrowNotSupported();
    }

    private void PlatformInitializeIeeeFloat(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
    {
        ThrowNotSupported();
    }

    private void PlatformInitializeAdpcm(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
    {
        ThrowNotSupported();
    }

    private void PlatformInitializeIma4(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
    {
        ThrowNotSupported();
    }

    private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
    {
        ThrowNotSupported();
    }

    private void PlatformInitializeXact(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;
        ThrowNotSupported();
    }

    private void PlatformSetupInstance(SoundEffectInstance inst)
    {
        ThrowNotSupported();
    }

    internal static void PlatformSetReverbSettings(ReverbSettings reverbSettings)
    {
        ThrowNotSupported();
    }

    private void PlatformDispose(bool disposing)
    {
    }

    internal static void PlatformInitialize()
    {
    }

    internal static void PlatformShutdown()
    {
    }
}
