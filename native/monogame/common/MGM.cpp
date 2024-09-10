// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

struct MGG_Texture;

#include "api_MGM.h"

#include "mg_common.h"


struct MGM_Song
{
	mgulong duration;

	void (*finishCallback)(void*);
	void* finishData;
};

MGM_Song* MGM_Song_Create(const char* mediaFilePath)
{
	assert(mediaFilePath != nullptr);

	// TODO: This should detect the media format using
	// standard native format decoder libraries that are
	// portable to all our target platforms:
	//
	// libvorbis
	// minimp3
	// wave ??
	// mp4 ??
	// FLAC ??
	//
	// It should then spin up thread which decodes the
	// audio and streams buffers to a native SoundEffect.

	auto song = new MGM_Song();
	song->duration = 0;
	song->finishCallback = nullptr;
	song->finishData = nullptr;
	return song;
}

mgulong MGM_Song_GetDuration(MGM_Song* song)
{
	assert(song != nullptr);
	return song->duration;
}

mgulong MGM_Song_GetPosition(MGM_Song* song)
{
	assert(song != nullptr);
	return 0;
}

mgfloat MGM_Song_GetVolume(MGM_Song* song)
{
	assert(song != nullptr);
	return 0;
}

void MGM_Song_SetVolume(MGM_Song* song, mgfloat volume)
{
	assert(song != nullptr);
}

void MGM_Song_Pause(MGM_Song* song)
{
	assert(song != nullptr);
}

void MGM_Song_Play(MGM_Song* song, mgulong startPositionMs, void (*callback)(void*), void* callbackData)
{
	assert(song != nullptr);

	song->finishCallback = callback;
	song->finishData = callbackData;
}

void MGM_Song_Resume(MGM_Song* song)
{
	assert(song != nullptr);
}

void MGM_Song_Stop(MGM_Song* song)
{
	assert(song != nullptr);
}

void MGM_Song_Destroy(MGM_Song* song)
{
	assert(song != nullptr);
	delete song;
}


struct MGM_Video
{
	mguint width;
	mguint height;
	mgfloat fps;
	mgulong duration;
	mgint cachedFrames;
};

MGM_Video* MGM_Video_Create(const char* mediaFilePath, mgint cachedFrameNum, mgint& width, mgint& height, mgfloat& fps, mgulong& duration)
{
	assert(mediaFilePath != nullptr);

	// TODO: Like Song above we should detect the media
	// format from a native decoder libraries that are
	// portable to all our target platforms:
	//
	// libtheora - free but limited quality/performance
	// OpenH264 -
	// ???
	//
	// It should then spin up thread which decodes the
	// video/audio streams.

	auto video = new MGM_Video();
	video->duration = duration = 0;
	video->width = width = 0;
	video->height = height = 0;
	video->fps = fps = 0.0f;
	video->cachedFrames = cachedFrameNum;

	return video;
}

void MGM_Video_Destroy(MGM_Video* video)
{
	assert(video != nullptr);
	delete video;
}

MGMediaState MGM_Video_GetState(MGM_Video* video)
{
	assert(video != nullptr);
	return MGMediaState::Stopped;
}

mgulong MGM_Video_GetPosition(MGM_Video* video)
{
	assert(video != nullptr);
	return 0;
}

void MGM_Video_SetVolume(MGM_Video* video, mgfloat volume)
{
	assert(video != nullptr);
}

void MGM_Video_SetLooped(MGM_Video* video, mgbool looped)
{
	assert(video != nullptr);
}

void MGM_Video_Play(MGM_Video* video)
{
	assert(video != nullptr);
}

void MGM_Video_Pause(MGM_Video* video)
{
	assert(video != nullptr);
}

void MGM_Video_Resume(MGM_Video* video)
{
	assert(video != nullptr);
}

void MGM_Video_Stop(MGM_Video* video)
{
	assert(video != nullptr);
}

void MGM_Video_GetFrame(MGM_Video* video, mguint& frame, MGG_Texture*& handle)
{
	assert(video != nullptr);
	frame = 0;
	handle = nullptr;
}

