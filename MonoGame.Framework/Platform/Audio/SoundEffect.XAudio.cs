// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

using SharpDX;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;

namespace Microsoft.Xna.Framework.Audio
{
    partial class SoundEffect
    {
        // These platforms are only limited by memory.
        internal const int MAX_PLAYING_INSTANCES = int.MaxValue;

        #region Static Fields & Properties

        internal static XAudio2 Device { get; private set; }
        internal static MasteringVoice MasterVoice { get; private set; }

        private static X3DAudio _device3D;
        private static bool _device3DDirty = true;
        private static Speakers _speakers = Speakers.Stereo;

        // XNA does not expose this, but it exists in X3DAudio.
        public static Speakers Speakers
        {
            get { return _speakers; }

            set
            {
                if (_speakers == value)
                    return;
                
                _speakers = value;
                _device3DDirty = true;
            }
        }

        internal static X3DAudio Device3D
        {
            get
            {
                if (_device3DDirty)
                {
                    _device3DDirty = false;
                    _device3D = new X3DAudio(_speakers);
                }

                return _device3D;
            }
        }


        private static SubmixVoice _reverbVoice;

        internal static SubmixVoice ReverbVoice
        {
            get
            {
                if (_reverbVoice == null)
                {
                    var details = MasterVoice.VoiceDetails;
                    _reverbVoice = new SubmixVoice(Device, details.InputChannelCount, details.InputSampleRate);

                    var reverb = new SharpDX.XAudio2.Fx.Reverb(Device);
                    var desc = new EffectDescriptor(reverb);
                    desc.InitialState = true;
                    desc.OutputChannelCount = details.InputChannelCount;
                    _reverbVoice.SetEffectChain(desc);
                }

                return _reverbVoice;
            }
        }

        #endregion

        internal DataStream _dataStream;
        internal AudioBuffer _buffer;
        internal AudioBuffer _loopedBuffer;
        internal WaveFormat _format;

        #region Initialization

        /// <summary>
        /// Initializes XAudio.
        /// </summary>
        internal static void PlatformInitialize()
        {
            try
            {
                if (Device == null)
                {
#if DEBUG
                    try
                    {
                        //Fails if the XAudio2 SDK is not installed
                        Device = new XAudio2(XAudio2Flags.DebugEngine, ProcessorSpecifier.DefaultProcessor);
                        Device.StartEngine();
                    }
                    catch
#endif
                    {
                        Device = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.DefaultProcessor);
                        Device.StartEngine();
                    }
                }

                // Just use the default device.
                const int deviceId = 0;

                if (MasterVoice == null)
                {
                    // Let windows autodetect number of channels and sample rate.
                    MasterVoice = new MasteringVoice(Device, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate);
                }

                // The autodetected value of MasterVoice.ChannelMask corresponds to the speaker layout.
                Speakers = Device.Version == XAudio2Version.Version27 ?
                    Device.GetDeviceDetails(deviceId).OutputFormat.ChannelMask:
                    (Speakers) MasterVoice.ChannelMask;
            }
            catch
            {
                // Release the device and null it as
                // we have no audio support.
                if (Device != null)
                {
                    Device.Dispose();
                    Device = null;
                }

                MasterVoice = null;
            }
        }

        private static DataStream ToDataStream(int offset, byte[] buffer, int length)
        {
            // NOTE: We make a copy here because old versions of 
            // DataStream.Create didn't work correctly for offsets.
            var data = new byte[length];
            Buffer.BlockCopy(buffer, offset, data, 0, length);

            return DataStream.Create(data, true, false);
        }

        private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            CreateBuffers(  new WaveFormat(sampleRate, sampleBits, (int)channels),
                            ToDataStream(offset, buffer, count),
                            loopStart, 
                            loopLength);
        }

        private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
        {
            var format = BitConverter.ToInt16(header, 0);
            var channels = BitConverter.ToInt16(header, 2);
            var sampleRate = BitConverter.ToInt32(header, 4);
            var blockAlignment = BitConverter.ToInt16(header, 12);
            var sampleBits = BitConverter.ToInt16(header, 14);

            WaveFormat waveFormat;
            if (format == 1)
                waveFormat = new WaveFormat(sampleRate, sampleBits, channels);
            else if (format == 2)
                waveFormat = new WaveFormatAdpcm(sampleRate, channels, blockAlignment);
            else if (format == 3)
                waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            else
                throw new NotSupportedException("Unsupported wave format!");

            CreateBuffers(  waveFormat,
                            ToDataStream(0, buffer, bufferSize),
                            loopStart,
                            loopLength);
        }

        private void PlatformInitializeXact(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength, out TimeSpan duration)
        {
            if (codec == MiniFormatTag.Adpcm)
            {
                duration = TimeSpan.FromSeconds((float)loopLength / sampleRate);

                CreateBuffers(  new WaveFormatAdpcm(sampleRate, channels, (blockAlignment + 22) * channels),
                                ToDataStream(0, buffer, buffer.Length),
                                loopStart,
                                loopLength);

                return;
            }

            throw new NotSupportedException("Unsupported sound format!");
        }

        private void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
        {
            SoundStream soundStream = null;
            try
            {
                soundStream = new SoundStream(stream);
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException("Ensure that the specified stream contains valid PCM or IEEE Float wave data.", ex);
            }

            var dataStream = soundStream.ToDataStream();
            int sampleCount = 0;
            switch (soundStream.Format.Encoding)
            {
                case WaveFormatEncoding.Adpcm:
                    {
                        var samplesPerBlock = (soundStream.Format.BlockAlign / soundStream.Format.Channels - 7) * 2 + 2;
                        sampleCount = ((int)dataStream.Length / soundStream.Format.BlockAlign) * samplesPerBlock;
                    }
                    break;
                case WaveFormatEncoding.Pcm:
                case WaveFormatEncoding.IeeeFloat:
                    sampleCount = (int)(dataStream.Length / ((soundStream.Format.Channels * soundStream.Format.BitsPerSample) / 8));
                    break;
                default:
                    throw new ArgumentException("Ensure that the specified stream contains valid PCM, MS-ADPCM or IEEE Float wave data.");
            }

            CreateBuffers(soundStream.Format, dataStream, 0, sampleCount);

            duration = TimeSpan.FromSeconds((float)sampleCount / (float)soundStream.Format.SampleRate);
        }

        private void CreateBuffers(WaveFormat format, DataStream dataStream, int loopStart, int loopLength)
        {
            _format = format;
            _dataStream = dataStream;

            _buffer = new AudioBuffer
            {
                Stream = _dataStream,
                AudioBytes = (int)_dataStream.Length,
                Flags = BufferFlags.EndOfStream,
                PlayBegin = loopStart,
                PlayLength = loopLength,
                Context = new IntPtr(42),
            };

            _loopedBuffer = new AudioBuffer
            {
                Stream = _dataStream,
                AudioBytes = (int)_dataStream.Length,
                Flags = BufferFlags.EndOfStream,
                LoopBegin = loopStart,
                LoopLength = loopLength,
                LoopCount = AudioBuffer.LoopInfinite,
                Context = new IntPtr(42),
            };
        }

        private void PlatformSetupInstance(SoundEffectInstance inst)
        {
            // If the instance came from the pool then it could
            // already have a valid voice assigned.
            var voice = inst._voice;

            if (voice != null)
            {
                // TODO: This really shouldn't be here.  Instead we should fix the 
                // SoundEffectInstancePool to internally to look for a compatible
                // instance or return a new instance without a voice.
                //
                // For now we do the same test that the pool should be doing here.
             
                if (!ReferenceEquals(inst._format, _format))
                {
                    if (inst._format.Encoding != _format.Encoding ||
                        inst._format.Channels != _format.Channels ||
                        inst._format.SampleRate != _format.SampleRate ||
                        inst._format.BitsPerSample != _format.BitsPerSample)
                    {
                        voice.DestroyVoice();
                        voice.Dispose();
                        voice = null;
                    }
                }
            }

            if (voice == null && Device != null)
            {
                voice = new SourceVoice(Device, _format, VoiceFlags.UseFilter, XAudio2.MaximumFrequencyRatio);
                inst._voice = voice;
                inst.UpdateOutputMatrix(); // Ensure the output matrix is set for this new voice
            }

            inst._format = _format;
        }

        #endregion

        internal static void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
            // All parameters related to sampling rate or time are relative to a 48kHz 
            // voice and must be scaled for use with other sampling rates.
            var timeScale = 48000.0f / ReverbVoice.VoiceDetails.InputSampleRate;

            var settings = new SharpDX.XAudio2.Fx.ReverbParameters
            {
                ReflectionsGain = reverbSettings.ReflectionsGainDb,
                ReverbGain = reverbSettings.ReverbGainDb,
                DecayTime = reverbSettings.DecayTimeSec,
                ReflectionsDelay = (byte)(reverbSettings.ReflectionsDelayMs * timeScale),
                ReverbDelay = (byte)(reverbSettings.ReverbDelayMs * timeScale),
                RearDelay = (byte)(reverbSettings.RearDelayMs * timeScale),
                RoomSize = reverbSettings.RoomSizeFeet,
                Density = reverbSettings.DensityPct,
                LowEQGain = (byte)reverbSettings.LowEqGain,
                LowEQCutoff = (byte)reverbSettings.LowEqCutoff,
                HighEQGain = (byte)reverbSettings.HighEqGain,
                HighEQCutoff = (byte)reverbSettings.HighEqCutoff,
                PositionLeft = (byte)reverbSettings.PositionLeft,
                PositionRight = (byte)reverbSettings.PositionRight,
                PositionMatrixLeft = (byte)reverbSettings.PositionLeftMatrix,
                PositionMatrixRight = (byte)reverbSettings.PositionRightMatrix,
                EarlyDiffusion = (byte)reverbSettings.EarlyDiffusion,
                LateDiffusion = (byte)reverbSettings.LateDiffusion,
                RoomFilterMain = reverbSettings.RoomFilterMainDb,
                RoomFilterFreq = reverbSettings.RoomFilterFrequencyHz * timeScale,
                RoomFilterHF = reverbSettings.RoomFilterHighFrequencyDb,
                WetDryMix = reverbSettings.WetDryMixPct
            };

            ReverbVoice.SetEffectParameters(0, settings);
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                if (_dataStream != null)
                    _dataStream.Dispose();
            }
            _dataStream = null;
        }

        internal static void PlatformShutdown()
        {
            if (_reverbVoice != null)
            {
                _reverbVoice.DestroyVoice();
                _reverbVoice.Dispose();
                _reverbVoice = null;
            }

            if (MasterVoice != null)
            {
                MasterVoice.Dispose();
                MasterVoice = null;
            }

            if (Device != null)
            {
                Device.StopEngine();
                Device.Dispose();
                Device = null;
            }

            _device3DDirty = true;
            _speakers = Speakers.Stereo;
        }
    }
}

