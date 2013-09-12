using System;

namespace Microsoft.Xna.Framework.Audio {
    public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance {
        private const int SAMPLE_WIDTH = 2;
        private int sampleRate;
        private int bytesPerSecond;

        public event EventHandler<EventArgs> BufferNeeded;

        public override bool IsLooped {
            get { return false; }
            set { throw new InvalidOperationException("cannot loop DynamicSoundEffect"); }
        }

        public int PendingBufferCount { get; private set; }

        internal void OnBufferNeeded(EventArgs args)
        {
            if (BufferNeeded != null) {
                BufferNeeded(this, args);
            }
        }

        public DynamicSoundEffectInstance (int sampleRate, AudioChannels channels)
        {
            soundState = SoundState.Stopped;
            this.sampleRate = sampleRate;
            setFormatFor(channels);
            bytesPerSecond = ((int) channels) * sampleRate * SAMPLE_WIDTH;
        }

        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            return new TimeSpan(0, 0, 0, 0, (int) ((long) sizeInBytes * 1000L) / bytesPerSecond);
        }

        public int GetSampleSizeInBytes(TimeSpan duration)
        {
            return (int) (duration.TotalSeconds * bytesPerSecond);
        }

        public void SubmitBuffer(byte[] buffer)
        {
            this.SubmitBuffer(buffer, 0, buffer.Length);
        }

        public void SubmitBuffer(byte[] buffer, int offset, int count)
        {
            QueueDataBuffer(buffer, format, offset, count, sampleRate);
        }

        public override void Play()
        {
            startPlaying();
        }

        public override void Resume()
        {
            Play();
        }

        public override void Stop()
        {
            soundState = SoundState.Stopped;
        }

        public override void Pause()
        {
            if (soundState == SoundState.Playing) {
                soundState = SoundState.Paused;
            }
        }
    }
}
