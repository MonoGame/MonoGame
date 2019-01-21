// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NUnit.Framework;

namespace MonoGame.Tests.Audio
{
    class DynamicSoundEffectInstanceTest
    {
        [SetUp]
        public void SetUp()
        {
            // Necessary to get audio initialised
            FrameworkDispatcher.Update();
        }

        [Test]
#if DESKTOPGL
        [Ignore("Intermetent failure on first run of test. Needs investigating")]
#endif
        public void BufferNeeded_DuringPlayback()
        {
            // XNA raises the event every time a buffer is consumed and there are less than two left.

            using (var instance = new DynamicSoundEffectInstance(8000, AudioChannels.Mono))
            {
                instance.BufferNeeded += BufferNeededEventHandler;
                instance.SubmitBuffer(GenerateSineWave(880, 8000, 1, 0.1f));
                instance.SubmitBuffer(GenerateSineWave(880, 8000, 1, 0.1f));
                instance.SubmitBuffer(GenerateSineWave(880, 8000, 1, 0.1f));

                var previousEventCount = _bufferNeededEventCount;
                instance.Play();
                SleepWhileDispatching(350);
                Assert.AreEqual(3, _bufferNeededEventCount - previousEventCount);

                // The event is raised on the same thread as FrameworkDispatcher.Update() is called.
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, _bufferNeededEventThread);
            }
        }

        [Test]
        public void BufferNeeded_MultipleConsumed()
        {
            // Both buffers should be consumed by the time the event routine is called by XNA.
            // This test verifies that each consumed buffer raises its own event.

            using (var instance = new DynamicSoundEffectInstance(8000, AudioChannels.Mono))
            {
                instance.BufferNeeded += BufferNeededEventHandler;
                instance.SubmitBuffer(GenerateSineWave(880, 8000, 1, 0.05f));
                instance.SubmitBuffer(GenerateSineWave(880, 8000, 1, 0.05f));

                var previousEventCount = _bufferNeededEventCount;
                instance.Play();
                
                Thread.Sleep(125);
                SleepWhileDispatching(10);

                Assert.AreEqual(3, _bufferNeededEventCount - previousEventCount);
            }
        }

        [Test]
        public void BufferNeeded_MoreThanThree()
        {
            // No events are raised when a buffer is consumed and there are more than 3 buffers submitted.

            using (var instance = new DynamicSoundEffectInstance(8000, AudioChannels.Mono))
            {
                instance.BufferNeeded += BufferNeededEventHandler;
                instance.SubmitBuffer(GenerateSineWave(880, 8000, 1, 0.25f));
                instance.SubmitBuffer(GenerateSineWave(880, 8000, 1, 0.25f));
                instance.SubmitBuffer(GenerateSineWave(880, 8000, 1, 0.05f));
                instance.SubmitBuffer(GenerateSineWave(880, 8000, 1, 0.05f));

                var previousEventCount = _bufferNeededEventCount;
                instance.Play();
                
                SleepWhileDispatching(300);

                Assert.AreEqual(0, _bufferNeededEventCount - previousEventCount);
            }
        }

        [Test]
        public void BufferNeeded_Play_NoneSubmitted()
        {
            using (var instance = new DynamicSoundEffectInstance(8000, AudioChannels.Mono))
            {
                instance.BufferNeeded += BufferNeededEventHandler;

                var previousEventCount = _bufferNeededEventCount;
                instance.Play();
                SleepWhileDispatching(20);
                Assert.AreEqual(1, _bufferNeededEventCount - previousEventCount);

                // The event is raised on the same thread as FrameworkDispatcher.Update() is called.
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, _bufferNeededEventThread);
            }
        }

        [Test]
        public void BufferNeeded_Play_AlreadySubmitted()
        {
            using (var instance = new DynamicSoundEffectInstance(8000, AudioChannels.Mono))
            {
                instance.BufferNeeded += BufferNeededEventHandler;
                instance.SubmitBuffer(GenerateSineWave(440, 8000, 1, 0.1f));

                var previousEventCount = _bufferNeededEventCount;
                instance.Play();
                SleepWhileDispatching(20);
                Assert.AreEqual(1, _bufferNeededEventCount - previousEventCount);
            }
        }

        [Test]
        public void BufferNeeded_Play_NoDispatcherCalled()
        {
            // No event is raised if FrameworkDispatcher.Update() is not called.

            using (var instance = new DynamicSoundEffectInstance(8000, AudioChannels.Mono))
            {
                instance.BufferNeeded += BufferNeededEventHandler;

                var previousEventCount = _bufferNeededEventCount;
                instance.Play();

                Thread.Sleep(20);

                Assert.AreEqual(0, _bufferNeededEventCount - previousEventCount);
            }
        }

        static int _bufferNeededEventCount = 0;
        static int _bufferNeededEventThread = 0;
        private static void BufferNeededEventHandler(object sender, EventArgs e)
        {
            _bufferNeededEventCount++;
            _bufferNeededEventThread = Thread.CurrentThread.ManagedThreadId;
        }

        [Test]
        public void Ctor()
        {
            // Valid sample rates
            var instance = new DynamicSoundEffectInstance(48000, AudioChannels.Stereo);
            instance.Dispose();
            instance = new DynamicSoundEffectInstance(8000, AudioChannels.Stereo);
            instance.Dispose();

            // Invalid sample rates
            Assert.Throws<ArgumentOutOfRangeException>(() => { instance = new DynamicSoundEffectInstance(7999, AudioChannels.Stereo); });
            if (instance != null) instance.Dispose();
            Assert.Throws<ArgumentOutOfRangeException>(() => { instance = new DynamicSoundEffectInstance(48001, AudioChannels.Stereo); });
            if (instance != null) instance.Dispose();

            // Valid channel counts
            instance = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);
            instance.Dispose();

            instance = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);
            instance.Dispose();

            // Invalid channel count
            Assert.Throws<ArgumentOutOfRangeException>(() => { instance = new DynamicSoundEffectInstance(44100, (AudioChannels)123); });
            if (instance != null) instance.Dispose();

        }

        [Test]
        public void GetSampleDuration()
        {
            var monoInstance = new DynamicSoundEffectInstance(8000, AudioChannels.Mono);
            var stereoInstance = new DynamicSoundEffectInstance(24000, AudioChannels.Stereo);

            // Zero length
            Assert.AreEqual(TimeSpan.Zero, monoInstance.GetSampleDuration(0));
            Assert.AreEqual(TimeSpan.Zero, stereoInstance.GetSampleDuration(0));

            // Nonzero length
            Assert.AreEqual(TimeSpan.FromSeconds(1), monoInstance.GetSampleDuration(16000));
            Assert.AreEqual(TimeSpan.FromSeconds(1), stereoInstance.GetSampleDuration(96000));

            // Length not aligned with format
            Assert.AreEqual(TimeSpan.FromMilliseconds(1), stereoInstance.GetSampleDuration(97));

            // Negative length
            Assert.Throws<ArgumentException>(() => { monoInstance.GetSampleDuration(-1); });

            // Disposed
            monoInstance.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { monoInstance.GetSampleDuration(0); });

            stereoInstance.Dispose();
        }

        [Test]
        public void GetSampleSizeInBytes()
        {
            var monoInstance = new DynamicSoundEffectInstance(48000, AudioChannels.Mono);
            var stereoInstance = new DynamicSoundEffectInstance(22050, AudioChannels.Stereo);

            // Zero length
            Assert.AreEqual(0, monoInstance.GetSampleSizeInBytes(TimeSpan.Zero));
            Assert.AreEqual(0, stereoInstance.GetSampleSizeInBytes(TimeSpan.Zero));

            // Nonzero length
            Assert.AreEqual(96000, monoInstance.GetSampleSizeInBytes(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(88200, stereoInstance.GetSampleSizeInBytes(TimeSpan.FromSeconds(1)));

            // Negative length
            Assert.Throws<ArgumentOutOfRangeException>(() => { monoInstance.GetSampleSizeInBytes(TimeSpan.FromSeconds(-1)); });

            // Disposed
            monoInstance.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { monoInstance.GetSampleSizeInBytes(TimeSpan.Zero); });

            stereoInstance.Dispose();
        }

        [Test]
        public void IsLooped()
        {
            var instance = new DynamicSoundEffectInstance(24000, AudioChannels.Mono);

            // Always returns false and cannot be set true
            Assert.IsFalse(instance.IsLooped);
            instance.IsLooped = false; // Setting it to false does not throw, however
            Assert.Throws<InvalidOperationException>(() => { instance.IsLooped = true; });

            instance.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { instance.IsLooped = false; });
        }

        [Test]
        public void PendingBufferCount()
        {
            var instance = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);
            Assert.AreEqual(0, instance.PendingBufferCount);

            instance.SubmitBuffer(GenerateSineWave(440, 44100, 2, 1.0f));
            Assert.AreEqual(1, instance.PendingBufferCount);

            instance.Play();
            SleepWhileDispatching(1050); // Give it time to finish
            Assert.AreEqual(0, instance.PendingBufferCount);

            // Throws ObjectDisposedException
            instance.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { var a = instance.PendingBufferCount; });
        }

        [Test]
        public void Playback()
        {
            using (var instance = new DynamicSoundEffectInstance(48000, AudioChannels.Mono))
            {
                // Initially, the playback is stopped
                Assert.AreEqual(SoundState.Stopped, instance.State);

                // Submitting a buffer will not change the state
                instance.SubmitBuffer(GenerateSineWave(440, 48000, 1, 0.5f));
                Assert.AreEqual(SoundState.Stopped, instance.State);

                // Start playing
                instance.Play();
                Assert.AreEqual(SoundState.Playing, instance.State);

                // While still playing, pause the playback
                SleepWhileDispatching(300);
                instance.Pause();
                Assert.AreEqual(SoundState.Paused, instance.State);

                // Let it continue and run out of buffers
                instance.Resume();
                SleepWhileDispatching(300);
                Assert.AreEqual(0, instance.PendingBufferCount);
                Assert.AreEqual(SoundState.Playing, instance.State);

                // Submit a buffer and the playback should continue
                instance.SubmitBuffer(GenerateSineWave(466, 48000, 1, 1.0f));
                Assert.AreEqual(SoundState.Playing, instance.State);
                SleepWhileDispatching(500);

                // Stop immediately
                Assert.AreEqual(SoundState.Playing, instance.State);
                instance.Stop();
                SleepWhileDispatching(10); // XNA does not stop it until FrameworkDispatcher.Update is called
                Assert.AreEqual(SoundState.Stopped, instance.State);

                // And then resume
                instance.Resume();
                Assert.AreEqual(SoundState.Playing, instance.State);
            }
        }

        [Test]
        public void Playback_Exceptions()
        {
            var instance = new DynamicSoundEffectInstance(16000, AudioChannels.Mono);

            instance.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { instance.Play(); });
            Assert.Throws<ObjectDisposedException>(() => { instance.Pause(); });
            Assert.Throws<ObjectDisposedException>(() => { instance.Resume(); });
            Assert.Throws<ObjectDisposedException>(() => { instance.Stop(); });
            Assert.Throws<ObjectDisposedException>(() => { instance.Stop(false); });
            Assert.Throws<ObjectDisposedException>(() => { instance.SubmitBuffer(new byte[0]); });
        }

        [Test]
        public void Stop_False()
        {
            // Calling Stop(false) has no effect

            using (var instance = new DynamicSoundEffectInstance(12000, AudioChannels.Mono))
            {
                instance.Play();
                Assert.AreEqual(SoundState.Playing, instance.State);

                instance.Stop(false);
                SleepWhileDispatching(20);
                Assert.AreEqual(SoundState.Playing, instance.State);
            }
        }

        [Test]
        public void Stop_RemovesBuffers()
        {
            using (var instance = new DynamicSoundEffectInstance(12000, AudioChannels.Mono))
            {
                instance.SubmitBuffer(GenerateSineWave(440, 12000, 1, 0.1f));
                instance.SubmitBuffer(GenerateSineWave(440, 12000, 1, 0.1f));
                instance.SubmitBuffer(GenerateSineWave(440, 12000, 1, 0.1f));
                Assert.AreEqual(3, instance.PendingBufferCount);
                
                instance.Stop();
                SleepWhileDispatching(20);
                Assert.AreEqual(0, instance.PendingBufferCount);
            }
        }

        [Test]
        public void SubmitBuffer_ParameterValidation_SimpleOverload()
        {
            using (var instance = new DynamicSoundEffectInstance(16000, AudioChannels.Stereo))
            {
                // Null or empty buffer - with different null behavior to the other overload
                Assert.Throws<NullReferenceException>(() => { instance.SubmitBuffer(null); });
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(new byte[0]); });

                // Invalid alignment
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(new byte[1]); });
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(new byte[2]); });
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(new byte[3]); });
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(new byte[13]); });

                // Correct alignment and size
                instance.SubmitBuffer(GenerateSineWave(440, 16000, 2, 0.1f));
            }
        }

        [Test]
        public void SubmitBuffer_ParameterValidation_ComplexOverload()
        {
            using (var instance = new DynamicSoundEffectInstance(16000, AudioChannels.Stereo))
            {
                // Null or empty buffer - with different null behavior to the other overload
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(null, 0, 4); });
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(new byte[0], 0, 4); });

                var buffer = GenerateSineWave(440, 16000, 2, 0.5f);

                // Correct alignment
                instance.SubmitBuffer(buffer, 0, 4); // One sample per channel
                instance.SubmitBuffer(buffer, 1000, 1000); // 250 samples

                // Invalid alignment
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(buffer, 0, 3); });
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(buffer, 1, 4); }); // Unaligned start position also throws

                // Invalid size
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(buffer, 0, 0); });
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(buffer, 0, -1); });
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(buffer, 0, buffer.Length + 1); });
                Assert.Throws<ArgumentException>(() => { instance.SubmitBuffer(buffer, buffer.Length - 4, 8); });
            }
        }

        [Test]
        public void SubmitBuffer_ShouldNotThrowOnStrangeOffset()
        {
            using (var instance = new DynamicSoundEffectInstance(16000, AudioChannels.Stereo))
            {
                var buffer = GenerateSineWave(440, 16000, 2, 0.5f);

                Assert.DoesNotThrow(() => { instance.SubmitBuffer(buffer, 0, 8); });
                Assert.DoesNotThrow(() => { instance.SubmitBuffer(buffer, 8, 8); });
                Assert.DoesNotThrow(() => { instance.SubmitBuffer(buffer, 16, 8); });
                Assert.DoesNotThrow(() => { instance.SubmitBuffer(buffer, 24, 8); });
                Assert.DoesNotThrow(() => { instance.SubmitBuffer(buffer, 3200, 8); });
                Assert.DoesNotThrow(() => { instance.SubmitBuffer(buffer, 8000, 8); });
            }
        }

        /// <summary>
        /// Sleeps for the specified amount of time while calling FrameworkDispatcher.Update() every 10 ms.
        /// </summary>
        private static void SleepWhileDispatching(int ms)
        {
            int cycles = ms / 10;
            for (int i = 0; i < cycles; i++)
            {
                FrameworkDispatcher.Update();
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Generates a audio buffer filled with a single frequency sine wave.
        /// </summary>
        /// <param name="frequency">Frequency in Hz.</param>
        /// <param name="sampleRate">Samples per second.</param>
        /// <param name="channelCount">Number of channels.</param>
        /// <param name="length">Length in seconds.</param>
        /// <returns>An audio buffer of sufficient size, filled with sine wave.</returns>
        private static byte[] GenerateSineWave(float frequency, int sampleRate, int channelCount, float length)
        {
            var sampleCount = (int)(sampleRate * length);
            var samples = new short[sampleCount * channelCount];
            var onePerSampleRate = 1.0f / sampleRate;

            for (int i = 0; i < sampleCount; i++)
            {
                var sample = Math.Sin(2 * Math.PI * frequency * onePerSampleRate * i) * 0.2;
                var sampleAsShort = (short)(sample * short.MaxValue);

                // Fill each channel
                for (int channel = 0; channel < channelCount; channel++)
                    samples[i * channelCount + channel] = sampleAsShort;
            }

            var byteArray = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                var bytes = BitConverter.GetBytes(samples[i]);
                byteArray[i * 2] = bytes[0];
                byteArray[i * 2 + 1] = bytes[1];
            }

            return byteArray;
        }
    }
}
