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

#if WINDOWS || LINUX
using OpenTK.Audio.OpenAL;
#elif ANDROID
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.Util;
using Stream = System.IO.Stream;
#elif IOS
using MonoTouch.AudioToolbox;
using MonoTouch.AudioUnit;
using OpenTK.Audio.OpenAL;
#elif MONOMAC
using MonoMac.AudioToolbox;
using MonoMac.AudioUnit;
using MonoMac.OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        internal byte[] _data;

		internal float Rate { get; set; }

        internal int Size { get; set; }

#if WINDOWS || LINUX || IOS || MONOMAC

        internal ALFormat Format { get; set; }
#endif

#if ANDROID
		private int _soundID = -1;
#endif

        #region Public Constructors

        private void PlatformLoadAudioStream(Stream s)
        {
#if WINDOWS || LINUX
            
            ALFormat format;
            int size;
            int freq;

            _data = AudioLoader.Load(s, out format, out size, out freq);

            Format = format;
            Size = size;
            Rate = freq;

            return;
#endif

#if MONOMAC || IOS

            AudioFileStream afs = new AudioFileStream (AudioFileType.WAVE);

			var audiodata = new byte[s.Length];
			s.Read(audiodata, 0, (int)s.Length);
            afs.ParseBytes (audiodata, false); // AudioFileStreamStatus status
            AudioStreamBasicDescription asbd = afs.StreamBasicDescription;
            
            Rate = (float)asbd.SampleRate;
            Size = (int)afs.DataByteCount;
            
            if (asbd.ChannelsPerFrame == 1)
                Format = asbd.BitsPerChannel == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
            else
                Format = asbd.BitsPerChannel == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;

            byte []d = new byte[afs.DataByteCount];
            Array.Copy (audiodata, afs.DataOffset, d, 0, afs.DataByteCount);

            _data = d;

            var _dblDuration = (Size / ((asbd.BitsPerChannel / 8) * ((asbd.ChannelsPerFrame == 0) ? 1 : asbd.ChannelsPerFrame))) / asbd.SampleRate;
            _duration = TimeSpan.FromSeconds(_dblDuration);

			afs.Close ();

#endif

#if ANDROID

			// Creating a soundeffect from a stream
			// doesn't seem to be supported in Android
#endif
        }

        private void PlatformInitialize(byte[] buffer, int sampleRate, AudioChannels channels)
        {
			Rate = (float)sampleRate;
            Size = (int)buffer.Length;

#if WINDOWS || LINUX

            _data = buffer;
            Format = (channels == AudioChannels.Stereo) ? ALFormat.Stereo16 : ALFormat.Mono16;
            return;

#endif

#if MONOMAC || IOS

            //buffer should contain 16-bit PCM wave data
            short bitsPerSample = 16;

            if ((int)channels <= 1)
                Format = bitsPerSample == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
            else
                Format = bitsPerSample == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;

            _name = "";
            _data = buffer;

#endif

#if ANDROID
            _name = "";

            _data = AudioUtil.FormatWavData(buffer, sampleRate, (int)channels);
#endif
        }

#if ANDROID

		internal SoundEffect(string fileName)
		{
			using (AssetFileDescriptor fd = Game.Activity.Assets.OpenFd(fileName))
				_soundID = SoundEffectInstance.SoundPool.Load(fd.FileDescriptor, fd.StartOffset, fd.Length, 1);
		}

#endif

        private void PlatformInitialize(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            _duration = GetSampleDuration(buffer.Length, sampleRate, channels);

            throw new NotImplementedException();
        }

        #endregion

        #region Additional SoundEffect/SoundEffectInstance Creation Methods

        private void PlatformSetupInstance(SoundEffectInstance inst)
        {
#if WINDOWS || LINUX || MONOMAC || IOS

            inst.InitializeSound();
            inst.BindDataBuffer(_data, Format, Size, (int)Rate);
#endif

#if ANDROID
            inst._soundId = _soundID;
            inst._sampleRate = Rate;
#endif
        }

        #endregion

        #region Static Members

        private static void PlatformSetMasterVolume()
        {
            var activeSounds = SoundEffectInstancePool.GetAllPlayingSounds();

            // A little gross here, but there's
            // no if(value == value) check in SFXInstance.Volume
            // This'll allow the sound's current volume to be recalculated
            // against SoundEffect.MasterVolume.
            foreach(var sound in activeSounds)
                sound.Volume = sound.Volume;
        }

        #endregion

        #region IDisposable Members

        private void PlatformDispose()
        {
            // A No-op for WINDOWS and LINUX. Note that isDisposed remains false!

#if ANDROID
            if (!isDisposed)
            {
				if (_soundID != -1)
					SoundEffectInstance.SoundPool.Unload(_soundID);

				_soundID = -1;

                isDisposed = true;
            }
#endif
        }

        #endregion
    }
}

