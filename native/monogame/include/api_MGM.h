// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
                
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes

#pragma once

#include "api_common.h"
#include "api_enums.h"
#include "api_structs.h"


struct MGM_AudioDecoder;
struct MGM_VideoDecoder;
struct MGG_GraphicsDevice;
struct MGG_Texture;

MG_EXPORT MGM_AudioDecoder* MGM_AudioDecoder_Create(const char* filepath, MGM_AudioDecoderInfo& info);
MG_EXPORT void MGM_AudioDecoder_Destroy(MGM_AudioDecoder* decoder);
MG_EXPORT void MGM_AudioDecoder_SetPosition(MGM_AudioDecoder* decoder, mgulong timeMS);
MG_EXPORT mgbool MGM_AudioDecoder_Decode(MGM_AudioDecoder* decoder, mgbyte*& buffer, mguint& size);
MG_EXPORT MGM_VideoDecoder* MGM_VideoDecoder_Create(MGG_GraphicsDevice* device, const char* filepath, MGM_VideoDecoderInfo& info);
MG_EXPORT void MGM_VideoDecoder_Destroy(MGM_VideoDecoder* decoder);
MG_EXPORT MGM_AudioDecoder* MGM_VideoDecoder_GetAudioDecoder(MGM_VideoDecoder* decoder, MGM_AudioDecoderInfo& info);
MG_EXPORT mgulong MGM_VideoDecoder_GetPosition(MGM_VideoDecoder* decoder);
MG_EXPORT void MGM_VideoDecoder_SetLooped(MGM_VideoDecoder* decoder, mgbool looped);
MG_EXPORT MGG_Texture* MGM_VideoDecoder_Decode(MGM_VideoDecoder* decoder);
