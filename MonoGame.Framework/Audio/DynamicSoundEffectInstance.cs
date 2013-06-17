using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if MONOMAC
using MonoMac.OpenAL;
#elif DIRECTX
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
#else
using OpenTK.Audio.OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public sealed class DynamicSoundEffectInstance : SoundEffectInstance
    {
#if DIRECTX
        private XAudio2 _xaudio2;
        private WaveFormat _waveFormat;
        private MasteringVoice _masteringVoice;
        private SourceVoice _sourceVoice;
        private bool _isPlaying;
        private Queue<DataStream> _dataStreams;
#else
        private ALFormat format;
#endif
        private AudioChannels _channels;
        private int _sampleRate;
        private bool _disposed;

        public new bool IsLooped { get; set; }
        public int PendingBufferCount { get; set; }

#if DIRECTX
        public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels)
        {
            _sampleRate = sampleRate;
            _channels = channels;
            _dataStreams = new Queue<DataStream>(); 
            _waveFormat = new WaveFormat(sampleRate, channels == AudioChannels.Mono ? 1 : 2);
            _xaudio2 = new XAudio2();
            _masteringVoice = new MasteringVoice(_xaudio2);
            _sourceVoice = new SourceVoice(_xaudio2, _waveFormat, true);
            _sourceVoice.BufferEnd += ptr => OnBufferNeeded(new EventArgs());
        }
#else
        public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels)
        {
            _sampleRate = sampleRate;
            _channels = channels;
            switch (channels)
            {
                case AudioChannels.Mono:
                    _format = ALFormat.Mono16;
                    break;
                case AudioChannels.Stereo:
                    _format = ALFormat.Stereo16;
                    break;
                default:
                    break;
            }                       
        }
#endif

        public event EventHandler<EventArgs> BufferNeeded;

        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            throw new NotImplementedException();
        }

        public int GetSampleSizeInBytes(TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        public new void Play()
        {
            throw new NotImplementedException();
        }

        public void SubmitBuffer(byte[] buffer)
        {
            SubmitBuffer(buffer, 0, buffer.Length);
        }

#if DIRECTX
        public void SubmitBuffer(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("DynamicSoundEffectInstance");

            var audioStream = DataStream.Create(buffer, true, true);
            var audioBuffer = new AudioBuffer
                             {
                                 Stream = audioStream,
                                 AudioBytes = count,
                                 Flags = BufferFlags.None
                             };
            _dataStreams.Enqueue(audioStream);
            _sourceVoice.SubmitSourceBuffer(audioBuffer, null);
            if (!_isPlaying)
            {
                _sourceVoice.Start();
                _isPlaying = true;
            }
        }
#else
        public void SubmitBuffer(byte[] buffer, int offset, int count)
        {
            BindDataBuffer(buffer, format, count, _sampleRate);
        }
#endif

#if DIRECTX
        internal void OnBufferNeeded(EventArgs args)
        {
            if (_dataStreams.Count > 0)
            {
                var dataStream = _dataStreams.Dequeue();
                if (dataStream != null) dataStream.Dispose();
            }

            var bufferNeeded = BufferNeeded;
            if (bufferNeeded != null)
            {
                bufferNeeded(this, args);
            }
        }
#else
        internal void OnBufferNeeded(EventArgs args)
        {
            var bufferNeeded = BufferNeeded;
            if (bufferNeeded != null)
            {
                bufferNeeded(this, args);
            }
        }
#endif

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _isPlaying = false;
#if DIRECTX
                    _xaudio2.Dispose();
                    _masteringVoice.Dispose();
                    _sourceVoice.Dispose();
                    while (_dataStreams.Count > 0)
                    {
                        _dataStreams.Dequeue().Dispose();
                    }
#endif
                }
#if DIRECTX
                _xaudio2 = null;
                _masteringVoice = null;
                _sourceVoice = null;
                _waveFormat = null;
                _dataStreams = null;
#endif
                _disposed = true;
            }
        }
    }
}
