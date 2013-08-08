using System;
#if MONOMAC
using MonoMac.OpenAL;
#else
using OpenTK.Audio.OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance
    {
        private ALFormat format;

        private void setFormatFor(AudioChannels channels)
        {
            switch (channels)
            {
                case AudioChannels.Mono:
                    this.format = ALFormat.Mono16;
                    break;
                case AudioChannels.Stereo:
                    this.format = ALFormat.Stereo16;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("channels", channels, "Expected Mono or Stereo");
            }                       
        }

        public override void Play()
        {
            throw new NotImplementedException();
        }
    }

}
