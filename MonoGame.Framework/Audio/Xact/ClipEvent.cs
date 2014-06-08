// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	abstract class ClipEvent
    {
        private float _curTime;
        private float _timeStamp;
        private float _randomOffset;
        private XactClip _clip;

        public ClipEvent(XactClip clip, float timeStamp, float randomOffset)
        {
            _clip = clip;
            _timeStamp = timeStamp;
            _randomOffset = randomOffset;
        }

		public abstract void Play();

        public virtual void Stop()
        {
            _curTime = 0.0f;
        }

		public abstract void Pause();
        public abstract void Resume();
        public abstract void SetFade(float fadeInDuration, float fadeOutDuration);
		public abstract bool Playing { get; }
		public abstract float Volume { get; set; }
		public abstract bool IsPaused { get; }

        public bool IsReady
        {
            get { return _curTime >= _timeStamp; }
        }

        public virtual void Update(float dt)
        {
            if (IsReady)
                return;

            _curTime += dt;

            if (!IsReady)
                return;

            _clip.Play();
        }
	}
}

