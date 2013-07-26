using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	internal class XactSound
	{
		bool complexSound;
		XactClip[] soundClips;
		SoundEffectInstance wave;
		
		public XactSound (SoundBank soundBank, BinaryReader soundReader, uint soundOffset)
		{
			long oldPosition = soundReader.BaseStream.Position;
			soundReader.BaseStream.Seek (soundOffset, SeekOrigin.Begin);
			
			byte flags = soundReader.ReadByte ();
			complexSound = (flags & 1) != 0;
			
			uint category = soundReader.ReadUInt16 ();
			soundReader.ReadByte (); //unkn
			uint volume = soundReader.ReadUInt16 (); //maybe pitch?
			soundReader.ReadByte (); //unkn
			uint entryLength = soundReader.ReadUInt16 ();
			
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
			
			if (complexSound) {
				soundClips = new XactClip[numClips];
				for (int i=0; i<numClips; i++) {
					soundReader.ReadByte (); //unkn
					uint clipOffset = soundReader.ReadUInt32 ();
					soundReader.ReadUInt32 (); //unkn
					
					soundClips[i] = new XactClip(soundBank, soundReader, clipOffset);
				}
			}

			var audioCategory = soundBank.AudioEngine.Categories[category];
			audioCategory.AddSound(this);

			soundReader.BaseStream.Seek (oldPosition, SeekOrigin.Begin);
		}
		
//		public XactSound (Sound sound) {
//			complexSound = false;
//			wave = sound;
//		}
		public XactSound (SoundEffectInstance sound) {
			complexSound = false;
			wave = sound;
		}		
		public void Play() {
			if (complexSound) {
				foreach (XactClip clip in soundClips) {
					clip.Play();
				}
			} else {
				if (wave.State == SoundState.Playing) wave.Stop ();
				wave.Play ();
			}
		}
		
		public void Stop() {
			if (complexSound) {
				foreach (XactClip clip in soundClips) {
					clip.Stop();
				}
			} else {
				wave.Stop ();
			}
		}
		
		public void Pause() {
			if (complexSound) {
				foreach (XactClip clip in soundClips) {
					clip.Pause();
				}
			} else {
				wave.Pause ();
			}
		}
                
		public void Resume() {
			if (complexSound) {
				foreach (XactClip clip in soundClips) {
					clip.Resume();
				}
			} else {
				wave.Resume ();
			}
		}
		
		public float Volume {
			get {
				if (complexSound) {
					return soundClips[0].Volume;
				} else {
					return wave.Volume;
				}
			}
			set {
				if (complexSound) {
					foreach (XactClip clip in soundClips) {
						clip.Volume = value;
					}
				} else {
					wave.Volume = value;
				}
			}
		}
		
		public bool Playing {
			get {
				if (complexSound) {
					foreach (XactClip clip in soundClips) {
						if (clip.Playing) return true;
					}
					return false;
				} else {
					return wave.State == SoundState.Playing;
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

