// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    enum VariationType
    {
        Ordered,
        OrderedFromRandom,
        Random,
        RandomNoImmediateRepeats,
        Shuffle
    };

	class PlayWaveEvent : ClipEvent
    {
        private readonly SoundBank _soundBank;

        private readonly VariationType _variation;

        private readonly int _loopCount;

        private readonly bool _newWaveOnLoop;

        private readonly int[] _tracks;
        private readonly int[] _waveBanks;
        
        private readonly byte[] _weights;
        private readonly int _totalWeights;

        private float _trackVolume;

        private readonly Vector2? _volumeVar;
        private readonly Vector2? _pitchVar;

        private int _wavIndex;
        private int _loopIndex;

        private SoundEffectInstance _wav;

        public PlayWaveEvent(   XactClip clip, float timeStamp, float randomOffset, SoundBank soundBank,
                                int[] waveBanks, int[] tracks, byte[] weights, int totalWeights,
                                VariationType variation, Vector2? volumeVar, Vector2? pitchVar, 
                                int loopCount, bool newWaveOnLoop)
            : base(clip, timeStamp, randomOffset)
        {
            _soundBank = soundBank;
            _waveBanks = waveBanks;
            _tracks = tracks;
            _weights = weights;
            _totalWeights = totalWeights;
            _volumeVar = volumeVar;
            _pitchVar = pitchVar;
            _wavIndex = -1;
            _loopIndex = 0;
            _trackVolume = 1.0f;
            _variation = variation;
            _loopCount = loopCount;
            _newWaveOnLoop = newWaveOnLoop;
        }

		public override void Play() 
        {
            if (_wav != null && _wav.State != SoundState.Stopped)
                _wav.Stop();

            Play(true);
        }

        private void Play(bool pickNewWav)
        {
            var trackCount = _tracks.Length;

            // Do we need to pick a new wav to play first?
            if (pickNewWav)
            {
                switch (_variation)
                {
                    case VariationType.Ordered:
                        _wavIndex = (_wavIndex + 1) % trackCount;
                        break;

                    case VariationType.OrderedFromRandom:
                        _wavIndex = (_wavIndex + 1) % trackCount;
                        break;

                    case VariationType.Random:
                        if (_weights == null || trackCount == 1)
                            _wavIndex = XactHelpers.Random.Next() % trackCount;
                        else
                        {
                            var sum = XactHelpers.Random.Next(_totalWeights);
                            for (var i=0; i < trackCount; i++)
                            {
                                sum -= _weights[i];
                                if (sum <= 0)
                                {
                                    _wavIndex = i;
                                    break;
                                }
                            }
                        }
                        break;

                    case VariationType.RandomNoImmediateRepeats:
                    {
                        if (_weights == null || trackCount == 1)
                            _wavIndex = XactHelpers.Random.Next() % trackCount;
                        else
                        {
                            var last = _wavIndex;
                            var sum = XactHelpers.Random.Next(_totalWeights);
                            for (var i=0; i < trackCount; i++)
                            {
                                sum -= _weights[i];
                                if (sum <= 0)
                                {
                                    _wavIndex = i;
                                    break;
                                }
                            }

                            if (_wavIndex == last)
                                _wavIndex = (_wavIndex + 1) % trackCount;
                        }
                        break;
                    }

                    case VariationType.Shuffle:
                        // TODO: Need some sort of deck implementation.
                        _wavIndex = XactHelpers.Random.Next() % trackCount;
                        break;
                };
            }

            _wav = _soundBank.GetSoundEffectInstance(_waveBanks[_wavIndex], _tracks[_wavIndex]);
            if (_wav == null)
            {
                // We couldn't create a sound effect instance, most likely
                // because we've reached the sound pool limits.
                return;
            }

            // Set the volume.
            SetTrackVolume(_trackVolume);

            // Set the pitch.
            if (_pitchVar.HasValue)
                _wav.Pitch = _pitchVar.Value.X + ((float)XactHelpers.Random.NextDouble() * _pitchVar.Value.Y);
            else
                _wav.Pitch = 0;

            // This is a shortcut for infinite looping of a single track.
            _wav.IsLooped = _loopCount == 255 && trackCount == 1;
            _wav.Play();
		}

		public override void Stop()
        {
            if (_wav != null)
            {
                _wav.Stop();
                _wav = null;
            }
            _loopIndex = 0;
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

        public override void SetTrackVolume(float volume)
        {
            _trackVolume = volume;

            if (_wav != null)
            {
                if (_volumeVar.HasValue)
                    _wav.Volume = _trackVolume * (_volumeVar.Value.X + ((float)XactHelpers.Random.NextDouble() * _volumeVar.Value.Y));
                else
                    _wav.Volume = _trackVolume;
            }
        }

        public override void SetFade(float fadeInDuration, float fadeOutDuration)
        {
            // TODO
        }

        public override bool Update(float dt)
        {
            if (_wav != null && _wav.State == SoundState.Stopped)
            {
                // If we're not looping or reached our loop 
                // limit then we can stop.
                if (_loopCount == 0 || _loopIndex >= _loopCount)
                {
                    _wav = null;
                    _loopIndex = 0;
                }
                else
                {
                    // Increment the loop count if it isn't infinite.
                    if (_loopCount != 255)
                        ++_loopIndex;

                    // Play the next track.
                    Play(_newWaveOnLoop);
                }
            }

            return _wav != null && _wav.State != SoundState.Stopped;
        }

        public override void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            if (_wav != null)
                _wav.Apply3D(listener, emitter);
        }
    }
}

