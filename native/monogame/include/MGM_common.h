// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "mg_common.h"

struct MGM_AudioDecoderInfo;
struct MGG_Texture;


/// <summary>
/// The base for all audio decoders.
/// </summary>
struct MGM_AudioDecoder
{
	virtual ~MGM_AudioDecoder() {}
	virtual void Initialize(const char* filepath, MGM_AudioDecoderInfo& info) = 0;
	virtual void SetPosition(mgulong timeMs) = 0;
	virtual bool Decode(mgbyte*& buffer, mguint& size) = 0;
};


/// <summary>
/// The base for all video decoders.
/// </summary>
struct MGM_VideoDecoder
{
	virtual ~MGM_VideoDecoder() {}
	virtual void Initialize(const char* filepath, MGM_VideoDecoderInfo& info) = 0;
	virtual MGM_AudioDecoder* GetAudioDecoder(MGM_AudioDecoderInfo& info) = 0;
	virtual mgulong GetPosition() = 0;
	virtual void SetLooped(mgbool looped) = 0;
	virtual MGG_Texture* Decode() = 0;
};



// This seems like enough to detect most file formats.
#define MGM_SIGNATURE char signature[16]


/// <summary>
///  Helper to read first bytes of a file to get its signature.
/// </summary>
void MGM_ReadSignature(const char* filepath, MGM_SIGNATURE);


// These are the common decoders supported on all platforms.
// They all work via optimized software decoding.

MGM_AudioDecoder* MGM_AudioDecoder_TryCreate_Ogg(MGM_SIGNATURE);
MGM_AudioDecoder* MGM_AudioDecoder_TryCreate_Mp3(MGM_SIGNATURE);

MGM_VideoDecoder* MGM_VideoDecoder_TryCreate_Theora(MGM_SIGNATURE);
MGM_VideoDecoder* MGM_VideoDecoder_TryCreate_OpenH264(MGM_SIGNATURE);
