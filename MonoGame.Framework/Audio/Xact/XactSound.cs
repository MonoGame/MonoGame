// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Audio
{
	class XactSound
	{
		private bool _complexSound;
        private XactClip[] _soundClips;

        private int _waveBankIndex;
        private int _trackIndex;
        private SoundEffectInstance _wave;

        private uint _categoryID;

        private SoundBank _soundBank;
		

        public XactSound(SoundBank soundBank, int waveBankIndex, int trackIndex)
        {
            _complexSound = false;

            _soundBank = soundBank;
            _waveBankIndex = waveBankIndex;
            _trackIndex = trackIndex;
        }

		public XactSound(SoundBank soundBank, BinaryReader soundReader, uint soundOffset)
		{
            _soundBank = soundBank;

			var oldPosition = soundReader.BaseStream.Position;
			soundReader.BaseStream.Seek (soundOffset, SeekOrigin.Begin);
			
			byte flags = soundReader.ReadByte ();
			_complexSound = (flags & 1) != 0;

            _categoryID = soundReader.ReadUInt16();
            var volume = XactHelpers.ParseVolumeFromDecibels(soundReader.ReadByte());
            var pitch = soundReader.ReadInt16() / 1000.0f;
			soundReader.ReadByte (); //unkn
            soundReader.ReadUInt16 (); // entryLength
			
			uint numClips = 0;
			if (_complexSound)
				numClips = (uint)soundReader.ReadByte ();
			else 
            {
				_trackIndex = soundReader.ReadUInt16 ();
				_waveBankIndex = soundReader.ReadByte ();
			}
			
			if ( (flags & 0x1E) != 0 ) 
            {
				uint extraDataLen = soundReader.ReadUInt16 ();
				//TODO: Parse RPC+DSP stuff
				
				// extraDataLen - 2, we need to account for extraDataLen itself!
				soundReader.BaseStream.Seek (extraDataLen - 2, SeekOrigin.Current);
			}
			
			if (_complexSound)
            {
				_soundClips = new XactClip[numClips];
				for (int i=0; i<numClips; i++) {
					soundReader.ReadByte (); //unkn
					uint clipOffset = soundReader.ReadUInt32 ();
					soundReader.ReadUInt32 (); //unkn
					
					_soundClips[i] = new XactClip(soundBank, soundReader, clipOffset);
				}
			}

            var category = soundBank.AudioEngine.Categories[_categoryID];
            category.AddSound(this);

			soundReader.BaseStream.Seek (oldPosition, SeekOrigin.Begin);
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
                    if(sound.Playing)
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
                    if (sound.IsPaused)
                        sound.Resume();
                }
			}
            else
            {
                if (_wave != null && _wave.State == SoundState.Paused)
                    _wave.Resume();
			}
		}

        // Used to set volume from an Audio category, so volume
        // scaling isn't applied twice.
        internal void SetVolumeInt(float newVol)
        {

            newVol = MathHelper.Clamp(newVol, 0, 1.0f);

            if (_complexSound)
            {
                foreach (XactClip clip in _soundClips)
                    clip.Volume = newVol;
            }
            else
            {
                if (_wave != null)
                    _wave.Volume = newVol;
            }
        }
		
		public float Volume 
        {
			get 
            {
				if (_complexSound)
					return _soundClips[0].Volume;
                else
					return _wave != null ? _wave.Volume : 0.0f;
			}

			set
            {
                var category = _soundBank.AudioEngine.Categories[_categoryID];
                value = MathHelper.Clamp(value * category._volume[0], 0, 1.0f);

                if (_complexSound)
                {
                    foreach (XactClip clip in _soundClips)
                        clip.Volume = value;
                }
                else
                {
                    if (_wave != null)
                        _wave.Volume = value;
                }
            }
		}
		
		public bool Playing 
        {
			get 
            {
				if (_complexSound)
                {
					foreach (XactClip clip in _soundClips)
						if (clip.Playing)
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
                    foreach (XactClip clip in _soundClips)
                        if (clip.Playing)
                            return false;

                    return true;
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
					foreach (XactClip clip in _soundClips)
						if (clip.IsPaused) 
                            return true;

					return false;
                }

                return _wave != null && _wave.State == SoundState.Paused;
			}
		}
		
	}
}

