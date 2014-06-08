// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	internal class XactClip
	{
		float volume;

        enum VariationType
        {
            Ordered,
            OrderedFromRandom,
            Random,
            RandomNoImmediateRepeats,
            Shuffle
        };
        
		abstract class ClipEvent
        {
            protected float _curTime;
            public float _timeStamp;
		    public float _randomOffset;

			public XactClip _clip;
			
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
            private SoundBank _soundBank;

            private VariationType _variation;

            private bool _isLooped;

            private float _volume;

            private int [] _tracks;
            private int [] _waveBanks;

            private int _wavIndex;

            private SoundEffectInstance _wav;

            internal EventPlayWave(SoundBank soundBank, int[] waveBanks, int[] tracks, VariationType variation, bool isLooped)
            {
                _soundBank = soundBank;
                _waveBanks = waveBanks;
                _tracks = tracks;
                _wavIndex = 0;
                _volume = 1.0f;
                _variation = variation;
                _isLooped = isLooped;
            }

			public override void Play() 
            {
                if (_wav != null && _wav.State != SoundState.Stopped)
                    _wav.Stop();

                var trackCount = _tracks.Length;
                Console.WriteLine("_variation=" + _variation);

                switch (_variation)
                {
                    case VariationType.Ordered:
                        _wavIndex = (_wavIndex + 1) % trackCount;
                        break;

                    case VariationType.OrderedFromRandom:
                        _wavIndex = (_wavIndex + 1) % trackCount;
                        break;

                    case VariationType.Random:
                        _wavIndex = AudioEngine.Random.Next() % trackCount;
                        break;

                    case VariationType.RandomNoImmediateRepeats:
                    {
                        var last = _wavIndex;
                        do
                        {
                            _wavIndex = AudioEngine.Random.Next() % trackCount;
                        }
                        while (last == _wavIndex && trackCount > 1);
                        break;
                    }

                    case VariationType.Shuffle:
                        // TODO: Need some sort of deck implementation.
                        _wavIndex = AudioEngine.Random.Next() % trackCount;
                        break;
                };

                _wav = _soundBank.GetWave(_waveBanks[_wavIndex], _tracks[_wavIndex]);
               
                _wav.Volume = _volume;
                _wav.IsLooped = _isLooped && trackCount == 1;
                _wav.Play();
			}

			public override void Stop()
            {
                if (_wav != null)
                {
                    _wav.Stop();
                    _wav = null;
                }

                _curTime = 0.0f;
			}

			public override void Pause() 
            {
                if (_wav != null)
                    _wav.Pause();
			}

			public override void Resume()
			{
                if (_wav != null && _wav.State == SoundState.Paused)
                    _wav.Resume();
			}

			public override bool Playing 
            {
				get 
                {
                    return _wav != null && _wav.State == SoundState.Playing;
				}
			}

			public override bool IsPaused
			{
				get
				{
                    return _wav != null && _wav.State == SoundState.Paused;
				}
			}

			public override float Volume 
            {
				get 
                {
                    return _volume;
				}

				set 
                {
                    _volume = value;
                    if (_wav != null)
                        _wav.Volume = value;
				}
			}

            public override void SetFade(float fadeInDuration, float fadeOutDuration)
            {
                // TODO
            }

            public override bool IsReady
            {
                get { return _curTime >= _timeStamp; }
            }

            public override void Update(float dt)
            {
                if (_wav != null)
                {
                    if (_wav.State == SoundState.Stopped)
                    {
                        _wav = null;

                        if (_isLooped && _tracks.Length > 1)
                            Play();
                    }
                }

                if (IsReady)
                    return;

                _curTime += dt;

                if (!IsReady)
                    return;

                _clip.Play();
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
                    throw new NotImplementedException("Stop event");

				case 1:
                {
					clipReader.ReadUInt16 (); //unkn
					int trackIndex = clipReader.ReadUInt16 ();
                    int waveBankIndex = clipReader.ReadByte();
					
					var loopCount = clipReader.ReadByte ();
				    // if loopCount == 255 its an infinite loop
					// otherwise it loops n times..
				    // unknown
					clipReader.ReadUInt16 ();
					clipReader.ReadUInt16 ();

                    events[i] = new EventPlayWave(soundBank, new[] { waveBankIndex }, new[] { trackIndex }, VariationType.Ordered, loopCount == 255); 
					break;
                }

                case 3:
                {
                    // Unknown value
                    clipReader.ReadByte();

                    /* Event Flags
                     * 0x01 = Break Loop
                     * 0x02 = Use Speaker Position
                     * 0x04 = Use Center Speaker
                     * 0x08 = New Speaker Position On Loop
                     */
                    clipReader.ReadByte();

                    // Unknown values
                    clipReader.ReadBytes(5);

                    // Number of WaveBank tracks
                    var numTracks = clipReader.ReadUInt16();

                    // Variation Playlist Type
                    var variationType = clipReader.ReadUInt16() & 0x000F;

                    // Unknown values
                    clipReader.ReadBytes(4);

                    var waveBanks = new int[numTracks];
                    var tracks = new int[numTracks];
                    var weights = new byte[numTracks];
                    for (ushort j = 0; j < numTracks; j += 1)
                    {
                        tracks[j] = clipReader.ReadUInt16();
                        waveBanks[j] = clipReader.ReadByte();

                        byte minWeight = clipReader.ReadByte();
                        byte maxWeight = clipReader.ReadByte();
                        weights[j] = (byte)(maxWeight - minWeight);
                    }

                    // Finally.
                    events[i] = new EventPlayWave(soundBank, waveBanks, tracks, (VariationType)variationType, false);
                    break;
                }

                case 7:
                    // Pitch Event
                    throw new NotImplementedException("Pitch event");

                case 8:
                    // Volume Event
                    throw new NotImplementedException("Volume event");

                case 9:
                    // Marker Event
                    throw new NotImplementedException("Marker event");

				default:
                    throw new NotSupportedException("Unknown event " + eventId);
				}

                events[i]._timeStamp = timeStamp;
                events[i]._randomOffset = randomOffset;
				events[i]._clip = this;
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

