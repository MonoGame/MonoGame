// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGA.h"

#include "mg_common.h"


#include "FAudio.h"
#include "FAudioFX.h"
#include "F3DAudio.h"

struct MGA_System
{
	FAudio* faudio = nullptr;
	FAudioMasteringVoice* masteringVoice = nullptr;
	FAudioSubmixVoice* reverbVoice = nullptr;
	F3DAUDIO_HANDLE f3daudio;
};

struct MGA_Buffer
{
	FAudioWaveFormatEx format;
	mgbyte* data = nullptr;
	mguint size = 0;
	mgulong duration = 0;
};

struct MGA_Voice
{
	FAudioSourceVoice* sourceVoice;
	MGA_System* system;
	MGSoundState state = MGSoundState::Stopped;
	mgfloat volume = 1.0f;
	mgfloat pitch = 0.0f;
	mgfloat pan = 0.0f;
	mgfloat reverbMix = 0.0f;
};


MGA_System* MGA_System_Create()
{
	// Create FAudio instance
	auto system = new MGA_System();
	uint32_t result = FAudioCreate(&system->faudio, 0, FAUDIO_DEFAULT_PROCESSOR);
	if (result != 0)
	{
		delete system;
		return nullptr;
	}

	// Create mastering voice
	result = FAudio_CreateMasteringVoice(
		system->faudio,
		&system->masteringVoice,
		FAUDIO_DEFAULT_CHANNELS,
		FAUDIO_DEFAULT_SAMPLERATE,
		0,
		0,
		nullptr
	);
	if (result != 0)
	{
		FAudio_Release(system->faudio);
		delete system;
		return nullptr;
	}

	// !TODO : Create reverb effect and submix voice

	// Initialize F3DAudio
	F3DAudioInitialize(SPEAKER_STEREO, 343.0f, system->f3daudio);

	return system;
}

void MGA_System_Destroy(MGA_System* system)
{
	assert(system != nullptr);

	// !TODO : Destroy reverb effect and submix voice

	if (system->masteringVoice)
	{
		FAudioVoice_DestroyVoice(system->masteringVoice);
	}
	if (system->faudio)
	{
		FAudio_Release(system->faudio);
	}

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
	if (buffer->data)
	{
		delete[] buffer->data;
	}
	delete buffer;
}

void MGA_Buffer_InitializeFormat(MGA_Buffer* buffer, mgbyte* waveHeader, mgbyte* waveData, mgint length, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveHeader != nullptr);
	assert(waveData != nullptr);
	assert(length > 0);

	// !TODO: Parse wave header and initialize format
	// For now, copy the data
	buffer->data = new mgbyte[length];
	memcpy(buffer->data, waveData, length);
	buffer->size = length;
}

void MGA_Buffer_InitializePCM(MGA_Buffer* buffer, mgbyte* waveData, mgint offset, mgint length, mgint sampleBits, mgint sampleRate, mgint channels, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveData != nullptr);
	assert(offset >=0);
	assert(length > 0);

	// Initialize PCM format
	buffer->format.wFormatTag = 1; // WAVE_FORMAT_PCM
	buffer->format.nChannels = channels;
	buffer->format.nSamplesPerSec = sampleRate;
	buffer->format.nAvgBytesPerSec = sampleRate * channels * (sampleBits / 8);
	buffer->format.nBlockAlign = channels * (sampleBits / 8);
	buffer->format.wBitsPerSample = sampleBits;
	buffer->format.cbSize = 0;

	// Copy audio data
	buffer->data = new mgbyte[length];
	memcpy(buffer->data, waveData + offset, length);
	buffer->size = length;

	// Calculate duration
	buffer->duration = (mgulong)((length * 1000) / buffer->format.nAvgBytesPerSec);
}

void MGA_Buffer_InitializeXact(MGA_Buffer* buffer, mguint codec, mgbyte* waveData, mgint length, mgint sampleRate, mgint blockAlignment, mgint channels, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveData != nullptr);
	assert(length > 0);

	// Initialize XACT format
	buffer->format.wFormatTag = codec;
	buffer->format.nChannels = channels;
	buffer->format.nSamplesPerSec = sampleRate;
	buffer->format.nAvgBytesPerSec = sampleRate * blockAlignment;
	buffer->format.nBlockAlign = blockAlignment;
	buffer->format.wBitsPerSample = 0; // Variable for compressed formats
	buffer->format.cbSize = 0;

	// Copy audio data
	buffer->data = new mgbyte[length];
	memcpy(buffer->data, waveData, length);
	buffer->size = length;

	// Calculate duration (approximate for compressed formats)
	if (codec == 0x166) // WMA
	{
		buffer->duration = (mgulong)((length * 1000) / (sampleRate * channels * 2));
	}
	else
	{
		buffer->duration = (mgulong)((length * 1000) / buffer->format.nAvgBytesPerSec);
	}
}

mgulong MGA_Buffer_GetDuration(MGA_Buffer* buffer)
{
	assert(buffer != nullptr);
	return buffer->duration;
}

MGA_Voice* MGA_Voice_Create(MGA_System* system, mgint sampleRate, mgint channels)
{
	assert(system != nullptr);
	auto voice = new MGA_Voice();

	voice->system = system;
	// Create source voice
	FAudioWaveFormatEx format;
	format.wFormatTag = 1; // WAVE_FORMAT_PCM
	format.nChannels = channels;
	format.nSamplesPerSec = sampleRate;
	format.nAvgBytesPerSec = sampleRate * channels * 2; // 16-bit
	format.nBlockAlign = channels * 2;
	format.wBitsPerSample = 16;
	format.cbSize = 0;
	uint32_t result = FAudio_CreateSourceVoice(
		system->faudio,
		&voice->sourceVoice,
		&format,
		0,
		FAUDIO_DEFAULT_FREQ_RATIO,
		nullptr,
		nullptr,
		nullptr
	);
	if (result != 0)
	{
		delete voice;
		return nullptr;
	}

	return voice;
}

void MGA_Voice_Destroy(MGA_Voice* voice)
{
	assert(voice != nullptr);
	if (voice->sourceVoice)
	{
		FAudioVoice_DestroyVoice(voice->sourceVoice);
	}
	delete voice;
}

mgint MGA_Voice_GetBufferCount(MGA_Voice* voice)
{
	assert(voice != nullptr);
	FAudioVoiceState state;
	FAudioSourceVoice_GetState(voice->sourceVoice, &state, 0);
	return state.BuffersQueued;
}

void MGA_Voice_SetBuffer(MGA_Voice* voice, MGA_Buffer* buffer)
{
	assert(voice != nullptr);

	// Stop and remove any pending buffers first.
	FAudioSourceVoice_Stop(voice->sourceVoice, 0, FAUDIO_COMMIT_NOW);
	FAudioSourceVoice_FlushSourceBuffers(voice->sourceVoice);

	// Append a buffer if we got one.
	if (buffer)
	{
		FAudioBuffer audioBuffer = {};
		audioBuffer.AudioBytes = buffer->size;
		audioBuffer.pAudioData = buffer->data;
		audioBuffer.PlayBegin = 0;
		audioBuffer.PlayLength = 0;
		audioBuffer.LoopBegin = 0;
		audioBuffer.LoopLength = 0;
		audioBuffer.LoopCount = 0;
		audioBuffer.pContext = nullptr;
		FAudioSourceVoice_SubmitSourceBuffer(voice->sourceVoice, &audioBuffer, nullptr);
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
	FAudioBuffer audioBuffer = {};
	audioBuffer.AudioBytes = size;
	audioBuffer.pAudioData = buffer;
	audioBuffer.PlayBegin = 0;
	audioBuffer.PlayLength = 0;
	audioBuffer.LoopBegin = 0;
	audioBuffer.LoopLength = 0;
	audioBuffer.LoopCount = 0;
	audioBuffer.pContext = nullptr;
	FAudioSourceVoice_SubmitSourceBuffer(voice->sourceVoice, &audioBuffer, nullptr);
}

void MGA_Voice_Play(MGA_Voice* voice, mgbyte looped)
{
	assert(voice != nullptr);

	FAudioSourceVoice_Start(voice->sourceVoice, 0, FAUDIO_COMMIT_NOW);
	voice->state = MGSoundState::Playing;
}

void MGA_Voice_Pause(MGA_Voice* voice)
{
	assert(voice != nullptr);

	FAudioSourceVoice_Stop(voice->sourceVoice, 0, FAUDIO_COMMIT_NOW);
	voice->state = MGSoundState::Paused;
}

void MGA_Voice_Resume(MGA_Voice* voice)
{
	assert(voice != nullptr);

	FAudioSourceVoice_Start(voice->sourceVoice, 0, FAUDIO_COMMIT_NOW);
	voice->state = MGSoundState::Playing;
}

void MGA_Voice_Stop(MGA_Voice* voice, mgbyte immediate)
{
	assert(voice != nullptr);

	FAudioSourceVoice_Stop(voice->sourceVoice, immediate ? FAUDIO_PLAY_TAILS : 0, FAUDIO_COMMIT_NOW);
	FAudioSourceVoice_FlushSourceBuffers(voice->sourceVoice);
	voice->state = MGSoundState::Stopped;
}

MGSoundState MGA_Voice_GetState(MGA_Voice* voice)
{
	assert(voice != nullptr);
	return voice->state;
}

mgulong MGA_Voice_GetPosition(MGA_Voice* voice)
{
	assert(voice != nullptr);

	FAudioVoiceState state;
	FAudioSourceVoice_GetState(voice->sourceVoice, &state, 0);
	return state.SamplesPlayed;
}

void MGA_Voice_SetPan(MGA_Voice* voice, mgfloat pan)
{
	assert(voice != nullptr);
	// !TODO: Implement output matrix update
}

void MGA_Voice_SetPitch(MGA_Voice* voice, mgfloat pitch)
{
	assert(voice != nullptr);

	voice->pitch = pitch;
	FAudioSourceVoice_SetFrequencyRatio(voice->sourceVoice, powf(2.0f, pitch), FAUDIO_COMMIT_NOW);
}

void MGA_Voice_SetVolume(MGA_Voice* voice, mgfloat volume)
{
	assert(voice != nullptr);

	voice->volume = volume;
	FAudioVoice_SetVolume(voice->sourceVoice, volume, FAUDIO_COMMIT_NOW);
}

void MGA_Voice_SetReverbMix(MGA_Voice* voice, mgfloat mix)
{
	assert(voice != nullptr);

	if (mix < 0)
		voice->reverbMix = 0.0f;
	else if (mix > 2.0f)
		voice->reverbMix = 2.0f;
	else
		voice->reverbMix = mix;
	if (voice->reverbMix > 0.0f)
	{
		FAudioSendDescriptor desc[2];
		desc[0].pOutputVoice = voice->system->reverbVoice;
		desc[0].Flags = 0;
		desc[1].pOutputVoice = voice->system->masteringVoice;
		desc[1].Flags = 0;
		FAudioVoiceSends sends;
		sends.SendCount = 2;
		sends.pSends = desc;
		FAudioVoice_SetOutputVoices(voice->sourceVoice, &sends);
		// Update output matrix for both sends
		FAudioVoiceDetails details;
		FAudioVoice_GetVoiceDetails(voice->sourceVoice, &details);
		int srcChannels = details.InputChannels;
		FAudioVoice_GetVoiceDetails(voice->system->reverbVoice, &details);
		int reverbChannels = details.InputChannels;
		FAudioVoice_GetVoiceDetails(voice->system->masteringVoice, &details);
		int masterChannels = details.InputChannels;
		// Set reverb send matrix
		float reverbMatrix[16] = { 0 };
		for (int i = 0; i < srcChannels * reverbChannels; i++)
		{
			reverbMatrix[i] = voice->reverbMix;
		}
		FAudioVoice_SetOutputMatrix(
			voice->sourceVoice,
			voice->system->reverbVoice,
			srcChannels,
			reverbChannels,
			reverbMatrix,
			FAUDIO_COMMIT_NOW
		);
		// Set master send matrix
		float masterMatrix[16] = { 0 };
		for (int i = 0; i < srcChannels* masterChannels; i++)
		{
			masterMatrix[i] = 1.0f - (voice->reverbMix > 1.0f ? 1.0f : voice->reverbMix);
		}
		FAudioVoice_SetOutputMatrix(
			voice->sourceVoice,
			voice->system->masteringVoice,
			srcChannels,
			masterChannels,
			masterMatrix,
			FAUDIO_COMMIT_NOW
		);
	}
	else
	{
		FAudioSendDescriptor desc[1];
		desc[0].pOutputVoice = voice->system->masteringVoice;
		desc[0].Flags = 0;
		FAudioVoiceSends sends;
		sends.SendCount = 1;
		sends.pSends = desc;
		FAudioVoice_SetOutputVoices(voice->sourceVoice, &sends);
		// Reset output matrix
		FAudioVoiceDetails details;
		FAudioVoice_GetVoiceDetails(voice->sourceVoice, &details);
		int srcChannels = details.InputChannels;
		FAudioVoice_GetVoiceDetails(voice->system->masteringVoice, &details);
		int masterChannels = details.InputChannels;
		float masterMatrix[16] = { 0 };
		for (int i = 0; i < srcChannels * masterChannels; i++)
		{
			masterMatrix[i] = 1.0f;
		}
		FAudioVoice_SetOutputMatrix(
			voice->sourceVoice,
			voice->system->masteringVoice,
			srcChannels,
			masterChannels,
			masterMatrix,
			FAUDIO_COMMIT_NOW
		);
	}
}

void MGA_Voice_SetFilterMode(MGA_Voice* voice, MGFilterMode mode, mgfloat filterQ, mgfloat frequency)
{
	assert(voice != nullptr);

	FAudioVoiceDetails details;
	FAudioVoice_GetVoiceDetails(voice->sourceVoice, &details);
	if (filterQ > 0.0f)
	{
		filterQ = 1.0f / filterQ;
		if (filterQ > FAUDIO_MAX_FILTER_ONEOVERQ)
			filterQ = FAUDIO_MAX_FILTER_ONEOVERQ;
	}
	else
	{
		filterQ = 1.0f;
	}
	FAudioFilterParameters params;
	params.Type = (FAudioFilterType)mode;
	params.Frequency = frequency;
	params.OneOverQ = filterQ;
	FAudioVoice_SetFilterParameters(voice->sourceVoice, &params, FAUDIO_COMMIT_NOW);
}

void MGA_Voice_ClearFilterMode(MGA_Voice* voice)
{
	assert(voice != nullptr);

	FAudioFilterParameters params;
	params.Type = FAudioLowPassFilter;
	params.Frequency = FAUDIO_MAX_FILTER_FREQUENCY;
	params.OneOverQ = 1.0f;
	FAudioVoice_SetFilterParameters(voice->sourceVoice, &params, FAUDIO_COMMIT_NOW);
}

void MGA_Voice_Apply3D(MGA_Voice* voice, Listener& listener, Emitter& emitter, mgfloat distanceScale)
{
	assert(voice != nullptr);

	F3DAUDIO_LISTENER f3dListener;
	f3dListener.OrientFront.x = listener.Forward.X;
	f3dListener.OrientFront.y = listener.Forward.Y;
	f3dListener.OrientFront.z = listener.Forward.Z;
	f3dListener.OrientTop.x = listener.Up.X;
	f3dListener.OrientTop.y = listener.Up.Y;
	f3dListener.OrientTop.z = listener.Up.Z;
	f3dListener.Position.x = listener.Position.X;
	f3dListener.Position.y = listener.Position.Y;
	f3dListener.Position.z = listener.Position.Z;
	f3dListener.Velocity.x = listener.Velocity.X;
	f3dListener.Velocity.y = listener.Velocity.Y;
	f3dListener.Velocity.z = listener.Velocity.Z;
	f3dListener.pCone = nullptr;
	FAudioVoiceDetails details;
	FAudioVoice_GetVoiceDetails(voice->sourceVoice, &details);
	int srcChannelCount = details.InputChannels;

	FAudioVoice_GetVoiceDetails(voice->system->masteringVoice, &details);
	int dstChannelCount = details.InputChannels;

	static float azimuths[4] = { 0, 0, 0, 0 };
	F3DAUDIO_EMITTER f3dEmitter;
	memset(&f3dEmitter, 0, sizeof(f3dEmitter));
	f3dEmitter.OrientFront.x = emitter.Forward.X;
	f3dEmitter.OrientFront.y = emitter.Forward.Y;
	f3dEmitter.OrientFront.z = emitter.Forward.Z;
	f3dEmitter.OrientTop.x = emitter.Up.X;
	f3dEmitter.OrientTop.y = emitter.Up.Y;
	f3dEmitter.OrientTop.z = emitter.Up.Z;
	f3dEmitter.Position.x = emitter.Position.X;
	f3dEmitter.Position.y = emitter.Position.Y;
	f3dEmitter.Position.z = emitter.Position.Z;
	f3dEmitter.Velocity.x = emitter.Velocity.X;
	f3dEmitter.Velocity.y = emitter.Velocity.Y;
	f3dEmitter.Velocity.z = emitter.Velocity.Z;
	f3dEmitter.DopplerScaler = emitter.DopplerScale;
	f3dEmitter.ChannelCount = srcChannelCount;
	f3dEmitter.pChannelAzimuths = azimuths;
	f3dEmitter.CurveDistanceScaler = 1.0f;
	static float DspMatrix[FAUDIO_MAX_AUDIO_CHANNELS * 8];
	F3DAUDIO_DSP_SETTINGS dsp;
	memset(&dsp, 0, sizeof(dsp));
	dsp.pMatrixCoefficients = DspMatrix;
	dsp.SrcChannelCount = srcChannelCount;
	dsp.DstChannelCount = dstChannelCount;

	uint32_t flags = F3DAUDIO_CALCULATE_MATRIX | F3DAUDIO_CALCULATE_DOPPLER;
	F3DAudioCalculate(voice->system->f3daudio, &f3dListener, &f3dEmitter, flags, &dsp);
	FAudioVoice_SetOutputMatrix(
		voice->sourceVoice,
		voice->system->masteringVoice,
		srcChannelCount,
		dstChannelCount,
		dsp.pMatrixCoefficients,
		FAUDIO_COMMIT_NOW
	);
	FAudioSourceVoice_SetFrequencyRatio(voice->sourceVoice, dsp.DopplerFactor, FAUDIO_COMMIT_NOW);
}

