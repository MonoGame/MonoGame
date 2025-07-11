// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once
#include <string.h>

#if defined(MG_DIRECTX12)
#define MG_BUILTIN_EFFECT_BYTES(name) ((mgbyte*)name##_dx12_mgfxo)
#elif defined(MG_VULKAN)
#define MG_BUILTIN_EFFECT_BYTES(name) ((mgbyte*)name##_vk_mgfxo)
#else
#error "Unsupported graphics backend, this header is intended for native builtin effects embedding only."
#endif

#define MG_BUILTIN_EFFECT_SIZE(name) (sizeof(MG_BUILTIN_EFFECT_BYTES(name)))

#define MG_HANDLE_BUILTIN_EFFECT(effectName, bytecode, size) \
    if (strcmp(name, #effectName) == 0) \
    { \
        (bytecode) = MG_BUILTIN_EFFECT_BYTES(effectName); \
        (size) = MG_BUILTIN_EFFECT_SIZE(effectName); \
    }

void MGG_EffectResource_GetBytecode(const char* name, mgbyte * &bytecode, mgint & size)
{
	MG_HANDLE_BUILTIN_EFFECT(AlphaTestEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(BasicEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(DualTextureEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(EnvironmentMapEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(SkinnedEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(SpriteEffect, bytecode, size)
}
