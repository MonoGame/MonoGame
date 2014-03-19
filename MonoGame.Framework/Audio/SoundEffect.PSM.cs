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

        private void PlatformInitializeInstance(SoundEffectInstance instance)
        {
            instance._audioBuffer = _audioBuffer;
            instance._soundPlayer = _audioBuffer.CreatePlayer();
        }
        
        private SoundEffectInstance PlatformCreateInstance()
        {
            var inst = new SoundEffectInstance();
            
            inst._audioBuffer = _audioBuffer;
            inst._soundPlayer = _audioBuffer.CreatePlayer();
            
            return inst;
        }

        private bool PlatformPlay()
        {
            return Play(1.0f, 0.0f, 0.0f);
        }

        private bool PlatformPlay(float volume, float pitch, float pan)
        {
            // TODO: While merging the SoundEffect classes together
            // I noticed that the return values seem to widly differ
            // between platforms. It also doesn't seem to match
            // what's written in the XNA docs.

            if ( MasterVolume > 0.0f )
            {
                if(_instance == null)
                    _instance = CreateInstance();
                
                _instance.Volume = volume;
                _instance.Pitch = pitch;
                _instance.Pan = pan;
                _instance.Play();
                
                return _instance._soundPlayer.Status == SoundStatus.Playing;
            }
            return false;
        }

        private TimeSpan PlatformGetDuration()
        {
            return _duration;
        }

        private static void PlatformSetMasterVolume()
        {
            // Appears to be a no-op on PSM?
        }

        private void PlatformDispose()
        {
            _audioBuffer.Dispose();
            
            isDisposed = true;

        }
    }
}

