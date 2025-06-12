// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance
    {
        private void PlatformCreate()
        {
        }

        private int PlatformGetPendingBufferCount()
        {
            return 0;
        }

        private void PlatformPlay()
        {
        }

        private void PlatformPause()
        {
        }

        private void PlatformResume()
        {
        }

        private void PlatformStop()
        {
        }

        private void PlatformSubmitBuffer(byte[] buffer, int offset, int count)
        {
        }

        private void PlatformDispose(bool disposing)
        {
        }

        private void PlatformUpdateQueue()
        {
        }
    }
}
