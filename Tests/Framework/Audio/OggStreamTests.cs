// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if DESKTOPGL

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoGame.OpenAL;

namespace MonoGame.Tests.Audio
{

    [Category("Audio")]
    [RunOnUiTestFixture]
    public class OggStreamTests : AudioTestFixtureBase
    {
        private const string StereoFile = "Assets/Audio/rock_loop_stereo.ogg";

        private static OggStream CreateStream(Action finishedAction = null)
        {
            OpenALSoundController.EnsureInitialized();
            return new OggStream(Path.GetFullPath(StereoFile), finishedAction);
        }

        private static int GetQueuedBuffers(OggStream stream)
        {
            int queued;
            AL.GetSource(stream.alSourceId, ALGetSourcei.BuffersQueued, out queued);
            ALHelper.CheckError("Failed to fetch queued buffers.");
            return queued;
        }

        private static int GetProcessedBuffers(OggStream stream)
        {
            int processed;
            AL.GetSource(stream.alSourceId, ALGetSourcei.BuffersProcessed, out processed);
            ALHelper.CheckError("Failed to fetch processed buffers.");
            return processed;
        }

        private static bool HasReachedEnd(OggStream stream)
        {
            lock (stream.prepareMutex)
                return stream.ReachedEnd;
        }

        [Test]
        public void SeekToPosition_WhileStreaming_DoesNotCrashStreamer()
        {
            var stream = CreateStream();
            var random = new Random(1);

            try
            {
                stream.Play();

                var stopWatch = Stopwatch.StartNew();
                while (stopWatch.Elapsed < TimeSpan.FromSeconds(1))
                    stream.SeekToPosition(TimeSpan.FromSeconds(random.NextDouble() * 7));

                // Lets the streamer restart the source and is shorter than the 0.9 sec the last seek leaves to play
                SleepWhileDispatching(500);
                Assert.AreEqual(ALSourceState.Playing, AL.GetSourceState(stream.alSourceId),
                    "The streamer did not restart the source after the last seek.");
            }
            finally
            {
                stream.Stop();
                stream.Dispose();
            }
        }

        [Test]
        public void SeekToPosition_WhileStreamerRestartsSource_DoesNotReplayAudioFromBeforeSeek()
        {
            var finished = new ManualResetEventSlim();
            var stream = CreateStream(finished.Set);

            try
            {
                stream.Play();

                var sinceSeek = new Stopwatch();
                // Hold the streamer between refilling buffers and restarting the source
                lock (stream.stopMutex)
                {
                    // A running streamer unqueues each played buffer within 0.1 sec, so two played buffers
                    // left in the queue mean it is blocked on stopMutex
                    bool streamerBlocked = SpinWait.SpinUntil(
                        () => GetProcessedBuffers(stream) >= 2,
                        TimeSpan.FromSeconds(3));
                    Assert.IsTrue(streamerBlocked, "The streamer did not block on stopMutex.");

                    stream.SeekToPosition(stream.GetLength() - TimeSpan.FromSeconds(0.3));
                    sinceSeek.Start();
                }

                Assert.IsTrue(finished.Wait(TimeSpan.FromSeconds(5)), "The stream did not finish.");
                // Replaying the buffers queued before the seek takes 1.5 sec
                Assert.Less(sinceSeek.Elapsed, TimeSpan.FromSeconds(1),
                    "Audio queued before the seek was played after it.");
            }
            finally
            {
                stream.Stop();
                stream.Dispose();
            }
        }

        [Test]
        public void SeekToPosition_AfterStreamReachedEnd_KeepsStreaming()
        {
            var stream = CreateStream();

            try
            {
                stream.Play();
                // Leave one full buffer and a short final one
                stream.SeekToPosition(stream.GetLength() - TimeSpan.FromSeconds(0.8));

                // The streamer unqueues the played full buffer without refilling it
                bool drained = WaitUntilDispatching(
                    () => HasReachedEnd(stream) && GetQueuedBuffers(stream) == 1,
                    TimeSpan.FromSeconds(2));
                Assert.IsTrue(drained, "The stream did not reach its end and unqueue its full buffer.");

                stream.SeekToPosition(TimeSpan.Zero);
                // Allow time for the streamer to refill the buffers and restart the source
                bool restarted = WaitUntilDispatching(
                    () => AL.GetSourceState(stream.alSourceId) == ALSourceState.Playing &&
                        GetQueuedBuffers(stream) == stream.BufferCount,
                    TimeSpan.FromSeconds(2));
                Assert.IsTrue(restarted, "The streamer did not refill the buffers and restart the source.");
            }
            finally
            {
                stream.Stop();
                stream.Dispose();
            }
        }

        [Test]
        public void Stop_AfterStreamReachedEnd_DoesNotInvokeFinishedAction()
        {
            int finishedCount = 0;
            var stream = CreateStream(() => Interlocked.Increment(ref finishedCount));

            try
            {
                stream.Play();
                // Decoding reaches the end well before playback does
                stream.SeekToPosition(stream.GetLength() - TimeSpan.FromSeconds(1.2));
                bool reachedEnd = WaitUntilDispatching(() => HasReachedEnd(stream), TimeSpan.FromSeconds(1));
                Assert.IsTrue(reachedEnd, "The stream did not reach its end.");

                // Keep Stop() waiting on stopMutex while the streamer runs
                var lockTaken = new ManualResetEventSlim();
                var holder = Task.Run(() =>
                {
                    lock (stream.stopMutex)
                    {
                        lockTaken.Set();
                        // About 4 streamer updates, and shorter than the 1.2 sec left to play
                        Thread.Sleep(400);
                    }
                });
                lockTaken.Wait();

                stream.Stop();
                holder.Wait();
                SleepWhileDispatching(300);

                Assert.AreEqual(0, Volatile.Read(ref finishedCount),
                    "FinishedAction ran for a stream that was stopped.");
            }
            finally
            {
                stream.Stop();
                stream.Dispose();
            }
        }

        [Test]
        public void GetPosition_WhileStreamReopens_DoesNotThrow()
        {
            var stream = CreateStream();
            stream.Prepare();
            var cancelled = false;

            // The streamer does this for looped streams
            var reopener = Task.Run(() =>
            {
                while (!Volatile.Read(ref cancelled))
                {
                    lock (stream.prepareMutex)
                    {
                        stream.Close();
                        stream.Open();
                    }
                }
            });

            try
            {
                var stopWatch = Stopwatch.StartNew();
                while (stopWatch.Elapsed < TimeSpan.FromSeconds(1))
                    stream.GetPosition();
            }
            finally
            {
                Volatile.Write(ref cancelled, true);
                reopener.Wait();
                stream.Dispose();
            }
        }
    }
}

#endif
