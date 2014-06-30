// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.IO;
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.Util;
using Stream = System.IO.Stream;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        internal byte[] _data;

		internal float Rate { get; set; }

        internal int Size { get; set; }

		private int _soundID = -1;

        #region Public Constructors

        private void PlatformLoadAudioStream(Stream s)
        {
			// Creating a soundeffect from a stream
			// doesn't seem to be supported in Android
        }

        private void PlatformInitialize(byte[] buffer, int sampleRate, AudioChannels channels)
        {
			Rate = (float)sampleRate;
            Size = (int)buffer.Length;

            _name = "";
            _data = AudioUtil.FormatWavData(buffer, sampleRate, (int)channels);
        }

		internal SoundEffect(string fileName)
		{
			using (AssetFileDescriptor fd = Game.Activity.Assets.OpenFd(fileName))
				_soundID = SoundEffectInstance.SoundPool.Load(fd.FileDescriptor, fd.StartOffset, fd.Length, 1);
		}

        private void PlatformInitialize(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            _duration = GetSampleDuration(buffer.Length, sampleRate, channels);

            throw new NotImplementedException();
        }

        #endregion

        #region Additional SoundEffect/SoundEffectInstance Creation Methods

        private void PlatformSetupInstance(SoundEffectInstance inst)
        {
            inst._soundId = _soundID;
        }

        #endregion

        #region Static Members

        private static void PlatformSetMasterVolume()
        {
            SoundEffectInstancePool.UpdateVolumes();
        }

        #endregion

        #region IDisposable Members

        private void PlatformDispose(bool disposing)
        {
            if (_soundID != -1)
            {
                SoundEffectInstance.SoundPool.Unload(_soundID);
                _soundID = -1;
            }
        }

        #endregion

        internal static void PlatformShutdown()
        {
        }
    }
}
