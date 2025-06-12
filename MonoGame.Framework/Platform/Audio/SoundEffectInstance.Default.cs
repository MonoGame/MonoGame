// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
    public partial class SoundEffectInstance : IDisposable
    {
        internal void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
        {
            throw new NotImplementedException();
        }

        internal void InitializeSound()
        {
            throw new NotImplementedException();
        }

        private void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
            throw new NotImplementedException();
        }

        private void PlatformPause()
        {
            throw new NotImplementedException();
        }

        private void PlatformPlay()
        {
            throw new NotImplementedException();
        }

        private void PlatformResume()
        {
            throw new NotImplementedException();
        }

        private void PlatformStop(bool immediate)
        {
            throw new NotImplementedException();
        }

        private void FreeSource()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsLooped(bool value)
        {
            throw new NotImplementedException();
        }

        private bool PlatformGetIsLooped()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetPan(float value)
        {
            throw new NotImplementedException();
        }

        private void PlatformSetPitch(float value)
        {
            throw new NotImplementedException();
        }

        private SoundState PlatformGetState()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetVolume(float value)
        {
            throw new NotImplementedException();
        }

        internal void PlatformSetReverbMix(float mix)
        {
            throw new NotImplementedException();
        }

        void ApplyReverb()
        {
            throw new NotImplementedException();
        }

        void ApplyFilter()
        {
            throw new NotImplementedException();
        }

        internal void PlatformSetFilter(FilterMode mode, float filterQ, float frequency)
        {
            throw new NotImplementedException();
        }

        internal void PlatformClearFilter()
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose(bool disposing)
        {
            throw new NotImplementedException();
        }
    }
}
