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
            string relativePath = "Assets/Audio/Song/one_two_three.mp3";
            string fullPath = Path.GetFullPath(relativePath);
            var path = new System.Uri(fullPath);
            var song = Song.FromUri("one_two_three", path);

            RunTests(song);

            song.Dispose();
        }
    }
}

#endif
