// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework
{
	public static class FrameworkDispatcher
	{
        // "Updates the status of various framework components
        // (such as power state and media), and raises related events"
		public static void Update()
		{
            // Once per frame, we need to check currently 
            // playing sounds to see if they've stopped,
            // and return them back to the pool if so.
            SoundEffectInstancePool.Update();
            DynamicSoundEffectInstanceManager.UpdatePlayingInstances();
        }
	}
}

