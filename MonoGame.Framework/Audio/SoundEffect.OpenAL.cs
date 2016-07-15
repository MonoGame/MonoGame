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
#if GLES || MONOMAC
using OpenTK.Audio.OpenAL;
#else 
using OpenAL;
#endif
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
        internal static uint ReverbSlot = 0;
        internal static uint ReverbEffect = 0;

        internal OALSoundBuffer SoundBuffer;

        internal float Rate { get; set; }

        internal int Size { get; set; }

        internal ALFormat Format { get; set; }

        #region Public Constructors

        private void PlatformLoadAudioStream(Stream s)
        {
            byte[] buffer;

#if OPENAL && !(MONOMAC || IOS)
            
            ALFormat format;
            int size;
            int freq;

            var stream = s;

            buffer = AudioLoader.Load(stream, out format, out size, out freq);

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

                buffer = new byte[afs.DataByteCount];
                Array.Copy (audiodata, afs.DataOffset, buffer, 0, afs.DataByteCount);

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
            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(buffer, Format, Size, (int)Rate);
        }

        private void PlatformInitializePCM(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            Rate = (float)sampleRate;
            Size = (int)count;
            Format = channels == AudioChannels.Stereo ? ALFormat.Stereo16 : ALFormat.Mono16;

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(buffer, Format, Size, (int)Rate);
        }

        private void PlatformInitializeADPCM (byte [] buffer, int offset, int count, int sampleRate, AudioChannels channels, int dataFormat, int loopStart, int loopLength)
        {
            Rate = (float)sampleRate;
            Size = (int)count;
            #if DESKTOPGL
            Format = channels == AudioChannels.Stereo ? ALFormat.StereoMSADPCM : ALFormat.MonoMSADPCM;
            #else
            Format = channels == AudioChannels.Stereo ? (ALFormat)0x1303 : (ALFormat)0x1302;
            #endif

            // bind buffer
            SoundBuffer = new OALSoundBuffer ();
            SoundBuffer.BindDataBuffer (buffer, Format, Size, (int)Rate, dataFormat);
        }

        private void PlatformInitializeFormat(byte[] buffer, int format, int sampleRate, int channels, int blockAlignment, int loopStart, int loopLength)
        {
            // We need to decode MSADPCM.
            var supportsADPCM = OpenALSoundController.GetInstance.SupportsADPCM;
            if (format == 2 && !supportsADPCM)
            {
                using (var stream = new MemoryStream(buffer))
                using (var reader = new BinaryReader(stream))
                {
                    buffer = MSADPCMToPCM.MSADPCM_TO_PCM (reader, (short)channels, (short)blockAlignment);
                    format = 1;
                }
            }

            if (!supportsADPCM && format != 1)
                throw new NotSupportedException("Unsupported wave format!");

            if (supportsADPCM && format == 2) {
                PlatformInitializeADPCM (buffer, 0, buffer.Length, sampleRate, (AudioChannels)channels, blockAlignment, loopStart, loopLength);
            } else {
                PlatformInitializePCM (buffer, 0, buffer.Length, sampleRate, (AudioChannels)channels, loopStart, loopLength);
            }
            _duration = TimeSpan.FromSeconds (SoundBuffer.Duration);
        }
        
        #endregion

        #region Additional SoundEffect/SoundEffectInstance Creation Methods

        private void PlatformSetupInstance(SoundEffectInstance inst)
        {
            inst.InitializeSound();
        }

        #endregion

        internal static void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
            if (!EffectsExtension.Instance.IsInitialized)
                return;

            if (ReverbEffect != 0)
                return;
            
            var efx = EffectsExtension.Instance;
            ReverbEffect = efx.GetEffect (out ReverbSlot);
            efx.SetEffectParam (ReverbEffect, EfxEffectf.ReflectionsDelay, reverbSettings.ReflectionsDelayMs / 1000.0f);
            // map these from range 0-15 to 0-1
            efx.SetEffectParam (ReverbEffect, EfxEffectf.Diffusion, reverbSettings.EarlyDiffusion / 15f);
            efx.SetEffectParam (ReverbEffect, EfxEffectf.Diffusion, reverbSettings.LateDiffusion / 15f);
            efx.SetEffectParam (ReverbEffect, EfxEffectf.GainLowFrequency, Math.Min (XactHelpers.ParseVolumeFromDecibels (reverbSettings.LowEqGain - 8f), 1.0f));
            efx.SetEffectParam (ReverbEffect, EfxEffectf.LowFrequencyReference, (reverbSettings.LowEqCutoff * 50f) + 50f);
            efx.SetEffectParam (ReverbEffect, EfxEffectf.GainHighFrequency, XactHelpers.ParseVolumeFromDecibels (reverbSettings.HighEqGain - 8f));
            efx.SetEffectParam (ReverbEffect, EfxEffectf.HighFrequencyReference, (reverbSettings.HighEqCutoff * 500f) + 1000f);
            efx.SetEffectParam (ReverbEffect, EfxEffectf.ReflectionsGain, Math.Min (XactHelpers.ParseVolumeFromDecibels (reverbSettings.ReflectionsGainDb), 1.0f));
            efx.SetEffectParam (ReverbEffect, EfxEffectf.Gain, Math.Min (XactHelpers.ParseVolumeFromDecibels (reverbSettings.ReverbGainDb), 1.0f));
            // map these from 0-100 down to 0-1
            efx.SetEffectParam (ReverbEffect, EfxEffectf.Density, reverbSettings.DensityPct / 100f);
            efx.SetEffectParam (ReverbEffect, EfxEffectf.Gain, reverbSettings.WetDryMixPct / 100f);

            // Dont know what to do with these EFX has no mapping for them. Just ignore for now
            // we can enable them as we go. 
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.PositionLeft, reverbSettings.PositionLeft);
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.PositionRight, reverbSettings.PositionRight);
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.PositionLeftMatrix, reverbSettings.PositionLeftMatrix);
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.PositionRightMatrix, reverbSettings.PositionRightMatrix);
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RearDelayMs);
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RoomFilterFrequencyHz);
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RoomFilterMainDb);
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RoomFilterHighFrequencyDb);
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.DecayTimeSec);
            //efx.SetEffectParam (ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RoomSizeFeet);

            efx.BindEffectToSlot (ReverbEffect, ReverbSlot);
        }

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
            if (ReverbEffect != 0)
                EffectsExtension.Instance.DeleteEffect (ReverbEffect, ReverbSlot);
            OpenALSoundController.DestroyInstance();
        }
    }
}

