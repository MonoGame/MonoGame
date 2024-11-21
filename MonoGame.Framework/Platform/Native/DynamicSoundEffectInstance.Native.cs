// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System;

namespace Microsoft.Xna.Framework.Audio;

public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance
{
    private unsafe void PlatformCreate()
    {
        Voice = MGA.Voice_Create(SoundEffect.System, _sampleRate, (int)_channels);
    }

    private unsafe int PlatformGetPendingBufferCount()
    {
        if (Voice != null)
            return MGA.Voice_GetBufferCount(Voice);

        return 0;
    }

    private unsafe void PlatformPlay()
    {
        if (Voice != null)
            MGA.Voice_Play(Voice, true);
    }

    private unsafe void PlatformPause()
    {
        if (Voice != null)
            MGA.Voice_Pause(Voice);
    }

    private unsafe void PlatformResume()
    {
        if (Voice != null)
            MGA.Voice_Resume(Voice);
    }

    private unsafe void PlatformStop()
    {
        if (Voice != null)
            MGA.Voice_Stop(Voice, true);
    }

    private unsafe void PlatformSubmitBuffer(byte[] buffer, int offset, int count)
    {
        if (Voice != null)
        {
            fixed (byte* ptr = buffer)
                MGA.Voice_AppendBuffer(Voice, ptr + offset, (uint)count);
        }
    }

    private unsafe void PlatformDispose(bool disposing)
    {
        if (disposing)
        {
            if (Voice != null)
            {
                MGA.Voice_Destroy(Voice);
                Voice = null;
            }
        }
    }

    private void PlatformUpdateQueue()
    {
        // TODO: This really shouldn't be per-instance
        // instead this should be handled internally by
        // the native sound system.
    }
}
