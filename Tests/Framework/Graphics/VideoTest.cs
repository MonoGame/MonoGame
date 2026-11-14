
// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using Microsoft.Xna.Framework.Media;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics;

[NonParallelizable]
[RunOnUiTestFixture]
class VideotTest : GraphicsDeviceTestFixtureBase
{
    [Test]
    public void Video_CanBeCreated()
    {
        var video = LoadVideo("video_h264_1280x720_5s");
        Assert.IsNotNull(video);
        Assert.AreEqual(60, video.FramesPerSecond);
        Assert.AreEqual(TimeSpan.Parse("00:00:05.0160000"), video.Duration);
        Assert.AreEqual(720, video.Height);
        Assert.AreEqual(1280, video.Width);
    }

    private Video LoadVideo(string filename)
    {
        return content.Load<Video>(Paths.Video(filename));
    }
}
