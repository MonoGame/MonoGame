// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance
    {
        private void PlatformCreate()
        {
            throw new NotImplementedException();
        }

        private int PlatformGetPendingBufferCount()
        {
            throw new NotImplementedException();
        }

        private void PlatformPlay()
        {
            throw new NotImplementedException();
        }

        private void PlatformPause()
        {
            throw new NotImplementedException();
        }

        private void PlatformResume()
        {
            throw new NotImplementedException();
        }

        private void PlatformStop()
        {
            throw new NotImplementedException();
        }

        private void PlatformSubmitBuffer(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        private void PlatformUpdateQueue()
        {
            throw new NotImplementedException();
        }
    }
}
