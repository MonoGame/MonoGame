// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "api_MGP.h"

void MGP_Web_OnPlatformDestroyed(MGP_Platform* platform);
void MGP_Web_OnWindowCreated(MGP_Window* window);
void MGP_Web_OnNativeWindowDestroyed(MGP_Window* window);
void MGP_Web_OnWindowDestroyed(MGP_Window* window);
mgbyte MGP_Web_IsPointerLockEnabled();
mgbyte MGP_Web_IsBrowserCanvasResizeManaged();
mgbyte MGP_Web_IsDuplicateBrowserResize(MGP_Window* window, mgint width, mgint height);
mgbyte MGP_Web_GetBrowserFullscreen(MGP_Window* window);
void MGP_Web_RequestBrowserFullscreen(MGP_Window* window, mgbyte fullscreen);
mgint MGP_Web_GetMaximumTouchCount();
mgbyte MGP_Web_Accelerometer_IsSupported();
mgint MGP_Web_Accelerometer_Start();
void MGP_Web_Accelerometer_Stop();
mgint MGP_Web_Accelerometer_GetState();
mgbyte MGP_Web_Accelerometer_GetReading(mgfloat& x, mgfloat& y, mgfloat& z, mgint& sequence);
