// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if VULKAN || DIRECTX12

using Microsoft.Xna.Framework.Media;
using NUnit.Framework;

namespace MonoGame.Tests.Audio
{

    [Category("Song")]
    public class SongTests : AudioTestFixtureBase
    {
        [Test]
        public void SongPlayPauseStop()
        {
            var song = _content.Load<Song>("Assets/Audio/Song/rock_loop_stereo");

            MediaPlayer.Play(song);
            SleepWhileDispatching(1500);
            Assert.AreEqual(MediaState.Playing, MediaPlayer.State);
            MediaPlayer.Pause();
            SleepWhileDispatching(500);
            Assert.AreEqual(MediaState.Paused, MediaPlayer.State);
            MediaPlayer.Resume();
            SleepWhileDispatching(1500);
            Assert.AreEqual(MediaState.Playing, MediaPlayer.State);
            MediaPlayer.Stop();
            SleepWhileDispatching(100);
            Assert.AreEqual(MediaState.Stopped, MediaPlayer.State);
        }
    }
}

#endif
