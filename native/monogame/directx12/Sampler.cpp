// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "directx12.h"

#include "Sampler.h"
#include "DeviceResources.h"
#include "GraphicsEnums.h"
#include <api_MGG.h>

using namespace DirectX;
using namespace DX;
using namespace Graphics;

Sampler::Sampler(DeviceResources* device, MGG_SamplerState_Info* info)
{
    impl = new InternalData();

    D3D12_FILTER filter;
    if (info->FilterMode == MGTextureFilterMode::Comparison)
        filter = TextureFilterToComparisonD3D12_FILTER[(int)info->Filter];
    else
        filter = TextureFilterToD3D12_FILTER[(int)info->Filter];

    D3D12_SAMPLER_DESC descSampler = {
        filter,
        TextureAddressModeToD3D12_TEXTURE_ADDRESS_MODE[(int)info->AddressU],
        TextureAddressModeToD3D12_TEXTURE_ADDRESS_MODE[(int)info->AddressV],
        TextureAddressModeToD3D12_TEXTURE_ADDRESS_MODE[(int)info->AddressW],
        info->MipMapLevelOfDetailBias,
        info->MaximumAnisotropy,
        CompareFunctionToD3D12_COMPARISON_FUNC[(int)info->ComparisonFunction],
        {
            ((info->BorderColor >> 0) & 0xFF) / 255.0f,
            ((info->BorderColor >> 8) & 0xFF) / 255.0f,
            ((info->BorderColor >> 16) & 0xFF) / 255.0f,
            ((info->BorderColor >> 24) & 0xFF) / 255.0f,
        },
        info->MaxMipLevel,
        D3D12_FLOAT32_MAX
    };
    impl->m_handle = device->GetGraphicsHeaps()->CreateSamplerHandle();
    device->GetD3DDevice()->CreateSampler(&descSampler, impl->m_handle);
}

Sampler::~Sampler() {
    delete impl;
}
