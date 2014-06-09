// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#region Using Statements
using System;
using System.IO;
using Sce.PlayStation.Core.Audio;
#endregion Statements

namespace Microsoft.Xna.Framework.Audio
{
    public partial class SoundEffectInstance : IDisposable
    {
        private SoundState soundState = SoundState.Stopped;
        
        internal Sound _audioBuffer;
        internal SoundPlayer _soundPlayer;
        
        private void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
        {            
            var audioData = AudioUtil.FormatWavData(buffer, sampleRate, channels);
            _audioBuffer = new Sound(audioData);
                
            _soundPlayer = _audioBuffer.CreatePlayer();
            _soundPlayer.Volume = SoundEffect.MasterVolume;
        }

        private void PlatformDispose(bool disposing)
        {
		    if (disposing)
            {
                if (_soundPlayer != null)
                {
                    _soundPlayer.Stop();
                    _soundPlayer.Dispose();
                }
                if (_audioBuffer != null)
                    _audioBuffer.Dispose();
            }
            _soundPlayer = null;
            _audioBuffer = null;
            soundState = SoundState.Stopped;
        }

        private void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
            // Looks like a no-op on PSM?
        }

        private void PlatformPause()
        {
            if (_soundPlayer != null)
                _soundPlayer.Stop();

            soundState = SoundState.Paused;
        }

        private void PlatformPlay()
        {
            if (_soundPlayer != null)
                _soundPlayer.Play();

            soundState = SoundState.Playing;
        }

        private void PlatformResume()
        {
            PlatformPlay();
        }

        private void PlatformStop(bool immediate)
        {
            if (_soundPlayer != null )
			{
				_soundPlayer.Stop();
			}
            
            soundState = SoundState.Stopped;
        }

        private void PlatformSetIsLooped(bool value)
        {
            if (_soundPlayer != null)
            {
                if (_soundPlayer.Loop != value)
                {
                    _soundPlayer.Loop = value;
                }
            }
        }

        private bool PlatformGetIsLooped()
        {
            if (_soundPlayer != null)
            {
                return _soundPlayer.Loop;
            }
            else
            {
                return false;
            }
        }

        private void PlatformSetPan(float value)
        {
            _pan = value;

            if (_soundPlayer != null)
            {
                if (_soundPlayer.Pan != value)
                {
                    _soundPlayer.Pan = value;
                }
            }
        }

        private void PlatformSetPitch(float value)
        {
            _pitch = value;

            if (_soundPlayer != null)
                _soundPlayer.PlaybackRate = value + 1.0f;
        }

        private SoundState PlatformGetState()
        {
            if (_soundPlayer != null)
            {
                if (soundState == SoundState.Playing && _soundPlayer.Status != SoundStatus.Playing)
                {
                    soundState = SoundState.Stopped;
                }
            }

            return soundState;
        }

        private void PlatformSetVolume(float value)
        {
            if (_soundPlayer == null)
                return;

            _volume = value;
            
            _soundPlayer.Volume = value * SoundEffect.MasterVolume;
        }
    }
}
