// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGA.h"

#include "mg_common.h"


// TODO: Implement against the C++ API for FAudio.

struct MGA_System
{
};

struct MGA_Buffer
{
};

struct MGA_Voice
{
};


MGA_System* MGA_System_Create()
{
	auto system = new MGA_System();
	return system;
}

void MGA_System_Destroy(MGA_System* system)
{
	assert(system != nullptr);

	// TODO: We're assuming here the C# side is cleaning up
	// buffers/voices, but if we want this to be a good C++
	// API as well, we likely should cleanup ourselves too.

	delete system;
}

mgint MGA_System_GetMaxInstances()
{
	return INT_MAX;
}

void MGA_System_SetReverbSettings(MGA_System* system, ReverbSettings& settings)
{
	assert(system != nullptr);
}

MGA_Buffer* MGA_Buffer_Create(MGA_System* system)
{
	assert(system != nullptr);
	auto buffer = new MGA_Buffer();
	return buffer;
}

void MGA_Buffer_Destroy(MGA_Buffer* buffer)
{
	assert(buffer != nullptr);
	delete buffer;
}

void MGA_Buffer_InitializeFormat(MGA_Buffer* buffer, mgbyte* waveHeader, mgbyte* waveData, mgint length, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveHeader != nullptr);
	assert(waveData != nullptr);
	assert(length > 0);
}

void MGA_Buffer_InitializePCM(MGA_Buffer* buffer, mgbyte* waveData, mgint offset, mgint length, mgint sampleBits, mgint sampleRate, mgint channels, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveData != nullptr);
	assert(offset >=0);
	assert(length > 0);
}

void MGA_Buffer_InitializeXact(MGA_Buffer* buffer, mguint codec, mgbyte* waveData, mgint length, mgint sampleRate, mgint blockAlignment, mgint channels, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveData != nullptr);
	assert(length > 0);
}

mgulong MGA_Buffer_GetDuration(MGA_Buffer* buffer)
{
	assert(buffer != nullptr);
	return 0;
}

MGA_Voice* MGA_Voice_Create(MGA_System* system, mgint sampleRate, mgint channels)
{
	assert(system != nullptr);
	auto voice = new MGA_Voice();
	return voice;
}

void MGA_Voice_Destroy(MGA_Voice* voice)
{
	assert(voice != nullptr);
	delete voice;
}

mgint MGA_Voice_GetBufferCount(MGA_Voice* voice)
{
	assert(voice != nullptr);
	return 0;
}

void MGA_Voice_SetBuffer(MGA_Voice* voice, MGA_Buffer* buffer)
{
	assert(voice != nullptr);

	// Stop and remove any pending buffers first.

	// Append a buffer if we got one.
	if (buffer)
	{

	}
}

void MGA_Voice_AppendBuffer(MGA_Voice* voice, mgbyte* buffer, mguint size)
{
	assert(voice != nullptr);
	assert(buffer != nullptr);

	// The idea here is that for streaming cases and dynamic buffers
	// we internally allocate a big chunk of memory for it and
	// break it up into smaller buffers for submission.
		
	// Allocate a dynamic buffer that can be reused later.	
	// Append the buffer.
}

void MGA_Voice_Play(MGA_Voice* voice, mgbool looped)
{
	assert(voice != nullptr);
}

void MGA_Voice_Pause(MGA_Voice* voice)
{
	assert(voice != nullptr);
}

void MGA_Voice_Resume(MGA_Voice* voice)
{
	assert(voice != nullptr);
}

void MGA_Voice_Stop(MGA_Voice* voice, mgbool immediate)
{
	assert(voice != nullptr);
}

MGSoundState MGA_Voice_GetState(MGA_Voice* voice)
{
	assert(voice != nullptr);
	return MGSoundState::Stopped;
}

mgulong MGA_Voice_GetPosition(MGA_Voice* voice)
{
	assert(voice != nullptr);
	return 0;
}

void MGA_Voice_SetPan(MGA_Voice* voice, mgfloat pan)
{
	assert(voice != nullptr);
}

void MGA_Voice_SetPitch(MGA_Voice* voice, mgfloat pitch)
{
	assert(voice != nullptr);
}

void MGA_Voice_SetVolume(MGA_Voice* voice, mgfloat volume)
{
	assert(voice != nullptr);
}

void MGA_Voice_SetReverbMix(MGA_Voice* voice, mgfloat mix)
{
	assert(voice != nullptr);
}

void MGA_Voice_SetFilterMode(MGA_Voice* voice, MGFilterMode mode, mgfloat filterQ, mgfloat frequency)
{
	assert(voice != nullptr);
}

void MGA_Voice_ClearFilterMode(MGA_Voice* voice)
{
	assert(voice != nullptr);
}

void MGA_Voice_Apply3D(MGA_Voice* voice, Listener& listener, Emitter& emitter, mgfloat distanceScale)
{
	assert(voice != nullptr);
}

