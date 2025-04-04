// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGA.h"

#include "mg_common.h"

#include <vector>

#define XAUDIO2_HELPER_FUNCTIONS
#include <xaudio2.h>
#include <xaudio2fx.h>
#include <x3daudio.h>

#pragma comment(lib, "xaudio2")

struct MGA_System
{
	IXAudio2* audio = nullptr;
	IXAudio2MasteringVoice* masterVoice = nullptr;
	IXAudio2SubmixVoice* reverbVoice = nullptr;
	X3DAUDIO_HANDLE x3daudio;
};

struct MGA_Buffer
{
	WAVEFORMATEX* format = nullptr;
	XAUDIO2_BUFFER buffer;
	XAUDIO2_BUFFER_WMA* wmaBuffer = nullptr;
	uint8_t* data = nullptr;
	uint32_t length = 0;
};

struct MGA_Voice
{
	MGA_System* system = nullptr;

	IXAudio2SourceVoice* voice = nullptr;

	MGA_Buffer* buffer = nullptr;

	float pan = 0.0f;
	float reverbMix = 0.0f;
	bool looped = false;
	bool paused = false;
};

static std::vector<MGA_Buffer*> s_FreeStreamingBuffers;

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

	return system;
}

void MGA_System_Destroy(MGA_System* system)
{
	assert(system != nullptr);

	// Release the streaming buffers.
	for (auto free : s_FreeStreamingBuffers)
		MGA_Buffer_Destroy(free);
	s_FreeStreamingBuffers.clear();

	// TODO: We're assuming here the C# side is cleaning up
	// buffers/voices, but if we want this to be a good C++
	// API as well, we likely should cleanup ourselves too.

	delete system;
}

mgint MGA_System_GetMaxInstances()
{
	// This seems like a reasonable number.
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
	params.ReverbDelay = (BYTE)(settings.ReverbDelayMs * timeScale);
	params.RearDelay = (BYTE)(settings.RearDelayMs * timeScale);
	params.RoomSize = settings.RoomSizeFeet;
	params.Density = settings.DensityPct;
	params.LowEQGain = (BYTE)settings.LowEqGain;
	params.LowEQCutoff = (BYTE)settings.LowEqCutoff;
	params.HighEQGain = (BYTE)settings.HighEqGain;
	params.HighEQCutoff = (BYTE)settings.HighEqCutoff;
	params.PositionLeft = (BYTE)settings.PositionLeft;
	params.PositionRight = (BYTE)settings.PositionRight;
	params.PositionMatrixLeft = (BYTE)settings.PositionLeftMatrix;
	params.PositionMatrixRight = (BYTE)settings.PositionRightMatrix;
	params.EarlyDiffusion = (BYTE)settings.EarlyDiffusion;
	params.LateDiffusion = (BYTE)settings.LateDiffusion;
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

	free(buffer->data);
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

	if (wformat->wFormatTag == WAVE_FORMAT_ADPCM)
	{
		// We are assuming MSADPCM here!

		auto format = (ADPCMWAVEFORMAT*)malloc(sizeof(ADPCMWAVEFORMAT) + (7 * sizeof(ADPCMCOEFSET)));
		memset(format, 0, sizeof(ADPCMWAVEFORMAT));
		format->wfx.wFormatTag = WAVE_FORMAT_ADPCM;
		format->wfx.nSamplesPerSec = wformat->nSamplesPerSec;
		format->wfx.nChannels = wformat->nChannels;
		format->wfx.nBlockAlign = wformat->nBlockAlign;
		format->wfx.wBitsPerSample = wformat->wBitsPerSample;
		format->wfx.nAvgBytesPerSec = wformat->nAvgBytesPerSec;
		format->wfx.cbSize = 32;
		format->wSamplesPerBlock = (wformat->nBlockAlign * 2) / (wformat->nChannels) - 12;
		format->wNumCoef = 7;
		format->aCoef[0] = { 256, 0 };
		format->aCoef[1] = { 512, -256 };
		format->aCoef[2] = { 0, 0 };
		format->aCoef[3] = { 192, 64 };
		format->aCoef[4] = { 240, 0 };
		format->aCoef[5] = { 460, -208 };
		format->aCoef[6] = { 392, -232 };

		buffer->format = (WAVEFORMATEX*)format;

		// Buffer should be block aligned.
		assert((length % wformat->nBlockAlign) == 0);

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
	else if (wformat->wFormatTag == WAVE_FORMAT_WMAUDIO2)
	{
		// TODO: The API here needs to change to pass
		// additional data for this format to work.
	}
}

void MGA_Buffer_InitializePCM(MGA_Buffer* buffer, mgbyte* waveData, mgint offset, mgint length, mgint sampleBits, mgint sampleRate, mgint channels, mgint loopStart, mgint loopLength)
{
	assert(buffer != nullptr);
	assert(waveData != nullptr);
	assert(offset >=0);
	assert(length > 0);

	auto format = (WAVEFORMATEX*)malloc(sizeof(WAVEFORMATEX));
	memset(format, 0, sizeof(WAVEFORMATEX));
	format->wFormatTag = WAVE_FORMAT_PCM;
	format->nSamplesPerSec = sampleRate;
	format->nChannels = (WORD)channels;
	format->nBlockAlign = (WORD)channels * 2;
	format->wBitsPerSample = 16;
	format->nAvgBytesPerSec = format->nSamplesPerSec * format->nBlockAlign;
	format->cbSize = 0;
	buffer->format = format;

	// Buffer should be block aligned.
	assert((length % format->nBlockAlign) == 0);

	buffer->length = length;
	buffer->data = (uint8_t*)malloc(length);
	if (waveData)
		memcpy(buffer->data, waveData + offset, length);

	memset(&buffer->buffer, 0, sizeof(XAUDIO2_BUFFER));
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
		auto format = (ADPCMWAVEFORMAT*)malloc(sizeof(ADPCMWAVEFORMAT) + (7 * sizeof(ADPCMCOEFSET)));
		memset(format, 0, sizeof(ADPCMWAVEFORMAT));
		format->wfx.wFormatTag = WAVE_FORMAT_ADPCM;
		format->wfx.nSamplesPerSec = sampleRate;
		format->wfx.nChannels = channels;
		format->wfx.nBlockAlign = blockAlignment;
		format->wfx.wBitsPerSample = 4;
		format->wfx.nAvgBytesPerSec = (sampleRate * 4) / 8;
		format->wfx.cbSize = 32;
		format->wSamplesPerBlock = (blockAlignment * 2) / (channels) - 12;
		format->wNumCoef = 7;
		format->aCoef[0] = { 256, 0 };
		format->aCoef[1] = { 512, -256 };
		format->aCoef[2] = { 0, 0 };
		format->aCoef[3] = { 192, 64 };
		format->aCoef[4] = { 240, 0 };
		format->aCoef[5] = { 460, -208 };
		format->aCoef[6] = { 392, -232 };

		buffer->format = (WAVEFORMATEX*)format;

		// Buffer should be block aligned.
		assert((length % blockAlignment) == 0);

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
	float seconds = (buffer->buffer.AudioBytes / buffer->format->nBlockAlign) / buffer->format->nSamplesPerSec;
	return (mgulong)(seconds * 1000);
}

class VoiceCallbacks : public IXAudio2VoiceCallback
{
	void OnVoiceProcessingPassStart(UINT32) { }
	void OnVoiceProcessingPassEnd() { }
	void OnStreamEnd() { }
	void OnBufferStart(void*) { }
	void OnLoopEnd(void*) { }
	void OnVoiceError(void*, HRESULT) { }

	void OnBufferEnd(void* pBufferContext)
	{
		if (pBufferContext == nullptr)
			return;

		s_FreeStreamingBuffers.push_back((MGA_Buffer*)pBufferContext);
	}
};

static VoiceCallbacks MGA_VoiceCallbacksHandler;

MGA_Voice* MGA_Voice_Create(MGA_System* system, mgint sampleRate, mgint channels)
{
	assert(system != nullptr);
	auto voice = new MGA_Voice();
	voice->system = system;
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

	XAUDIO2_VOICE_STATE state;
	voice->voice->GetState(&state, XAUDIO2_VOICE_NOSAMPLESPLAYED);
	return state.BuffersQueued;
}

void MGA_Voice_SetBuffer(MGA_Voice* voice, MGA_Buffer* buffer)
{
	assert(voice != nullptr);

	if (voice->voice)
	{
		voice->voice->Stop();
		voice->voice->FlushSourceBuffers();
	}
	else
	{
		auto hr = voice->system->audio->CreateSourceVoice(&voice->voice, buffer->format, XAUDIO2_VOICE_USEFILTER, XAUDIO2_DEFAULT_FREQ_RATIO, &MGA_VoiceCallbacksHandler);
		assert(hr == S_OK);
	}

	voice->buffer = buffer;
}

void MGA_Voice_AppendBuffer(MGA_Voice* voice, mgbyte* buffer, mguint size)
{
	assert(voice != nullptr);
	assert(buffer != nullptr);

	// Find a free buffer.
	MGA_Buffer* free = nullptr;
	for (int i = 0; i < s_FreeStreamingBuffers.size(); i++)
	{
		auto f = s_FreeStreamingBuffers[i];
		if (f->length < size)
			continue;

		free = f;
		s_FreeStreamingBuffers.erase(s_FreeStreamingBuffers.begin() + i);
	}

	auto format = voice->buffer->format;

	if (free == nullptr)
	{
		free = new MGA_Buffer;
		MGA_Buffer_InitializePCM(free, nullptr, 0, size, 16, format->wBitsPerSample, format->nChannels, 0, 0);
	}

	memcpy(free->data, buffer, size);

	if (MGA_Voice_GetState(voice) != MGSoundState::Playing)
		return;

	auto xbuffer = free->buffer;
	xbuffer.LoopBegin = xbuffer.LoopLength = xbuffer.LoopCount = 0;
	xbuffer.pContext = free;

	voice->voice->SubmitSourceBuffer(&xbuffer, nullptr);

}

void MGA_Voice_Play(MGA_Voice* voice, mgbool looped)
{
	assert(voice != nullptr);

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

	voice->voice->Start();
	voice->paused = false;
}

void MGA_Voice_Pause(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->paused)
		return;

	XAUDIO2_VOICE_STATE state;
	voice->voice->GetState(&state, XAUDIO2_VOICE_NOSAMPLESPLAYED);
	if (state.BuffersQueued == 0)
		return;

	voice->voice->Stop();
	voice->paused = true;
}

void MGA_Voice_Resume(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (!voice->paused)
		MGA_Voice_Play(voice, voice->looped);
	else
	{
		voice->voice->Start();
		voice->paused = false;
	}
}

void MGA_Voice_Stop(MGA_Voice* voice, mgbool immediate)
{
	assert(voice != nullptr);

	voice->voice->Stop();
	voice->voice->FlushSourceBuffers();
	voice->paused = false;
}

MGSoundState MGA_Voice_GetState(MGA_Voice* voice)
{
	assert(voice != nullptr);

	if (voice->paused)
		return MGSoundState::Paused;

	XAUDIO2_VOICE_STATE state;
	voice->voice->GetState(&state, XAUDIO2_VOICE_NOSAMPLESPLAYED);
	if (state.BuffersQueued == 0)
		return MGSoundState::Stopped;

	return MGSoundState::Playing;
}

mgulong MGA_Voice_GetPosition(MGA_Voice* voice)
{
	return 0;
}

static float* MGA_Voice_CalculatePanMatrix(float pan, float scale, float* matrix, int srcChannels)
{
	if (srcChannels == 1)
	{
		matrix[0] = (pan >= 0 ? (1.f - pan) : 1.f) * scale; // Left
		matrix[1] = (pan <= 0 ? (-pan - 1.f) : 1.f) * scale; // Right
	}
	else if (srcChannels == 2)
	{
		if (-1.0f <= pan && pan <= 0.0f)
		{
			matrix[0] = (0.5f * pan + 1.0f) * scale;	// .5 when pan is -1, 1 when pan is 0
			matrix[1] = (0.5f * -pan) * scale;			// .5 when pan is -1, 0 when pan is 0
			matrix[2] = 0.0f;							//  0 when pan is -1, 0 when pan is 0
			matrix[3] = (pan + 1.0f) * scale;			//  0 when pan is -1, 1 when pan is 0
		}
		else
		{
			matrix[0] = (-pan + 1.0f) * scale;			//  1 when pan is 0,   0 when pan is 1
			matrix[1] = 0.0f;							//  0 when pan is 0,   0 when pan is 1
			matrix[2] = (0.5f * pan) * scale;			//  0 when pan is 0, .5f when pan is 1
			matrix[3] = (0.5f * -pan + 1.0f) * scale;	//  1 when pan is 0. .5f when pan is 1
		}
	}

	return matrix;
}

static void MGA_Voice_UpdateOutputMatrix(MGA_Voice* voice)
{
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
	voice->pan = pan;
	MGA_Voice_UpdateOutputMatrix(voice);
}

void MGA_Voice_SetPitch(MGA_Voice* voice, mgfloat pitch)
{
	assert(voice != nullptr);

	float ratio = powf(2.0f, pitch);
	voice->voice->SetFrequencyRatio(ratio);
}

void MGA_Voice_SetVolume(MGA_Voice* voice, mgfloat volume)
{
	assert(voice != nullptr);
	voice->voice->SetVolume(volume);
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

