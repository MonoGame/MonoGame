// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	abstract class ClipEvent
    {
        protected XactClip _clip;

	    protected ClipEvent(XactClip clip, float timeStamp, float randomOffset)
        {
            _clip = clip;
            TimeStamp = timeStamp;
            RandomOffset = randomOffset;
        }

	    public float RandomOffset { get; private set; }

	    public float TimeStamp { get; private set; }

	    public abstract void Play();
	    public abstract void Stop();
		public abstract void Pause();
        public abstract void Resume();
        public abstract void SetFade(float fadeInDuration, float fadeOutDuration);
        public abstract void SetTrackVolume(float volume);
	    public abstract bool Update(float dt);

        public abstract void Apply3D(AudioListener listener, AudioEmitter emitter);
    }
}

