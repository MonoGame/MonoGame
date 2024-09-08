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


struct MGA_System;
struct MGA_Buffer;
struct MGA_Voice;

MG_EXPORT MGA_System* MGA_System_Create();
MG_EXPORT void MGA_System_Destroy(MGA_System* system);
MG_EXPORT mgint MGA_System_GetMaxInstances();
MG_EXPORT MGA_Buffer* MGA_Buffer_Create();
MG_EXPORT void MGA_Buffer_Destroy(MGA_Buffer* buffer);
MG_EXPORT void MGA_Buffer_InitializeFormat(MGA_Buffer* buffer, mgbyte* waveHeader, mgbyte* waveData, mgint length, mgint loopStart, mgint loopLength);
MG_EXPORT void MGA_Buffer_InitializePCM(MGA_Buffer* buffer, mgbyte* waveData, mgint offset, mgint length, mgint sampleBits, mgint sampleRate, mgint channels, mgint loopStart, mgint loopLength);
MG_EXPORT void MGA_Buffer_InitializeXact(MGA_Buffer* buffer, mguint codec, mgbyte* waveData, mgint length, mgint sampleRate, mgint blockAlignment, mgint channels, mgint loopStart, mgint loopLength);
MG_EXPORT mgulong MGA_Buffer_GetDuration(MGA_Buffer* buffer);
MG_EXPORT MGA_Voice* MGA_Voice_Create();
MG_EXPORT void MGA_Voice_Destroy(MGA_Voice* voice);
MG_EXPORT void MGA_Voice_AppendBuffer(MGA_Voice* voice, MGA_Buffer* buffer, mgbool clear);
MG_EXPORT void MGA_Voice_Play(MGA_Voice* voice, mgbool looped);
MG_EXPORT void MGA_Voice_Pause(MGA_Voice* voice);
MG_EXPORT void MGA_Voice_Resume(MGA_Voice* voice);
MG_EXPORT void MGA_Voice_Stop(MGA_Voice* voice, mgbool immediate);
MG_EXPORT MGSoundState MGA_Voice_GetState(MGA_Voice* voice);
MG_EXPORT void MGA_Voice_SetPan(MGA_Voice* voice, mgfloat pan);
MG_EXPORT void MGA_Voice_SetPitch(MGA_Voice* voice, mgfloat pitch);
MG_EXPORT void MGA_Voice_SetVolume(MGA_Voice* voice, mgfloat volume);
