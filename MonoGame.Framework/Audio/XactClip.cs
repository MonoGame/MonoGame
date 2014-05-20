using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	internal class XactClip
	{
		float volume;
		
		abstract class ClipEvent
        {
            protected float _curTime;
            public float timeStamp;
		    public float randomOffset;

			public XactClip clip;
			
			public abstract void Play();
			public abstract void Stop();
			public abstract void Pause();
			public abstract void Resume();
            public abstract void Update(float dt);
            public abstract void SetFade(float fadeInDuration, float fadeOutDuration);
            public abstract bool IsReady { get; }
			public abstract bool Playing { get; }
			public abstract float Volume { get; set; }
			public abstract bool IsPaused { get; }
		}
		
		class EventPlayWave : ClipEvent
        {
			public SoundEffectInstance wave;
			public override void Play() {
				wave.Volume = clip.Volume;
				if (wave.State == SoundState.Playing) wave.Stop ();
				wave.Play ();
			}
			public override void Stop()
            {
				wave.Stop ();
                _curTime = 0.0f;
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
					return wave.State != SoundState.Stopped;
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

            public override void SetFade(float fadeInDuration, float fadeOutDuration)
            {
                // TODO
            }

            public override bool IsReady
            {
                get { return _curTime >= timeStamp; }
            }

            public override void Update(float dt)
            {
                if (IsReady)
                    return;

                _curTime += dt;

                if (!IsReady)
                    return;

                clip.Play();
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
				var eventInfo = clipReader.ReadUInt32();
                var randomOffset = clipReader.ReadUInt16() * 0.001f;

                // TODO: eventInfo still has 11 bits that are unknown!
				var eventId = eventInfo & 0x1F;
                var timeStamp = ((eventInfo >> 5) & 0xFFFF) * 0.001f;
                var unknown = eventInfo >> 21;

				switch (eventId) {
                case 0:
                    // Stop Event
                    throw new NotImplementedException();

				case 1:
					EventPlayWave evnt = new EventPlayWave();
					
					clipReader.ReadUInt16 (); //unkn
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

                case 7:
                    // Pitch Event
                    throw new NotImplementedException();

                case 8:
                    // Volume Event
                    throw new NotImplementedException();

                case 9:
                    // Marker Event
                    throw new NotImplementedException();

				default:
					throw new NotSupportedException();
				}

                events[i].timeStamp = timeStamp;
                events[i].randomOffset = randomOffset;
				events[i].clip = this;
			}
			
			clipReader.BaseStream.Seek (oldPosition, SeekOrigin.Begin);
		}

        internal void Update(float dt)
        {
            foreach (var evt in events)
                evt.Update(dt);
        }

        internal void SetFade(float fadeInDuration, float fadeOutDuration)
        {
            foreach(var evt in events)
                (evt as EventPlayWave).SetFade(fadeInDuration, fadeOutDuration);
        }
		
		public void Play()
        {
			//TODO: run events
            foreach (var evt in events)
            {
                if (evt.IsReady)
                    evt.Play();
            }
		}

		public void Resume()
		{
            foreach (var evt in events)
            {
                if (evt.IsReady)
                    evt.Resume();
            }
		}
		
		public void Stop()
        {
            foreach (var evt in events)
                evt.Stop();
		}
		
		public void Pause()
        {
            foreach (var evt in events)
                evt.Pause();
		}
		
		public bool Playing
        {
			get
            {
                foreach (var evt in events)
                {
                    if (evt.Playing)
                        return true;
                }

				return false;
			}
		}
		
		public float Volume
        {
			get {
				return volume;
			}
			set {
				volume = value;
                foreach(var evt in events)
				    evt.Volume = value;
			}
		}

		public bool IsPaused
        { 
			get
            {
                foreach (var evt in events)
                {
                    if (evt.IsPaused)
                        return true;
                }

				return false; 
			} 
		}
	}
}

