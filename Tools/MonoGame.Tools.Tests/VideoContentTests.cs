// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    [Category("Video")]
    class VideoContentTests
    {
        [Test]
        public void Ctors()
        {
            // Bad and nonexistent filenames
            Assert.Throws<Exception>(() => new VideoContent(null));
            Assert.Throws<Exception>(() => new VideoContent(""));
            Assert.Throws<Exception>(() => new VideoContent(@"Not/A/File"));
            Assert.Throws<Exception>(() => new VideoContent(@"Assets/Video/video_h264_1280x720_20s.mp4"));
            Assert.Throws<Exception>(() => new VideoContent(@"Assets/Video/video_h264_1920x1080_20s.mp4"));
            Assert.Throws<Exception>(() => new VideoContent(@"Assets/Video/video_h264_1280x720_20s.wmv"));
            Assert.Throws<Exception>(() => new VideoContent(@"Assets/Video/video_h264_1920x1080_20s.wmv"));
        }

        [TestCase(@"Assets/Video/video_h264_1280x720_5s.mp4", 1280, 720, 60, 42451, 5)]
        [TestCase(@"Assets/Video/video_h264_1280x720_15s.mp4", 1280, 720, 60, 40240, 15)]
        [TestCase(@"Assets/Video/video_h264_2560x1440_5s.mp4", 2560, 1440, 60, 92021, 5)]
        [TestCase(@"Assets/Video/video_h264_2560x1440_15s.mp4", 2560, 1440, 60, 89831, 15)]
        public void VideoContent(string filename, int width, int height, int framesPerSecond, int bitsPerSecond, int seconds)
        {
            var content = new VideoContent(filename);
            Assert.AreEqual(filename, content.Filename);
            Assert.AreEqual(width, content.Width);
            Assert.AreEqual(height, content.Height);
            Assert.AreEqual(framesPerSecond, content.FramesPerSecond);
            Assert.AreEqual(bitsPerSecond, content.BitsPerSecond);
            Assert.AreEqual(seconds, content.Duration.Seconds);
        }
    }
}
