// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGA.h"

#include "mg_common.h"

#include <vector>
#include <atomic>
#include <mutex>

#define XAUDIO2_HELPER_FUNCTIONS
#include <xaudio2.h>
#include <xaudio2fx.h>
#include <x3daudio.h>

#pragma comment(lib, "xaudio2")


struct MGA_VoiceCallbacks;

struct MGA_RawBuffer
{
	MGA_Voice* voice = nullptr;
	uint8_t* data = nullptr;
	uint32_t length = 0;
};

struct MGA_System
{
	IXAudio2* audio = nullptr;
	IXAudio2MasteringVoice* masterVoice = nullptr;
	IXAudio2SubmixVoice* reverbVoice = nullptr;
	X3DAUDIO_HANDLE x3daudio;
	MGA_VoiceCallbacks* callbacks = nullptr;

	std::mutex lock;
	std::vector<MGA_RawBuffer*> freeRawBuffers;
};

struct MGA_Buffer
{
	WAVEFORMATEX* format = nullptr;
	XAUDIO2_BUFFER buffer;
	XAUDIO2_BUFFER_WMA* wmaBuffer = nullptr;
	uint8_t* data = nullptr;
	uint32_t length = 0;
	mgulong duration = 0;
};

struct MGA_Voice
{
	MGA_System* system = nullptr;

	IXAudio2SourceVoice* voice = nullptr;
	MGSoundState state = MGSoundState::Stopped;
	MGA_Buffer* buffer = nullptr;
	WAVEFORMATEX format;

	std::atomic<int> finishedBuffers = 0;

	float pan = 0.0f;
	float reverbMix = 0.0f;
	bool looped = false;
};

struct MGA_VoiceCallbacks : IXAudio2VoiceCallback
{
private:

	MGA_System* _system = nullptr;

public:

	MGA_VoiceCallbacks(MGA_System* system_) : _system(system_)
	{
	}

	void OnVoiceProcessingPassStart(UINT32) { }
	void OnVoiceProcessingPassEnd() { }
	void OnStreamEnd() { }
	void OnBufferStart(void*) { }
	void OnLoopEnd(void*) { }
	void OnVoiceError(void*, HRESULT) { }

	void OnBufferEnd(void* pBufferContext)
	{
		MGA_RawBuffer* raw = (MGA_RawBuffer*)pBufferContext;
		if (raw == nullptr)
			return;

		++raw->voice->finishedBuffers;

		std::lock_guard guard(_system->lock);
		_system->freeRawBuffers.push_back(raw);
	}
};


MGA_System* MGA_System_Create()
{
	auto system = new MGA_System();

	auto err = XAudio2Create(&system->audio, 0, XAUDIO2_DEFAULT_PROCESSOR);
	assert(err >= S_OK);

#ifdef _DEBUG
	// Enable debugging features
	XAUDIO2_DEBUG_CONFIGURATION debug = { 0 };
	debug.TraceMask = XAUDIO2_LOG_ERRORS | XAUDIO2_LOG_WARNINGS;
	debug.BreakMask = XAUDIO2_LOG_ERRORS;
	system->audio->SetDebugConfiguration(&debug, 0);
#endif

	err = system->audio->CreateMasteringVoice(&system->masterVoice);
	assert(err >= S_OK);

	XAUDIO2_VOICE_DETAILS details;
	memset(&details, 0, sizeof(details));
	system->masterVoice->GetVoiceDetails(&details);

	err = system->audio->CreateSubmixVoice(&system->reverbVoice, details.InputChannels, details.InputSampleRate);
	assert(err >= S_OK);

	XAUDIO2_EFFECT_DESCRIPTOR desc;
	desc.InitialState = true;
	desc.OutputChannels = details.InputChannels;
	err = XAudio2CreateReverb(&desc.pEffect);
	assert(err >= S_OK);

	XAUDIO2_EFFECT_CHAIN chain;
	chain.EffectCount = 1;
	chain.pEffectDescriptors = &desc;
	err = system->reverbVoice->SetEffectChain(&chain);
	assert(err >= S_OK);

	err = X3DAudioInitialize(SPEAKER_STEREO, X3DAUDIO_SPEED_OF_SOUND, *(X3DAUDIO_HANDLE*)system->x3daudio);
	assert(err >= S_OK);
	
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
		system->reverbVoice->DestroyVoice();		
	if (system->masterVoice)
		system->masterVoice->DestroyVoice();
	if (system->audio)
		system->audio->Release();

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

	XAUDIO2_VOICE_DETAILS details;
	memset(&details, 0, sizeof(details));
	system->reverbVoice->GetVoiceDetails(&details);

	// All parameters related to sampling rate or time are relative to a 48kHz 
	// voice and must be scaled for use with other sampling rates.
	float timeScale = 48000.0f / details.InputSampleRate;

	XAUDIO2FX_REVERB_PARAMETERS params;
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

	auto err = system->reverbVoice->SetEffectParameters(0, &params, sizeof(params));
	assert(err >= S_OK);
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
		
	if (buffer->wmaBuffer != nullptr)
	{
		free((void*)buffer->wmaBuffer->pDecodedPacketCumulativeBytes);
		free(buffer->wmaBuffer);
	}

	delete buffer;
}

void MGA_Buffer_InitializeFormat(MGA_Buffer* buffer, mgbyte* waveHeader, mgbyte* waveData, mgint length, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveHeader != nullptr);
	assert(waveData != nullptr);
	assert(length > 0);

	auto wformat = (WAVEFORMATEX*)waveHeader;

	if (wformat->wFormatTag == WAVE_FORMAT_PCM)
	{
		MGA_Buffer_InitializePCM(
			buffer,
			waveData,
			0,
			length,
			wformat->wBitsPerSample,
			wformat->nSamplesPerSec,
			wformat->nChannels,
			loopStart,
			loopLength);

		return;
	}

	if (wformat->wFormatTag == WAVE_FORMAT_IEEE_FLOAT)
	{
		buffer->format = (WAVEFORMATEX*)malloc(sizeof(WAVEFORMATEX));
		memset(buffer->format, 0, sizeof(WAVEFORMATEX));
		buffer->format->wFormatTag = WAVE_FORMAT_IEEE_FLOAT;
		buffer->format->nSamplesPerSec = wformat->nSamplesPerSec;
		buffer->format->nChannels = wformat->nChannels;
		buffer->format->nBlockAlign = wformat->nBlockAlign;
		buffer->format->wBitsPerSample = wformat->wBitsPerSample;
		buffer->format->nAvgBytesPerSec = buffer->format->nSamplesPerSec * buffer->format->nBlockAlign;
		buffer->format->cbSize = 0;

		// Buffer should be block aligned.
		assert((length % wformat->nBlockAlign) == 0);

		// Calculate duration
		buffer->duration = ((mgulong)length * 1000) / buffer->format->nAvgBytesPerSec;

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

	if (wformat->wFormatTag == WAVE_FORMAT_ADPCM)
	{
		// We are assuming MSADPCM here!
		const size_t size = sizeof(ADPCMWAVEFORMAT) + (7 * sizeof(ADPCMCOEFSET));
		auto format = (ADPCMWAVEFORMAT*)malloc(size);
		memset(format, 0, sizeof(ADPCMWAVEFORMAT));
		format->wfx.wFormatTag = WAVE_FORMAT_ADPCM;
		format->wfx.nSamplesPerSec = wformat->nSamplesPerSec;
		format->wfx.nChannels = wformat->nChannels;
		format->wfx.nBlockAlign = wformat->nBlockAlign;
		format->wfx.wBitsPerSample = wformat->wBitsPerSample;
		format->wfx.nAvgBytesPerSec = wformat->nAvgBytesPerSec;
		format->wfx.cbSize = size - sizeof(WAVEFORMATEX);
		format->wSamplesPerBlock = (format->wfx.nBlockAlign / format->wfx.nChannels - 7) * 2 + 2;
		format->wNumCoef = 7;
		format->aCoef[0] = { 256, 0 };
		format->aCoef[1] = { 512, -256 };
		format->aCoef[2] = { 0, 0 };
		format->aCoef[3] = { 192, 64 };
		format->aCoef[4] = { 240, 0 };
		format->aCoef[5] = { 460, -208 };
		format->aCoef[6] = { 392, -232 };

		mgulong totalBlocks = length / wformat->nBlockAlign;
		mgulong totalSamples = totalBlocks * format->wSamplesPerBlock;
		buffer->duration = (mgulong)((totalSamples * 1000) / wformat->nSamplesPerSec);

		// NOTE: XAudio only supports up to 512 as the samples per block.
		// Larger values are not supported and the sound will be wrong.
		if (format->wSamplesPerBlock > 512)
			format->wSamplesPerBlock = 512;

		buffer->format = (WAVEFORMATEX*)format;

		// Buffer should be block aligned.
		assert((length % wformat->nBlockAlign) == 0);

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


	if (wformat->wFormatTag == WAVE_FORMAT_WMAUDIO2)
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

	buffer->format = (WAVEFORMATEX*)malloc(sizeof(WAVEFORMATEX));
	memset(buffer->format, 0, sizeof(WAVEFORMATEX));
	buffer->format->wFormatTag = WAVE_FORMAT_PCM;
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
	buffer->duration = ((mgulong)length * 1000) / buffer->format->nAvgBytesPerSec;

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

	if (codec == 0x2) // Adpcm
	{
		// We are assuming MSADPCM here!

		const size_t size = sizeof(ADPCMWAVEFORMAT) + (7 * sizeof(ADPCMCOEFSET));
		auto format = (ADPCMWAVEFORMAT*)malloc(size);
		memset(format, 0, sizeof(ADPCMWAVEFORMAT));
		format->wfx.wFormatTag = WAVE_FORMAT_ADPCM;
		format->wfx.nSamplesPerSec = sampleRate;
		format->wfx.nChannels = channels;
		format->wfx.nBlockAlign = blockAlignment;
		format->wfx.wBitsPerSample = 4;
		format->wfx.nAvgBytesPerSec = (sampleRate * 4) / 8;
		format->wfx.cbSize = size - sizeof(WAVEFORMATEX);
		format->wSamplesPerBlock = (blockAlignment * 2) / (channels) - 12;
		format->wNumCoef = 7;
		format->aCoef[0] = { 256, 0 };
		format->aCoef[1] = { 512, -256 };
		format->aCoef[2] = { 0, 0 };
		format->aCoef[3] = { 192, 64 };
		format->aCoef[4] = { 240, 0 };
		format->aCoef[5] = { 460, -208 };
		format->aCoef[6] = { 392, -232 };

		// NOTE: XAudio only supports up to 512 as the samples per block.
		// Larger values are not supported and the sound will be wrong.
		if (format->wSamplesPerBlock > 512)
			format->wSamplesPerBlock = 512;

		// We must be one of these.
		assert(
			format->wSamplesPerBlock == 32 ||
			format->wSamplesPerBlock == 64 ||
			format->wSamplesPerBlock == 128 ||
			format->wSamplesPerBlock == 256 ||
			format->wSamplesPerBlock == 512
		);

		buffer->format = (WAVEFORMATEX*)format;

		// Buffer should be block aligned.
		//assert((length % blockAlignment) == 0);

		buffer->length = length;
		buffer->data = (uint8_t*)malloc(length);
		memcpy(buffer->data, waveData, length);

		memset(&buffer->buffer, 0, sizeof(XAUDIO2_BUFFER));
		buffer->buffer.pAudioData = buffer->data;
		buffer->buffer.AudioBytes = length;
		buffer->buffer.LoopBegin = loopStart;
		buffer->buffer.LoopLength = loopLength;
		buffer->buffer.LoopCount = 0;
		buffer->buffer.Flags = 0;
		buffer->buffer.pContext = nullptr;
	}
	else if (codec == 0x1) // Platform specific!
	{
#if defined(_GAMING_XBOX)

		// XSDPCM
		auto format = (ADPCMWAVEFORMAT*)malloc(50);
		memcpy(format, waveData, 50);
		buffer->format = (WAVEFORMATEX*)format;

		length -= 50;
		buffer->length = length;
		buffer->data = (uint8_t*)malloc(length);
		memcpy(buffer->data, waveData + 50, length);

		memset(&buffer->buffer, 0, sizeof(XAUDIO2_BUFFER));
		buffer->buffer.pAudioData = buffer->data;
		buffer->buffer.AudioBytes = length;
		buffer->buffer.LoopBegin = loopStart;
		buffer->buffer.LoopLength = loopLength;
		buffer->buffer.LoopCount = 0;
		buffer->buffer.Flags = 0;
		buffer->buffer.pContext = nullptr;

#else
		// Support XWMA on Windows?
#endif
	}
	else
	{
		// Others are not supported yet!
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
		auto& format = voice->format;
		format.wFormatTag = WAVE_FORMAT_PCM;
		format.nChannels = channels;
		format.nSamplesPerSec = sampleRate;
		format.nAvgBytesPerSec = sampleRate * 2 * channels;
		format.nBlockAlign = 2 * channels;
		format.wBitsPerSample = 16;
		format.cbSize = 0;

		auto result = voice->system->audio->CreateSourceVoice(
			&voice->voice, 
			&format, 
			XAUDIO2_VOICE_USEFILTER, 
			XAUDIO2_DEFAULT_FREQ_RATIO, 
			voice->system->callbacks);

		assert(result == S_OK);
	}

	return voice;
}

void MGA_Voice_Destroy(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice)
		voice->voice->DestroyVoice();

	delete voice;
}

mgint MGA_Voice_GetBufferCount(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return 0;

	XAUDIO2_VOICE_STATE state;
	voice->voice->GetState(&state, XAUDIO2_VOICE_NOSAMPLESPLAYED);
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
		&& buffer->format
		&& (voice->format.wFormatTag != buffer->format->wFormatTag
			|| voice->format.nChannels != buffer->format->nChannels
			|| voice->format.nSamplesPerSec != buffer->format->nSamplesPerSec
			|| voice->format.wBitsPerSample != buffer->format->wBitsPerSample))
	{
		voice->voice->DestroyVoice();
		voice->voice = nullptr;
	}

	// Stop and remove any pending buffers first.
	if (voice->voice)
	{
		voice->voice->Stop();
		voice->voice->FlushSourceBuffers();
	}
	else if (buffer)
	{
		auto result = voice->system->audio->CreateSourceVoice(
			&voice->voice, 
			buffer->format, 
			XAUDIO2_VOICE_USEFILTER, 
			XAUDIO2_DEFAULT_FREQ_RATIO, 
			nullptr);
		assert(result == S_OK);
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
	XAUDIO2_BUFFER vbuffer;
	memset(&vbuffer, 0, sizeof(vbuffer));
	vbuffer.pAudioData = raw->data;
	vbuffer.AudioBytes = size;
	vbuffer.pContext = raw;

	voice->voice->SubmitSourceBuffer(&vbuffer, nullptr);
}

void MGA_Voice_Play(MGA_Voice* voice, mgbyte looped)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	if (voice->buffer != nullptr)
	{
		voice->voice->Stop();
		voice->voice->FlushSourceBuffers();

		voice->looped = looped;

		auto buffer = voice->buffer->buffer;
		if (looped)
			buffer.LoopCount = XAUDIO2_LOOP_INFINITE;
		else
			buffer.LoopBegin = buffer.LoopLength = buffer.LoopCount = 0;

		voice->voice->SubmitSourceBuffer(&buffer, voice->buffer->wmaBuffer);
	}

	voice->finishedBuffers = 0;
	voice->state = MGSoundState::Playing;
	voice->voice->Start();
}

void MGA_Voice_Pause(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;
		
	if (voice->state == MGSoundState::Paused)
		return;

	XAUDIO2_VOICE_STATE state;
	voice->voice->GetState(&state, XAUDIO2_VOICE_NOSAMPLESPLAYED);
	if (state.BuffersQueued == 0)
		return;

	voice->voice->Stop(0, XAUDIO2_COMMIT_NOW);
	voice->state = MGSoundState::Paused;
}

void MGA_Voice_Resume(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	if (voice->state != MGSoundState::Paused)
	{
		MGA_Voice_Play(voice, voice->looped);
		return;
	}

	voice->voice->Start();
	voice->state = MGSoundState::Playing;
}

void MGA_Voice_Stop(MGA_Voice* voice, mgbyte immediate)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;
		
	voice->voice->Stop(immediate ? XAUDIO2_PLAY_TAILS : 0, XAUDIO2_COMMIT_NOW);
	voice->voice->FlushSourceBuffers();
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

	XAUDIO2_VOICE_STATE state;
	voice->voice->GetState(&state, XAUDIO2_VOICE_NOSAMPLESPLAYED);
	if (state.BuffersQueued == 0)
		return MGSoundState::Stopped;

	return MGSoundState::Playing;
}

mgulong MGA_Voice_GetPosition(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return 0;
				
	XAUDIO2_VOICE_STATE state;
	voice->voice->GetState(&state, 0);

	float msec = (state.SamplesPlayed / (float)voice->format.nSamplesPerSec) * 1000.0f;
	return (mgulong)msec;
}

static void MGA_Voice_UpdateOutputMatrix(MGA_Voice* voice)
{
	if (voice->voice == nullptr)
		return;

	XAUDIO2_VOICE_DETAILS details;
	memset(&details, 0, sizeof(details));
	voice->voice->GetVoiceDetails(&details);
	int srcChannelCount = details.InputChannels;
	voice->system->masterVoice->GetVoiceDetails(&details);
	int dstChannelCount = details.InputChannels;

	// Default to zero volume on all channels.
	float panMatrix[16];
	memset(panMatrix, 0, sizeof(panMatrix));

	// Set the pan on the correct channels based on the reverb mix.
	if (!(voice->reverbMix > 0.0f))
		voice->voice->SetOutputMatrix(nullptr, srcChannelCount, dstChannelCount,
			MGA_Voice_CalculatePanMatrix(voice->pan, 1.0f, panMatrix, srcChannelCount));
	else
	{
		voice->voice->SetOutputMatrix(voice->system->reverbVoice, srcChannelCount, dstChannelCount,
			MGA_Voice_CalculatePanMatrix(voice->pan, voice->reverbMix, panMatrix, srcChannelCount));
		voice->voice->SetOutputMatrix(voice->system->masterVoice, srcChannelCount, dstChannelCount,
			MGA_Voice_CalculatePanMatrix(voice->pan, 1.0f - (voice->reverbMix > 1.0f ? 1.0f : voice->reverbMix), panMatrix, srcChannelCount));
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
	voice->voice->SetFrequencyRatio(ratio);
}

void MGA_Voice_SetVolume(MGA_Voice* voice, mgfloat volume)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	voice->voice->SetVolume(volume);
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
		XAUDIO2_SEND_DESCRIPTOR desc[2];
		desc[0].pOutputVoice = voice->system->reverbVoice;
		desc[0].Flags = 0;
		desc[1].pOutputVoice = voice->system->masterVoice;
		desc[1].Flags = 0;

		XAUDIO2_VOICE_SENDS sends;
		sends.SendCount = 2;
		sends.pSends = desc;
		voice->voice->SetOutputVoices(&sends);
	}
	else
	{
		XAUDIO2_SEND_DESCRIPTOR desc[1];
		desc[0].pOutputVoice = voice->system->masterVoice;
		desc[0].Flags = 0;
		
		XAUDIO2_VOICE_SENDS sends;
		sends.SendCount = 1;
		sends.pSends = desc;
		voice->voice->SetOutputVoices(&sends);
	}

	MGA_Voice_UpdateOutputMatrix(voice);
}

void MGA_Voice_SetFilterMode(MGA_Voice* voice, MGFilterMode mode, mgfloat filterQ, mgfloat frequency)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	XAUDIO2_VOICE_DETAILS details;
	memset(&details, 0, sizeof(details));
	voice->voice->GetVoiceDetails(&details);

	if (filterQ > 0.0f)
	{
		filterQ = 1.0f / filterQ;
		if (filterQ > XAUDIO2_MAX_FILTER_ONEOVERQ)
			filterQ = XAUDIO2_MAX_FILTER_ONEOVERQ;
	}
	else
		filterQ = 1.0f;

	XAUDIO2_FILTER_PARAMETERS params;
	params.Type = (XAUDIO2_FILTER_TYPE)mode;
	params.Frequency = XAudio2CutoffFrequencyToRadians(frequency, details.InputSampleRate);
	params.OneOverQ = filterQ;
	voice->voice->SetFilterParameters(&params);
}

void MGA_Voice_ClearFilterMode(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->voice == nullptr)
		return;

	XAUDIO2_FILTER_PARAMETERS params;
	params.Type = XAUDIO2_FILTER_TYPE::LowPassFilter;
	params.Frequency = 1.0f;
	params.OneOverQ = 1.0f;
	voice->voice->SetFilterParameters(&params);
}

void MGA_Voice_Apply3D(MGA_Voice* voice, Listener& listener, Emitter& emitter, mgfloat distanceScale)
{
	assert(voice != nullptr);

	X3DAUDIO_LISTENER xListener;
	xListener.OrientFront.x = listener.Forward.X;
	xListener.OrientFront.y = listener.Forward.Y;
	xListener.OrientFront.z = listener.Forward.Z;
	xListener.OrientTop.x = listener.Up.X;
	xListener.OrientTop.y = listener.Up.Y;
	xListener.OrientTop.z = listener.Up.Z;
	xListener.Position.x = listener.Position.X;
	xListener.Position.y = listener.Position.Y;
	xListener.Position.z = listener.Position.Z;
	xListener.Velocity.x = listener.Velocity.X;
	xListener.Velocity.y = listener.Velocity.Y;
	xListener.Velocity.z = listener.Velocity.Z;
	xListener.pCone = nullptr;

	XAUDIO2_VOICE_DETAILS details;
	memset(&details, 0, sizeof(details));
	voice->voice->GetVoiceDetails(&details);
	int srcChannelCount = details.InputChannels;

	static float azimuths[4] = { 0, 0, 0, 0 };

	X3DAUDIO_EMITTER xEmitter;
	memset(&xEmitter, 0, sizeof(xEmitter));
	xEmitter.OrientFront.x = emitter.Forward.X;
	xEmitter.OrientFront.y = emitter.Forward.Y;
	xEmitter.OrientFront.z = emitter.Forward.Z;
	xEmitter.OrientTop.x = emitter.Up.X;
	xEmitter.OrientTop.y = emitter.Up.Y;
	xEmitter.OrientTop.z = emitter.Up.Z;
	xEmitter.Position.x = emitter.Position.X;
	xEmitter.Position.y = emitter.Position.Y;
	xEmitter.Position.z = emitter.Position.Z;
	xEmitter.Velocity.x = emitter.Velocity.X;
	xEmitter.Velocity.y = emitter.Velocity.Y;
	xEmitter.Velocity.z = emitter.Velocity.Z;
	xEmitter.DopplerScaler = emitter.DopplerScale;
	xEmitter.ChannelCount = srcChannelCount;
	xEmitter.pChannelAzimuths = azimuths;
	xEmitter.CurveDistanceScaler = 1.0f;

	static float DspMatrix[XAUDIO2_MAX_AUDIO_CHANNELS * 8];

	X3DAUDIO_DSP_SETTINGS dsp;
	memset(&dsp, 0, sizeof(dsp));
	dsp.pMatrixCoefficients = DspMatrix;

	UINT32 flags = X3DAUDIO_CALCULATE_MATRIX | X3DAUDIO_CALCULATE_DOPPLER;
	X3DAudioCalculate(voice->system->x3daudio, &xListener, &xEmitter, flags, &dsp);

	voice->system->masterVoice->GetVoiceDetails(&details);
	int dstChannelCount = details.InputChannels;

	voice->voice->SetOutputMatrix(nullptr, srcChannelCount, dstChannelCount, dsp.pMatrixCoefficients, 0);

	voice->voice->SetFrequencyRatio(dsp.DopplerFactor);
}

