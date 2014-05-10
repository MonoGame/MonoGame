// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.IO;

#if MONOMAC
using MonoMac.AudioToolbox;
using MonoMac.AudioUnit;
using MonoMac.AVFoundation;
using MonoMac.Foundation;
using MonoMac.OpenAL;
#elif OPENAL
using OpenTK.Audio.OpenAL;
#if IOS
using MonoTouch.AudioToolbox;
using MonoTouch.AudioUnit;
using MonoTouch.AVFoundation;
using MonoTouch.Foundation;
#endif
#elif ANDROID
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.Util;
using Stream = System.IO.Stream;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        internal byte[] _data;

		internal float Rate { get; set; }

        internal int Size { get; set; }

#if OPENAL

        internal ALFormat Format { get; set; }
#endif

#if ANDROID
		private int _soundID = -1;
#endif

        #region Public Constructors

        private void PlatformLoadAudioStream(Stream s)
        {
#if WINDOWS || LINUX || ANGLE
            
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

            var audiodata = new byte[s.Length];
            s.Read(audiodata, 0, (int)s.Length);

            using (AudioFileStream afs = new AudioFileStream (AudioFileType.WAVE))
            {
                afs.ParseBytes (audiodata, false);
                Size = (int)afs.DataByteCount;

                _data = new byte[afs.DataByteCount];
                Array.Copy (audiodata, afs.DataOffset, _data, 0, afs.DataByteCount);

                AudioStreamBasicDescription asbd = afs.DataFormat;
                int channelsPerFrame = asbd.ChannelsPerFrame;
                int bitsPerChannel = asbd.BitsPerChannel;

                // There is a random chance that properties asbd.ChannelsPerFrame and asbd.BitsPerChannel are invalid because of a bug in Xamarin.iOS
                // See: https://bugzilla.xamarin.com/show_bug.cgi?id=11074 (Failed to get buffer attributes error when playing sounds)
                if (channelsPerFrame <= 0 || bitsPerChannel <= 0)
                {
                    NSError err;
                    using (NSData nsData = NSData.FromArray(audiodata))
                    using (AVAudioPlayer player = AVAudioPlayer.FromData(nsData, out err))
                    {
                        channelsPerFrame = (int)player.NumberOfChannels;
                        bitsPerChannel = player.SoundSetting.LinearPcmBitDepth.GetValueOrDefault(16);

                        Rate = player.Settings.SampleRate;
                        _duration = TimeSpan.FromSeconds(player.Duration);
                    }
                }
                else
                {
                    Rate = (float)asbd.SampleRate;
                    double duration = (Size / ((bitsPerChannel / 8) * channelsPerFrame)) / asbd.SampleRate;
                    _duration = TimeSpan.FromSeconds(duration);
                }

                if (channelsPerFrame == 1)
                    Format = (bitsPerChannel == 8) ? ALFormat.Mono8 : ALFormat.Mono16;
                else
                    Format = (bitsPerChannel == 8) ? ALFormat.Stereo8 : ALFormat.Stereo16;
            }

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

#if WINDOWS || LINUX || ANGLE

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
#if OPENAL

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

        internal static void PlatformShutdown()
        {
#if OPENAL
            OpenALSoundController.DestroyInstance();
#endif
        }
    }
}

