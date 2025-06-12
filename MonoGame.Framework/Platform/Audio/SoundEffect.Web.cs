// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        // This platform is only limited by memory.
        internal const int MAX_PLAYING_INSTANCES = int.MaxValue;

        private void PlatformLoadAudioStream(Stream s, out TimeSpan duration)
        {
            duration = TimeSpan.Zero;
        }

        private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
        }

        private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
        {
        }

        private void PlatformInitializeXact(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength, out TimeSpan duration)
        {
            throw new NotSupportedException("Unsupported sound format!");
        }

        private void PlatformSetupInstance(SoundEffectInstance instance)
        {
        }

        private void PlatformDispose(bool disposing)
        {
        }

        internal static void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
        }

        internal static void PlatformInitialize()
        {
        }

        internal static void PlatformShutdown()
        {
        }
    }
}

