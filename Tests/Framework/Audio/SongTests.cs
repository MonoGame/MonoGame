// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if VULKAN || DIRECTX12

using Microsoft.Xna.Framework.Media;
using NUnit.Framework;
using System;
using System.IO;

namespace MonoGame.Tests.Audio
{

    [Category("Song")]
    public class SongTests : AudioTestFixtureBase
    {
        private void RunTests(Song song)
        {
            // Play the song.
            MediaPlayer.Play(song);
            SleepWhileDispatching(1500);
            Assert.AreEqual(MediaState.Playing, MediaPlayer.State);

            // Pause it now.
            MediaPlayer.Pause();
            SleepWhileDispatching(500);
            Assert.AreEqual(MediaState.Paused, MediaPlayer.State);

            // Test the play position.
            var pos = MediaPlayer.PlayPosition;
            Assert.AreEqual(1.5f, pos.TotalSeconds, 0.1f);

            // Resume from where it was paused.
            MediaPlayer.Resume();
            SleepWhileDispatching(1500);
            Assert.AreEqual(MediaState.Playing, MediaPlayer.State);

            // Stop it.
            MediaPlayer.Stop();
            SleepWhileDispatching(100);
            Assert.AreEqual(MediaState.Stopped, MediaPlayer.State);
            
            // Start it with offset.
            MediaPlayer.Play(song, TimeSpan.FromSeconds(3));
            SleepWhileDispatching(1000);
            Assert.AreEqual(MediaState.Playing, MediaPlayer.State);

            // Ok stop!
            MediaPlayer.Stop();
        }

        [Test]
        public void SongTestOgg()
        {
            var song = _content.Load<Song>("Assets/Audio/Song/rock_loop_stereo");
            RunTests(song);
        }

        [Test]
        public void SongTestMP3()
        {
            string relativePath = "Assets/Audio/Song/rock_loop_stereo2.mp3";
            string fullPath = Path.GetFullPath(relativePath);
            var path = new System.Uri(fullPath);
            var song = Song.FromUri("rock_loop_stereo", path);

            RunTests(song);

            song.Dispose();
        }
    }
}

#endif
