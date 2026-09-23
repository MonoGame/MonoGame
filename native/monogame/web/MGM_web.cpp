#include "../include/api_MGM.h"

#include <emscripten/emscripten.h>

#include <queue>
#include <string>
#include <unordered_map>

struct MGM_AudioDecoder
{
};

struct MGM_VideoDecoder
{
};

struct MGM_Song
{
    mgint id;
    mgulong generation = 0;
    mguint commandId = 0;
    mgfloat volume = 1.0f;
    bool isPlaying = false;
    std::string mediaPath;
    std::queue<MGM_SongEvent> events;
};

static std::unordered_map<mgint, MGM_Song*> s_songs;
static mgint s_nextSongId = 1;

EM_JS(mgbyte, MGM_Web_Song_Play, (mgint id, const char* mediaPath, mgdouble positionMS, mgfloat volume, mguint commandId),
{
    const host = globalThis.MonoGameWebHost?.getActiveHost?.();
    if (host == null)
        return 0;

    return host.playSong(id, UTF8ToString(mediaPath), positionMS, volume, commandId) ? 1 : 0;
});

EM_JS(void, MGM_Web_Song_Pause, (mgint id, mguint commandId),
{
    globalThis.MonoGameWebHost?.getActiveHost?.()?.pauseSong(id, commandId);
});

EM_JS(void, MGM_Web_Song_Resume, (mgint id, mguint commandId),
{
    globalThis.MonoGameWebHost?.getActiveHost?.()?.resumeSong(id, commandId);
});

EM_JS(void, MGM_Web_Song_Stop, (mgint id),
{
    globalThis.MonoGameWebHost?.getActiveHost?.()?.stopSong(id);
});

EM_JS(void, MGM_Web_Song_SetVolume, (mgint id, mgfloat volume),
{
    globalThis.MonoGameWebHost?.getActiveHost?.()?.setSongVolume(id, volume);
});

EM_JS(mgdouble, MGM_Web_Song_GetPosition, (mgint id),
{
    return globalThis.MonoGameWebHost?.getActiveHost?.()?.getSongPosition(id) ?? 0;
});

EM_JS(mgdouble, MGM_Web_Song_GetDuration, (mgint id),
{
    return globalThis.MonoGameWebHost?.getActiveHost?.()?.getSongDuration(id) ?? 0;
});

static mguint MGM_Song_NextCommandId(MGM_Song* song)
{
    song->commandId++;
    if (song->commandId == 0)
        song->commandId++;

    return song->commandId;
}

static void MGM_Song_StopInternal(MGM_Song* song)
{
    MGM_Song_NextCommandId(song);
    song->isPlaying = false;
    std::queue<MGM_SongEvent>().swap(song->events);
    MGM_Web_Song_Stop(song->id);
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGM_Web_NotifySongEvent(mgint id, mguint commandId, mgint type)
{
    std::unordered_map<mgint, MGM_Song*>::iterator songIterator = s_songs.find(id);
    if (songIterator == s_songs.end())
        return;

    MGM_Song* song = songIterator->second;
    if (!song->isPlaying || song->commandId != commandId)
        return;

    if (type != static_cast<mgint>(MGSongEventType::Completed)
        && type != static_cast<mgint>(MGSongEventType::Failed))
    {
        return;
    }

    song->events.push({ static_cast<MGSongEventType>(type), song->generation });
    song->isPlaying = false;
}

MG_EXPORT MGM_Song* MGM_Song_Create(const char* filepath, MGA_System* system, MGM_SongInfo& info)
{
    (void)system;

    if (filepath == nullptr)
        return nullptr;

    MGM_Song* song = new MGM_Song();
    song->id = s_nextSongId++;
    song->mediaPath = filepath;
    s_songs.emplace(song->id, song);

    info.duration = 0;
    return song;
}

MG_EXPORT void MGM_Song_Destroy(MGM_Song* song)
{
    if (song == nullptr)
        return;

    MGM_Song_StopInternal(song);
    s_songs.erase(song->id);
    delete song;
}

MG_EXPORT mgbyte MGM_Song_Play(MGM_Song* song, mgulong positionMS, mgulong generation)
{
    if (song == nullptr)
        return 0;

    MGM_Song_StopInternal(song);
    mguint commandId = MGM_Song_NextCommandId(song);
    song->generation = generation;
    song->isPlaying = true;

    if (MGM_Web_Song_Play(
        song->id,
        song->mediaPath.c_str(),
        static_cast<mgdouble>(positionMS),
        song->volume,
        commandId) == 0)
    {
        song->events.push({ MGSongEventType::Failed, generation });
        song->isPlaying = false;
    }

    return 1;
}

MG_EXPORT void MGM_Song_Pause(MGM_Song* song)
{
    if (song != nullptr && song->isPlaying)
        MGM_Web_Song_Pause(song->id, song->commandId);
}

MG_EXPORT void MGM_Song_Resume(MGM_Song* song)
{
    if (song != nullptr && song->isPlaying)
        MGM_Web_Song_Resume(song->id, song->commandId);
}

MG_EXPORT void MGM_Song_Stop(MGM_Song* song)
{
    if (song != nullptr)
        MGM_Song_StopInternal(song);
}

MG_EXPORT void MGM_Song_SetVolume(MGM_Song* song, mgfloat volume)
{
    if (song == nullptr)
        return;

    song->volume = volume;
    if (song->isPlaying)
        MGM_Web_Song_SetVolume(song->id, volume);
}

MG_EXPORT mgulong MGM_Song_GetPosition(MGM_Song* song)
{
    if (song == nullptr || !song->isPlaying)
        return 0;

    mgdouble position = MGM_Web_Song_GetPosition(song->id);
    return position > 0.0 ? static_cast<mgulong>(position) : 0;
}

MG_EXPORT mgulong MGM_Song_GetDuration(MGM_Song* song)
{
    if (song == nullptr)
        return 0;

    mgdouble duration = MGM_Web_Song_GetDuration(song->id);
    return duration > 0.0 ? static_cast<mgulong>(duration) : 0;
}

MG_EXPORT mgbyte MGM_Song_TryDequeueEvent(MGM_Song* song, MGM_SongEvent& songEvent)
{
    if (song == nullptr || song->events.empty())
        return 0;

    songEvent = song->events.front();
    song->events.pop();
    return 1;
}


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
