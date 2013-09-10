using System;
using NUnit.Framework;
using Microsoft.Xna.Framework.Audio;

namespace MonoGame.Tests {
	[TestFixture]
	public class DynamicEffectInstanceTest {
		readonly int rate = 22000;
		readonly TimeSpan oneSecond = new TimeSpan (0, 0, 1);
		readonly TimeSpan halfSecond = new TimeSpan (0, 0, 0, 0, 500);
		readonly int sampleDepthInBytes = 2;

		[Test]
		public void throwsWhenGivenInvalidAudioChannels()
		{
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { new DynamicSoundEffectInstance(1, (AudioChannels) 666); });
		}

		[Test]
		public void durationIsOneSecondWithMonoChannel() {
			int sampleSizeInBytes = (int) AudioChannels.Mono * rate * sampleDepthInBytes;
			var effect = new DynamicSoundEffectInstance (rate, AudioChannels.Mono);
			Assert.That(effect.GetSampleDuration (sampleSizeInBytes), Is.EqualTo (oneSecond));
		}

		[Test]
		public void durationIsOneSecondWithStereoChannel() {
			int sampleSizeInBytes = (int) AudioChannels.Stereo * rate * sampleDepthInBytes;
			var effect = new DynamicSoundEffectInstance (rate, AudioChannels.Stereo);
			Assert.That(effect.GetSampleDuration (sampleSizeInBytes), Is.EqualTo (oneSecond));
		}

		[Test]
		public void durationIsOneHalfSecond() {
			int sampleSizeInBytes = (int) AudioChannels.Stereo * rate * sampleDepthInBytes / 2;
			var effect = new DynamicSoundEffectInstance (rate, AudioChannels.Stereo);
			Assert.That(effect.GetSampleDuration (sampleSizeInBytes), Is.EqualTo (halfSecond));
		}

		[Test]
		public void sampleSizeWhenDurationIsOneSecond() {
			int sampleSizeInBytes = (int) AudioChannels.Mono * rate * sampleDepthInBytes;
			var effect = new DynamicSoundEffectInstance (rate, AudioChannels.Mono);
			Assert.That (effect.GetSampleSizeInBytes (oneSecond), Is.EqualTo (sampleSizeInBytes));
		}

		[Test]
		public void sampleSizeWhenDurationIsOneHalfSecond() {
			int sampleSizeInBytes = (int) AudioChannels.Mono * rate * sampleDepthInBytes / 2;
			var effect = new DynamicSoundEffectInstance (rate, AudioChannels.Mono);
			Assert.That (effect.GetSampleSizeInBytes (halfSecond), Is.EqualTo (sampleSizeInBytes));
		}

	}
}

