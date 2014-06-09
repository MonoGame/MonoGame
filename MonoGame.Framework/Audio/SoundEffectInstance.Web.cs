// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    public partial class SoundEffectInstance : IDisposable
    {
        internal void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
        {
        }

        private void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
        }

        private void PlatformPause()
        {
        }

        private void PlatformPlay()
        {
        }

        private void PlatformResume()
        {
        }

        private void PlatformStop(bool immediate)
        {
        }

        private void PlatformSetIsLooped(bool value)
        {
        }

        private bool PlatformGetIsLooped()
        {
            return false;
        }

        private void PlatformSetPan(float value)
        {
        }

        private void PlatformSetPitch(float value)
        {
        }

        private SoundState PlatformGetState()
        {
            return SoundState.Stopped;
        }

        private void PlatformSetVolume(float value)
        {
        }

        private void PlatformDispose(bool disposing)
        {
        }
    }
}
