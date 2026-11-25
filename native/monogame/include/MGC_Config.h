// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "api_common.h"
#include "api_enums.h"

// Internal config getters called by backend code (Vulkan, DX12).
// Exported setters are declared in the auto-generated api_MGC.h.
mgint MGC_Config_GetDefaultInt(MGPlatformConfigKey key);
const char* MGC_Config_GetDefaultString(MGPlatformConfigKey key);
mgint MGC_Config_GetInt(MGPlatformConfigKey key);
const char* MGC_Config_GetString(MGPlatformConfigKey key);
