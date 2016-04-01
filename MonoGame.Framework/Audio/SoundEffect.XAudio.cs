// MonoGame - Copyright (C) The MonoGame Team
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
    public sealed partial class SoundEffect : IDisposable
    {
#if WINDOWS || (WINRT && !WINDOWS_PHONE)

        // These platforms are only limited by memory.
        internal const int MAX_PLAYING_INSTANCES = int.MaxValue;

#elif WINDOWS_PHONE

        // Reference: http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.instanceplaylimitexception.aspx
        internal const int MAX_PLAYING_INSTANCES = 64;

#endif

        #region Static Fields & Properties

        internal static XAudio2 Device { get; private set; }
        internal static MasteringVoice MasterVoice { get; private set; }

        private static X3DAudio _device3D;
        private static bool _device3DDirty = true;
        private static Speakers _speakers = Speakers.Stereo;

        // XNA does not expose this, but it exists in X3DAudio.
        [CLSCompliant(false)]
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

        #endregion

        internal DataStream _dataStream;
        internal AudioBuffer _buffer;
        internal AudioBuffer _loopedBuffer;
        internal WaveFormat _format;

        #region Initialization

        static SoundEffect()
        {
            InitializeSoundEffect();
        }

        internal static void InitializeSoundEffect()
        {
            try
            {
                if (Device == null)
                {
#if !WINRT && DEBUG
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
#if WINRT
                string deviceId = null;
#else
                const int deviceId = 0;
#endif

                if (MasterVoice == null)
                {
                    // Let windows autodetect number of channels and sample rate.
                    MasterVoice = new MasteringVoice(Device, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate, deviceId);
                }

                // The autodetected value of MasterVoice.ChannelMask corresponds to the speaker layout.
#if WINRT
                Speakers = (Speakers)MasterVoice.ChannelMask;
#else
                var deviceDetails = Device.GetDeviceDetails(deviceId);
                Speakers = deviceDetails.OutputFormat.ChannelMask;
#endif
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

        private void PlatformInitialize(byte[] buffer, int sampleRate, AudioChannels channels)
        {
            CreateBuffers(  new WaveFormat(sampleRate, (int)channels),
                            DataStream.Create(buffer, true, false),
                            0, 
                            buffer.Length);
        }

        private void PlatformInitialize(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            CreateBuffers(  new WaveFormat(sampleRate, (int)channels),
                            DataStream.Create(buffer, true, false, offset),
                            loopStart, 
                            loopLength);
        }

        private void PlatformLoadAudioStream(Stream s)
        {
            var soundStream = new SoundStream(s);
            var dataStream = soundStream.ToDataStream();
            var sampleLength = (int)(dataStream.Length / ((soundStream.Format.Channels * soundStream.Format.BitsPerSample) / 8));
            CreateBuffers(  soundStream.Format,
                            dataStream,
                            0,
                            sampleLength);
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
                voice = new SourceVoice(Device, _format, VoiceFlags.None, XAudio2.MaximumFrequencyRatio);

            inst._voice = voice;
            inst._format = _format;
        }

        #endregion

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
            SoundEffectInstancePool.Shutdown();

            if (MasterVoice != null)
            {
                MasterVoice.DestroyVoice();
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

