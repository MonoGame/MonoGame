// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    class XactSound
    {
        private readonly bool _complexSound;
        private readonly XactClip[] _soundClips;
        private readonly int _waveBankIndex;
        private readonly int _trackIndex;
        private readonly float _volume;
        private readonly float _pitch;
        private readonly uint _categoryID;
        private readonly SoundBank _soundBank;
        private readonly bool _useReverb;

        private SoundEffectInstance _wave;
        private bool _streaming;

        private float _cueVolume = 1;
        private float _cuePitch = 0;
        private float _cueReverbMix = 0;
        private float? _cueFilterFrequency;
        private float? _cueFilterQFactor;

        internal readonly int[] RpcCurves;
        
        public XactSound(SoundBank soundBank, int waveBankIndex, int trackIndex)
        {
            _complexSound = false;

            _soundBank = soundBank;
            _waveBankIndex = waveBankIndex;
            _trackIndex = trackIndex;
            RpcCurves = new int[0];
        }

        public XactSound(AudioEngine engine, SoundBank soundBank, BinaryReader soundReader)
        {
            _soundBank = soundBank;
            
            var flags = soundReader.ReadByte();
            _complexSound = (flags & 0x1) != 0;
            var hasRPCs = (flags & 0x0E) != 0;
            var hasDSPs = (flags & 0x10) != 0;

            _categoryID = soundReader.ReadUInt16();
            _volume = XactHelpers.ParseVolumeFromDecibels(soundReader.ReadByte());
            _pitch = soundReader.ReadInt16() / 1000.0f;
            soundReader.ReadByte(); //priority
            soundReader.ReadUInt16(); // filter stuff?
            
            var numClips = 0;
            if (_complexSound)
                numClips = soundReader.ReadByte();
            else 
            {
                _trackIndex = soundReader.ReadUInt16();
                _waveBankIndex = soundReader.ReadByte();
            }

            if (!hasRPCs)
                RpcCurves = new int[0];
            else
            {
                var current = soundReader.BaseStream.Position;

                // This doesn't seem to be used... might have been there
                // to allow for some future file format expansion.
                var dataLength = soundReader.ReadUInt16();

                var numPresets = soundReader.ReadByte();
                RpcCurves = new int[numPresets];
                for (var i = 0; i < numPresets; i++)
                    RpcCurves[i] = engine.GetRpcIndex(soundReader.ReadUInt32());

                // Just in case seek to the right spot.
                soundReader.BaseStream.Seek(current + dataLength, SeekOrigin.Begin);
            }

            if (!hasDSPs)
                _useReverb = false;
            else
            {
                // The file format for this seems to follow the pattern for 
                // the RPC curves above, but in this case XACT only supports
                // a single effect...  Microsoft Reverb... so just set it.
                _useReverb = true;
                soundReader.BaseStream.Seek(7, SeekOrigin.Current);
            }

            if (_complexSound)
            {
                _soundClips = new XactClip[numClips];
                for (int i = 0; i < numClips; i++)
                    _soundClips[i] = new XactClip(soundBank, soundReader, _useReverb);
            }

            var category = engine.Categories[_categoryID];
            category.AddSound(this);
        }

        internal void SetFade(float fadeInTime, float fadeOutTime)
        {
            if (fadeInTime == 0.0f &&
                fadeOutTime == 0.0f )
                return;

            if (_complexSound)
            {
                foreach (var sound in _soundClips)
                    sound.SetFade(fadeInTime, fadeOutTime);
            }
            else
            {
                // TODO:
            }
        }

        public void Play(float volume, AudioEngine engine)
        {
            _cueVolume = volume;
            var category = engine.Categories[_categoryID];

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

            float finalVolume = _volume * _cueVolume * category._volume[0];
            float finalPitch = _pitch + _cuePitch;
            float finalMix = _useReverb ? _cueReverbMix : 0.0f;

            if (_complexSound) 
            {
                foreach (XactClip clip in _soundClips)
                {
                    clip.UpdateState(finalVolume, finalPitch, finalMix, _cueFilterFrequency, _cueFilterQFactor);
                    clip.Play();
                }
            } 
            else 
            {
                if (_wave != null)
                {
                    if (_streaming)
                        _wave.Dispose();
					else
						_wave._isXAct = false;					
                    _wave = null;
                }

                    _wave = _soundBank.GetSoundEffectInstance(_waveBankIndex, _trackIndex, out _streaming);

                if (_wave == null)
                {
                    // We couldn't create a sound effect instance, most likely
                    // because we've reached the sound pool limits.
                    return;
                }

                _wave.Pitch = finalPitch;
                _wave.Volume = finalVolume;
                _wave.PlatformSetReverbMix(finalMix);
                _wave.Play();
            }
        }

        internal void Update(float dt)
        {
            if (_complexSound)
            {
                foreach (var sound in _soundClips)
                    sound.Update(dt);
            }
            else
            {
                if (_wave != null && _wave.State == SoundState.Stopped)
                {
                    if (_streaming)
                        _wave.Dispose();
					else
						_wave._isXAct = false;					
                    _wave = null;
                }
            }
        }

        internal void StopAll(AudioStopOptions options)
        {
            if (_complexSound)
            {
                foreach (XactClip clip in _soundClips)
                    clip.Stop();
            }
            else
            {
                if (_wave != null)
                {
                    _wave.Stop();
                    if (_streaming)
                        _wave.Dispose();
 					else
						_wave._isXAct = false;					
                   _wave = null;
                }
            }
        }
        
        public void Stop(AudioStopOptions options)
        {
            if (_complexSound)
            {
                foreach (var sound in _soundClips)
                    sound.Stop();
            }
            else
            {
                if (_wave != null)
                {
                    _wave.Stop();
                    if (_streaming)
                        _wave.Dispose();
					else
						_wave._isXAct = false;					
                    _wave = null;
                }
            }
        }
        
        public void Pause()
        {
            if (_complexSound)
            {
                foreach (var sound in _soundClips)
                {
                    if (sound.State == SoundState.Playing)
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
            if (_complexSound)
            {
                foreach (var sound in _soundClips)
                {
                    if (sound.State == SoundState.Paused)
                        sound.Resume();
                }
            }
            else
            {
                if (_wave != null && _wave.State == SoundState.Paused)
                    _wave.Resume();
            }
        }

        internal void UpdateCategoryVolume(float categoryVolume)
        {
            // The different volumes modulate each other.
            var volume = _volume * _cueVolume * categoryVolume;

            if (_complexSound)
            {
                foreach (var clip in _soundClips)
                    clip.SetVolumeScale(volume);
            }
            else
            {
                if (_wave != null)
                    _wave.Volume = volume;
            }
        }

        internal void UpdateState(AudioEngine engine, float volume, float pitch, float reverbMix, float? filterFrequency, float? filterQFactor)
        {
            _cueVolume = volume;
            var finalVolume = _volume * _cueVolume * engine.Categories[_categoryID]._volume[0];

            _cueReverbMix = reverbMix;
            _cueFilterFrequency = filterFrequency;
            _cueFilterQFactor = filterQFactor;

            _cuePitch = pitch;
            var finalPitch = _pitch + _cuePitch;

            if (_complexSound)
            {
                foreach (var clip in _soundClips)
                    clip.UpdateState(finalVolume, finalPitch, _useReverb ? _cueReverbMix : 0.0f, _cueFilterFrequency, _cueFilterQFactor);
            }
            else if (_wave != null)
            {
                _wave.PlatformSetReverbMix(_useReverb ? _cueReverbMix : 0.0f);
                _wave.Pitch = finalPitch;
                _wave.Volume = finalVolume;
            }
        }

        internal void SetCuePan(float pan)
        {
            if (_complexSound)
            {
                foreach (var clip in _soundClips)
                    clip.SetPan(pan);
            }
            else
            {
                if (_wave != null)
                    _wave.Pan = pan;
            }
        }

        public bool Playing 
        {
            get 
            {
                if (_complexSound)
                {
                    foreach (var clip in _soundClips)
                        if (clip.State == SoundState.Playing)
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
                if (_complexSound)
                {
                    var notStopped = false;

                    // All clips must be stopped for the sound to be stopped.
                    foreach (var clip in _soundClips)
                    {
                        if (clip.State != SoundState.Stopped)
                            notStopped = true;
                    }

                    return !notStopped;
                }

                // We null the wave when it it stopped.
                return _wave == null;
            }
        }

        public bool IsPaused
        {
            get
            {
                if (_complexSound) 
                {
                    foreach (var clip in _soundClips)
                        if (clip.State == SoundState.Paused) 
                            return true;

                    return false;
                }

                return _wave != null && _wave.State == SoundState.Paused;
            }
        }
    }
}

