#include "../include/api_MGM.h"

struct MGM_AudioDecoder
{
};

struct MGM_VideoDecoder
{
};


// TODO: implement media decoding, this is to keep the ABI linkable until it's implemented.
MG_EXPORT MGM_AudioDecoder* MGM_AudioDecoder_Create(const char* filepath, MGM_AudioDecoderInfo& info)
{
    (void)filepath;

    info.samplerate = 0;
    info.channels = 0;
    info.duration = 0;
    return nullptr;
}

MG_EXPORT void MGM_AudioDecoder_Destroy(MGM_AudioDecoder* decoder)
{
    (void)decoder;
}

MG_EXPORT void MGM_AudioDecoder_SetPosition(MGM_AudioDecoder* decoder, mgulong timeMS)
{
    (void)decoder;
    (void)timeMS;
}

MG_EXPORT mgbyte MGM_AudioDecoder_Decode(MGM_AudioDecoder* decoder, mgbyte*& buffer, mguint& size)
{
    (void)decoder;

    buffer = nullptr;
    size = 0;
    return 1;
}

MG_EXPORT MGM_VideoDecoder* MGM_VideoDecoder_Create(MGG_GraphicsDevice* device, const char* filepath, MGM_VideoDecoderInfo& info)
{
    (void)device;
    (void)filepath;

    info.width = 0;
    info.height = 0;
    info.fps = 0;
    info.duration = 0;
    return nullptr;
}

MG_EXPORT void MGM_VideoDecoder_Destroy(MGM_VideoDecoder* decoder)
{
    (void)decoder;
}

MG_EXPORT MGM_AudioDecoder* MGM_VideoDecoder_GetAudioDecoder(MGM_VideoDecoder* decoder, MGM_AudioDecoderInfo& info)
{
    (void)decoder;

    info.samplerate = 0;
    info.channels = 0;
    info.duration = 0;
    return nullptr;
}

MG_EXPORT mgulong MGM_VideoDecoder_GetPosition(MGM_VideoDecoder* decoder)
{
    (void)decoder;
    return 0;
}

MG_EXPORT void MGM_VideoDecoder_SetLooped(MGM_VideoDecoder* decoder, mgbyte looped)
{
    (void)decoder;
    (void)looped;
}

MG_EXPORT MGG_Texture* MGM_VideoDecoder_Decode(MGM_VideoDecoder* decoder)
{
    (void)decoder;
    return nullptr;
}
