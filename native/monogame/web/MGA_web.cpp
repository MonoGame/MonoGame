#include "../include/api_MGA.h"

struct MGA_System
{
};

struct MGA_Buffer
{
};

struct MGA_Voice
{
    MGSoundState state = MGSoundState::Stopped;
};

MG_EXPORT MGA_System* MGA_System_Create()
{
    return new MGA_System();
}

MG_EXPORT void MGA_System_Destroy(MGA_System* system)
{
    delete system;
}

MG_EXPORT mgint MGA_System_GetMaxInstances()
{
    // TODO: browser audio not wired yet, so just reporting no capacity until it's implemented.
    return 0;
}

MG_EXPORT void MGA_System_SetReverbSettings(MGA_System* system, ReverbSettings& settings)
{
    (void)system;
    (void)settings;
}

MG_EXPORT MGA_Buffer* MGA_Buffer_Create(MGA_System* system)
{
    (void)system;
    return new MGA_Buffer();
}

MG_EXPORT void MGA_Buffer_Destroy(MGA_Buffer* buffer)
{
    delete buffer;
}

MG_EXPORT void MGA_Buffer_InitializeFormat(MGA_Buffer* buffer, mgbyte* waveHeader, mgbyte* waveData, mgint length, mgint loopStart, mgint loopLength)
{
    (void)buffer;
    (void)waveHeader;
    (void)waveData;
    (void)length;
    (void)loopStart;
    (void)loopLength;
}

MG_EXPORT void MGA_Buffer_InitializePCM(MGA_Buffer* buffer, mgbyte* waveData, mgint offset, mgint length, mgint sampleBits, mgint sampleRate, mgint channels, mgint loopStart, mgint loopLength)
{
    (void)buffer;
    (void)waveData;
    (void)offset;
    (void)length;
    (void)sampleBits;
    (void)sampleRate;
    (void)channels;
    (void)loopStart;
    (void)loopLength;
}

MG_EXPORT void MGA_Buffer_InitializeXact(MGA_Buffer* buffer, mguint codec, mgbyte* waveData, mgint length, mgint sampleRate, mgint blockAlignment, mgint channels, mgint loopStart, mgint loopLength)
{
    (void)buffer;
    (void)codec;
    (void)waveData;
    (void)length;
    (void)sampleRate;
    (void)blockAlignment;
    (void)channels;
    (void)loopStart;
    (void)loopLength;
}

MG_EXPORT mgulong MGA_Buffer_GetDuration(MGA_Buffer* buffer)
{
    (void)buffer;
    return 0;
}

MG_EXPORT MGA_Voice* MGA_Voice_Create(MGA_System* system, mgint sampleRate, mgint channels)
{
    (void)system;
    (void)sampleRate;
    (void)channels;
    return new MGA_Voice();
}

MG_EXPORT void MGA_Voice_Destroy(MGA_Voice* voice)
{
    delete voice;
}

MG_EXPORT mgint MGA_Voice_GetBufferCount(MGA_Voice* voice)
{
    (void)voice;
    return 0;
}

MG_EXPORT mgint MGA_Voice_GetFinishedBufferCount(MGA_Voice* voice)
{
    (void)voice;
    return 0;
}

MG_EXPORT void MGA_Voice_SetBuffer(MGA_Voice* voice, MGA_Buffer* buffer)
{
    (void)voice;
    (void)buffer;
}

MG_EXPORT void MGA_Voice_AppendBuffer(MGA_Voice* voice, mgbyte* buffer, mguint size)
{
    (void)voice;
    (void)buffer;
    (void)size;
}

MG_EXPORT void MGA_Voice_Play(MGA_Voice* voice, mgbyte looped)
{
    (void)looped;

    if (voice != nullptr)
        voice->state = MGSoundState::Playing;
}

MG_EXPORT void MGA_Voice_Pause(MGA_Voice* voice)
{
    if (voice != nullptr)
        voice->state = MGSoundState::Paused;
}

MG_EXPORT void MGA_Voice_Resume(MGA_Voice* voice)
{
    if (voice != nullptr)
        voice->state = MGSoundState::Playing;
}

MG_EXPORT void MGA_Voice_Stop(MGA_Voice* voice, mgbyte immediate)
{
    (void)immediate;

    if (voice != nullptr)
        voice->state = MGSoundState::Stopped;
}

MG_EXPORT MGSoundState MGA_Voice_GetState(MGA_Voice* voice)
{
    if (voice == nullptr)
        return MGSoundState::Stopped;

    return voice->state;
}

MG_EXPORT mgulong MGA_Voice_GetPosition(MGA_Voice* voice)
{
    (void)voice;
    return 0;
}

MG_EXPORT void MGA_Voice_SetPan(MGA_Voice* voice, mgfloat pan)
{
    (void)voice;
    (void)pan;
}

MG_EXPORT void MGA_Voice_SetPitch(MGA_Voice* voice, mgfloat pitch)
{
    (void)voice;
    (void)pitch;
}

MG_EXPORT void MGA_Voice_SetVolume(MGA_Voice* voice, mgfloat volume)
{
    (void)voice;
    (void)volume;
}

MG_EXPORT void MGA_Voice_SetReverbMix(MGA_Voice* voice, mgfloat mix)
{
    (void)voice;
    (void)mix;
}

MG_EXPORT void MGA_Voice_SetFilterMode(MGA_Voice* voice, MGFilterMode mode, mgfloat filterQ, mgfloat frequency)
{
    (void)voice;
    (void)mode;
    (void)filterQ;
    (void)frequency;
}

MG_EXPORT void MGA_Voice_ClearFilterMode(MGA_Voice* voice)
{
    (void)voice;
}

MG_EXPORT void MGA_Voice_Apply3D(MGA_Voice* voice, Listener& listener, Emitter& emitter, mgfloat distanceScale)
{
    (void)voice;
    (void)listener;
    (void)emitter;
    (void)distanceScale;
}
