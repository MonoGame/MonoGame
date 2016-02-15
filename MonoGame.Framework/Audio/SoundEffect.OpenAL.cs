// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.IO;

#if MONOMAC && PLATFORM_MACOS_LEGACY
using MonoMac.AudioToolbox;
using MonoMac.AudioUnit;
using MonoMac.AVFoundation;
using MonoMac.Foundation;
using MonoMac.OpenAL;
#elif OPENAL
using OpenTK.Audio.OpenAL;
#if IOS || MONOMAC
using AudioToolbox;
using AudioUnit;
using AVFoundation;
using Foundation;
#endif
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        internal const int MAX_PLAYING_INSTANCES = OpenALSoundController.MAX_NUMBER_OF_SOURCES;

        internal byte[] _data;

        internal OALSoundBuffer SoundBuffer;

        internal float Rate { get; set; }

        internal int Size { get; set; }

        internal ALFormat Format { get; set; }

        #region Public Constructors

        private void PlatformLoadAudioStream(Stream s)
        {
#if OPENAL && !(MONOMAC || IOS)
            
            ALFormat format;
            int size;
            int freq;

            var stream = s;
#if ANDROID
            var needsDispose = false;
            try
            {
                // If seek is not supported (usually an indicator of a stream opened into the AssetManager), then copy
                // into a temporary MemoryStream.
                if (!s.CanSeek)
                {
                    needsDispose = true;
                    stream = new MemoryStream();
                    s.CopyTo(stream);
                    stream.Position = 0;
                }
#endif
                _data = AudioLoader.Load(stream, out format, out size, out freq);
#if ANDROID
            }
            finally
            {
                if (needsDispose)
                    stream.Dispose();
            }
#endif
            Format = format;
            Size = size;
            Rate = freq;

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

						Rate = (float)player.SoundSetting.SampleRate;
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
        }

        private void PlatformInitialize(byte[] buffer, int sampleRate, AudioChannels channels)
        {
			Rate = (float)sampleRate;
            Size = (int)buffer.Length;

#if OPENAL && !(MONOMAC || IOS)

            _data = buffer;
            Format = (channels == AudioChannels.Stereo) ? ALFormat.Stereo16 : ALFormat.Mono16;

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
            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(_data, Format, Size, (int)Rate);
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
            inst.InitializeSound();
        }

        #endregion

        #region IDisposable Members

        private void PlatformDispose(bool disposing)
        {
            if (SoundBuffer != null)
            {
                SoundBuffer.Dispose();
                SoundBuffer = null;
            }
        }

        #endregion

        internal static void PlatformShutdown()
        {
            OpenALSoundController.DestroyInstance();
        }
    }
}

