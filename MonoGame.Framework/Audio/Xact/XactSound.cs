// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	class XactSound
	{
		private readonly bool _complexSound;
        private readonly XactClip[] _soundClips;
        private readonly int _waveBankIndex;
        private readonly int _trackIndex;
        private readonly float _volume;
        private readonly float _pitch;
        private readonly uint _categoryID;
        private readonly SoundBank _soundBank;

        private SoundEffectInstance _wave;

        private float _cueVolume;

        public XactSound(SoundBank soundBank, int waveBankIndex, int trackIndex)
        {
            _complexSound = false;

            _soundBank = soundBank;
            _waveBankIndex = waveBankIndex;
            _trackIndex = trackIndex;
        }

		public XactSound(SoundBank soundBank, BinaryReader soundReader)
		{
            _soundBank = soundBank;
			
            var flags = soundReader.ReadByte();
            _complexSound = (flags & 0x1) != 0;
            var hasRPCs = (flags & 0x0E) != 0;
            var hasEffects = (flags & 0x10) != 0;

            _categoryID = soundReader.ReadUInt16();
            _volume = XactHelpers.ParseVolumeFromDecibels(soundReader.ReadByte());
            _pitch = soundReader.ReadInt16() / 1000.0f;
			soundReader.ReadByte(); //priority
            soundReader.ReadUInt16(); // filter stuff?
			
			uint numClips = 0;
			if (_complexSound)
				numClips = soundReader.ReadByte();
			else 
            {
				_trackIndex = soundReader.ReadUInt16();
				_waveBankIndex = soundReader.ReadByte();
			}

			if (hasRPCs)
			{
				var current = soundReader.BaseStream.Position;
				var dataLength = soundReader.ReadUInt16();
				soundReader.BaseStream.Seek(current + dataLength, SeekOrigin.Begin);
			}

			if (hasEffects)
			{
				var current = soundReader.BaseStream.Position;
				var dataLength = soundReader.ReadUInt16();
				soundReader.BaseStream.Seek(current + dataLength, SeekOrigin.Begin);
			}

			if (_complexSound)
            {
				_soundClips = new XactClip[numClips];
				for (int i=0; i<numClips; i++) 
					_soundClips[i] = new XactClip(soundBank, soundReader);
			}

            var category = soundBank.AudioEngine.Categories[_categoryID];
            category.AddSound(this);
		}

        internal void SetFade(float fadeInTime, float fadeOutTime)
        {
            if (fadeInTime == 0.0f &&
                fadeOutTime == 0.0f )
                return;

            if (_complexSound)
            {
                foreach (var sound in _soundClips)
                    sound.SetFade(fadeInTime, fadeOutTime);
            }
            else
            {
                // TODO:
            }
        }

		public void Play()
        {
            var category = _soundBank.AudioEngine.Categories[_categoryID];
            var curInstances = category.GetPlayingInstanceCount();
            if (curInstances >= category.maxInstances)
            {
                var prevSound = category.GetOldestInstance();

                if (prevSound != null)
                {
                    prevSound.SetFade(0.0f, category.fadeOut);
                    prevSound.Stop(AudioStopOptions.Immediate);
                    SetFade(category.fadeIn, 0.0f);
                }
            }

			if (_complexSound) 
            {
				foreach (XactClip clip in _soundClips)
					clip.Play();
			} 
            else 
            {
                if (_wave != null && _wave.State != SoundState.Stopped && _wave.IsLooped)
                    _wave.Stop();
                else
                    _wave = _soundBank.GetSoundEffectInstance(_waveBankIndex, _trackIndex);

                if (_wave == null)
                {
                    // We couldn't create a sound effect instance, most likely
                    // because we've reached the sound pool limits.
                    return;
                }

                _wave.Pitch = _pitch;
                _wave.Volume = _volume * category._volume[0];
                _wave.Play();
			}
		}

        internal void Update(float dt)
        {
            if (_complexSound)
            {
                foreach (var sound in _soundClips)
                    sound.Update(dt);
            }
            else
            {
                if (_wave != null && _wave.State == SoundState.Stopped)
                    _wave = null;
            }
        }

        internal void StopAll(AudioStopOptions options)
        {
            if (_complexSound)
            {
                foreach (XactClip clip in _soundClips)
                    clip.Stop();
            }
            else
            {
                if (_wave != null)
                {
                    _wave.Stop();
                    _wave = null;
                }
            }
        }
		
		public void Stop(AudioStopOptions options)
        {
            if (_complexSound)
            {
                foreach (var sound in _soundClips)
                    sound.Stop();
            }
            else
            {
                if (_wave != null)
                {
                    _wave.Stop();
                    _wave = null;
                }
            }
		}
		
		public void Pause()
        {
			if (_complexSound)
            {
                foreach (var sound in _soundClips)
                {
                    if (sound.State == SoundState.Playing)
                        sound.Pause();
                }
			}
            else
            {
                if (_wave != null && _wave.State == SoundState.Playing)
                    _wave.Pause();
			}
		}
                
		public void Resume()
        {
			if (_complexSound)
            {
                foreach (var sound in _soundClips)
                {
                    if (sound.State == SoundState.Paused)
                        sound.Resume();
                }
			}
            else
            {
                if (_wave != null && _wave.State == SoundState.Paused)
                    _wave.Resume();
			}
		}

        internal void UpdateCategoryVolume(float categoryVolume)
        {
            // The different volumes modulate each other.
            var volume = _volume * _cueVolume * categoryVolume;

            if (_complexSound)
            {
                foreach (var clip in _soundClips)
                    clip.SetVolumeScale(volume);
            }
            else
            {
                if (_wave != null)
                    _wave.Volume = volume;
            }
        }

        internal void SetCueVolume(float volume)
		{
            _cueVolume = volume;
            var category = _soundBank.AudioEngine.Categories[_categoryID];
            UpdateCategoryVolume(category._volume[0]);
        }

		public bool Playing 
        {
			get 
            {
				if (_complexSound)
                {
                    foreach (var clip in _soundClips)
                        if (clip.State == SoundState.Playing)
                            return true;

                    return false;
				} 

                return _wave != null && _wave.State == SoundState.Playing;
			}
		}

        public bool Stopped
        {
            get
            {
                if (_complexSound)
                {
                    foreach (var clip in _soundClips)
                        if (clip.State == SoundState.Stopped)
                            return true;

                    return false;
                }

                return _wave == null || _wave.State == SoundState.Stopped;
            }
        }

		public bool IsPaused
		{
			get
			{
				if (_complexSound) 
                {
					foreach (var clip in _soundClips)
						if (clip.State == SoundState.Paused) 
                            return true;

					return false;
                }

                return _wave != null && _wave.State == SoundState.Paused;
			}
		}

        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            if (_complexSound)
            {
                foreach (var clip in _soundClips)
                    clip.Apply3D(listener, emitter);
            }
            else
            {
                if (_wave != null)
                    _wave.Apply3D(listener, emitter);
            }
        }
    }
}

