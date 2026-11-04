// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if DESKTOPGL

using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Microsoft.Xna.Framework.Media;

namespace MonoGame.Tests.Audio
{

    [Category("Song")]
    [RunOnUiTestFixture]
    public class SongDesktopGLTests : AudioTestFixtureBase
    {
        private static Song CreateSong(string name)
        {
            string relativePath = "Assets/Audio/Song/one_two_three.ogg";
            string fullPath = Path.GetFullPath(relativePath);
            Uri uri = new Uri(fullPath);

            return Song.FromUri(name, uri);
        }

        [Test]
        public void Play_SongCompletes_PlaysUntilEnd()
        {
            Song song = CreateSong("complete");

            try
            {
                MediaPlayer.IsRepeating = false;

                // Leaves one full 1 sec buffer and a 0.9 sec final one, so losing the final buffer
                // shortens playback by much more than the streamer latency adds to it
                TimeSpan startPosition = song.Duration - TimeSpan.FromSeconds(1.9);
                var stopWatch = Stopwatch.StartNew();
                MediaPlayer.Play(song, startPosition);
                TimeSpan timeout = song.Duration;
                bool completed = WaitUntilDispatching(
                    () => MediaPlayer.State == MediaState.Stopped,
                    timeout);
                stopWatch.Stop();

                Assert.IsTrue(completed, "The Song did not complete.");
                TimeSpan remainingDuration = song.Duration - startPosition;
                Assert.GreaterOrEqual(stopWatch.Elapsed, remainingDuration, "The Song stopped before its end.");
            }
            finally
            {
                MediaPlayer.Stop();
                song.Dispose();
            }
        }

        [Test]
        public void Play_ReplacedSongReachedEnd_DoesNotStopReplacement()
        {
            Song firstSong = CreateSong("first");
            Song secondSong = CreateSong("second");

            try
            {
                MediaPlayer.IsRepeating = false;
                MediaPlayer.IsShuffled = false;

                MediaPlayer.Play(firstSong, TimeSpan.FromSeconds(2));
                bool firstSongDecoded = WaitUntilDispatching(
                    () => MediaPlayer.PlayPosition >= firstSong.Duration,
                    firstSong.Duration);
                Assert.IsTrue(firstSongDecoded, "The original Song did not reach its end.");

                MediaPlayer.Play(secondSong);
                // A stale end-of-stream flag stops the Song after its first 1 sec buffer, whole Song lasts 3 sec
                SleepWhileDispatching(2000);

                Assert.AreEqual(MediaState.Playing, MediaPlayer.State, "The new Song was stopped.");
            }
            finally
            {
                MediaPlayer.Stop();
                firstSong.Dispose();
                secondSong.Dispose();
            }
        }

        [Test]
        public void Play_SameSongAfterReachingEnd_KeepsPlaying()
        {
            Song song = CreateSong("replay");

            try
            {
                MediaPlayer.IsRepeating = false;

                MediaPlayer.Play(song, TimeSpan.FromSeconds(2));
                bool songDecoded = WaitUntilDispatching(
                    () => MediaPlayer.PlayPosition >= song.Duration,
                    song.Duration);
                Assert.IsTrue(songDecoded, "The Song did not reach its end.");

                MediaPlayer.Play(song, TimeSpan.Zero);
                SleepWhileDispatching(2000);

                Assert.AreEqual(MediaState.Playing, MediaPlayer.State, "The replayed Song was stopped.");
            }
            finally
            {
                MediaPlayer.Stop();
                song.Dispose();
            }
        }
    }
}

#endif
