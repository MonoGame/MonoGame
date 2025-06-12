// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
                
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes

#pragma once

#include "api_common.h"
#include "api_enums.h"
#include "api_structs.h"


struct MGP_Platform;
struct MGP_Window;
struct MGP_Cursor;

MG_EXPORT MGP_Platform* MGP_Platform_Create(MGGameRunBehavior& behavior);
MG_EXPORT void MGP_Platform_Destroy(MGP_Platform* platform);
MG_EXPORT void MGP_Platform_BeforeInitialize(MGP_Platform* platform);
MG_EXPORT mgbool MGP_Platform_PollEvent(MGP_Platform* platform, MGP_Event& event_);
MG_EXPORT void MGP_Platform_StartRunLoop(MGP_Platform* platform);
MG_EXPORT mgbool MGP_Platform_BeforeRun(MGP_Platform* platform);
MG_EXPORT mgbool MGP_Platform_BeforeUpdate(MGP_Platform* platform);
MG_EXPORT mgbool MGP_Platform_BeforeDraw(MGP_Platform* platform);
MG_EXPORT const char* MGP_Platform_MakePath(const char* location, const char* path);
MG_EXPORT MGMonoGamePlatform MGP_Platform_GetPlatform();
MG_EXPORT MGGraphicsBackend MGP_Platform_GetGraphicsBackend();
MG_EXPORT MGP_Window* MGP_Window_Create(MGP_Platform* platform, mgint& width, mgint& height, const char* title);
MG_EXPORT void MGP_Window_Destroy(MGP_Window* window);
MG_EXPORT void MGP_Window_SetIconBitmap(MGP_Window* window, mgbyte* icon, mgint length);
MG_EXPORT void* MGP_Window_GetNativeHandle(MGP_Window* window);
MG_EXPORT mgbool MGP_Window_GetAllowUserResizing(MGP_Window* window);
MG_EXPORT void MGP_Window_SetAllowUserResizing(MGP_Window* window, mgbool allow);
MG_EXPORT mgbool MGP_Window_GetIsBorderless(MGP_Window* window);
MG_EXPORT void MGP_Window_SetIsBorderless(MGP_Window* window, mgbool borderless);
MG_EXPORT void MGP_Window_SetTitle(MGP_Window* window, const char* title);
MG_EXPORT void MGP_Window_Show(MGP_Window* window, mgbool show);
MG_EXPORT void MGP_Window_GetPosition(MGP_Window* window, mgint& x, mgint& y);
MG_EXPORT void MGP_Window_SetPosition(MGP_Window* window, mgint x, mgint y);
MG_EXPORT void MGP_Window_SetClientSize(MGP_Window* window, mgint width, mgint height);
MG_EXPORT void MGP_Window_SetCursor(MGP_Window* window, MGP_Cursor* cursor);
MG_EXPORT mgint MGP_Window_ShowMessageBox(MGP_Window* window, const char* title, const char* description, const char** buttons, mgint count);
MG_EXPORT void MGP_Window_EnterFullScreen(MGP_Window* window, mgbool useHardwareModeSwitch);
MG_EXPORT void MGP_Window_ExitFullScreen(MGP_Window* window);
MG_EXPORT void MGP_Mouse_SetVisible(MGP_Platform* platform, mgbool visible);
MG_EXPORT void MGP_Mouse_WarpPosition(MGP_Window* window, mgint x, mgint y);
MG_EXPORT MGP_Cursor* MGP_Cursor_Create(MGSystemCursor cursor);
MG_EXPORT MGP_Cursor* MGP_Cursor_CreateCustom(mgbyte* rgba, mgint width, mgint height, mgint originx, mgint originy);
MG_EXPORT void MGP_Cursor_Destroy(MGP_Cursor* cursor);
MG_EXPORT mgint MGP_GamePad_GetMaxSupported();
MG_EXPORT void MGP_GamePad_GetCaps(MGP_Platform* platform, mgint identifer, MGP_ControllerCaps* caps);
MG_EXPORT mgbool MGP_GamePad_SetVibration(MGP_Platform* platform, mgint identifer, mgfloat leftMotor, mgfloat rightMotor, mgfloat leftTrigger, mgfloat rightTrigger);
