// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	class VolumeEvent : ClipEvent
	{
	    private readonly float _volume;

        public VolumeEvent(XactClip clip, float timeStamp, float randomOffset, float volume)
            : base(clip, timeStamp, randomOffset)
        {
            _volume = volume;
        }

		public override void Play() 
        {
            _clip.SetVolume(_volume);
        }

	    public override void Stop()
	    {
	    }

	    public override void Pause() 
        {
		}

		public override void Resume()
		{
		}

		public override void SetTrackVolume(float volume)
        {
		}

	    public override void SetTrackPan(float pan)
	    {
	    }

	    public override void SetState(float volume, float pitch, float reverbMix, float? filterFrequency, float? filterQFactor)
	    {
	    }

	    public override bool Update(float dt)
	    {
	        return false;
	    }

	    public override void SetFade(float fadeInDuration, float fadeOutDuration)
        {
        }

    }
}

