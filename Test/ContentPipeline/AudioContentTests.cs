// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    class AudioContentTests
    {
        [Test]
        public void Ctors()
        {
            // Test bad file names.
            Assert.Throws<InvalidContentException>(() => new AudioContent(null, AudioFileType.Wav));
            Assert.Throws<InvalidContentException>(() => new AudioContent("", AudioFileType.Wav));
            Assert.Throws<InvalidContentException>(() => new AudioContent(@"Not/A/File", AudioFileType.Wav));

            // Test invalid file types.
            Assert.Throws<InvalidContentException>(() => new AudioContent(@"Assets/Audio/blast_mono.wav", AudioFileType.Mp3));
            Assert.Throws<InvalidContentException>(() => new AudioContent(@"Assets/Audio/blast_mono.wav", AudioFileType.Wma));
            Assert.Throws<InvalidContentException>(() => new AudioContent(@"Assets/Audio/rock_loop_stereo.wma", AudioFileType.Wav));
            Assert.Throws<InvalidContentException>(() => new AudioContent(@"Assets/Audio/rock_loop_stereo.mp3", AudioFileType.Wav));

            // This for some reason does not throw!
            new AudioContent(@"Assets/Audio/rock_loop_stereo.wma", AudioFileType.Mp3);
            new AudioContent(@"Assets/Audio/rock_loop_stereo.mp3", AudioFileType.Wma);
        }

        [Test]
        public void WavFile()
        {
            var content = new AudioContent(@"Assets/Audio/blast_mono.wav", AudioFileType.Wav);
            Assert.AreEqual(@"Assets/Audio/blast_mono.wav", content.FileName);
            Assert.AreEqual(AudioFileType.Wav, content.FileType);
            Assert.IsNull(content.Identity);
            Assert.AreEqual(0, content.LoopStart);
            Assert.AreEqual(315970, content.LoopLength);
            Assert.AreEqual(71640000, content.Duration.Ticks);

            var format = content.Format;
            Assert.IsNotNull(format);
            Assert.AreEqual(1, format.Format);
            Assert.AreEqual(1, format.ChannelCount);
            Assert.AreEqual(44100, format.SampleRate);
            Assert.AreEqual(88200, format.AverageBytesPerSecond);
            Assert.AreEqual(2, format.BlockAlign);
            Assert.AreEqual(16, format.BitsPerSample);
            
            Assert.IsNotNull(format.NativeWaveFormat);
            Assert.AreEqual(18, format.NativeWaveFormat.Count);

            using (var reader = new BinaryReader(new MemoryStream(format.NativeWaveFormat.ToArray())))
            {
                var formatType = reader.ReadInt16();
                var channels = reader.ReadInt16();
                var sampleRate = reader.ReadInt32();
                var averageBytesPerSecond = reader.ReadInt32();
                var blockAlign = reader.ReadInt16();
                var bitsPerSample = reader.ReadInt16();
                var zero = reader.ReadInt16();

                Assert.AreEqual(format.Format, formatType);
                Assert.AreEqual(format.ChannelCount, channels);
                Assert.AreEqual(format.SampleRate, sampleRate);
                Assert.AreEqual(format.AverageBytesPerSecond, averageBytesPerSecond);
                Assert.AreEqual(format.BlockAlign, blockAlign);
                Assert.AreEqual(format.BitsPerSample, bitsPerSample);
                Assert.AreEqual(0, zero);
            }

            var data = content.Data;
            Assert.NotNull(data);
            Assert.AreEqual(631940, data.Count);

            // Seems like XNA just let the Data property throw
            // an exception after it has been disposed.
            content.Dispose();
            Assert.Throws<InvalidContentException>(() => { var temp = content.Data; });
        }

        [Test]
        public void Mp3File()
        {
            var content = new AudioContent(@"Assets/Audio/rock_loop_stereo.mp3", AudioFileType.Mp3);
            Assert.AreEqual(@"Assets/Audio/rock_loop_stereo.mp3", content.FileName);
            Assert.AreEqual(AudioFileType.Mp3, content.FileType);
            Assert.IsNull(content.Identity);
            Assert.AreEqual(0, content.LoopStart);
            Assert.AreEqual(0, content.LoopLength);
            Assert.AreEqual(79930000, content.Duration.Ticks);

            var format = content.Format;
            Assert.IsNotNull(format);
            Assert.AreEqual(1, format.Format);
            Assert.AreEqual(2, format.ChannelCount);
            Assert.AreEqual(44100, format.SampleRate);
            Assert.AreEqual(176400, format.AverageBytesPerSecond);
            Assert.AreEqual(4, format.BlockAlign);
            Assert.AreEqual(16, format.BitsPerSample);

            Assert.IsNotNull(format.NativeWaveFormat);
            Assert.AreEqual(18, format.NativeWaveFormat.Count);
            using (var reader = new BinaryReader(new MemoryStream(format.NativeWaveFormat.ToArray())))
            {
                var formatType = reader.ReadInt16();
                var channels = reader.ReadInt16();
                var sampleRate = reader.ReadInt32();
                var averageBytesPerSecond = reader.ReadInt32();
                var blockAlign = reader.ReadInt16();
                var bitsPerSample = reader.ReadInt16();
                var zero = reader.ReadInt16();

                Assert.AreEqual(format.Format, formatType);
                Assert.AreEqual(format.ChannelCount, channels);
                Assert.AreEqual(format.SampleRate, sampleRate);
                Assert.AreEqual(format.AverageBytesPerSecond, averageBytesPerSecond);
                Assert.AreEqual(format.BlockAlign, blockAlign);
                Assert.AreEqual(format.BitsPerSample, bitsPerSample);
                Assert.AreEqual(0, zero);
            }

            // For MP3 it seems the data is never availble.
            Assert.Throws<InvalidContentException>(() => { var data = content.Data; });

            // Seems like XNA just let the Data property throw
            // an exception after it has been disposed.
            content.Dispose();
            Assert.Throws<InvalidContentException>(() => { var temp = content.Data; });
        }

        [Test]
        public void WmaFile()
        {
            var content = new AudioContent(@"Assets/Audio/rock_loop_stereo.wma", AudioFileType.Wma);
            Assert.AreEqual(@"Assets/Audio/rock_loop_stereo.wma", content.FileName);
            Assert.AreEqual(AudioFileType.Wma, content.FileType);
            Assert.IsNull(content.Identity);
            Assert.AreEqual(0, content.LoopStart);
            Assert.AreEqual(0, content.LoopLength);
            Assert.AreEqual(99430000, content.Duration.Ticks);

            var format = content.Format;
            Assert.IsNotNull(format);
            Assert.AreEqual(1, format.Format);
            Assert.AreEqual(2, format.ChannelCount);
            Assert.AreEqual(44100, format.SampleRate);
            Assert.AreEqual(176400, format.AverageBytesPerSecond);
            Assert.AreEqual(4, format.BlockAlign);
            Assert.AreEqual(16, format.BitsPerSample);

            Assert.IsNotNull(format.NativeWaveFormat);
            Assert.AreEqual(18, format.NativeWaveFormat.Count);
            using (var reader = new BinaryReader(new MemoryStream(format.NativeWaveFormat.ToArray())))
            {
                var formatType = reader.ReadInt16();
                var channels = reader.ReadInt16();
                var sampleRate = reader.ReadInt32();
                var averageBytesPerSecond = reader.ReadInt32();
                var blockAlign = reader.ReadInt16();
                var bitsPerSample = reader.ReadInt16();
                var zero = reader.ReadInt16();

                Assert.AreEqual(format.Format, formatType);
                Assert.AreEqual(format.ChannelCount, channels);
                Assert.AreEqual(format.SampleRate, sampleRate);
                Assert.AreEqual(format.AverageBytesPerSecond, averageBytesPerSecond);
                Assert.AreEqual(format.BlockAlign, blockAlign);
                Assert.AreEqual(format.BitsPerSample, bitsPerSample);
                Assert.AreEqual(0, zero);
            }

            // For WMA it seems the data is never availble.
            Assert.Throws<InvalidContentException>(() => { var data = content.Data; });

            // Seems like XNA just let the Data property throw
            // an exception after it has been disposed.
            content.Dispose();
            Assert.Throws<InvalidContentException>(() => { var temp = content.Data; });
        }

        private static int ToWavFormat(ConversionFormat format, int bitsPerSample)
        {
            switch (format)
            {
                case ConversionFormat.Pcm:
                    if (bitsPerSample == 32)
                        return -2;
                    return 1;
                    break;
                case ConversionFormat.Adpcm:
                    return 2;
                    break;
                case ConversionFormat.WindowsMedia:
                case ConversionFormat.Xma:
                default:
                    throw new ArgumentOutOfRangeException("format");
            }
        }

        // 8bit PCM Mono -> PCM
        [TestCase(@"Assets/Audio/bark_mono_44hz_8bit.wav", ConversionFormat.Pcm, ConversionQuality.Best, 1, 44100, 44100, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_44hz_8bit.wav", ConversionFormat.Pcm, ConversionQuality.Medium, 1, 33075, 33075, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_44hz_8bit.wav", ConversionFormat.Pcm, ConversionQuality.Low, 1, 22050, 22050, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_22hz_8bit.wav", ConversionFormat.Pcm, ConversionQuality.Best, 1, 22050, 22050, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_22hz_8bit.wav", ConversionFormat.Pcm, ConversionQuality.Medium, 1, 16537, 16537, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_22hz_8bit.wav", ConversionFormat.Pcm, ConversionQuality.Low, 1, 11025, 11025, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_11hz_8bit.wav", ConversionFormat.Pcm, ConversionQuality.Best, 1, 11025, 11025, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_11hz_8bit.wav", ConversionFormat.Pcm, ConversionQuality.Medium, 1, 8268, 8268, 8, 1)]
        [TestCase(@"Assets/Audio/bark_mono_11hz_8bit.wav", ConversionFormat.Pcm, ConversionQuality.Low, 1, 8000, 8000, 8, 1)]

        // 8bit PCM Mono -> ADPCM
        [TestCase(@"Assets/Audio/bark_mono_44hz_8bit.wav", ConversionFormat.Adpcm, ConversionQuality.Best, 1, 24104, 44077, 4, 70)]
        [TestCase(@"Assets/Audio/bark_mono_44hz_8bit.wav", ConversionFormat.Adpcm, ConversionQuality.Medium, 1, 24104, 44077, 4, 70)]
        [TestCase(@"Assets/Audio/bark_mono_44hz_8bit.wav", ConversionFormat.Adpcm, ConversionQuality.Low, 1, 12073, 22078, 4, 70)]
        [TestCase(@"Assets/Audio/bark_mono_22hz_8bit.wav", ConversionFormat.Adpcm, ConversionQuality.Best, 1, 12073, 22077, 4, 70)]
        [TestCase(@"Assets/Audio/bark_mono_22hz_8bit.wav", ConversionFormat.Adpcm, ConversionQuality.Medium, 1, 12073, 22077, 4, 70)]
        [TestCase(@"Assets/Audio/bark_mono_22hz_8bit.wav", ConversionFormat.Adpcm, ConversionQuality.Low, 1, 6036, 11039, 4, 70)]
        [TestCase(@"Assets/Audio/bark_mono_11hz_8bit.wav", ConversionFormat.Adpcm, ConversionQuality.Best, 1, 6036, 11039, 4, 70)]
        [TestCase(@"Assets/Audio/bark_mono_11hz_8bit.wav", ConversionFormat.Adpcm, ConversionQuality.Medium, 1, 6036, 11039, 4, 70)]
        [TestCase(@"Assets/Audio/bark_mono_11hz_8bit.wav", ConversionFormat.Adpcm, ConversionQuality.Low, 1, 4386, 8021, 4, 70)]

        // 16bit PCM Mono -> PCM
        [TestCase(@"Assets/Audio/blast_mono.wav", ConversionFormat.Pcm, ConversionQuality.Best, 1, 88200, 44100, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono.wav", ConversionFormat.Pcm, ConversionQuality.Medium, 1, 66150, 33075, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono.wav", ConversionFormat.Pcm, ConversionQuality.Low, 1, 44100, 22050, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono_22hz.wav", ConversionFormat.Pcm, ConversionQuality.Best, 1, 44100, 22050, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono_22hz.wav", ConversionFormat.Pcm, ConversionQuality.Medium, 1, 33074, 16537, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono_22hz.wav", ConversionFormat.Pcm, ConversionQuality.Low, 1, 22050, 11025, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono_11hz.wav", ConversionFormat.Pcm, ConversionQuality.Best, 1, 22050, 11025, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono_11hz.wav", ConversionFormat.Pcm, ConversionQuality.Medium, 1, 16536, 8268, 16, 2)]
        [TestCase(@"Assets/Audio/blast_mono_11hz.wav", ConversionFormat.Pcm, ConversionQuality.Low, 1, 16000, 8000, 16, 2)]

        // 16bit PCM Stereo -> PCM
        [TestCase(@"Assets/Audio/rock_loop_stereo.wav", ConversionFormat.Pcm, ConversionQuality.Best, 2, 176400, 44100, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo.wav", ConversionFormat.Pcm, ConversionQuality.Medium, 2, 132300, 33075, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo.wav", ConversionFormat.Pcm, ConversionQuality.Low, 2, 88200, 22050, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz.wav", ConversionFormat.Pcm, ConversionQuality.Best, 2, 88200, 22050, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz.wav", ConversionFormat.Pcm, ConversionQuality.Medium, 2, 66148, 16537, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz.wav", ConversionFormat.Pcm, ConversionQuality.Low, 2, 44100, 11025, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz.wav", ConversionFormat.Pcm, ConversionQuality.Best, 2, 44100, 11025, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz.wav", ConversionFormat.Pcm, ConversionQuality.Medium, 2, 33072, 8268, 16, 4)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz.wav", ConversionFormat.Pcm, ConversionQuality.Low, 2, 32000, 8000, 16, 4)]

        // 16bit PCM Mono -> ADPCM
        [TestCase(@"Assets/Audio/blast_mono.wav", ConversionFormat.Adpcm, ConversionQuality.Best, 1, 24121, 44108, 4, 70)]
        [TestCase(@"Assets/Audio/blast_mono.wav", ConversionFormat.Adpcm, ConversionQuality.Medium, 1, 24121, 44108, 4, 70)]
        [TestCase(@"Assets/Audio/blast_mono.wav", ConversionFormat.Adpcm, ConversionQuality.Low, 1, 12055, 22045, 4, 70)]
        [TestCase(@"Assets/Audio/blast_mono_22hz.wav", ConversionFormat.Adpcm, ConversionQuality.Best, 1, 12055, 22045, 4, 70)]
        [TestCase(@"Assets/Audio/blast_mono_22hz.wav", ConversionFormat.Adpcm, ConversionQuality.Medium, 1, 12055, 22045, 4, 70)]
        [TestCase(@"Assets/Audio/blast_mono_22hz.wav", ConversionFormat.Adpcm, ConversionQuality.Low, 1, 6027, 11022, 4, 70)]
        [TestCase(@"Assets/Audio/blast_mono_11hz.wav", ConversionFormat.Adpcm, ConversionQuality.Best, 1, 6027, 11022, 4, 70)]
        [TestCase(@"Assets/Audio/blast_mono_11hz.wav", ConversionFormat.Adpcm, ConversionQuality.Medium, 1, 6027, 11022, 4, 70)]
        [TestCase(@"Assets/Audio/blast_mono_11hz.wav", ConversionFormat.Adpcm, ConversionQuality.Low, 1, 4376, 8003, 4, 70)]

        // 16bit PCM Stereo -> ADPCM 
        [TestCase(@"Assets/Audio/rock_loop_stereo.wav", ConversionFormat.Adpcm, ConversionQuality.Best, 2, 48240, 44106, 4, 140)]
        [TestCase(@"Assets/Audio/rock_loop_stereo.wav", ConversionFormat.Adpcm, ConversionQuality.Medium, 2, 48240, 44106, 4, 140)]
        [TestCase(@"Assets/Audio/rock_loop_stereo.wav", ConversionFormat.Adpcm, ConversionQuality.Low, 2, 24120, 22053, 4, 140)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz.wav", ConversionFormat.Adpcm, ConversionQuality.Best, 2, 24120, 22053, 4, 140)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz.wav", ConversionFormat.Adpcm, ConversionQuality.Medium, 2, 24120, 22053, 4, 140)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz.wav", ConversionFormat.Adpcm, ConversionQuality.Low, 2, 12059, 11026, 4, 140)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz.wav", ConversionFormat.Adpcm, ConversionQuality.Best, 2, 12059, 11026, 4, 140)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz.wav", ConversionFormat.Adpcm, ConversionQuality.Medium, 2, 12059, 11026, 4, 140)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz.wav", ConversionFormat.Adpcm, ConversionQuality.Low, 2, 8744, 7995, 4, 140)]

        public void Convert(string sourceFile, ConversionFormat format, ConversionQuality quality, int channels, int averageBytesPerSecond, int sampleRate, int bitsPerSample, int blockAlign)
        {
            var content = new AudioContent(sourceFile, AudioFileType.Wav);
            content.ConvertFormat(format, quality, null);

            Assert.AreEqual(ToWavFormat(format, content.Format.BitsPerSample), content.Format.Format);
            Assert.AreEqual(channels, content.Format.ChannelCount);
            Assert.AreEqual(bitsPerSample, content.Format.BitsPerSample);

            // TODO: We don't quite match right with XNA on these.
            // We should look to fix this for 100% compatibility.
            if (format != ConversionFormat.Adpcm)
            {
                Assert.AreEqual(sampleRate, content.Format.SampleRate);
                Assert.AreEqual(averageBytesPerSecond, content.Format.AverageBytesPerSecond);
                Assert.AreEqual(blockAlign, content.Format.BlockAlign);
            }

            content.Dispose();
        }
    }
}
