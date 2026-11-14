// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "mg_common.h"
#include "api_MG_Asset.h"

struct MGM_AudioDecoderInfo;
struct MGG_Texture;
enum class MGV_TYPE : mguint { VIDEO = 0, AuDIO = 1 };


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
/// </summary>
struct MGM_VideoContainer
{
	virtual ~MGM_VideoContainer() {}
	virtual void Initialize(MG_Asset* asset) = 0;
	virtual bool Decode(mgbyte*& buffer, mguint& size, MGV_TYPE& type) = 0;
};


/// <summary>
/// The base for all video decoders.
/// </summary>
struct MGM_VideoDecoder
{
	virtual ~MGM_VideoDecoder() {}
	virtual void Initialize(const char* filepath, MGM_VideoInfo& info) = 0;
	virtual MGM_AudioDecoder* GetAudioDecoder(MGM_AudioDecoderInfo& info) = 0;
	virtual mgulong GetPosition() = 0;
	virtual void SetLooped(mgbool looped) = 0;
	virtual MGG_Texture* Decode() = 0;
};

struct MGM_Dav1dDecoder : MGM_VideoDecoder
{
	virtual ~MGM_Dav1dDecoder() {}
	virtual void Initialize(const char* filepath, MGM_VideoInfo& info) override = 0;
	virtual MGM_AudioDecoder* GetAudioDecoder(MGM_AudioDecoderInfo& info) override = 0;
	virtual mgulong GetPosition() override = 0;
	virtual void SetLooped(mgbool looped) override = 0;
	virtual MGG_Texture* Decode() override = 0;
};


/// <summary>
///  Helper to read first bytes of a file to get its signature.
/// </summary>
void MGM_ReadSignature(const char* filepath, const uint8_t* signature);


// These are the common decoders supported on all platforms.
// They all work via optimized software decoding.

MGM_AudioDecoder* MGM_AudioDecoder_TryCreate_Ogg(const uint8_t* signature);
MGM_AudioDecoder* MGM_AudioDecoder_TryCreate_Mp3(const uint8_t* signature);

MGM_VideoDecoder* MGM_VideoDecoder_TryCreate_Theora(const uint8_t* signature);
MGM_VideoDecoder* MGM_VideoDecoder_TryCreate_OpenH264(const uint8_t* signature);
MGM_VideoDecoder* MGM_VideoDecoder_TryCreate_Dav1d(const uint8_t* signature);
