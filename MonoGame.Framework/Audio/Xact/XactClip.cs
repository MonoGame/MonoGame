// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	class XactClip
	{
        private float _volume;

		private ClipEvent[] _events;
		
		public XactClip (SoundBank soundBank, BinaryReader clipReader)
		{
            _volume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
            var clipOffset = clipReader.ReadUInt32();

            // Unknown!
            clipReader.ReadUInt32();

			var oldPosition = clipReader.BaseStream.Position;
			clipReader.BaseStream.Seek(clipOffset, SeekOrigin.Begin);
			
			var numEvents = clipReader.ReadByte();
			_events = new ClipEvent[numEvents];
			
			for (int i=0; i<numEvents; i++) 
            {
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
                    // Unknown!
					clipReader.ReadUInt16();

					int trackIndex = clipReader.ReadUInt16();
                    int waveBankIndex = clipReader.ReadByte();
					
					var loopCount = clipReader.ReadByte();
				    // if loopCount == 255 its an infinite loop
					// otherwise it loops n times..
				    
                    // Unknown!
					clipReader.ReadUInt16();
					clipReader.ReadUInt16();

                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp, 
                        randomOffset,
                        soundBank, 
                        new[] { waveBankIndex }, 
                        new[] { trackIndex }, 
                        VariationType.Ordered, 
                        loopCount,
                        false);

					break;
                }

                case 3:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var playRelease = (eventFlags & 0x01) == 0x01;

                    var loopCount = clipReader.ReadByte();

                    // Unknown!
                    clipReader.ReadBytes(4);

                    // The number of tracks for the variations.
                    int numTracks = clipReader.ReadUInt16();

                    // Not sure what most of this is.
                    var moreFlags = clipReader.ReadByte();
                    var newWaveOnLoop = (moreFlags & 0x40) == 0x40;
                    
                    // The variation playlist type seems to be 
                    // stored in the bottom 4bits only.
                    var variationType = moreFlags & 0x0F;

                    // Unknown!
                    clipReader.ReadBytes(5);

                    // Read in the variation playlist.
                    var waveBanks = new int[numTracks];
                    var tracks = new int[numTracks];
                    var weights = new byte[numTracks];
                    for (var j = 0; j < numTracks; j++)
                    {
                        tracks[j] = clipReader.ReadUInt16();
                        waveBanks[j] = clipReader.ReadByte();

                        var minWeight = clipReader.ReadByte();
                        var maxWeight = clipReader.ReadByte();
                        weights[j] = (byte)(maxWeight - minWeight);
                    }

                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank, 
                        waveBanks, 
                        tracks, 
                        (VariationType)variationType,
                        loopCount,
                        newWaveOnLoop);

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
			}
			
			clipReader.BaseStream.Seek (oldPosition, SeekOrigin.Begin);
		}

        internal void Update(float dt)
        {
            foreach (var evt in _events)
                evt.Update(dt);
        }

        internal void SetFade(float fadeInDuration, float fadeOutDuration)
        {
            foreach(var evt in _events)
                (evt as PlayWaveEvent).SetFade(fadeInDuration, fadeOutDuration);
        }
		
		public void Play()
        {
			//TODO: run events
            foreach (var evt in _events)
            {
                if (evt.IsReady)
                    evt.Play();
            }
		}

		public void Resume()
		{
            foreach (var evt in _events)
            {
                if (evt.IsReady)
                    evt.Resume();
            }
		}
		
		public void Stop()
        {
            foreach (var evt in _events)
                evt.Stop();
		}
		
		public void Pause()
        {
            foreach (var evt in _events)
                evt.Pause();
		}
		
		public bool Playing
        {
			get
            {
                foreach (var evt in _events)
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
				return _volume;
			}
			set {
				_volume = value;
                foreach(var evt in _events)
				    evt.Volume = value;
			}
		}

		public bool IsPaused
        { 
			get
            {
                foreach (var evt in _events)
                {
                    if (evt.IsPaused)
                        return true;
                }

				return false; 
			} 
		}
	}
}

