// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "shader_profiles.h"

#include "std_helpers.h"


ShaderProfileManager::ShaderProfileManager(ShaderProfile* profile)
{
	profiles().push_back(profile);
}

std::vector<ShaderProfile*>& ShaderProfileManager::profiles()
{
	static std::vector<ShaderProfile*> profiles;
	return profiles;
}

ShaderProfile* ShaderProfileManager::Get(const char* profileOrPlatform)
{
	for (ShaderProfile* profile : profiles())
	{
		if (profile->Supports(profileOrPlatform))
			return profile;
	}

	return nullptr;
}
