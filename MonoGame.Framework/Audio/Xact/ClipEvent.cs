// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	abstract class ClipEvent
    {
        protected float _curTime;
        public float _timeStamp;
		public float _randomOffset;

		public XactClip _clip;
			
		public abstract void Play();
		public abstract void Stop();
		public abstract void Pause();
        public abstract void Resume();
        public abstract void Update(float dt);
        public abstract void SetFade(float fadeInDuration, float fadeOutDuration);
        public abstract bool IsReady { get; }
		public abstract bool Playing { get; }
		public abstract float Volume { get; set; }
		public abstract bool IsPaused { get; }
	}
}

