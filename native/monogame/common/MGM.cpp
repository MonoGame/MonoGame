// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

struct MGG_Texture;

#include "api_MGM.h"

#include "MGM_common.h"
#include "api_MGA.h"
#include "api_MGG.h"

#include <stdio.h>


void MGM_ReadSignature(const char* filepath, MGM_SIGNATURE)
{
	memset(signature, 0, 16);

	FILE* handle = fopen(filepath, "rb");
	if (handle == nullptr)
		return;

	fread(signature, 1, 16, handle);
	fclose(handle);
}

MGM_AudioDecoder* MGM_AudioDecoder_TryCreate_Ogg(MGM_SIGNATURE)
{
	// TODO: Implement me!
	//
	// - This should be moved into its own CPP.
	// - We need to add Ogg support to native build.
	// - How do we compile Ogg for consoles?
	// 
	return nullptr;
}

MGM_AudioDecoder* MGM_AudioDecoder_TryCreate_Mp3(MGM_SIGNATURE)
{
	// TODO: Implement me!
	//
	// - This should be moved into its own CPP.
	// - Should we use a single header mp3 decoder?
	//
	return nullptr;
}

#if defined(_WIN32)

MGM_AudioDecoder* MGM_AudioDecoder_Create(const char* filepath, MGM_AudioDecoderInfo& info)
{
	assert(filepath != nullptr);

	MGM_SIGNATURE;
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

#endif

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

mgbool MGM_AudioDecoder_Decode(MGM_AudioDecoder* decoder, mgbyte*& buffer, mguint& size)
{
	assert(decoder != nullptr);
	return decoder->Decode(buffer, size);
}



MGM_VideoDecoder* MGM_VideoDecoder_TryCreate_Theora(MGM_SIGNATURE)
{
	// TODO: Implement me!
	//
	// - This should be moved into its own CPP.
	// - We need to add Theora support to native build.
	// - How do we compile Theora for consoles?
	// 
	return nullptr;
}

MGM_VideoDecoder* MGM_VideoDecoder_TryCreate_OpenH264(MGM_SIGNATURE)
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

#if defined(_WIN32)

MGM_VideoDecoder* MGM_VideoDecoder_Create(MGG_GraphicsDevice* device, const char* filepath, MGM_VideoDecoderInfo& info)
{
	assert(filepath != nullptr);

	MGM_SIGNATURE;
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

#endif

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

void MGM_VideoDecoder_SetLooped(MGM_VideoDecoder* decoder, mgbool looped)
{
	assert(decoder != nullptr);
	decoder->SetLooped(looped);
}

MGG_Texture* MGM_VideoDecoder_Decode(MGM_VideoDecoder* decoder)
{
	assert(decoder != nullptr);
	return decoder->Decode();
}

