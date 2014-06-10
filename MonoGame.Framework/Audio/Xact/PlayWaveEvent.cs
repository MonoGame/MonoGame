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
        private SoundBank _soundBank;

        private VariationType _variation;

        private bool _isLooped;

        private float _volume;

        private int [] _tracks;
        private int [] _waveBanks;

        private int _wavIndex;

        private SoundEffectInstance _wav;

        public PlayWaveEvent(   XactClip clip, float timeStamp, float randomOffset, SoundBank soundBank, 
                                int[] waveBanks, int[] tracks, VariationType variation, bool isLooped)
            : base(clip, timeStamp, randomOffset)
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

            switch (_variation)
            {
                case VariationType.Ordered:
                    _wavIndex = (_wavIndex + 1) % trackCount;
                    break;

                case VariationType.OrderedFromRandom:
                    _wavIndex = (_wavIndex + 1) % trackCount;
                    break;

                case VariationType.Random:
                    _wavIndex = XactHelpers.Random.Next() % trackCount;
                    break;

                case VariationType.RandomNoImmediateRepeats:
                {
                    var last = _wavIndex;
                    do
                    {
                        _wavIndex = XactHelpers.Random.Next() % trackCount;
                    }
                    while (last == _wavIndex && trackCount > 1);
                    break;
                }

                case VariationType.Shuffle:
                    // TODO: Need some sort of deck implementation.
                    _wavIndex = XactHelpers.Random.Next() % trackCount;
                    break;
            };

            _wav = _soundBank.GetSoundEffectInstance(_waveBanks[_wavIndex], _tracks[_wavIndex]);
            if (_wav == null)
            {
                // We couldn't create a sound effect instance, most likely
                // because we've reached the sound pool limits.
                return;
            }

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

            base.Stop();
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

            base.Update(dt);
        }
	}
}

