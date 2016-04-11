using System;
using Microsoft.Xna.Framework.Audio;
using NUnit.Framework;

namespace MonoGame.Tests.Framework.Audio
{
    public class SoundEffectTests
    {
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
    }
}