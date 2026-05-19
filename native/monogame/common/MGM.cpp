// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

struct MGG_Texture;

#include "api_MGM.h"

#include "MGM_common.h"
#include "api_MGA.h"
#include "api_MGG.h"

#include <stdio.h>
#include <string.h>

#define OGG_IMPL
#define VORBIS_IMPL
#include "minivorbis.h"

#define MINIMP3_ONLY_MP3
#define MINIMP3_ONLY_SIMD
#define MINIMP3_IMPLEMENTATION
#include "minimp3_ex.h"


void MGM_ReadSignature(const char* filepath, uint8_t* signature)
{
	memset(signature, 0, 16);

	FILE* handle = fopen(filepath, "rb");
	if (handle == nullptr)
		return;

	fread(signature, 1, 16, handle);
	fclose(handle);
}

struct MGM_AudioDecoder_Ogg : MGM_AudioDecoder
{
	OggVorbis_File* _vreader = nullptr;
	int16_t* _buffer = nullptr;
	int _sizeInBytes = 0;

	bool _finished = false;

	virtual ~MGM_AudioDecoder_Ogg();
	virtual void Initialize(const char* filepath, MGM_AudioDecoderInfo& info);
	virtual void SetPosition(mgulong timeMs);
	virtual bool Decode(mgbyte*& buffer, mguint& size);
};


MGM_AudioDecoder_Ogg::~MGM_AudioDecoder_Ogg()
{
	if (_vreader)
	{
		ov_clear(_vreader);
		delete _vreader;
	}

	if (_buffer)
		delete [] _buffer;
}

void MGM_AudioDecoder_Ogg::Initialize(const char* filepath, MGM_AudioDecoderInfo& info)
{
	_vreader = new OggVorbis_File();

	int err = ov_fopen(filepath, _vreader);

	vorbis_info* vinfo = ov_info(_vreader, -1);
	ogg_int64_t samples = ov_pcm_total(_vreader, -1);

	info.samplerate = vinfo->rate;
	info.channels = vinfo->channels;
	info.duration = (samples / (float)info.samplerate) * 1000ull;

	// Decode 250ms of audio per decode step.
	_sizeInBytes = ((info.samplerate / 4) * info.channels) * 2;
	_buffer = new int16_t[_sizeInBytes / 2];

	_finished = false;
}

void MGM_AudioDecoder_Ogg::SetPosition(mgulong timeMs)
{
	if (!_vreader)
		return;

	if (ov_time_seek(_vreader, timeMs * 0.001) == 0)
		_finished = false;
}

bool MGM_AudioDecoder_Ogg::Decode(mgbyte*& buffer, mguint& size)
{
	buffer = nullptr;
	size = 0;

	if (!_vreader || _finished)
		return true;

	int bitstream = 0;
	int readBytes = 0;
	char* dest = (char*)_buffer;

	while (readBytes < _sizeInBytes)
	{
		int decoded = ov_read(_vreader, dest, _sizeInBytes - readBytes, 0, 2, 1, &bitstream);

		// If we got an error call it finished.
		if (decoded < 0)
		{
			_finished = true;
			break;
		}

		// We have no more data so we're finished.
		if (decoded == 0)
		{
			_finished = readBytes == 0;
			break;
		}

		readBytes += decoded;
		dest += decoded;
	}

	size = readBytes;
	buffer = (mgbyte*)_buffer;

	return _finished;
}


MGM_AudioDecoder* MGM_AudioDecoder_TryCreate_Ogg(const uint8_t* signature)
{
	// Simple header detection.
	if (signature[0] != 'O' ||
		signature[1] != 'g' ||
		signature[2] != 'g' ||
		signature[3] != 'S' ||
		signature[4] != 0)
		return nullptr;

	return new MGM_AudioDecoder_Ogg();
}


struct MGM_AudioDecoder_Mp3 : MGM_AudioDecoder
{
	mp3dec_ex_t* _mp3d;
	int16_t* _buffer = nullptr;
	int _sizeInSamples = 0;

	bool _finished = false;

	virtual ~MGM_AudioDecoder_Mp3();
	virtual void Initialize(const char* filepath, MGM_AudioDecoderInfo& info);
	virtual void SetPosition(mgulong timeMs);
	virtual bool Decode(mgbyte*& buffer, mguint& size);
};


MGM_AudioDecoder_Mp3::~MGM_AudioDecoder_Mp3()
{
	if (_mp3d)
		delete _mp3d;

	if (_buffer)
		delete[] _buffer;
}

void MGM_AudioDecoder_Mp3::Initialize(const char* filepath, MGM_AudioDecoderInfo& info)
{
	_mp3d = new mp3dec_ex_t();

	if (mp3dec_ex_open(_mp3d, filepath, MP3D_SEEK_TO_SAMPLE))
	{
		_finished = true;
		return;
	}

	info.samplerate = _mp3d->info.hz;
	info.channels = _mp3d->info.channels;
	info.duration = ((_mp3d->samples / info.channels) / (float)info.samplerate) * 1000ull;

	// Decode 250ms of audio per decode step.
	_sizeInSamples = ((info.samplerate / 4) * info.channels);
	_buffer = new int16_t[_sizeInSamples];

	_finished = false;
}

void MGM_AudioDecoder_Mp3::SetPosition(mgulong timeMs)
{
	if (!_mp3d)
		return;

	uint64_t pos = (timeMs / 1000.0f) * _mp3d->info.hz;
	if (mp3dec_ex_seek(_mp3d, pos) == 0)
		_finished = false;
}

bool MGM_AudioDecoder_Mp3::Decode(mgbyte*& buffer, mguint& size)
{
	buffer = nullptr;
	size = 0;

	if (!_mp3d || _finished)
		return true;

	int bitstream = 0;
	int readSamples = 0;
	char* dest = (char*)_buffer;

	while (readSamples < _sizeInSamples)
	{
		size_t readed = mp3dec_ex_read(_mp3d, (mp3d_sample_t*)dest, _sizeInSamples - readSamples);

		// If we got an error call it finished.
		if (readed < 0)
		{
			_finished = true;
			break;
		}

		// We have no more data so we're finished.
		if (readed == 0)
		{
			_finished = readSamples == 0;
			break;
		}

		readSamples += readed;
		dest += readed * 2;
	}

	size = readSamples * 2;
	buffer = (mgbyte*)_buffer;

	return _finished;
}

MGM_AudioDecoder* MGM_AudioDecoder_TryCreate_Mp3(const uint8_t* signature)
{
	if ((signature[0] != 'I' || signature[1] != 'D' || signature[2] != '3') &&	// ID3 tag		
		(signature[0] != 0xFF || (signature[1] & 0xE0) != 0xE0)) // MPEG frame sync
		return nullptr;

	return new MGM_AudioDecoder_Mp3();
}

MGM_AudioDecoder* MGM_AudioDecoder_Create(const char* filepath, MGM_AudioDecoderInfo& info)
{
	assert(filepath != nullptr);

	uint8_t signature[16];
	MGM_ReadSignature(filepath, signature);

	// Try the common decoders.
	MGM_AudioDecoder* decoder = nullptr;
	decoder = decoder ? decoder : MGM_AudioDecoder_TryCreate_Ogg(signature);
	decoder = decoder ? decoder : MGM_AudioDecoder_TryCreate_Mp3(signature);

	if (decoder == nullptr)
	{
		info.samplerate = 0;
		info.channels = 0;
		info.duration = 0;
		return nullptr;
	}

	decoder->Initialize(filepath, info);
	return decoder;
}

void MGM_AudioDecoder_Destroy(MGM_AudioDecoder* decoder)
{
	assert(decoder != nullptr);
	delete decoder;
}

void MGM_AudioDecoder_SetPosition(MGM_AudioDecoder* decoder, mgulong timeMS)
{
	assert(decoder != nullptr);
	decoder->SetPosition(timeMS);
}

mgbyte MGM_AudioDecoder_Decode(MGM_AudioDecoder* decoder, mgbyte*& buffer, mguint& size)
{
	assert(decoder != nullptr);
	return decoder->Decode(buffer, size);
}

MGM_VideoDecoder* MGM_VideoDecoder_TryCreate_Theora(const uint8_t* signature)
{
	// TODO: Implement me!
	//
	// - This should be moved into its own CPP.
	// - We need to add Theora support to native build.
	// - How do we compile Theora for consoles?
	// 
	return nullptr;
}

MGM_VideoDecoder* MGM_VideoDecoder_TryCreate_OpenH264(const uint8_t* signature)
{
	// TODO: Implement me!
	//
	// See https://github.com/cisco/openh264
	// 
	// - This should be moved into its own CPP.
	// - We need to add lib to native build.
	// - How do we compile lib for consoles?
	//
	return nullptr;
}

MGM_VideoDecoder* MGM_VideoDecoder_Create(MGG_GraphicsDevice* device, const char* filepath, MGM_VideoDecoderInfo& info)
{
	assert(filepath != nullptr);

	uint8_t signature[16];
	MGM_ReadSignature(filepath, signature);

	// Try the common decoders.
	MGM_VideoDecoder* decoder = nullptr;
	decoder = decoder ? decoder : MGM_VideoDecoder_TryCreate_Theora(signature);
	decoder = decoder ? decoder : MGM_VideoDecoder_TryCreate_OpenH264(signature);

	if (decoder == nullptr)
	{
		info.width = 0;
		info.height = 0;
		info.fps = 0;
		info.duration = 0;
		return nullptr;
	}

	decoder->Initialize(filepath, info);
	return decoder;
}

void MGM_VideoDecoder_Destroy(MGM_VideoDecoder* decoder)
{
	assert(decoder != nullptr);
	delete decoder;
}

MGM_AudioDecoder* MGM_VideoDecoder_GetAudioDecoder(MGM_VideoDecoder* decoder, MGM_AudioDecoderInfo& info)
{
	assert(decoder != nullptr);
	return decoder->GetAudioDecoder(info);
}

mgulong MGM_VideoDecoder_GetPosition(MGM_VideoDecoder* decoder)
{
	assert(decoder != nullptr);
	return decoder->GetPosition();
}

void MGM_VideoDecoder_SetLooped(MGM_VideoDecoder* decoder, mgbyte looped)
{
	assert(decoder != nullptr);
	decoder->SetLooped(looped);
}

MGG_Texture* MGM_VideoDecoder_Decode(MGM_VideoDecoder* decoder)
{
	assert(decoder != nullptr);
	return decoder->Decode();
}

