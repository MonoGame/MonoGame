using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	internal class XactClip
	{
		float volume;
		
		abstract class ClipEvent {
			public XactClip clip;
			
			public abstract void Play();
			public abstract void Stop();
			public abstract void Pause();
			public abstract void Resume();
			public abstract bool Playing { get; }
			public abstract float Volume { get; set; }
			public abstract bool IsPaused { get; }
		}
		
		class EventPlayWave : ClipEvent {
			public SoundEffectInstance wave;
			public override void Play() {
				wave.Volume = clip.Volume;
				if (wave.State == SoundState.Playing) wave.Stop ();
				wave.Play ();
			}
			public override void Stop() {
				wave.Stop ();
			}
			public override void Pause() {
				wave.Pause ();
			}
			public override void Resume()
			{
				wave.Volume = clip.Volume;
				if (wave.State == SoundState.Paused)
					wave.Resume();
			}
			public override bool Playing {
				get {
					return wave.State == SoundState.Playing;
				}
			}
			public override bool IsPaused
			{
				get
				{
					return wave.State == SoundState.Paused;
				}
			}
			public override float Volume {
				get {
					return wave.Volume;
				}
				set {
					wave.Volume = value;
				}
			}
		}
		
		ClipEvent[] events;
		
		public XactClip (SoundBank soundBank, BinaryReader clipReader, uint clipOffset)
		{
			long oldPosition = clipReader.BaseStream.Position;
			clipReader.BaseStream.Seek (clipOffset, SeekOrigin.Begin);
			
			byte numEvents = clipReader.ReadByte();
			events = new ClipEvent[numEvents];
			
			for (int i=0; i<numEvents; i++) {
				uint eventInfo = clipReader.ReadUInt32();
				
				uint eventId = eventInfo & 0x1F;
				switch (eventId) {
				case 1:
					EventPlayWave evnt = new EventPlayWave();
					
					
					clipReader.ReadUInt32 (); //unkn
					uint trackIndex = clipReader.ReadUInt16 ();
					byte waveBankIndex = clipReader.ReadByte ();
					
					
					var loopCount = clipReader.ReadByte ();
				    // if loopCount == 255 its an infinite loop
					// otherwise it loops n times..
				    // unknown
					clipReader.ReadUInt16 ();
					clipReader.ReadUInt16 ();
					
					evnt.wave = soundBank.GetWave(waveBankIndex, trackIndex);
					evnt.wave.IsLooped = loopCount == 255;
					
					events[i] = evnt;
					break;
				default:
					throw new NotImplementedException();
				}
				
				events[i].clip = this;
			}
			
			
			clipReader.BaseStream.Seek (oldPosition, SeekOrigin.Begin);
		}
		
		public void Play() {
			//TODO: run events
			events[0].Play ();
		}

		public void Resume()
		{
			events[0].Resume();
		}
		
		public void Stop() {
			events[0].Stop ();
		}
		
		public void Pause() {
			events[0].Pause();
		}
		
		public bool Playing {
			get {
				return events[0].Playing;
			}
		}
		
		public float Volume {
			get {
				return volume;
			}
			set {
				volume = value;
				events[0].Volume = value;
			}
		}


		public bool IsPaused { 
			get { 
				return events[0].IsPaused; 
			} 
		}
	}
}

