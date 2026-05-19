// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        internal const int MAX_PLAYING_INSTANCES = 0;

        private void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            throw new NotImplementedException();
        }

        private void PlatformInitializeIeeeFloat(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            throw new NotImplementedException();
        }

        private void PlatformInitializeAdpcm(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
        {
            throw new NotImplementedException();
        }

        private void PlatformInitializeIma4(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
        {
            throw new NotImplementedException();
        }

        private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
        {
            throw new NotImplementedException();
        }

        private void PlatformInitializeXact(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength, out TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        private void PlatformSetupInstance(SoundEffectInstance inst)
        {
            throw new NotImplementedException();
        }

        internal static void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        internal static void PlatformInitialize()
        {
            throw new NotImplementedException();
        }

        internal static void PlatformShutdown()
        {
            throw new NotImplementedException();
        }
    }
}

