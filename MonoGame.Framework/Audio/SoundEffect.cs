#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License
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
        private bool isDisposed = false;

        #region Internal Audio Data

        private string _name;

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
            SoundEffectInstance instance;
            instance = PlatformCreateInstance();
            return instance;
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

