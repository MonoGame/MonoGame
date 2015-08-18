// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	class XactClip
	{
        private readonly float _defaultVolume;
        private float _volumeScale;
        private float _volume;

		private readonly ClipEvent[] _events;
        private float _time;
        private int _nextEvent;

		public XactClip (SoundBank soundBank, BinaryReader clipReader)
		{
#pragma warning disable 0219
            State = SoundState.Stopped;

		    var volumeDb = XactHelpers.ParseDecibels(clipReader.ReadByte());
            _defaultVolume = XactHelpers.ParseVolumeFromDecibels(volumeDb);
            var clipOffset = clipReader.ReadUInt32();

            // Unknown!
            clipReader.ReadUInt32();

			var oldPosition = clipReader.BaseStream.Position;
			clipReader.BaseStream.Seek(clipOffset, SeekOrigin.Begin);
			
			var numEvents = clipReader.ReadByte();
			_events = new ClipEvent[numEvents];
			
			for (var i=0; i<numEvents; i++) 
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
                    clipReader.ReadByte();

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var playRelease = (eventFlags & 0x01) == 0x01;
                    var panEnabled = (eventFlags & 0x02) == 0x02;
                    var useCenterSpeaker = (eventFlags & 0x04) == 0x04;

					int trackIndex = clipReader.ReadUInt16();
                    int waveBankIndex = clipReader.ReadByte();					
					var loopCount = clipReader.ReadByte();
                    var panAngle = clipReader.ReadUInt16() / 100.0f;
                    var panArc = clipReader.ReadUInt16() / 100.0f;
                    
                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp, 
                        randomOffset,
                        soundBank, 
                        new[] { waveBankIndex }, 
                        new[] { trackIndex },
                        null,
                        0,
                        VariationType.Ordered, 
                        null,
                        null,
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
                    var panEnabled = (eventFlags & 0x02) == 0x02;
                    var useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    var loopCount = clipReader.ReadByte();
                    var panAngle = clipReader.ReadUInt16() / 100.0f;
                    var panArc = clipReader.ReadUInt16() / 100.0f;

                    // The number of tracks for the variations.
                    var numTracks = clipReader.ReadUInt16();

                    // Not sure what most of this is.
                    var moreFlags = clipReader.ReadByte();
                    var newWaveOnLoop = (moreFlags & 0x40) == 0x40;
                    
                    // The variation playlist type seems to be 
                    // stored in the bottom 4bits only.
                    var variationType = (VariationType)(moreFlags & 0x0F);

                    // Unknown!
                    clipReader.ReadBytes(5);

                    // Read in the variation playlist.
                    var waveBanks = new int[numTracks];
                    var tracks = new int[numTracks];
                    var weights = new byte[numTracks];
                    var totalWeights = 0;
                    for (var j = 0; j < numTracks; j++)
                    {
                        tracks[j] = clipReader.ReadUInt16();
                        waveBanks[j] = clipReader.ReadByte();
                        var minWeight = clipReader.ReadByte();
                        var maxWeight = clipReader.ReadByte();
                        weights[j] = (byte)(maxWeight - minWeight);
                        totalWeights += weights[j];
                    }

                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank, 
                        waveBanks, 
                        tracks,
                        weights,
                        totalWeights,
                        variationType,
                        null,
                        null,
                        loopCount,
                        newWaveOnLoop);

                    break;
                }

                case 4:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var playRelease = (eventFlags & 0x01) == 0x01;
                    var panEnabled = (eventFlags & 0x02) == 0x02;
                    var useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    int trackIndex = clipReader.ReadUInt16();
                    int waveBankIndex = clipReader.ReadByte();
                    var loopCount = clipReader.ReadByte();
                    var panAngle = clipReader.ReadUInt16() / 100.0f;
                    var panArc = clipReader.ReadUInt16() / 100.0f;

                    // Pitch variation range
                    var minPitch = clipReader.ReadInt16() / 1000.0f;
                    var maxPitch = clipReader.ReadInt16() / 1000.0f;

                    // Volume variation range
                    var minVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
                    var maxVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());

                    // Filter variation
                    var minFrequency = clipReader.ReadSingle() / 1000.0f;
                    var maxFrequency = clipReader.ReadSingle() / 1000.0f;
                    var minQ = clipReader.ReadSingle();
                    var maxQ = clipReader.ReadSingle();

                    // Unknown!
                    clipReader.ReadByte();

                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank,
                        new[] { waveBankIndex },
                        new[] { trackIndex }, 
                        null,
                        0,
                        VariationType.Ordered,
                        new Vector2(minVolume, maxVolume - minVolume),
                        new Vector2(minPitch, maxPitch - minPitch), 
                        loopCount,
                        false);

                    break;
                }

                case 6:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var playRelease = (eventFlags & 0x01) == 0x01;
                    var panEnabled = (eventFlags & 0x02) == 0x02;
                    var useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    var loopCount = clipReader.ReadByte();
                    var panAngle = clipReader.ReadUInt16() / 100.0f;
                    var panArc = clipReader.ReadUInt16() / 100.0f;

                    // Pitch variation range
                    var minPitch = clipReader.ReadInt16() / 1000.0f;
                    var maxPitch = clipReader.ReadInt16() / 1000.0f;

                    // Volume variation range
                    var minVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
                    var maxVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());

                    // Filter variation range
                    var minFrequency = clipReader.ReadSingle() / 1000.0f;
                    var maxFrequency = clipReader.ReadSingle() / 1000.0f;
                    var minQ = clipReader.ReadSingle();
                    var maxQ = clipReader.ReadSingle();

                    // Unknown!
                    clipReader.ReadByte();

                    // TODO: Still has unknown bits!
                    var variationFlags = clipReader.ReadByte();

                    // Enable pitch variation
                    Vector2? pitchVar = null;
                    if ((variationFlags & 0x10) == 0x10)
                        pitchVar = new Vector2(minPitch, maxPitch - minPitch);

                    // Enable volume variation
                    Vector2? volumeVar = null;
                    if ((variationFlags & 0x20) == 0x20)
                        volumeVar = new Vector2(minVolume, maxVolume - minVolume);

                    // Enable filter variation
                    Vector4? filterVar = null;
                    if ((variationFlags & 0x40) == 0x40)
                        filterVar = new Vector4(minFrequency, maxFrequency - minFrequency, minQ, maxQ - minQ);

                    // The number of tracks for the variations.
                    var numTracks = clipReader.ReadUInt16();

                    // Not sure what most of this is.
                    var moreFlags = clipReader.ReadByte();
                    var newWaveOnLoop = (moreFlags & 0x40) == 0x40;

                    // The variation playlist type seems to be 
                    // stored in the bottom 4bits only.
                    var variationType = (VariationType)(moreFlags & 0x0F);

                    // Unknown!
                    clipReader.ReadBytes(5);

                    // Read in the variation playlist.
                    var waveBanks = new int[numTracks];
                    var tracks = new int[numTracks];
                    var weights = new byte[numTracks];
                    var totalWeights = 0;
                    for (var j = 0; j < numTracks; j++)
                    {
                        tracks[j] = clipReader.ReadUInt16();
                        waveBanks[j] = clipReader.ReadByte();
                        var minWeight = clipReader.ReadByte();
                        var maxWeight = clipReader.ReadByte();
                        weights[j] = (byte)(maxWeight - minWeight);
                        totalWeights += weights[j];
                    }

                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank,
                        waveBanks,
                        tracks,
                        weights,
                        totalWeights,
                        variationType,
                        volumeVar,
                        pitchVar, 
                        loopCount,
                        newWaveOnLoop);

                    break;
                }

                case 7:
                    // Pitch Event
                    throw new NotImplementedException("Pitch event");

                case 8:
                {
                    // Unknown!
                    clipReader.ReadBytes(2);

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var isAdd = (eventFlags & 0x01) == 0x01;

                    // The replacement or additive volume.
                    var decibles = clipReader.ReadSingle() / 100.0f;
                    var volume = XactHelpers.ParseVolumeFromDecibels(decibles + (isAdd ? volumeDb : 0));

                    // Unknown!
                    clipReader.ReadBytes(9);

                    _events[i] = new VolumeEvent(   this, 
                                                    timeStamp, 
                                                    randomOffset, 
                                                    volume);
                    break;
                }

                case 17:
                    // Volume Repeat Event
                    throw new NotImplementedException("Volume repeat event");

                case 9:
                    // Marker Event
                    throw new NotImplementedException("Marker event");

				default:
                    throw new NotSupportedException("Unknown event " + eventId);
				}
			}
			
            clipReader.BaseStream.Seek (oldPosition, SeekOrigin.Begin);
#pragma warning restore 0219
		}

        internal void Update(float dt)
        {
            if (State != SoundState.Playing)
                return;

            _time += dt;

            // Play the next event.
            while (_nextEvent < _events.Length)
            {
                var evt = _events[_nextEvent];
                if (_time < evt.TimeStamp)
                    break;

                evt.Play();
                ++_nextEvent;
            }

            // Update all the active events.
            var isPlaying = _nextEvent < _events.Length;
            for (var i = 0; i < _nextEvent; i++)
            {
                var evt = _events[i];
                isPlaying |= evt.Update(dt);
            }

            // Update the state.
            if (!isPlaying)
                State = SoundState.Stopped;
        }

        internal void SetFade(float fadeInDuration, float fadeOutDuration)
        {
            foreach (var evt in _events)
            {
                if (evt is PlayWaveEvent)
                    evt.SetFade(fadeInDuration, fadeOutDuration);
            }
        }
		
		public void Play()
		{
		    _time = 0.0f;
            _nextEvent = 0;
		    SetVolume(_defaultVolume);
            State = SoundState.Playing; 
            Update(0);
        }

		public void Resume()
		{
            foreach (var evt in _events)
                evt.Resume();

            State = SoundState.Playing;
		}
		
		public void Stop()
        {
            foreach (var evt in _events)
                evt.Stop();

            State = SoundState.Stopped;
        }
		
		public void Pause()
        {
		    foreach (var evt in _events)
		        evt.Pause();

            State = SoundState.Paused;
        }

        public SoundState State { get; private set; }

        /// <summary>
        /// Set the combined volume scale from the parent objects.
        /// </summary>
        /// <param name="volume">The volume scale.</param>
        public void SetVolumeScale(float volume)
        {
            _volumeScale = volume;
		    UpdateVolumes();
        }

        /// <summary>
        /// Set the volume for the clip.
        /// </summary>
        /// <param name="volume">The volume level.</param>
        public void SetVolume(float volume)
        {
            _volume = volume;
            UpdateVolumes();
        }

	    private void UpdateVolumes()
	    {
            var volume = _volume * _volumeScale;
            foreach (var evt in _events)
                evt.SetTrackVolume(volume);
	    }

        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            foreach (var evt in _events)
                evt.Apply3D(listener, emitter);
        }
    }
}

