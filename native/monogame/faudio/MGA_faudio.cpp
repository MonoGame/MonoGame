// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGA.h"

#include "mg_common.h"

#include <vector>
#include <atomic>
#include <mutex>

#define _USE_MATH_DEFINES
#include <math.h>

#include "FAudio.h"
#include "FAPO.h"
#include "FAudioFX.h"
#include "F3DAudio.h"


struct MGA_VoiceCallbacks;

struct MGA_RawBuffer
{
	MGA_Voice* voice = nullptr;
	uint8_t* data = nullptr;
	uint32_t length = 0;
};

struct MGA_System
{
	FAudio* faudio = nullptr;
	FAudioMasteringVoice* masteringVoice = nullptr;
	FAudioSubmixVoice* reverbVoice = nullptr;
	FAPO* reverbEffect = nullptr;
	FAudioEffectDescriptor reverbEffectDesc;
	FAudioEffectChain reverbEffectChain;
	F3DAUDIO_HANDLE f3daudio;
	MGA_VoiceCallbacks* callbacks = nullptr;

	std::mutex lock;
	std::vector<MGA_RawBuffer*> freeRawBuffers;
};

struct MGA_Buffer
{
	FAudioWaveFormatEx* format = nullptr;
	FAudioBuffer buffer = {};
	//XAUDIO2_BUFFER_WMA* wmaBuffer = nullptr;
	uint8_t* data = nullptr;
	uint32_t length = 0;
	mgulong duration = 0;
};

struct MGA_Voice
{
	MGA_System* system = nullptr;

	FAudioSourceVoice* voice = nullptr;
	MGSoundState state = MGSoundState::Stopped;
	MGA_Buffer* buffer = nullptr;
	FAudioWaveFormatEx format;

	std::atomic<int> finishedBuffers = 0;

	float pan = 0.0f;
	float reverbMix = 0.0f;
	bool looped = false;
};

struct MGA_VoiceCallbacks : FAudioVoiceCallback
{
private:

	MGA_System* _system = nullptr;

public:

	MGA_VoiceCallbacks(MGA_System* system_) : _system(system_)
	{
		OnBufferEnd = HandleOnBufferEnd;
		OnBufferStart = nullptr;
		OnLoopEnd = nullptr;
		OnStreamEnd = nullptr;
		OnVoiceError = nullptr;
		OnVoiceProcessingPassEnd = nullptr;
		OnVoiceProcessingPassStart = nullptr;
	}

	static void HandleOnBufferEnd(FAudioVoiceCallback* callback, void* pBufferContext)
	{
		MGA_RawBuffer* raw = (MGA_RawBuffer*)pBufferContext;
		if (raw == nullptr)
			return;

		auto callbacks = (MGA_VoiceCallbacks*)callback;

		++raw->voice->finishedBuffers;

		std::lock_guard guard(callbacks->_system->lock);
		callbacks->_system->freeRawBuffers.push_back(raw);
	}
};


MGA_System* MGA_System_Create()
{
	auto system = new MGA_System();

	uint32_t result = FAudioCreate(&system->faudio, 0, FAUDIO_DEFAULT_PROCESSOR);
	if (result != 0)
	{
		delete system;
		return nullptr;
	}

	/*
#if _DEBUG
	// TODO: Work this out so we can get error messages in debug builds.	
	FAudioDebugConfiguration debug;
	memset(&debug, 0, sizeof(debug));
	debug.TraceMask = FAUDIO_LOG_ERRORS | FAUDIO_LOG_WARNINGS | FAUDIO_LOG_DETAIL;
	FAudio_SetDebugConfiguration(system->faudio, &debug, nullptr);
#endif
	*/

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

	result = FAudioCreateReverb(&system->reverbEffect, 0);
	if (result != 0)
	{
		FAudioVoice_DestroyVoice(system->masteringVoice);
		FAudio_Release(system->faudio);
		delete system;
		return nullptr;
	}

	FAudioVoiceDetails details;
	memset(&details, 0, sizeof(details));
	FAudioVoice_GetVoiceDetails(system->masteringVoice, &details);

	system->reverbEffectDesc.InitialState = true;
	system->reverbEffectDesc.OutputChannels = details.InputChannels;
	system->reverbEffectDesc.pEffect = system->reverbEffect;

	system->reverbEffectChain.EffectCount = 1;
	system->reverbEffectChain.pEffectDescriptors = &system->reverbEffectDesc;

	result = FAudio_CreateSubmixVoice(
		system->faudio,
		&system->reverbVoice,
		details.InputChannels,
		details.InputSampleRate,
		0,
		0,
		nullptr,
		&system->reverbEffectChain
	);
	if (result != 0)
	{
		FAudioVoice_DestroyVoice(system->masteringVoice);
		FAudio_Release(system->faudio);
		delete system;
		return nullptr;
	}

	// Match the same setting on XAudio.
	const float X3DAUDIO_SPEED_OF_SOUND = 343.5f;
	F3DAudioInitialize(SPEAKER_STEREO, X3DAUDIO_SPEED_OF_SOUND, system->f3daudio);

	// Used to track streaming buffers.
	system->callbacks = new MGA_VoiceCallbacks(system);

	return system;
}

void MGA_System_Destroy(MGA_System* system)
{
	assert(system != nullptr);

	// TODO: Should we be stopping any playing sounds here first?


	// Destroy system resources.
	for (auto raw : system->freeRawBuffers)
	{
		free(raw->data);
		delete raw;
	}
	if (system->reverbVoice)
		FAudioVoice_DestroyVoice(system->reverbVoice);
	if (system->reverbEffect)
		system->reverbEffect->Release(system->reverbEffect);
	if (system->masteringVoice)
		FAudioVoice_DestroyVoice(system->masteringVoice);
	if (system->faudio)
		FAudio_Release(system->faudio);
	delete system->callbacks;

	delete system;
}

mgint MGA_System_GetMaxInstances()
{
	// If your game needs more than 256 sounds it likely
	// a bug in your code or a bad design.  You cannot
	// hear more than a dozen sounds or more at once.
	// It also won't scale to other platforms like consoles.
	return 256;
}

void MGA_System_SetReverbSettings(MGA_System* system, ReverbSettings& settings)
{
	assert(system != nullptr);

	FAudioVoiceDetails details;
	FAudioVoice_GetVoiceDetails(system->reverbVoice, &details);

	// All parameters related to sampling rate or time are relative to a 48kHz 
	// voice and must be scaled for use with other sampling rates.
	float timeScale = 48000.0f / details.InputSampleRate;

	FAudioFXReverbParameters params;
	params.ReflectionsGain = settings.ReflectionsGainDb;
	params.ReverbGain = settings.ReverbGainDb;
	params.DecayTime = settings.DecayTimeSec;
	params.ReflectionsDelay = (uint32_t)(settings.ReflectionsDelayMs * timeScale);
	params.ReverbDelay = (uint8_t)(settings.ReverbDelayMs * timeScale);
	params.RearDelay = (uint8_t)(settings.RearDelayMs * timeScale);
	params.RoomSize = settings.RoomSizeFeet;
	params.Density = settings.DensityPct;
	params.LowEQGain = (uint8_t)settings.LowEqGain;
	params.LowEQCutoff = (uint8_t)settings.LowEqCutoff;
	params.HighEQGain = (uint8_t)settings.HighEqGain;
	params.HighEQCutoff = (uint8_t)settings.HighEqCutoff;
	params.PositionLeft = (uint8_t)settings.PositionLeft;
	params.PositionRight = (uint8_t)settings.PositionRight;
	params.PositionMatrixLeft = (uint8_t)settings.PositionLeftMatrix;
	params.PositionMatrixRight = (uint8_t)settings.PositionRightMatrix;
	params.EarlyDiffusion = (uint8_t)settings.EarlyDiffusion;
	params.LateDiffusion = (uint8_t)settings.LateDiffusion;
	params.RoomFilterMain = settings.RoomFilterMainDb;
	params.RoomFilterFreq = settings.RoomFilterFrequencyHz * timeScale;
	params.RoomFilterHF = settings.RoomFilterHighFrequencyDb;
	params.WetDryMix = settings.WetDryMixPct;

	uint32_t result = FAudioVoice_SetEffectParameters(system->reverbVoice, 0, &params, sizeof(params), FAUDIO_COMMIT_NOW);
	assert(result == 0);
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
		free(buffer->data);
	if (buffer->format)
		free(buffer->format);

	delete buffer;
}

void MGA_Buffer_InitializeFormat(MGA_Buffer* buffer, mgbyte* waveHeader, mgbyte* waveData, mgint length, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveHeader != nullptr);
	assert(waveData != nullptr);
	assert(length > 0);

	FAudioWaveFormatEx wformat = *(FAudioWaveFormatEx*)waveHeader;

	if (wformat.wFormatTag == FAUDIO_FORMAT_PCM)
	{
		MGA_Buffer_InitializePCM(
			buffer,
			waveData,
			0,
			length,
			wformat.wBitsPerSample,
			wformat.nSamplesPerSec,
			wformat.nChannels,
			loopStart,
			loopLength);

		return;
	}

	if (wformat.wFormatTag == FAUDIO_FORMAT_IEEE_FLOAT)
	{
		buffer->format = (FAudioWaveFormatEx*)malloc(sizeof(FAudioWaveFormatEx));
		memset(buffer->format, 0, sizeof(FAudioWaveFormatEx));
		buffer->format->wFormatTag = FAUDIO_FORMAT_IEEE_FLOAT;
		buffer->format->nSamplesPerSec = wformat.nSamplesPerSec;
		buffer->format->nChannels = wformat.nChannels;
		buffer->format->nBlockAlign = wformat.nBlockAlign;
		buffer->format->wBitsPerSample = wformat.wBitsPerSample;
		buffer->format->nAvgBytesPerSec = buffer->format->nSamplesPerSec * buffer->format->nBlockAlign;
		buffer->format->cbSize = 0;

		// Buffer should be block aligned.
		assert((length % wformat.nBlockAlign) == 0);

		// Calculate duration
		buffer->duration = (mgulong)((length * 1000) / buffer->format->nAvgBytesPerSec);

		buffer->length = length;
		buffer->data = (uint8_t*)malloc(length);
		memcpy(buffer->data, waveData, length);

		memset(&buffer->buffer, 0, sizeof(buffer->buffer));
		buffer->buffer.pAudioData = buffer->data;
		buffer->buffer.AudioBytes = length;
		buffer->buffer.LoopBegin = loopStart;
		buffer->buffer.LoopLength = loopLength;
		buffer->buffer.LoopCount = 0;
		buffer->buffer.Flags = 0;
		buffer->buffer.pContext = nullptr;

		return;
	}

	if (wformat.wFormatTag == FAUDIO_FORMAT_MSADPCM)
	{
		const size_t size = sizeof(FAudioADPCMWaveFormat) + (7 * sizeof(FAudioADPCMCoefSet));
		FAudioADPCMWaveFormat* format = (FAudioADPCMWaveFormat*)malloc(size);
		memset(format, 0, sizeof(FAudioADPCMWaveFormat));
		format->wfx.wFormatTag = FAUDIO_FORMAT_MSADPCM;
		format->wfx.nSamplesPerSec = wformat.nSamplesPerSec;
		format->wfx.nChannels = wformat.nChannels;
		format->wfx.nBlockAlign = wformat.nBlockAlign;
		format->wfx.wBitsPerSample = wformat.wBitsPerSample;
		format->wfx.nAvgBytesPerSec = wformat.nAvgBytesPerSec;
		format->wfx.cbSize = size - sizeof(FAudioWaveFormatEx);
		format->wSamplesPerBlock = (format->wfx.nBlockAlign / format->wfx.nChannels - 7) * 2 + 2;
		format->wNumCoef = 7;
		format->aCoef[0] = { 256, 0 };
		format->aCoef[1] = { 512, -256 };
		format->aCoef[2] = { 0, 0 };
		format->aCoef[3] = { 192, 64 };
		format->aCoef[4] = { 240, 0 };
		format->aCoef[5] = { 460, -208 };
		format->aCoef[6] = { 392, -232 };

		mgulong totalBlocks = length / wformat.nBlockAlign;
		mgulong totalSamples = totalBlocks * format->wSamplesPerBlock;
		buffer->duration = (mgulong)((totalSamples * 1000) / wformat.nSamplesPerSec);

		// NOTE: XAudio only supports up to 512 as the samples per block.
		// Larger values are not supported and the sound will be wrong.
		if (format->wSamplesPerBlock > 512)
			format->wSamplesPerBlock = 512;

		buffer->format = (FAudioWaveFormatEx*)format;

		// Buffer should be block aligned.
		assert((length % wformat.nBlockAlign) == 0);

		buffer->length = length;
		buffer->data = (uint8_t*)malloc(length);
		memcpy(buffer->data, waveData, length);

		memset(&buffer->buffer, 0, sizeof(buffer->buffer));
		buffer->buffer.pAudioData = buffer->data;
		buffer->buffer.AudioBytes = length;
		buffer->buffer.LoopBegin = loopStart;
		buffer->buffer.LoopLength = loopLength;
		buffer->buffer.LoopCount = 0;
		buffer->buffer.Flags = 0;
		buffer->buffer.pContext = nullptr;

		return;
	}


	if (wformat.wFormatTag == FAUDIO_FORMAT_WMAUDIO2)
	{
		// TODO: The API here needs to change to pass
		// additional data for this format to work.
	}
	
	// TODO: This API doesn't have a way to indicate that the format
	// provided was not supported and this buffer is uninitialized.
	throw 0;
}

void MGA_Buffer_InitializePCM(MGA_Buffer* buffer, mgbyte* waveData, mgint offset, mgint length, mgint sampleBits, mgint sampleRate, mgint channels, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(offset >=0);
	assert(length > 0);

	buffer->format = (FAudioWaveFormatEx*)malloc(sizeof(FAudioWaveFormatEx));
	memset(buffer->format, 0, sizeof(FAudioWaveFormatEx));
	buffer->format->wFormatTag = FAUDIO_FORMAT_PCM;
	buffer->format->nSamplesPerSec = sampleRate;
	buffer->format->nChannels = channels;
	buffer->format->nBlockAlign = channels * (sampleBits / 8);
	buffer->format->wBitsPerSample = sampleBits;
	buffer->format->nAvgBytesPerSec = buffer->format->nSamplesPerSec * buffer->format->nBlockAlign;
	buffer->format->cbSize = 0;

	// Buffer should be block aligned.
	assert((length % buffer->format->nBlockAlign) == 0);

	buffer->length = length;
	buffer->data = (uint8_t*)malloc(length);

	// When we are pre-creating a buffer for streaming we don't
	// have audio data to copy yet... so skip the copy.
	if (waveData)
		memcpy(buffer->data, waveData + offset, length);

	// Calculate duration
	buffer->duration = (mgulong)((length * 1000) / buffer->format->nAvgBytesPerSec);

	// Set the buffer structure passed to SubmitSourceBuffer.
	memset(&buffer->buffer, 0, sizeof(buffer->buffer));
	buffer->buffer.pAudioData = buffer->data;
	buffer->buffer.AudioBytes = length;
	buffer->buffer.LoopBegin = loopStart;
	buffer->buffer.LoopLength = loopLength;
	buffer->buffer.LoopCount = 0;
	buffer->buffer.Flags = 0;
	buffer->buffer.pContext = nullptr;
}

void MGA_Buffer_InitializeXact(MGA_Buffer* buffer, mguint codec, mgbyte* waveData, mgint length, mgint sampleRate, mgint blockAlignment, mgint channels, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveData != nullptr);
	assert(length > 0);

	// Initialize XACT format
	buffer->format = (FAudioWaveFormatEx*)malloc(sizeof(FAudioWaveFormatEx));
	buffer->format->wFormatTag = codec;
	buffer->format->nChannels = channels;
	buffer->format->nSamplesPerSec = sampleRate;
	buffer->format->nAvgBytesPerSec = sampleRate * blockAlignment;
	buffer->format->nBlockAlign = blockAlignment;
	buffer->format->wBitsPerSample = 0; // Variable for compressed formats
	buffer->format->cbSize = 0;

    buffer->length = length;
	buffer->data = (uint8_t*)malloc(length);
	memcpy(buffer->data, waveData, length);

	// Calculate duration (approximate for compressed formats)
	if (codec == 0x166) // WMA
	{
		buffer->duration = (mgulong)((length * 1000) / (sampleRate * channels * 2));
	}
	else
	{
		buffer->duration = (mgulong)((length * 1000) / buffer->format->nAvgBytesPerSec);
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

	if (sampleRate > 0 && channels > 0)
	{
		// Prepare the format for all streaming buffers passed to this voice.
		FAudioWaveFormatEx& format = voice->format;
		format.wFormatTag = FAUDIO_FORMAT_PCM;
		format.nChannels = channels;
		format.nSamplesPerSec = sampleRate;
		format.nAvgBytesPerSec = sampleRate * 2 * channels;
		format.nBlockAlign = 2 * channels;
		format.wBitsPerSample = 16;
		format.cbSize = 0;

		uint32_t result = FAudio_CreateSourceVoice(
			voice->system->faudio,
			&voice->voice,
			&format,
			FAUDIO_VOICE_USEFILTER,
			FAUDIO_DEFAULT_FREQ_RATIO,
			system->callbacks,
			nullptr,
			nullptr
		);

		assert(result == 0);
	}

	return voice;
}

void MGA_Voice_Destroy(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice)
		FAudioVoice_DestroyVoice(voice->voice);

	delete voice;
}

mgint MGA_Voice_GetBufferCount(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return 0;

	FAudioVoiceState state;
	FAudioSourceVoice_GetState(voice->voice, &state, FAUDIO_VOICE_NOSAMPLESPLAYED);
	return state.BuffersQueued;
}

mgint MGA_Voice_GetFinishedBufferCount(MGA_Voice* voice)
{
	assert(voice != nullptr);
	return voice->finishedBuffers.exchange(0);
}

void MGA_Voice_SetBuffer(MGA_Voice* voice, MGA_Buffer* buffer)
{
	assert(voice != nullptr);

	// If the voice has an existing source voice but the new buffer format doesn't match:
	// We should destroy and recreate the source voice to be safe.
	if (voice->voice
		&& buffer
		&& voice->buffer
		&& voice->buffer->format
		&& buffer->format
		&& (voice->buffer->format->wFormatTag != buffer->format->wFormatTag
			|| voice->buffer->format->nChannels != buffer->format->nChannels
			|| voice->buffer->format->nSamplesPerSec != buffer->format->nSamplesPerSec
			|| voice->buffer->format->wBitsPerSample != buffer->format->wBitsPerSample))
	{
		FAudioVoice_DestroyVoice(voice->voice);
		voice->voice = nullptr;
	}

	// Stop and remove any pending buffers first.
	if (voice->voice)
	{
		FAudioSourceVoice_Stop(voice->voice, 0, FAUDIO_COMMIT_NOW);
		FAudioSourceVoice_FlushSourceBuffers(voice->voice);
	}
	else if (buffer)
	{
		uint32_t result = FAudio_CreateSourceVoice(
			voice->system->faudio,
			&voice->voice,
			buffer->format,
			FAUDIO_VOICE_USEFILTER,
			FAUDIO_DEFAULT_FREQ_RATIO,
			nullptr,
			nullptr,
			nullptr
		);
		
		assert(result == 0);
	}

	voice->buffer = buffer;
}

void MGA_Voice_AppendBuffer(MGA_Voice* voice, mgbyte* buffer, mguint size)
{
	assert(voice != nullptr);
	assert(buffer != nullptr);

	if (voice->voice == nullptr)
		return;

	// Find a free buffer.
	MGA_RawBuffer* raw = nullptr;
	{
		std::lock_guard guard(voice->system->lock);

		auto& freeRawBuffers = voice->system->freeRawBuffers;		
		for (int i = 0; i < freeRawBuffers.size(); i++)
		{
			auto r = freeRawBuffers[i];
			if (r->length < size)
				continue;

			raw = r;
			freeRawBuffers.erase(freeRawBuffers.begin() + i);
		}
	}

	if (raw == nullptr)
	{
		auto& format = voice->format;

		raw = new MGA_RawBuffer;
		raw->data = (uint8_t*)malloc(size);
		raw->length = size;
	}

	raw->voice = voice;

	assert(raw->length >= size);
	memcpy(raw->data, buffer, size);

	// Copy the buffer structure and fix the looping state.
	FAudioBuffer vbuffer;
	memset(&vbuffer, 0, sizeof(vbuffer));
	vbuffer.pAudioData = raw->data;
	vbuffer.AudioBytes = size;
	vbuffer.pContext = raw;

	FAudioSourceVoice_SubmitSourceBuffer(voice->voice, &vbuffer, nullptr);
}

void MGA_Voice_Play(MGA_Voice* voice, mgbyte looped)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	if (voice->buffer != nullptr)
	{
		FAudioSourceVoice_Stop(voice->voice, 0, FAUDIO_COMMIT_NOW);
		FAudioSourceVoice_FlushSourceBuffers(voice->voice);

		voice->looped = looped;

		auto buffer = voice->buffer->buffer;
		if (looped)
			buffer.LoopCount = FAUDIO_LOOP_INFINITE;
		else
			buffer.LoopBegin = buffer.LoopLength = buffer.LoopCount = 0;

		FAudioSourceVoice_SubmitSourceBuffer(voice->voice, &buffer, nullptr);
	}

	voice->finishedBuffers = 0;
	voice->state = MGSoundState::Playing;
	FAudioSourceVoice_Start(voice->voice, 0, FAUDIO_COMMIT_NOW);
}

void MGA_Voice_Pause(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;
		
	if (voice->state == MGSoundState::Paused)
		return;

	FAudioVoiceState state;
	FAudioSourceVoice_GetState(voice->voice, &state, FAUDIO_VOICE_NOSAMPLESPLAYED);
	if (state.BuffersQueued == 0)
		return;

	FAudioSourceVoice_Stop(voice->voice, 0, FAUDIO_COMMIT_NOW);
	voice->state = MGSoundState::Paused;
}

void MGA_Voice_Resume(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->state != MGSoundState::Paused)
	{
		MGA_Voice_Play(voice, voice->looped);
		return;
	}
	
	FAudioSourceVoice_Start(voice->voice, 0, FAUDIO_COMMIT_NOW);
	voice->state = MGSoundState::Playing;
}

void MGA_Voice_Stop(MGA_Voice* voice, mgbyte immediate)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;
		
	FAudioSourceVoice_Stop(voice->voice, immediate ? FAUDIO_PLAY_TAILS : 0, FAUDIO_COMMIT_NOW);
	FAudioSourceVoice_FlushSourceBuffers(voice->voice);
	voice->state = MGSoundState::Stopped;
	voice->finishedBuffers = 0;
}

MGSoundState MGA_Voice_GetState(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return MGSoundState::Stopped;
				
	if (voice->state == MGSoundState::Paused)
		return MGSoundState::Paused;

	FAudioVoiceState state;
	FAudioSourceVoice_GetState(voice->voice, &state, FAUDIO_VOICE_NOSAMPLESPLAYED);
	if (state.BuffersQueued == 0)
		return MGSoundState::Stopped;

	return MGSoundState::Playing;
}

mgulong MGA_Voice_GetPosition(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return 0;
				
	FAudioVoiceState state;
	FAudioSourceVoice_GetState(voice->voice, &state, 0);

	float msec = (state.SamplesPlayed / (float)voice->format.nSamplesPerSec) * 1000.0f;
	return (mgulong)msec;
}

static void MGA_Voice_UpdateOutputMatrix(MGA_Voice* voice)
{
	FAudioVoiceDetails details;
	FAudioVoice_GetVoiceDetails(voice->voice, &details);
	int srcChannelCount = details.InputChannels;
	FAudioVoice_GetVoiceDetails(voice->system->masteringVoice, &details);
	int dstChannelCount = details.InputChannels;

	// Default to zero volume on all channels.
	float panMatrix[16];
	memset(panMatrix, 0, sizeof(panMatrix));

	// Set the pan on the correct channels based on the reverb mix.
	if (!(voice->reverbMix > 0.0f))
		FAudioVoice_SetOutputMatrix(voice->voice, nullptr, srcChannelCount, dstChannelCount,
			MGA_Voice_CalculatePanMatrix(voice->pan, 1.0f, panMatrix, srcChannelCount), FAUDIO_COMMIT_NOW);
	else
	{
		FAudioVoice_SetOutputMatrix(voice->voice, voice->system->reverbVoice, srcChannelCount, dstChannelCount,
			MGA_Voice_CalculatePanMatrix(voice->pan, voice->reverbMix, panMatrix, srcChannelCount), FAUDIO_COMMIT_NOW);
		FAudioVoice_SetOutputMatrix(voice->voice, voice->system->masteringVoice, srcChannelCount, dstChannelCount,
			MGA_Voice_CalculatePanMatrix(voice->pan, 1.0f - (voice->reverbMix > 1.0f ? 1.0f : voice->reverbMix), panMatrix, srcChannelCount), FAUDIO_COMMIT_NOW);
	}
}

void MGA_Voice_SetPan(MGA_Voice* voice, mgfloat pan)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	voice->pan = pan;
	MGA_Voice_UpdateOutputMatrix(voice);
}

void MGA_Voice_SetPitch(MGA_Voice* voice, mgfloat pitch)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	float ratio = powf(2.0f, pitch);
	FAudioSourceVoice_SetFrequencyRatio(voice->voice, ratio, FAUDIO_COMMIT_NOW);
}

void MGA_Voice_SetVolume(MGA_Voice* voice, mgfloat volume)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	FAudioVoice_SetVolume(voice->voice, volume, FAUDIO_COMMIT_NOW);
}

void MGA_Voice_SetReverbMix(MGA_Voice* voice, mgfloat mix)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

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
		FAudioVoice_SetOutputVoices(voice->voice, &sends);
	}
	else
	{
		FAudioSendDescriptor desc[1];
		desc[0].pOutputVoice = voice->system->masteringVoice;
		desc[0].Flags = 0;
		
		FAudioVoiceSends sends;
		sends.SendCount = 1;
		sends.pSends = desc;
		FAudioVoice_SetOutputVoices(voice->voice, &sends);
	}

	MGA_Voice_UpdateOutputMatrix(voice);
}

// Taken from XAudio2 implementation
inline float CutoffFrequencyToRadians(float CutoffFrequency, uint32_t SampleRate)
{
	if ((uint32_t)(CutoffFrequency * 6.0f) >= SampleRate)
	{
		return FAUDIO_MAX_FILTER_FREQUENCY;
	}
	return 2.0f * sinf((float)M_PI * CutoffFrequency / (float)SampleRate);
}

void MGA_Voice_SetFilterMode(MGA_Voice* voice, MGFilterMode mode, mgfloat filterQ, mgfloat frequency)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	FAudioVoiceDetails details;
	memset(&details, 0, sizeof(details));
	FAudioVoice_GetVoiceDetails(voice->voice, &details);

	if (filterQ > 0.0f)
	{
		filterQ = 1.0f / filterQ;
		if (filterQ > FAUDIO_MAX_FILTER_ONEOVERQ)
			filterQ = FAUDIO_MAX_FILTER_ONEOVERQ;
	}
	else
		filterQ = 1.0f;

	FAudioFilterParameters params;
	params.Type = (FAudioFilterType)mode;
	params.Frequency = CutoffFrequencyToRadians(frequency, details.InputSampleRate);
	params.OneOverQ = filterQ;
	FAudioVoice_SetFilterParameters(voice->voice, &params, FAUDIO_COMMIT_NOW);
}

void MGA_Voice_ClearFilterMode(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	FAudioFilterParameters params;
	params.Type = FAudioLowPassFilter;
	params.Frequency = FAUDIO_MAX_FILTER_FREQUENCY;
	params.OneOverQ = 1.0f;
	FAudioVoice_SetFilterParameters(voice->voice, &params, FAUDIO_COMMIT_NOW);
}

void MGA_Voice_Apply3D(MGA_Voice* voice, Listener& listener, Emitter& emitter, mgfloat distanceScale)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

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
	memset(&details, 0, sizeof(details));
	FAudioVoice_GetVoiceDetails(voice->voice, &details);
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
		voice->voice,
		voice->system->masteringVoice,
		srcChannelCount,
		dstChannelCount,
		dsp.pMatrixCoefficients,
		FAUDIO_COMMIT_NOW
	);
	FAudioSourceVoice_SetFrequencyRatio(voice->voice, dsp.DopplerFactor, FAUDIO_COMMIT_NOW);
}

