// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGG.h"
#include <unordered_map>
#include <string>
#include <iterator>

static std::unordered_map<mgint, std::string> s_strings;
static std::unordered_map<mgint, mgint>       s_ints;

struct MGConfigEntry {
    mgint value;
    const char* key;
};

static const MGConfigEntry s_configKeyEntries[] = {
#define MG_CONFIG_DEFINE_ENUM_VALUE(name, val) { static_cast<mgint>(MGConfigKey::name), #name },
    MG_CONFIG_KEYS(MG_CONFIG_DEFINE_ENUM_VALUE)
#undef MG_CONFIG_DEFINE_ENUM_VALUE
};

MG_EXPORT mgint MGG_Config_GetKeyCount()
{
    return static_cast<mgint>(std::size(s_configKeyEntries));
}

MG_EXPORT void MGG_Config_GetKeyDetails(mgint index, const char** configKey, mgint* configValue)
{
    if (index >= 0 && index < MGG_Config_GetKeyCount())
    {
        if (configKey != nullptr)
        {
            *configKey = s_configKeyEntries[index].key;
        }
        if (configValue != nullptr)
        {
            *configValue = s_configKeyEntries[index].value;
        }
    }
}

#pragma region Defaults

mgint MGG_Config_GetDefaultInt(MGConfigKey key)
{
    switch (key)
    {
	    case MGConfigKey::VulkanUniformRingbufferSize:
	        return 32 * 1024 * 1024; // 32 MiB (matches current hard-coded value)
	    case MGConfigKey::VulkanDescriptorPoolSize:
	        return 16384;            // matches current DefaultPoolSize
	    case MGConfigKey::Dx12PreferredBlockSize:
	        return 0;                // 0 = keep D3D12MA default (64 MiB)
	    case MGConfigKey::Dx12MaxUploadBufferPoolSize:
	        return 32;               // matches current MAX_BUFFER_POOL_SIZE
	    default:
	        return 0;
    }
}

const char* MGG_Config_GetDefaultString(MGConfigKey key)
{
    switch (key)
    {
	    case MGConfigKey::VulkanInstanceExtensions:
	    case MGConfigKey::VulkanDeviceExtensions:
	    default:
	        return nullptr;
    }
}

#pragma endregion Defaults

#pragma region Public Setters

MG_EXPORT void MGG_Config_SetString(MGConfigKey key, const char* value)
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

MG_EXPORT void MGG_Config_SetInt(MGConfigKey key, mgint value)
{
    s_ints[static_cast<mgint>(key)] = value;
}

#pragma endregion Public Setters

#pragma region Internal Getters

const char* MGG_Config_GetString(MGConfigKey key)
{
    auto it = s_strings.find(static_cast<mgint>(key));
    if (it != s_strings.end())
    {
        return it->second.c_str();
    }
    return MGG_Config_GetDefaultString(key);
}

mgint MGG_Config_GetInt(MGConfigKey key)
{
    auto it = s_ints.find(static_cast<mgint>(key));
    if (it != s_ints.end())
    {
        return it->second;
    }
    return MGG_Config_GetDefaultInt(key);
}

#pragma endregion Internal Getters
