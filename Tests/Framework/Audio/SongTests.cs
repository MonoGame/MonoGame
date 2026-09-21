// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if VULKAN || DIRECTX12

using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Microsoft.Xna.Framework.Media;

namespace MonoGame.Tests.Audio
{

    [Category("Song")]
    public class SongTests : AudioTestFixtureBase
    {
        private static Song CreateSong(string name)
        {
            string relativePath = "Assets/Audio/Song/one_two_three.mp3";
            string fullPath = Path.GetFullPath(relativePath);
            Uri uri = new Uri(fullPath);

            return Song.FromUri(name, uri);
        }

        private void RunTests(Song song)
        {
            Assert.AreEqual(3.0f, song.Duration.TotalSeconds, 0.01f);

            var stopWatch = Stopwatch.StartNew();

            // Play the "One".
            MediaPlayer.Play(song);
            SleepWhileDispatching(1000);
            Assert.AreEqual(MediaState.Playing, MediaPlayer.State);

            // Pause it now.
            MediaPlayer.Pause();
            stopWatch.Stop();
            
            SleepWhileDispatching(500);
            Assert.AreEqual(MediaState.Paused, MediaPlayer.State);

            // Test the play position against actual elapsed real-world time.
            var pos = MediaPlayer.PlayPosition;
            Assert.AreEqual(stopWatch.Elapsed.TotalSeconds, pos.TotalSeconds, 0.1f);

            // Resume from where it was paused to play "Two" and "Three".
            MediaPlayer.Resume();
            SleepWhileDispatching(1000);
            Assert.AreEqual(MediaState.Playing, MediaPlayer.State);

            // Stop it.
            MediaPlayer.Stop();
            SleepWhileDispatching(100);
            Assert.AreEqual(MediaState.Stopped, MediaPlayer.State);

            // Start it with offset to play the "Three".
            MediaPlayer.Play(song, TimeSpan.FromSeconds(2));
            SleepWhileDispatching(100);
            Assert.AreEqual(MediaState.Playing, MediaPlayer.State);

            // Wait for it to end.
            SleepWhileDispatching(1000);
            Assert.AreEqual(MediaState.Stopped, MediaPlayer.State);

            // Go back and play "Two" then "One" testing seek.
            MediaPlayer.Play(song, TimeSpan.FromSeconds(1));
            SleepWhileDispatching(1000);
            MediaPlayer.Play(song, TimeSpan.FromSeconds(0));
            SleepWhileDispatching(1000);
            MediaPlayer.Stop();
            SleepWhileDispatching(100);
            Assert.AreEqual(MediaState.Stopped, MediaPlayer.State);
        }

        [Test]
        public void SongTestOgg()
        {
            var song = _content.Load<Song>("Assets/Audio/Song/one_two_three");
            RunTests(song);
        }

        [Test]
        public void SongTestMP3()
        {
            Song song = CreateSong("one_two_three");

            RunTests(song);

            song.Dispose();
        }

        [Test]
        public void Play_CollectionSongCompletes_AdvancesToNextSong()
        {
            Song firstSong = CreateSong("first");
            Song secondSong = CreateSong("second");
            SongCollection songs = new SongCollection();
            songs.Add(firstSong);
            songs.Add(secondSong);

            try
            {
                MediaPlayer.IsRepeating = false;
                MediaPlayer.IsShuffled = false;

                MediaPlayer.Play(songs);
                TimeSpan timeout = firstSong.Duration + secondSong.Duration;
                bool advanced = WaitUntilDispatching(
                    () => object.ReferenceEquals(secondSong, MediaPlayer.Queue.ActiveSong) &&
                        MediaPlayer.State == MediaState.Playing,
                    timeout);

                Assert.IsTrue(advanced, "The completed Song did not advance the queue.");
            }
            finally
            {
                MediaPlayer.Stop();
                firstSong.Dispose();
                secondSong.Dispose();
            }
        }

        [Test]
        public void Play_StopBeforeSongCompletion_DoesNotAdvanceQueue()
        {
            Song firstSong = CreateSong("first");
            Song secondSong = CreateSong("second");
            SongCollection songs = new SongCollection();
            songs.Add(firstSong);
            songs.Add(secondSong);

            try
            {
                MediaPlayer.IsRepeating = false;
                MediaPlayer.IsShuffled = false;

                MediaPlayer.Play(songs);
                SleepWhileDispatching(100);
                MediaPlayer.Stop();
                SleepWhileDispatching((int)firstSong.Duration.TotalMilliseconds);

                Assert.AreSame(firstSong, MediaPlayer.Queue.ActiveSong);
                Assert.AreEqual(MediaState.Stopped, MediaPlayer.State);
            }
            finally
            {
                MediaPlayer.Stop();
                firstSong.Dispose();
                secondSong.Dispose();
            }
        }

        [Test]
        public void Play_ReplacedSongWouldComplete_DoesNotAdvanceReplacementQueue()
        {
            Song firstSong = CreateSong("first");
            Song secondSong = CreateSong("second");

            try
            {
                MediaPlayer.IsRepeating = false;
                MediaPlayer.IsShuffled = false;

                TimeSpan startPosition = TimeSpan.FromSeconds(2);
                MediaPlayer.Play(firstSong, startPosition);
                bool firstSongStarted = WaitUntilDispatching(
                    () => MediaPlayer.PlayPosition > TimeSpan.Zero,
                    firstSong.Duration);
                Assert.IsTrue(firstSongStarted, "The original Song did not start playing.");

                MediaPlayer.Play(secondSong);
                TimeSpan originalRemainingDuration = firstSong.Duration - startPosition;
                bool replacementStayedActive = WaitUntilDispatching(
                    () => object.ReferenceEquals(secondSong, MediaPlayer.Queue.ActiveSong) &&
                        MediaPlayer.State == MediaState.Playing &&
                        MediaPlayer.PlayPosition >= originalRemainingDuration,
                    secondSong.Duration);

                Assert.IsTrue(replacementStayedActive, "The replacement Song did not remain active.");
            }
            finally
            {
                MediaPlayer.Stop();
                firstSong.Dispose();
                secondSong.Dispose();
            }
        }

        [Test]
        public void Play_RepeatingSongCompletes_RestartsSong()
        {
            Song song = CreateSong("repeat");

            try
            {
                MediaPlayer.IsRepeating = true;

                MediaPlayer.Play(song);
                TimeSpan timeout = song.Duration + song.Duration;
                bool restarted = WaitUntilDispatching(
                    () => object.ReferenceEquals(song, MediaPlayer.Queue.ActiveSong) &&
                        MediaPlayer.State == MediaState.Playing &&
                        song.PlayCount > 1,
                    timeout);

                Assert.IsTrue(restarted, "The repeating Song did not restart.");
            }
            finally
            {
                MediaPlayer.Stop();
                MediaPlayer.IsRepeating = false;
                song.Dispose();
            }
        }

        [Test]
        public void Play_DisposedSong_ThrowsObjectDisposedException()
        {
            Song song = CreateSong("disposed");
            song.Dispose();

            Assert.Throws<ObjectDisposedException>(() => MediaPlayer.Play(song));
        }
    }
}

#endif
