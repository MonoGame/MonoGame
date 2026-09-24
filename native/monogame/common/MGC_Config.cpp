// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGC.h"
#include "MGC_Config.h"
#include <unordered_map>
#include <string>

static std::unordered_map<mgint, std::string> s_strings;
static std::unordered_map<mgint, mgint>       s_ints;

// Defaults
mgint MGC_Config_GetDefaultInt(MGPlatformConfigKey key)
{
    switch (key)
    {
        case MGPlatformConfigKey::VulkanUniformRingbufferSize:
            return 32 * 1024 * 1024;    // 32 MiB (matches current hard-coded value)
        case MGPlatformConfigKey::VulkanDescriptorPoolSize:
            return 16384;   // Matches current DefaultPoolSize
        case MGPlatformConfigKey::Dx12PreferredBlockSize:
            return 0;   // 0 = keep D3D12MA default (64 MiB)
        case MGPlatformConfigKey::Dx12MaxUploadBufferPoolSize:
            return 32;  // Matches current MAX_BUFFER_POOL_SIZE
        default:
            return 0;
    }
}

const char* MGC_Config_GetDefaultString(MGPlatformConfigKey key)
{
    switch (key)
    {
        case MGPlatformConfigKey::VulkanInstanceExtensions:
        case MGPlatformConfigKey::VulkanDeviceExtensions:
        default:
            return nullptr;
    }
}

// Public Setters
MG_EXPORT void MGC_Config_SetInt(MGPlatformConfigKey key, mgint value)
{
    s_ints[static_cast<mgint>(key)] = value;
}

MG_EXPORT void MGC_Config_SetString(MGPlatformConfigKey key, const char* value)
{
    if (value != nullptr)
    {
        s_strings[static_cast<mgint>(key)] = value;
    }
    else
    {
        s_strings.erase(static_cast<mgint>(key));
    }
}

// Internal Getters
mgint MGC_Config_GetInt(MGPlatformConfigKey key)
{
    auto it = s_ints.find(static_cast<mgint>(key));
    if (it != s_ints.end())
    {
        return it->second;
    }
    return MGC_Config_GetDefaultInt(key);
}

const char* MGC_Config_GetString(MGPlatformConfigKey key)
{
    auto it = s_strings.find(static_cast<mgint>(key));
    if (it != s_strings.end())
    {
        return it->second.c_str();
    }
    return MGC_Config_GetDefaultString(key);
}
