// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
                
//
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes
//

#pragma once

#include "csharp_common.h"
#include "csharp_enums.h"
#include "csharp_structs.h"


struct MGM_Song;
struct MGM_Video;

MG_EXPORT MGM_Song* MGM_Song_Create(const char* mediaFilePath);
MG_EXPORT void MGM_Song_Destroy(MGM_Song* song);
MG_EXPORT mgulong MGM_Song_GetDuration(MGM_Song* song);
MG_EXPORT mgulong MGM_Song_GetPosition(MGM_Song* song);
MG_EXPORT mgfloat MGM_Song_GetVolume(MGM_Song* song);
MG_EXPORT void MGM_Song_SetVolume(MGM_Song* song, mgfloat volume);
MG_EXPORT void MGM_Song_Play(MGM_Song* song, mgulong startPositionMs, void (*callback)(void*), void* callbackData);
MG_EXPORT void MGM_Song_Pause(MGM_Song* song);
MG_EXPORT void MGM_Song_Resume(MGM_Song* song);
MG_EXPORT void MGM_Song_Stop(MGM_Song* song);
MG_EXPORT MGM_Video* MGM_Video_Create(const char* mediaFilePath, mgint cachedFrameNum, mgint& width, mgint& height, mgfloat& fps, mgulong& duration);
MG_EXPORT void MGM_Video_Destroy(MGM_Video* video);
MG_EXPORT MGMediaState MGM_Video_GetState(MGM_Video* video);
MG_EXPORT mgulong MGM_Video_GetPosition(MGM_Video* video);
MG_EXPORT void MGM_Video_SetVolume(MGM_Video* video, mgfloat volume);
MG_EXPORT void MGM_Video_SetLooped(MGM_Video* video, mgbool looped);
MG_EXPORT void MGM_Video_Play(MGM_Video* video);
MG_EXPORT void MGM_Video_Pause(MGM_Video* video);
MG_EXPORT void MGM_Video_Resume(MGM_Video* video);
MG_EXPORT void MGM_Video_Stop(MGM_Video* video);
MG_EXPORT void MGM_Video_GetFrame(MGM_Video* video, mguint& frame, MGG_Texture*& handle);
