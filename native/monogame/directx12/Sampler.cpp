// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "directx12.h"

#include "Sampler.h"
#include "DeviceResources.h"
#include "GraphicsEnums.h"

using namespace DirectX;
using namespace DX;
using namespace Graphics;

Sampler::Sampler(DeviceResources* device, MGTextureFilter filter, MGTextureAddressMode u, MGTextureAddressMode v, MGTextureAddressMode w) {
    impl = new InternalData();

    D3D12_SAMPLER_DESC descSampler = {
        TextureFilterToD3D12_FILTER[(int)filter],
        TextureAddressModeToD3D12_TEXTURE_ADDRESS_MODE[(int)u],
        TextureAddressModeToD3D12_TEXTURE_ADDRESS_MODE[(int)v],
        TextureAddressModeToD3D12_TEXTURE_ADDRESS_MODE[(int)w],
        0.0f,                           // FLOAT MipLODBias;
        1U,                             // UINT MaxAnisotropy;
        D3D12_COMPARISON_FUNC_ALWAYS,   // D3D12_COMPARISON_FUNC ComparisonFunc;
        { 0.f, 0.f, 0.f, 0.f },         // FLOAT BorderColor[4];
        0.0f,                           // FLOAT MinLOD;
        D3D12_FLOAT32_MAX,              // FLOAT MaxLOD;
    };
    impl->m_handle = device->GetGraphicsHeaps()->CreateSamplerHandle();
    device->GetD3DDevice()->CreateSampler(&descSampler, impl->m_handle);
}

Sampler::~Sampler() {
    delete impl;
}
