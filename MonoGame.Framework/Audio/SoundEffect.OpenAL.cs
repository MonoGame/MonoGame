// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.IO;

#if MONOMAC && PLATFORM_MACOS_LEGACY
using MonoMac.AudioToolbox;
using MonoMac.AudioUnit;
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

        #region Public Constructors

        private void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
        {
            byte[] buffer;

            ALFormat format;
            int freq;
            int blockAlignment;
            int bitsPerSample;
            int samplesPerBlock;
            int sampleCount;
            buffer = AudioLoader.Load(stream, out format, out freq, out blockAlignment, out bitsPerSample, out samplesPerBlock, out sampleCount);

            duration = TimeSpan.FromSeconds((float)sampleCount / (float)freq);

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(buffer, format, buffer.Length, freq, samplesPerBlock);
        }

        private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            var format = AudioLoader.GetSoundFormat(1, (int)channels, sampleBits);

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(buffer, format, count, sampleRate);
        }

        private void PlatformInitializeIeeeFloat(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            var format = AudioLoader.GetSoundFormat(3, (int)channels, 32);

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(buffer, format, count, sampleRate);
        }

        private void PlatformInitializeAdpcm(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
        {
            var format = AudioLoader.GetSoundFormat(2, (int)channels, 0);
            int sampleAlignment = AudioLoader.SampleAlignment(format, blockAlignment);

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            // Buffer length must be aligned with the block alignment
            int alignedCount = count - (count % blockAlignment);
            SoundBuffer.BindDataBuffer(buffer, format, alignedCount, sampleRate, sampleAlignment);
        }

        private void PlatformInitializeIma4(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
        {
            var format = AudioLoader.GetSoundFormat(17, (int)channels, 0);
            int sampleAlignment = AudioLoader.SampleAlignment(format, blockAlignment);

            // bind buffer
            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(buffer, format, count, sampleRate, sampleAlignment);
        }

        private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
        {
            var format = BitConverter.ToInt16(header, 0);
            var channels = BitConverter.ToInt16(header, 2);
            var sampleRate = BitConverter.ToInt32(header, 4);
            var blockAlignment = BitConverter.ToInt16(header, 12);
            var bitsPerSample = BitConverter.ToInt16(header, 14);

            // We may need to decode MSADPCM.
            var supportsADPCM = OpenALSoundController.GetInstance.SupportsADPCM;
            if (format == 2 && !supportsADPCM)
            {
                using (var reader = new BinaryReader(new MemoryStream(buffer, 0, bufferSize)))
                {
                    buffer = MSADPCMToPCM.MSADPCM_TO_PCM(reader, (short)channels, (short)blockAlignment);
                    format = 1;
                }
            }

            switch (format)
            {
                case 1: // PCM
                    PlatformInitializePcm(buffer, 0, bufferSize, bitsPerSample, sampleRate, (AudioChannels)channels, loopStart, loopLength);
                    break;
                case 2: // MS-ADPCM
                    PlatformInitializeAdpcm(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, blockAlignment, loopStart, loopLength);
                    break;
                case 3: // IEEE Float
                    PlatformInitializeIeeeFloat(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, loopStart, loopLength);
                    break;
                case 17: // IMA/ADPCM
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
                PlatformInitializeAdpcm(buffer, 0, buffer.Length, sampleRate, (AudioChannels)channels, (blockAlignment + 16) * channels, loopStart, loopLength);
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
#if SUPPORTS_EFX
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
#endif
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

        internal static void InitializeSoundEffect()
        {
            try
            {
                // Getting the instance for the first time initializes OpenAL
                var oal = OpenALSoundController.GetInstance;
            }
            catch (DllNotFoundException ex)
            {
                throw new NoAudioHardwareException("Failed to init OpenALSoundController", ex);
            }
        }

        internal static void PlatformShutdown()
        {
#if SUPPORTS_EFX
            if (ReverbEffect != 0) {
                OpenALSoundController.Efx.DeleteAuxiliaryEffectSlot ((int)ReverbSlot);
                OpenALSoundController.Efx.DeleteEffect ((int)ReverbEffect);
            }
#endif
            OpenALSoundController.DestroyInstance();
        }
    }
}

