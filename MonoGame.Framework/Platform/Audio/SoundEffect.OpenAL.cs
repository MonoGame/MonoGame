// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.IO;

#if OPENAL
using MonoGame.OpenAL;
#endif
#if IOS
using AudioToolbox;
using AudioUnit;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        internal const int MAX_PLAYING_INSTANCES = OpenALSoundController.MAX_NUMBER_OF_SOURCES;
        internal static uint ReverbSlot = 0;
        internal static uint ReverbEffect = 0;

        internal OALSoundBuffer SoundBuffer;

        #region Public Constructors

        private void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
        {
            byte[] buffer;

            ALFormat format;
            int freq;
            int channels;
            int blockAlignment;
            int bitsPerSample;
            int samplesPerBlock;
            int sampleCount;
            buffer = AudioLoader.Load(stream, out format, out freq, out channels, out blockAlignment, out bitsPerSample, out samplesPerBlock, out sampleCount);

            duration = TimeSpan.FromSeconds((float)sampleCount / (float)freq);

            PlatformInitializeBuffer(buffer, buffer.Length, format, channels, freq, blockAlignment, bitsPerSample, 0, 0);
        }

        private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            if (sampleBits == 24)
            {
                // Convert 24-bit signed PCM to 16-bit signed PCM
                buffer = AudioLoader.Convert24To16(buffer, offset, count);
                offset = 0;
                count = buffer.Length;
                sampleBits = 16;
            }

            var format = AudioLoader.GetSoundFormat(AudioLoader.FormatPcm, (int)channels, sampleBits);

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(buffer, format, count, sampleRate);
        }

        private void PlatformInitializeIeeeFloat(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            if (!OpenALSoundController.Instance.SupportsIeee)
            {
                // If 32-bit IEEE float is not supported, convert to 16-bit signed PCM
                buffer = AudioLoader.ConvertFloatTo16(buffer, offset, count);
                PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                return;
            }

            var format = AudioLoader.GetSoundFormat(AudioLoader.FormatIeee, (int)channels, 32);

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(buffer, format, count, sampleRate);
        }

        private void PlatformInitializeAdpcm(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
        {
            if (!OpenALSoundController.Instance.SupportsAdpcm)
            {
                // If MS-ADPCM is not supported, convert to 16-bit signed PCM
                buffer = AudioLoader.ConvertMsAdpcmToPcm(buffer, offset, count, (int)channels, blockAlignment);
                PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                return;
            }

            var format = AudioLoader.GetSoundFormat(AudioLoader.FormatMsAdpcm, (int)channels, 0);
            int sampleAlignment = AudioLoader.SampleAlignment(format, blockAlignment);

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            // Buffer length must be aligned with the block alignment
            int alignedCount = count - (count % blockAlignment);
            SoundBuffer.BindDataBuffer(buffer, format, alignedCount, sampleRate, sampleAlignment);
        }

        private void PlatformInitializeIma4(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
        {
            if (!OpenALSoundController.Instance.SupportsIma4)
            {
                // If IMA/ADPCM is not supported, convert to 16-bit signed PCM
                buffer = AudioLoader.ConvertIma4ToPcm(buffer, offset, count, (int)channels, blockAlignment);
                PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                return;
            }

            var format = AudioLoader.GetSoundFormat(AudioLoader.FormatIma4, (int)channels, 0);
            int sampleAlignment = AudioLoader.SampleAlignment(format, blockAlignment);

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(buffer, format, count, sampleRate, sampleAlignment);
        }

        private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
        {
            var wavFormat = BitConverter.ToInt16(header, 0);
            var channels = BitConverter.ToInt16(header, 2);
            var sampleRate = BitConverter.ToInt32(header, 4);
            var blockAlignment = BitConverter.ToInt16(header, 12);
            var bitsPerSample = BitConverter.ToInt16(header, 14);

            var format = AudioLoader.GetSoundFormat(wavFormat, channels, bitsPerSample);
            PlatformInitializeBuffer(buffer, bufferSize, format, channels, sampleRate, blockAlignment, bitsPerSample, loopStart, loopLength);
        }

        private void PlatformInitializeBuffer(byte[] buffer, int bufferSize, ALFormat format, int channels, int sampleRate, int blockAlignment, int bitsPerSample, int loopStart, int loopLength)
        {
            switch (format)
            {
                case ALFormat.Mono8:
                case ALFormat.Mono16:
                case ALFormat.Stereo8:
                case ALFormat.Stereo16:
                    PlatformInitializePcm(buffer, 0, bufferSize, bitsPerSample, sampleRate, (AudioChannels)channels, loopStart, loopLength);
                    break;
                case ALFormat.MonoMSAdpcm:
                case ALFormat.StereoMSAdpcm:
                    PlatformInitializeAdpcm(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, blockAlignment, loopStart, loopLength);
                    break;
                case ALFormat.MonoFloat32:
                case ALFormat.StereoFloat32:
                    PlatformInitializeIeeeFloat(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, loopStart, loopLength);
                    break;
                case ALFormat.MonoIma4:
                case ALFormat.StereoIma4:
                    PlatformInitializeIma4(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, blockAlignment, loopStart, loopLength);
                    break;
                default:
                    throw new NotSupportedException("Unsupported wave format!");
            }
        }

        private void PlatformInitializeXact(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength, out TimeSpan duration)
        {
            if (codec == MiniFormatTag.Adpcm)
            {
                PlatformInitializeAdpcm(buffer, 0, buffer.Length, sampleRate, (AudioChannels)channels, (blockAlignment + 22) * channels, loopStart, loopLength);
                duration = TimeSpan.FromSeconds(SoundBuffer.Duration);
                return;
            }

            throw new NotSupportedException("Unsupported sound format!");
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
            if (!OpenALSoundController.Efx.IsInitialized)
                return;

            if (ReverbEffect != 0)
                return;
            
            var efx = OpenALSoundController.Efx;
            efx.GenAuxiliaryEffectSlots (1, out ReverbSlot);
            efx.GenEffect (out ReverbEffect);
            efx.Effect (ReverbEffect, EfxEffecti.EffectType, (int)EfxEffectType.Reverb);
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbReflectionsDelay, reverbSettings.ReflectionsDelayMs / 1000.0f);
            efx.Effect (ReverbEffect, EfxEffectf.LateReverbDelay, reverbSettings.ReverbDelayMs / 1000.0f);
            // map these from range 0-15 to 0-1
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbDiffusion, reverbSettings.EarlyDiffusion / 15f);
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbDiffusion, reverbSettings.LateDiffusion / 15f);
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbGainLF, Math.Min (XactHelpers.ParseVolumeFromDecibels (reverbSettings.LowEqGain - 8f), 1.0f));
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbLFReference, (reverbSettings.LowEqCutoff * 50f) + 50f);
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbGainHF, XactHelpers.ParseVolumeFromDecibels (reverbSettings.HighEqGain - 8f));
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbHFReference, (reverbSettings.HighEqCutoff * 500f) + 1000f);
            // According to Xamarin docs EaxReverbReflectionsGain Unit: Linear gain Range [0.0f .. 3.16f] Default: 0.05f
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbReflectionsGain, Math.Min (XactHelpers.ParseVolumeFromDecibels (reverbSettings.ReflectionsGainDb), 3.16f));
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbGain, Math.Min (XactHelpers.ParseVolumeFromDecibels (reverbSettings.ReverbGainDb), 1.0f));
            // map these from 0-100 down to 0-1
            efx.Effect (ReverbEffect, EfxEffectf.EaxReverbDensity, reverbSettings.DensityPct / 100f);
            efx.AuxiliaryEffectSlot (ReverbSlot, EfxEffectSlotf.EffectSlotGain, reverbSettings.WetDryMixPct / 200f);

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

            efx.BindEffectToAuxiliarySlot (ReverbSlot, ReverbEffect);
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

        internal static void PlatformInitialize()
        {
            OpenALSoundController.EnsureInitialized();
        }

        internal static void PlatformShutdown()
        {
            if (ReverbEffect != 0)
            {
                OpenALSoundController.Efx.DeleteAuxiliaryEffectSlot((int)ReverbSlot);
                OpenALSoundController.Efx.DeleteEffect((int)ReverbEffect);
            }
            OpenALSoundController.DestroyInstance();
        }
    }
}

