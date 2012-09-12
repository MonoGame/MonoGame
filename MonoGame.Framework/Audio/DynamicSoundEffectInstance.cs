using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if MONOMAC
using MonoMac.OpenAL;
#else
using OpenTK.Audio.OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public sealed class DynamicSoundEffectInstance : SoundEffectInstance
    {
        private AudioChannels channels;
        private int sampleRate;
        private ALFormat format;
        private bool looped;
        private int pendingBufferCount;
        // Events
        public event EventHandler<EventArgs> BufferNeeded;

        internal void OnBufferNeeded(EventArgs args)
        {
            if (BufferNeeded != null)
            {
                BufferNeeded(this, args);
            }
        }

        public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels)
        {
            this.sampleRate = sampleRate;
            this.channels = channels;
            switch (channels)
            {
                case AudioChannels.Mono:
                    this.format = ALFormat.Mono16;
                    break;
                case AudioChannels.Stereo:
                    this.format = ALFormat.Stereo16;
                    break;
                default:
                    break;
            }                       
        }

        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            throw new NotImplementedException();
        }

        public int GetSampleSizeInBytes(TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        public override void Play()
        {
            throw new NotImplementedException();
        }

        public void SubmitBuffer(byte[] buffer)
        {
            this.SubmitBuffer(buffer, 0, buffer.Length);
        }

        public void SubmitBuffer(byte[] buffer, int offset, int count)
        {
            BindDataBuffer(buffer, format, count, sampleRate);
        }

        public override bool IsLooped
        {
            get
            {
                return looped;
            }

            set
            {
                looped = value;                
            }
        }

        public int PendingBufferCount 
        {
            get
            {
                return pendingBufferCount;
            }
            private set
            {
                pendingBufferCount = value;
            }
        }
    }


}
