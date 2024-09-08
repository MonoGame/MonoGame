// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Interop;


namespace Microsoft.Xna.Framework.Audio;

public partial class SoundEffectInstance : IDisposable
{
    internal unsafe MGA_Voice* Voice;


    private unsafe void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
    {
        var listener_ = new Listener()
        {
            Position = listener.Position,
            Forward = listener.Forward,
            Up = listener.Up,
            Velocity = listener.Velocity,
        };

        var emitter_ = new Emitter()
        {
            Position = emitter.Position,
            Forward = emitter.Forward,
            Up = emitter.Up,
            Velocity = emitter.Velocity,
            DopplerScale = emitter.DopplerScale
        };

        if (Voice != null)
            MGA.Voice_Apply3D(Voice, in listener_, in emitter_, SoundEffect.DistanceScale);
    }

    private unsafe void PlatformPause()
    {
        if (Voice != null)
            MGA.Voice_Pause(Voice);
    }

    private unsafe void PlatformPlay()
    {
        if (Voice != null)
            MGA.Voice_Play(Voice, _isLooped);
    }

    private unsafe void PlatformResume()
    {
        if (Voice != null)
            MGA.Voice_Resume(Voice);
    }

    private unsafe void PlatformStop(bool immediate)
    {
        if (Voice != null)
            MGA.Voice_Stop(Voice, immediate);
    }

    private unsafe void PlatformSetPan(float pan)
    {
        if (Voice != null)
            MGA.Voice_SetPan(Voice, pan);
    }

    private unsafe void PlatformSetPitch(float pitch)
    {
        if (Voice != null)
            MGA.Voice_SetPitch(Voice, pitch);
    }

    private unsafe SoundState PlatformGetState()
    {
        if (Voice != null)
            return MGA.Voice_GetState(Voice);

        return SoundState.Stopped;
    }

    private unsafe void PlatformSetVolume(float volume)
    {
        if (Voice != null)
            MGA.Voice_SetVolume(Voice, volume);
    }

    internal unsafe void PlatformSetReverbMix(float mix)
    {
        if (Voice != null)
            MGA.Voice_SetReverbMix(Voice, mix);
    }

    internal unsafe void PlatformSetFilter(FilterMode mode, float filterQ, float frequency)
    {
        if (Voice != null)
            MGA.Voice_SetFilterMode(Voice, mode, filterQ, frequency);
    }

    internal unsafe void PlatformClearFilter()
    {
        if (Voice != null)
            MGA.Voice_ClearFilterMode(Voice);
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
}
