// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "api_MGP.h"

void MGP_Web_OnPlatformDestroyed(MGP_Platform* platform);
mgint MGP_Web_GetMaximumTouchCount();
void MGP_Web_RequestFullscreen();
void MGP_Web_ExitFullscreen();
mgbyte MGP_Web_Accelerometer_IsSupported();
mgint MGP_Web_Accelerometer_Start();
void MGP_Web_Accelerometer_Stop();
mgint MGP_Web_Accelerometer_GetState();
mgbyte MGP_Web_Accelerometer_GetReading(mgfloat& x, mgfloat& y, mgfloat& z, mgint& sequence);
