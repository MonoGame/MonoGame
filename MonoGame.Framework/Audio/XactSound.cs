using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Audio
{
	internal class XactSound
	{
		bool complexSound;
		XactClip[] soundClips;
		SoundEffectInstance wave;

        uint _categoryID;

        AudioEngine _engine;
		
		public XactSound (SoundBank soundBank, BinaryReader soundReader, uint soundOffset)
		{
            _engine = soundBank.AudioEngine;

			long oldPosition = soundReader.BaseStream.Position;
			soundReader.BaseStream.Seek (soundOffset, SeekOrigin.Begin);
			
			byte flags = soundReader.ReadByte ();
			complexSound = (flags & 1) != 0;

            _categoryID = soundReader.ReadUInt16();
			soundReader.ReadByte (); //unkn
            soundReader.ReadUInt16 (); // volume, maybe pitch?
			soundReader.ReadByte (); //unkn
            soundReader.ReadUInt16 (); // entryLength
			
			uint numClips = 0;
			if (complexSound) {
				numClips = (uint)soundReader.ReadByte ();
			} else {
				uint trackIndex = soundReader.ReadUInt16 ();
				byte waveBankIndex = soundReader.ReadByte ();
				wave = soundBank.GetWave(waveBankIndex, trackIndex);
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
		
//		public XactSound (Sound sound) {
//			complexSound = false;
//			wave = sound;
//		}
		public XactSound (SoundEffectInstance sound)
        {
			complexSound = false;
			wave = sound;
		}

		public void Play()
        {
            var category = _engine.Categories[_categoryID];
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

			if (complexSound) {
				foreach (XactClip clip in soundClips) {
					clip.Play();
				}
			} else {
				if (wave.State == SoundState.Playing) wave.Stop ();
				wave.Play ();
			}
		}

        internal void Update(float dt)
        {
            if (complexSound)
            {
                foreach(var sound in soundClips)
                    sound.Update(dt);
            }
        }

        internal void StopAll(AudioStopOptions options)
        {
            if (complexSound)
            {
                foreach (XactClip clip in soundClips)
                {
                    clip.Stop();
                }
            }
            else
            {
                wave.Stop();
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
                wave.Stop();
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
				wave.Pause ();
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
				wave.Resume ();
			}
		}

        // Used to set volume from an Audio category, so volume
        // scaling isn't applied twice.
        internal void SetVolumeInt(float newVol)
        {
            if (complexSound)
            {
                foreach (XactClip clip in soundClips)
                    clip.Volume = newVol;
            }
            else
                wave.Volume = newVol;
        }
		
		public float Volume {
			get {
				if (complexSound)
                {
					return soundClips[0].Volume;
				} else {
					return wave.Volume;
				}
			}
			set
            {
                var category = _engine.Categories[_categoryID];

                value *= category._volume[0];

                if (complexSound)
                {
                    foreach (XactClip clip in soundClips)
                    {
                        clip.Volume = value;
                    }
                }
                else
                {
                    wave.Volume = value;
                }
            }
		}
		
		public bool Playing {
			get {
				if (complexSound)
                {
					foreach (XactClip clip in soundClips)
                    {
						if (clip.Playing)
                            return true;
					}
					return false;
				} else {
					return wave.State != SoundState.Stopped;
				}
			}
		}

		public bool IsPaused
		{
			get
			{
				if (complexSound) {
					foreach (XactClip clip in soundClips) {
						if (clip.IsPaused) return true;
					}
					return false;
				} else {
					return wave.State == SoundState.Paused;
				}
			}
		}
		
	}
}

