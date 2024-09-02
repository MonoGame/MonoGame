// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "csharp_MGM.h"

#include "stl_common.h"


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
	// standard native format parsing libraries that are
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
