// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    class Mp3ImporterTests
    {
        [Test]
        public void Arguments()
        {
            var context = new TestImporterContext("TestObj", "TestBin");
            Assert.Throws<ArgumentNullException>(() => new Mp3Importer().Import(null, context));
            Assert.Throws<ArgumentNullException>(() => new Mp3Importer().Import("", context));
            Assert.Throws<ArgumentNullException>(() => new Mp3Importer().Import(@"Assets/Audio/rock_loop_stereo.mp3", null));
            Assert.Throws<FileNotFoundException>(() => new Mp3Importer().Import(@"this\does\not\exist.mp3", context));
        }

        public void InvalidFormat()
        {
            Assert.Throws<InvalidContentException>(() => new Mp3Importer().Import(@"Assets/Audio/rock_loop_stereo.wav", new TestImporterContext("TestObj", "TestBin")));
        }

        [TestCase(@"Assets/Audio/rock_loop_stereo.mp3", 2, 176400, 44100, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo.wma", 2, 176400, 44100, 16, 4)]
        public void Import(string sourceFile, int channels, int averageBytesPerSecond, int sampleRate, int bitsPerSample, int blockAlign)
        {
            var content = new Mp3Importer().Import(sourceFile, new TestImporterContext("TestObj", "TestBin"));

            Assert.AreEqual(1, content.Format.Format);
            Assert.AreEqual(channels, content.Format.ChannelCount);
            Assert.AreEqual(averageBytesPerSecond, content.Format.AverageBytesPerSecond);
            Assert.AreEqual(sampleRate, content.Format.SampleRate);
            Assert.AreEqual(bitsPerSample, content.Format.BitsPerSample);
            Assert.AreEqual(blockAlign, content.Format.BlockAlign);

            content.Dispose();
        }
    }
}
