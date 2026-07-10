// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance
    {
        private Queue<AudioBuffer> _queuedBuffers;
        private Queue<byte[]> _pooledBuffers;
        private static ByteBufferPool _bufferPool = new ByteBufferPool();

        private void PlatformCreate()
        {
            _format = new WaveFormat(_sampleRate, (int)_channels);
            _voice = new SourceVoice(SoundEffect.Device, _format, true);
            _voice.BufferEnd += OnBufferEnd;
            _queuedBuffers = new Queue<AudioBuffer>();
            _pooledBuffers = new Queue<byte[]>();
        }

        private int PlatformGetPendingBufferCount()
        {
            return _queuedBuffers.Count;
        }

        private void PlatformPlay()
        {
            _voice.Start();
        }

        private void PlatformPause()
        {
            _voice.Stop();
        }

        private void PlatformResume()
        {
            _voice.Start();
        }

        private void PlatformStop()
        {
            _voice.Stop();

            // Dequeue all the submitted buffers
            _voice.FlushSourceBuffers();
        }

        private void PlatformSubmitBuffer(byte[] buffer, int offset, int count)
        {
            // XAudio2 callbacks are sent from a different thread,
            // so we need to lock our queues
            lock (_queuedBuffers)
            {
                if (_queuedBuffers.Count >= 64)
                    throw new ArgumentException("You have reached the limit of 64 queued buffers of XAudio2. Please use PendingBufferCount to avoid submitting that many buffers.");

                // we need to copy so datastream does not pin the buffer that the user might modify later
                byte[] pooledBuffer;
                pooledBuffer = _bufferPool.Get(count);
                _pooledBuffers.Enqueue(pooledBuffer);
                Buffer.BlockCopy(buffer, offset, pooledBuffer, 0, count);

                var stream = DataStream.Create(pooledBuffer, true, false, 0, true);
                var audioBuffer = new AudioBuffer(stream);
                audioBuffer.AudioBytes = count;

                _voice.SubmitSourceBuffer(audioBuffer, null);
                _queuedBuffers.Enqueue(audioBuffer);
            }
        }

        private void PlatformUpdateQueue()
        {
            // The XAudio implementation utilizes callbacks, so no work here.
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                lock (_queuedBuffers)
                {
                    while (_queuedBuffers.Count > 0)
                    {
                        var buffer = _queuedBuffers.Dequeue();
                        buffer.Stream.Dispose();
                        _bufferPool.Return(_pooledBuffers.Dequeue());
                    }
                }
            }
            // _voice is disposed by SoundEffectInstance.PlatformDispose
        }

        private void OnBufferEnd(IntPtr obj)
        {
            // XAudio2 callbacks are sent from a different thread,
            // so we need to lock our queues
            lock (_queuedBuffers)
            {
                // Release the buffer
                if (_queuedBuffers.Count > 0)
                {
                    var buffer = _queuedBuffers.Dequeue();
                    buffer.Stream.Dispose();
                    _bufferPool.Return(_pooledBuffers.Dequeue());
                }

                CheckBufferCount();
            }
        }

    }
}
