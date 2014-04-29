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

using Sce.PlayStation.Core.Audio;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        private Sound _audioBuffer;
        private SoundEffectInstance _instance;
        
        private void PlatformInitialize(byte[] buffer, int sampleRate, AudioChannels channels)
        {
            _name = "";
            
            _audioBuffer = new Sound(AudioUtil.FormatWavData(buffer, sampleRate, (int)channels));
        }
        
        private void PlatformLoadAudioStream(Stream s)
        {
            var data = new byte[s.Length];
            s.Read(data, 0, (int)s.Length);
            
            _audioBuffer = new Sound(data);
        }

        private void PlatformInitialize(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            throw new NotImplementedException();
        }

        private void PlatformSetupInstance(SoundEffectInstance inst)
        {   
            inst._audioBuffer = _audioBuffer;
            inst._soundPlayer = _audioBuffer.CreatePlayer();
        }

        private static void PlatformSetMasterVolume()
        {
            var activeSounds = SoundEffectInstancePool.GetAllPlayingSounds();

            // A little gross here, but there's
            // no if(value == value) check in SFXInstance.Volume
            // This'll allow the sound's current volume to be recalculated
            // against SoundEffect.MasterVolume.
            foreach (var sound in activeSounds)
                sound.Volume = sound.Volume;
        }

        private void PlatformDispose()
        {
            _audioBuffer.Dispose();
            
            isDisposed = true;
        }

        internal static void PlatformShutdown()
        {
        }
    }
}

