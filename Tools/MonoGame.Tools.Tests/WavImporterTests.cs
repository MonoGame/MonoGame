// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    class WavImporterTests
    {
        [Test]
        public void Arguments()
        {
            var context = new TestImporterContext("TestObj", "TestBin");
            Assert.Throws<ArgumentNullException>(() => new WavImporter().Import(null, context));
            Assert.Throws<ArgumentNullException>(() => new WavImporter().Import("", context));
            Assert.Throws<ArgumentNullException>(() => new WavImporter().Import(@"Assets/Audio/bark_mono_44hz_8bit.wav", null));
            Assert.Throws<FileNotFoundException>(() => new WavImporter().Import(@"this\does\not\exist.wav", context));
        }

        [TestCase(@"Assets/Audio/rock_loop_stereo.mp3")]
        [TestCase(@"Assets/Audio/rock_loop_stereo.wma")]
        public void InvalidFormat(string sourceFile)
        {
            Assert.Throws<InvalidContentException>(() => new WavImporter().Import(sourceFile, new TestImporterContext("TestObj", "TestBin")));
        }

        [TestCase(@"Assets/Audio/bark_mono_44hz_32bit.wav")]
        public void InvalidBitDepth(string sourceFile)
        {
            Assert.Throws<InvalidContentException>(() => new WavImporter().Import(sourceFile, new TestImporterContext("TestObj", "TestBin")));
        }

        // TODO: Need to add tests for channel counts and sample rate most likely!

        [TestCase(@"Assets/Audio/bark_mono_88hz_16bit.wav")]
        public void InvalidSampleRate(string sourceFile)
        {
            Assert.Throws<InvalidContentException>(() => new WavImporter().Import(sourceFile, new TestImporterContext("TestObj", "TestBin")));
        }

        // 8bit Mono
        [TestCase(@"Assets/Audio/bark_mono_44hz_8bit.wav", 1, 44100, 44100, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_22hz_8bit.wav", 1, 22050, 22050, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_11hz_8bit.wav", 1, 11025, 11025, 8, 1)]

        // 8bit Stereo
        [TestCase(@"Assets/Audio/rock_loop_stereo_44hz_8bit.wav", 2, 88200, 44100, 8, 2)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz_8bit.wav", 2, 44100, 22050, 8, 2)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz_8bit.wav", 2, 22050, 11025, 8, 2)]

        // 16bit Mono
        [TestCase(@"Assets/Audio/blast_mono.wav", 1, 88200, 44100, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono_22hz.wav", 1, 44100, 22050, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono_11hz.wav", 1, 22050, 11025, 16, 2)]

        // 16bit Stereo
        [TestCase(@"Assets/Audio/rock_loop_stereo.wav", 2, 176400, 44100, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz.wav", 2, 88200, 22050, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz.wav", 2, 44100, 11025, 16, 4)]

        public void Import(string sourceFile, int channels, int averageBytesPerSecond, int sampleRate, int bitsPerSample, int blockAlign)
        {
            var content = new WavImporter().Import(sourceFile, new TestImporterContext("TestObj", "TestBin"));

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
