// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        #region Internal Audio Data

        private string _name;
        
        private bool isDisposed = false;
        private TimeSpan _duration = TimeSpan.Zero;

        #endregion

        #region Internal Constructors

        internal SoundEffect() { }

        #endregion

        #region Public Constructors

        public SoundEffect(byte[] buffer, int sampleRate, AudioChannels channels)
        {            
            PlatformInitialize(buffer, sampleRate, channels);
        }

        public SoundEffect(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {            
            PlatformInitialize(buffer, offset, count, sampleRate, channels, loopStart, loopLength);
        }

        #endregion

        #region Additional SoundEffect/SoundEffectInstance Creation Methods

        public SoundEffectInstance CreateInstance()
        {
            return PlatformCreateInstance();
        }

        public static SoundEffect FromStream(Stream s)
        {
            var sfx = new SoundEffect();

            sfx.PlatformLoadAudioStream(s);

            return sfx;
        }

        #endregion

        #region Play

        public bool Play()
        {
            return PlatformPlay();
        }

        public bool Play(float volume, float pitch, float pan)
        {
            return PlatformPlay(volume, pitch, pan);
        }

        #endregion

        #region Public Properties

        public TimeSpan Duration { get { return PlatformGetDuration(); } }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        #endregion

        #region Static Members

        static float _masterVolume = 1.0f;
        public static float MasterVolume 
        { 
            get { return _masterVolume; }
            set
            {
                if (_masterVolume != value)
                    _masterVolume = value;

                PlatformSetMasterVolume();
            }
        }

        static float _distanceScale = 1.0f;
        public static float DistanceScale
        {
            get { return _distanceScale; }
            set
            {
                if (value <= 0f)
                    throw new ArgumentOutOfRangeException ("value of DistanceScale");

                _distanceScale = value;
            }
        }

        static float _dopplerScale = 1f;
        public static float DopplerScale
        {
            get { return _dopplerScale; }
            set
            {
                // As per documenation it does not look like the value can be less than 0
                //   although the documentation does not say it throws an error we will anyway
                //   just so it is like the DistanceScale
                if (value < 0f)
                    throw new ArgumentOutOfRangeException ("value of DopplerScale");

                _dopplerScale = value;
            }
        }

        static float speedOfSound = 343.5f;
        public static float SpeedOfSound
        {
            get { return speedOfSound; }
            set { speedOfSound = value; }
        }

        #endregion

        #region IDisposable Members

        public bool IsDisposed { get { return isDisposed; } }

        public void Dispose()
        {
            PlatformDispose();
        }

        #endregion

    }
}

