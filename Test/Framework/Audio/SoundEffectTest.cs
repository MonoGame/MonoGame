// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Audio
{
    public class SoundEffectTests
    {
        [SetUp]
        public void Setup()
        {
            // Necessary to get audio initialised
            FrameworkDispatcher.Update();
        }

        [Test]
        public void Statics()
        {
            // NOTE: These can break if someone has changed them in
            // a prior test.  We just hope no one else messes with these.
            Assert.AreEqual(1.0f, SoundEffect.DistanceScale);
            Assert.AreEqual(1.0f, SoundEffect.DopplerScale);
            Assert.AreEqual(1.0f, SoundEffect.MasterVolume);
            Assert.AreEqual(343.5f, SoundEffect.SpeedOfSound);

            // TODO: Add some range tests.
        }

        [Test]
        public void GetSampleDuration()
        {
            // Test sizeInBytes range.
            Assert.Throws<ArgumentException>(() => SoundEffect.GetSampleDuration(-1, 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleDuration(1, 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleDuration(2, 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleDuration(3, 8000, AudioChannels.Mono));
            
            // Test sampleRate range.
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleDuration(2, -1, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleDuration(2, 0, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleDuration(2, 8000-1, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleDuration(2, 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleDuration(2, 48000, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleDuration(2, 48000 + 1, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleDuration(2, int.MaxValue, AudioChannels.Mono));

            // Test channel range.
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleDuration(2, 8000, (AudioChannels)(-1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleDuration(2, 8000, (AudioChannels)0));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleDuration(2, 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleDuration(2, 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleDuration(2, 8000, (AudioChannels)3));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleDuration(2, 8000, (AudioChannels)int.MaxValue));

            // Test for zero duration.
            Assert.AreEqual(TimeSpan.Zero, SoundEffect.GetSampleDuration(0, 8000, AudioChannels.Mono));
            Assert.AreEqual(TimeSpan.Zero, SoundEffect.GetSampleDuration(0, 48000, AudioChannels.Mono));
            Assert.AreEqual(TimeSpan.Zero, SoundEffect.GetSampleDuration(0, 8000, AudioChannels.Stereo));
            Assert.AreEqual(TimeSpan.Zero, SoundEffect.GetSampleDuration(0, 48000, AudioChannels.Stereo));

            // Test for one second.
            Assert.AreEqual(TimeSpan.FromSeconds(1), SoundEffect.GetSampleDuration(16000, 8000, AudioChannels.Mono));
            Assert.AreEqual(TimeSpan.FromSeconds(1), SoundEffect.GetSampleDuration(96000, 48000, AudioChannels.Mono));
            Assert.AreEqual(TimeSpan.FromSeconds(1), SoundEffect.GetSampleDuration(32000, 8000, AudioChannels.Stereo));
            Assert.AreEqual(TimeSpan.FromSeconds(1), SoundEffect.GetSampleDuration(192000, 48000, AudioChannels.Stereo));
        }

        [Test]
        public void GetSampleSizeInBytes()
        {
            // Test duration range.
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.MinValue, 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(0x7FFFFFF), 8000, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(0x7FFFFFF + 1), 8000, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.MaxValue, 8000, AudioChannels.Mono));

            // Test sampleRate range.
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, -1, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 0, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000 - 1, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 48000, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 48000 + 1, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, int.MaxValue, AudioChannels.Mono));

            // Test channel range.
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, (AudioChannels)(-1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, (AudioChannels)0));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, (AudioChannels)3));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, (AudioChannels)int.MaxValue));

            // Test for zero duration.
            Assert.AreEqual(0, SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, AudioChannels.Mono));
            Assert.AreEqual(0, SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 48000, AudioChannels.Mono));
            Assert.AreEqual(0, SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 8000, AudioChannels.Stereo));
            Assert.AreEqual(0, SoundEffect.GetSampleSizeInBytes(TimeSpan.Zero, 48000, AudioChannels.Stereo));

            // Test for one second.
            Assert.AreEqual(16000, SoundEffect.GetSampleSizeInBytes(TimeSpan.FromSeconds(1), 8000, AudioChannels.Mono));
            Assert.AreEqual(96000, SoundEffect.GetSampleSizeInBytes(TimeSpan.FromSeconds(1), 48000, AudioChannels.Mono));
            Assert.AreEqual(32000, SoundEffect.GetSampleSizeInBytes(TimeSpan.FromSeconds(1), 8000, AudioChannels.Stereo));
            Assert.AreEqual(192000, SoundEffect.GetSampleSizeInBytes(TimeSpan.FromSeconds(1), 48000, AudioChannels.Stereo));
        }

        [Test]
        public void Ctor1()
        {
            // Test buffer mono.
            Assert.Throws<ArgumentException>(() => new SoundEffect(null, 8000, AudioChannels.Mono));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[0], 8000, AudioChannels.Mono));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[1], 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 8000, AudioChannels.Mono));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[3], 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[4], 8000, AudioChannels.Mono));

            // Test buffer sterio.
            Assert.Throws<ArgumentException>(() => new SoundEffect(null, 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[0], 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[1], 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[3], 8000, AudioChannels.Stereo));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[4], 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[5], 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[6], 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[7], 8000, AudioChannels.Stereo));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[8], 8000, AudioChannels.Stereo));

            // Test sampleRate range.
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], -1, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 0, AudioChannels.Mono));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 48000, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 48000 + 1, AudioChannels.Mono));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], int.MaxValue, AudioChannels.Mono));

            // Test channel range.
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 8000, (AudioChannels)(-1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 8000, (AudioChannels)0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 8000, AudioChannels.Mono));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[4], 8000, AudioChannels.Stereo));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 8000, (AudioChannels)3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 8000, (AudioChannels)int.MaxValue));

            // Test the duration mono.
            {
                var s = new SoundEffect(new byte[2], 8000, AudioChannels.Mono);
                Assert.AreEqual(TimeSpan.Zero, s.Duration);
                s.Dispose();
                s = new SoundEffect(new byte[16000], 8000, AudioChannels.Mono);
                Assert.AreEqual(TimeSpan.FromSeconds(1), s.Duration);
                s.Dispose();
                s = new SoundEffect(new byte[2], 48000, AudioChannels.Mono);
                Assert.AreEqual(TimeSpan.Zero, s.Duration);
                s.Dispose();
                s = new SoundEffect(new byte[96000], 48000, AudioChannels.Mono);
                Assert.AreEqual(TimeSpan.FromSeconds(1), s.Duration);
                s.Dispose();
            }

            // Test the duration stereo.
            {
                var s = new SoundEffect(new byte[4], 8000, AudioChannels.Stereo);
                Assert.AreEqual(TimeSpan.Zero, s.Duration);
                s.Dispose();
                s = new SoundEffect(new byte[32000], 8000, AudioChannels.Stereo);
                Assert.AreEqual(TimeSpan.FromSeconds(1), s.Duration);
                s.Dispose();
                s = new SoundEffect(new byte[4], 48000, AudioChannels.Stereo);
                Assert.AreEqual(TimeSpan.Zero, s.Duration);
                s.Dispose();
                s = new SoundEffect(new byte[192000], 48000, AudioChannels.Stereo);
                Assert.AreEqual(TimeSpan.FromSeconds(1), s.Duration);
                s.Dispose();
            }

            // Test misc state.
            {
                var s = new SoundEffect(new byte[2], 8000, AudioChannels.Mono);
                Assert.AreEqual(string.Empty, s.Name);
                Assert.AreEqual(false, s.IsDisposed);
                s.Dispose();
                Assert.AreEqual(true, s.IsDisposed);
            }
        }

        [Test]
        public void Ctor2()
        {
            // Test buffer mono.
            Assert.Throws<ArgumentException>(() => new SoundEffect(null, 0, 0, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[0], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[1], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[3], 0, 4, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[4], 0, 4, 8000, AudioChannels.Mono, 0, 0));

            // Test buffer stereo.
            Assert.Throws<ArgumentException>(() => new SoundEffect(null, 0, 0, 8000, AudioChannels.Stereo, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[0], 0, 4, 8000, AudioChannels.Stereo, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[1], 0, 4, 8000, AudioChannels.Stereo, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 4, 8000, AudioChannels.Stereo, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[3], 0, 4, 8000, AudioChannels.Stereo, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[4], 0, 4, 8000, AudioChannels.Stereo, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[5], 0, 8, 8000, AudioChannels.Stereo, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[6], 0, 8, 8000, AudioChannels.Stereo, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[7], 0, 8, 8000, AudioChannels.Stereo, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[8], 0, 8, 8000, AudioChannels.Stereo, 0, 0));

            // Test offset.
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], int.MinValue, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], -1, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 1, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 2, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 3, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], int.MaxValue, 2, 8000, AudioChannels.Mono, 0, 0));

            // Test count.
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, int.MinValue, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, -1, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 0, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 1, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 3, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[4], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[4], 2, 2, 8000, AudioChannels.Mono, 0, 0));

            // XNA seems to not allow misaligned offsets even when
            // the data is within range of the buffer.  We go ahead
            // and allow this in MonoGame.
#if XNA
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[4], 1, 2, 8000, AudioChannels.Mono, 0, 0));
#else
            Assert.DoesNotThrow(() => new SoundEffect(new byte[4], 1, 2, 8000, AudioChannels.Mono, 0, 0));
#endif

            // Test sampleRate range.
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 0, 2, -1, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 0, 2, 0, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 48000, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 0, 2, 48000 + 1, AudioChannels.Mono, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 0, 2, int.MaxValue, AudioChannels.Mono, 0, 0));

            // Test channel range.
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 0, 2, 8000, (AudioChannels)(-1), 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 0, 2, 8000, (AudioChannels)0, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[4], 0, 4, 8000, AudioChannels.Stereo, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 0, 2, 8000, (AudioChannels)3, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoundEffect(new byte[2], 0, 2, 8000, (AudioChannels)int.MaxValue, 0, 0));

            // Test loop start.
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, int.MinValue, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, -1, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 1, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 2, 0));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, int.MaxValue, 0));

            // Test loop end.
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, int.MinValue));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, -1));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, 0));
            Assert.DoesNotThrow(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, 1));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, 2));
            Assert.Throws<ArgumentException>(() => new SoundEffect(new byte[2], 0, 2, 8000, AudioChannels.Mono, 0, int.MaxValue));
        }

        [Test, Ignore("creating/disposing a Game should not create/dispose the master voice")]
        public void InstanceNotDisposedWhenGameDisposed()
        {
            var game = new Game();

            var se = new SoundEffect(new byte[16000], 8000, AudioChannels.Mono);
            var s = se.CreateInstance();
            var d = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);

            game.Dispose();

            Assert.IsFalse(s.IsDisposed);
            Assert.IsFalse(d.IsDisposed);

            s.Dispose();
            d.Dispose();
        }

        private byte[] LoadRiff(string filename, out int sampleRate, out AudioChannels channels)
        {
            using (var stream = File.OpenRead(filename))
            using (var reader = new BinaryReader(stream))
            {
                var signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new Exception("Missing RIFF header!");

                reader.ReadInt32(); // riff_chunck_size

                var wformat = new string(reader.ReadChars(4));
                if (wformat != "WAVE")
                    throw new Exception("Not WAVE format!");

                // Look for the format chunk.
                while (true)
                {
                    var chunkSignature = new string(reader.ReadChars(4));
                    if (chunkSignature.ToLowerInvariant() == "fmt ")
                        break;
                    reader.BaseStream.Seek(reader.ReadInt32(), SeekOrigin.Current);
                }

                // Read the format header.
                var headerSize = reader.ReadInt32();
                var header = reader.ReadBytes(headerSize);

                channels = (AudioChannels)BitConverter.ToInt16(header, 2);
                sampleRate = BitConverter.ToInt32(header, 4);

                // Look for the data chunk.
                while (true)
                {
                    var chunkSignature = new string(reader.ReadChars(4));
                    if (chunkSignature.ToLowerInvariant() == "data")
                        break;
                    reader.BaseStream.Seek(reader.ReadInt32(), SeekOrigin.Current);
                }

                var dataSize = reader.ReadInt32();
                return reader.ReadBytes(dataSize);
            }
        }

        [TestCase(@"Assets/Audio/blast_mono.wav", 71650000)]
        [TestCase(@"Assets/Audio/blast_mono_22hz.wav", 71650000)]
        [TestCase(@"Assets/Audio/blast_mono_11hz.wav", 71650000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo.wav", 79400000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz.wav", 79400000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz.wav", 79400000)]
        public void LoadCtor1_16Bit(string filename, long durationTicks)
        {
            int sampleRate; AudioChannels channels;
            var data = LoadRiff(filename, out sampleRate, out channels);
            var sound = new SoundEffect(data, sampleRate, channels);
            Assert.AreEqual(durationTicks, sound.Duration.Ticks);
        }

        [TestCase(@"Assets/Audio/bark_mono_44hz_8bit.wav")]
        [TestCase(@"Assets/Audio/bark_mono_22hz_8bit.wav")]
        [TestCase(@"Assets/Audio/bark_mono_11hz_8bit.wav")]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz_8bit.wav")]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz_8bit.wav")]
        public void LoadCtor1_8Bit_Throws(string filename)
        {
            int sampleRate; AudioChannels channels;
            var data = LoadRiff(filename, out sampleRate, out channels);
            Assert.Throws<ArgumentException >(() => new SoundEffect(data, sampleRate, channels));
        }

        // These 8bit PCMs pass although the SoundEffect constructors although
        // they don't support 8bit PCM.  This is because it is interpreting it
        // as 16bit and generating a bad sound... hence half the duration.
        [TestCase(@"Assets/Audio/rock_loop_stereo_44hz_8bit.wav", 79400000)]
        public void LoadCtor1_8Bit_BadDuration(string filename, long durationTicks)
        {
            int sampleRate; AudioChannels channels;
            var data = LoadRiff(filename, out sampleRate, out channels);
            var sound = new SoundEffect(data, sampleRate, channels);
            Assert.AreEqual(durationTicks / 2, sound.Duration.Ticks);
        }

        // MSADPCM data can be passed into the constructors, but
        // it calculates and incorrect duration and plays static.
        [TestCase(@"Assets/Audio/blast_mono_44hz_adpcm_ms.wav", 18110000)]
        [TestCase(@"Assets/Audio/blast_mono_22hz_adpcm_ms.wav", 18110000)]
        [TestCase(@"Assets/Audio/blast_mono_11hz_adpcm_ms.wav", 18110000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_44hz_adpcm_ms.wav", 20140000)]
        public void LoadCtor1_MsAdpcm_BadDuration(string filename, long durationTicks)
        {
            int sampleRate; AudioChannels channels;
            var data = LoadRiff(filename, out sampleRate, out channels);
            var sound = new SoundEffect(data, sampleRate, channels);
            Assert.AreEqual(durationTicks, sound.Duration.Ticks);
        }

        [Test]
        public void FromStream_NotNull()
        {
            Assert.Throws<ArgumentNullException>(() => SoundEffect.FromStream(null));
        }

        [TestCase(@"Assets/Audio/blast_mono.wav", 71650000)]
        [TestCase(@"Assets/Audio/blast_mono_22hz.wav", 71650000)]
        [TestCase(@"Assets/Audio/blast_mono_11hz.wav", 71650000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo.wav", 79400000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz.wav", 79400000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz.wav", 79400000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_44hz_8bit.wav", 79400000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_22hz_8bit.wav", 79400000)]
        [TestCase(@"Assets/Audio/rock_loop_stereo_11hz_8bit.wav", 79400000)]
        [TestCase(@"Assets/Audio/bark_mono_44hz_8bit.wav", 16120000)]
        [TestCase(@"Assets/Audio/bark_mono_22hz_8bit.wav", 16120000)]
        [TestCase(@"Assets/Audio/bark_mono_11hz_8bit.wav", 16120000)]
        [TestCase(@"Assets/Audio/tone_mono_44khz_8bit.wav", 5000000)]
        [TestCase(@"Assets/Audio/tone_stereo_44khz_8bit.wav", 5000000)]
        [TestCase(@"Assets/Audio/tone_mono_44khz_16bit.wav", 5000000)]
        [TestCase(@"Assets/Audio/tone_stereo_44khz_16bit.wav", 5000000)]
#if !XNA
        // XNA does not support 24-bit, 32-bit float, MS-ADPCM or IMA/ADPCM in SoundEffect.FromStream, but MonoGame does
#if DIRECTX
        [TestCase(@"Assets/Audio/blast_mono_44hz_adpcm_ms.wav", 72020000)]
        [TestCase(@"Assets/Audio/blast_mono_22hz_adpcm_ms.wav", 72020000)]
        [TestCase(@"Assets/Audio/blast_mono_11hz_adpcm_ms.wav", 72020000)]
#else
        [TestCase(@"Assets/Audio/tone_mono_44khz_imaadpcm.wav", 5560000)]
        [TestCase(@"Assets/Audio/tone_stereo_44khz_imaadpcm.wav", 5090000)]
#endif
        [TestCase(@"Assets/Audio/tone_mono_44khz_msadpcm.wav", 5080000)]
        [TestCase(@"Assets/Audio/tone_stereo_44khz_msadpcm.wav", 5050000)]
        [TestCase(@"Assets/Audio/tone_mono_44khz_float.wav", 5000000)]
        [TestCase(@"Assets/Audio/tone_stereo_44khz_float.wav", 5000000)]
        [TestCase(@"Assets/Audio/tone_mono_44khz_24bit.wav", 5000000)]
        [TestCase(@"Assets/Audio/tone_stereo_44khz_24bit.wav", 5000000)]
#endif
        public void SoundEffectFromStream_Supported_Formats(string filename, long durationTicks)
        {
            using (var stream = File.OpenRead(filename))
            {
                var sound = SoundEffect.FromStream(stream);
                Assert.AreEqual(durationTicks, sound.Duration.Ticks);
            }
        }

#if XNA
        // MonoGame now supports loading ADPCM through SoundEffect.FromStream()
        [TestCase(@"Assets/Audio/blast_mono_44hz_adpcm_ms.wav")]
        [TestCase(@"Assets/Audio/blast_mono_22hz_adpcm_ms.wav")]
        [TestCase(@"Assets/Audio/blast_mono_11hz_adpcm_ms.wav")]
        public void SoundEffectFromStream_Unsupported_Formats(string filename)
        {
            using (var stream = File.OpenRead(filename))
                Assert.Throws<ArgumentException>(() => SoundEffect.FromStream(stream));
        }
#endif

        // Proxy for the content manager used in SoundEffectFromContent
        class GraphicsDeviceProxy : IGraphicsDeviceService
        {
            public GraphicsDevice GraphicsDevice
            {
                get { return null; }
            }

            public event EventHandler<EventArgs> DeviceCreated;

            public event EventHandler<EventArgs> DeviceDisposing;

            public event EventHandler<EventArgs> DeviceReset;

            public event EventHandler<EventArgs> DeviceResetting;
        }

        class ContentManagerProxy : ContentManager
        {
            public ContentManagerProxy(IServiceProvider services): base(services) {}

            protected override Stream OpenStream(string assetName)
            {
                var fileName = Path.Combine(RootDirectory, assetName + ".xnb");
                if (File.Exists(fileName))
                    return new FileStream(fileName, FileMode.Open, FileAccess.Read);
                return base.OpenStream(assetName);
            }
        }

        [TestCase("tone_mono_44khz_8bit", 5000000)]
        [TestCase("tone_stereo_44khz_8bit", 5000000)]
        [TestCase("tone_mono_44khz_16bit", 5000000)]
        [TestCase("tone_stereo_44khz_16bit", 5000000)]
#if !XNA
        // XNA does not support 32-bit float, MS-ADPCM or IMA/ADPCM in SoundEffect.FromStream, but MonoGame does
#if !DIRECTX
        [TestCase("tone_mono_44khz_imaadpcm", 6010000)]
        [TestCase("tone_stereo_44khz_imaadpcm", 5300000)]
#endif
        [TestCase("tone_mono_44khz_float", 5000000)]
        [TestCase("tone_stereo_44khz_float", 5000000)]
        // XNA cannot seem to load our MS-ADPCM XNBs.
        [TestCase("tone_mono_44khz_msadpcm", 5070000)]
        [TestCase("tone_stereo_44khz_msadpcm", 5040000)]
#endif
        public void SoundEffectFromContent(string filename, long durationTicks)
        {
            var services = new GameServiceContainer();
            services.AddService<IGraphicsDeviceService>(new GraphicsDeviceProxy());
            var content = new ContentManagerProxy(services);
            var soundEffect = content.Load<SoundEffect>(Paths.Audio(filename));
            Assert.AreEqual(durationTicks, soundEffect.Duration.Ticks);
        }
    }
}
