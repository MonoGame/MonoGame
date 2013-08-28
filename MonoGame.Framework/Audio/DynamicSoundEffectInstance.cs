using System;

namespace Microsoft.Xna.Framework.Audio {
    public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance {
        private const int SAMPLE_WIDTH = 2;
        private int sampleRate;
        private int bytesPerSecond;
        public int PendingBufferCount { get; private set; }

        // Events
        public event EventHandler<EventArgs> BufferNeeded;

        internal void OnBufferNeeded (EventArgs args)
        {
            if (BufferNeeded != null) {
                BufferNeeded (this, args);
            }
        }

        public DynamicSoundEffectInstance (int sampleRate, AudioChannels channels)
        {
            soundState = SoundState.Stopped;
            this.sampleRate = sampleRate;
            setFormatFor (channels);
            bytesPerSecond = ((int) channels) * sampleRate * SAMPLE_WIDTH;
        }

        public TimeSpan GetSampleDuration (int sizeInBytes)
        {
            return new TimeSpan (0, 0, 0, 0, (int) ((long) sizeInBytes * 1000L) / bytesPerSecond);
        }

        public int GetSampleSizeInBytes (TimeSpan duration)
        {
            return (int) (duration.TotalSeconds * bytesPerSecond);
        }

        public void SubmitBuffer (byte [] buffer)
        {
            this.SubmitBuffer (buffer, 0, buffer.Length);
        }

        public void SubmitBuffer (byte [] buffer, int offset, int count)
        {
            QueueDataBuffer (buffer, format, offset, count, sampleRate);
        }
    }
}
