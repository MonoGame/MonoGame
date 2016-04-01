// MonoGame - Copyright (C) The MonoGame Team
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

        private void PlatformLoadAudioStream(Stream s)
        {
        }

        private void PlatformInitialize(byte[] buffer, int sampleRate, AudioChannels channels)
        {
        }
        
        private void PlatformInitialize(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
        }
        
        private void PlatformSetupInstance(SoundEffectInstance instance)
        {
        }

        private void PlatformDispose(bool disposing)
        {
        }

        internal static void PlatformShutdown()
        {
        }
    }
}

