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
		bool complexSound;
		XactClip[] soundClips;

        int _waveBankIndex;
        int _trackIndex;
        SoundEffectInstance _wave;

        uint _categoryID;

        SoundBank _soundBank;
		
		internal static float ParseDecibel(byte binaryValue)
		{
			/* FIXME: This calculation probably came from someone's TI-83.
			    * I plotted out Codename Naddachance's bytes out, and
			    * the closest formula I could come up with (hastily)
			    * was this:
			    * dBValue = 37.5 * Math.Log10(binaryValue * 2.0) - 96.0
			    * But of course, volumes are still wrong. So I dunno.
			    * -flibit
			    */
			var decibles = (float)((
				(-96.0 - 67.7385212334047) /
				(1 + Math.Pow(
					binaryValue / 80.1748600297963,
					0.432254984608615
				))
			) + 67.7385212334047);

            return (float)Math.Pow(10, decibles / 20.0);
		}


        public XactSound(SoundBank soundBank, int waveBankIndex, int trackIndex)
        {
            complexSound = false;

            _soundBank = soundBank;
            _waveBankIndex = waveBankIndex;
            _trackIndex = trackIndex;
        }

		public XactSound(SoundBank soundBank, BinaryReader soundReader, uint soundOffset)
		{
            _soundBank = soundBank;

			long oldPosition = soundReader.BaseStream.Position;
			soundReader.BaseStream.Seek (soundOffset, SeekOrigin.Begin);
			
			byte flags = soundReader.ReadByte ();
			complexSound = (flags & 1) != 0;

            _categoryID = soundReader.ReadUInt16();
            var volume = ParseDecibel(soundReader.ReadByte());
            var pitch = soundReader.ReadInt16() / 1000.0f;
			soundReader.ReadByte (); //unkn
            soundReader.ReadUInt16 (); // entryLength
			
			uint numClips = 0;
			if (complexSound) {
				numClips = (uint)soundReader.ReadByte ();
			} else {
				_trackIndex = soundReader.ReadUInt16 ();
				_waveBankIndex = soundReader.ReadByte ();
			}
			
			if ( (flags & 0x1E) != 0 ) {
				uint extraDataLen = soundReader.ReadUInt16 ();
				//TODO: Parse RPC+DSP stuff
				
				// extraDataLen - 2, we need to account for extraDataLen itself!
				soundReader.BaseStream.Seek (extraDataLen - 2, SeekOrigin.Current);
			}
			
			if (complexSound){
				soundClips = new XactClip[numClips];
				for (int i=0; i<numClips; i++) {
					soundReader.ReadByte (); //unkn
					uint clipOffset = soundReader.ReadUInt32 ();
					soundReader.ReadUInt32 (); //unkn
					
					soundClips[i] = new XactClip(soundBank, soundReader, clipOffset);
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

            if (complexSound)
            {
                foreach (var sound in soundClips)
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

			if (complexSound) 
            {
				foreach (XactClip clip in soundClips)
					clip.Play();
			} 
            else 
            {
                if (_wave != null && _wave.State != SoundState.Stopped && _wave.IsLooped)
                    _wave.Stop();
                else
                    _wave = _soundBank.GetWave(_waveBankIndex, _trackIndex);

                _wave.Play();
			}
		}

        internal void Update(float dt)
        {
            if (complexSound)
            {
                foreach (var sound in soundClips)
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
            if (complexSound)
            {
                foreach (XactClip clip in soundClips)
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
            if (complexSound)
            {
                foreach (var sound in soundClips)
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
			if (complexSound)
            {
                foreach (var sound in soundClips)
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
			if (complexSound)
            {
                foreach (var sound in soundClips)
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

            if (complexSound)
            {
                foreach (XactClip clip in soundClips)
                    clip.Volume = newVol;
            }
            else
            {
                if (_wave != null)
                    _wave.Volume = newVol;
            }
        }
		
		public float Volume {
			get 
            {
				if (complexSound)
					return soundClips[0].Volume;
                else
					return _wave != null ? _wave.Volume : 0.0f;
			}

			set
            {
                var category = _soundBank.AudioEngine.Categories[_categoryID];
                value = MathHelper.Clamp(value * category._volume[0], 0, 1.0f);

                if (complexSound)
                {
                    foreach (XactClip clip in soundClips)
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
				if (complexSound)
                {
					foreach (XactClip clip in soundClips)
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
                if (complexSound)
                {
                    foreach (XactClip clip in soundClips)
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
				if (complexSound) 
                {
					foreach (XactClip clip in soundClips)
						if (clip.IsPaused) 
                            return true;

					return false;
                }

                return _wave != null && _wave.State == SoundState.Paused;
			}
		}
		
	}
}

