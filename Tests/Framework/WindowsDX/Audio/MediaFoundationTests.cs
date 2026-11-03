// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Media;
using NUnit.Framework;
using System;
using System.IO;

namespace MonoGame.Tests.Audio
{
    [Category("Audio")]
    [NonParallelizable]
    [RunOnUiTestFixture]
    public class MediaFoundationTests : AudioTestFixtureBase
    {
        private static string GetSongPath() => Path.GetFullPath("Assets/Audio/Song/one_two_three.mp3");
        private static string GetVideoPath() => Path.GetFullPath("Assets/Video/video_h264_1280x720_5s.mp4");

        [Test]
        public void Song_Constructor_LoadsSong()
        {
            using var song = new Song(GetSongPath(), 3000);

            Assert.AreEqual("one_two_three", song.Name);
            Assert.IsNotNull(song.Topology);
            Assert.AreEqual(TimeSpan.FromSeconds(3), song.Duration);
        }

        [Test]
        public void Song_FromUri_LoadsSong()
        {
            var uri = new Uri(GetSongPath());
            using var song = Song.FromUri("one_two_three", uri);

            Assert.AreEqual("one_two_three", song.Name);
            Assert.IsNotNull(song.Topology);
        }

        [Test]
        public void MediaPlayer_PlaySong_EntersPlayingState()
        {
            using var song = new Song(GetSongPath(), 3000);

            try
            {
                MediaPlayer.Play(song);

                var startedPlaying = WaitUntilDispatching(
                    () => MediaPlayer.State == MediaState.Playing
                          && MediaPlayer.PlayPosition > TimeSpan.Zero,
                    TimeSpan.FromSeconds(3));

                Assert.IsTrue(startedPlaying, "MediaPlayer did not start playing song!");
            }
            finally
            {
                MediaPlayer.Stop();
            }
        }

        [Test]
        public void Video_Constructor_LoadsVideo()
        {
            using var video = new Video(GetVideoPath(), 5000f);

            Assert.IsNotNull("video_h264_1280x720_5s.mp4", video.FileName);
            Assert.IsNotNull(video.Topology);
            Assert.IsNotNull(video.SampleGrabber);
            Assert.AreEqual(TimeSpan.FromSeconds(5), video.Duration);
        }
    }
}
