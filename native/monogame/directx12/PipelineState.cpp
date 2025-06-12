// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "directx12.h"

#include "PipelineState.h"
#include "DeviceResources.h"
#include "GraphicsEnums.h"
#include "Texture.h"
#include "CommandContext.h"

using namespace DirectX;
using namespace DX;
using namespace Graphics;


PipelineStateManager::PipelineStateManager(DeviceResources* device) {
    impl = new InternalData();
    impl->m_deviceRes = device;

    impl->m_currentPSODesc = {};
    impl->m_currentPSODesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
    impl->m_currentPSODesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);
    impl->m_currentPSODesc.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC(D3D12_DEFAULT);
    impl->m_currentPSODesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
}

PipelineStateManager::~PipelineStateManager() {
    delete impl;
}

void PipelineStateManager::Reset() {
    impl->m_lastPSOHash = 0;
    impl->m_psoHashMap.clear();
}

void PipelineStateManager::SetDeviceParameters() {
    impl->m_deviceRes->GetCommandContext()->SetPSODeviceParameters(impl->m_currentPSODesc);
}

// Taken from Microsoft's MiniEngine https://github.com/microsoft/DirectX-Graphics-Samples/blob/master/MiniEngine/Core/Hash.h
#define ENABLE_SSE_CRC32 1
inline size_t HashRange(const uint32_t* const Begin, const uint32_t* const End, size_t Hash) {
#if ENABLE_SSE_CRC32
    const uint64_t* Iter64 = (const uint64_t*)AlignUp((uint64_t)Begin, 8);
    const uint64_t* const End64 = (const uint64_t* const)AlignDown((uint64_t)End, 8);

    // If not 64-bit aligned, start with a single u32
    if ((uint32_t*)Iter64 > Begin)
        Hash = _mm_crc32_u32((uint32_t)Hash, *Begin);

    // Iterate over consecutive u64 values
    while (Iter64 < End64)
        Hash = _mm_crc32_u64((uint64_t)Hash, *Iter64++);

    // If there is a 32-bit remainder, accumulate that
    if ((uint32_t*)Iter64 < End)
        Hash = _mm_crc32_u32((uint32_t)Hash, *(uint32_t*)Iter64);
#else
    // An inexpensive hash for CPUs lacking SSE4.2
    for (const uint32_t* Iter = Begin; Iter < End; ++Iter)
        Hash = 16777619U * Hash ^ *Iter;
#endif

    return Hash;
}

template <typename T> inline size_t HashState(const T* StateDesc, size_t Count = 1, size_t Hash = 2166136261U) {
    static_assert((sizeof(T) & 3) == 0 && alignof(T) >= 4, "State object is not word-aligned");
    return HashRange((uint32_t*)StateDesc, (uint32_t*)(StateDesc + Count), Hash);
}

// Taken from Microsoft's MiniEngine https://github.com/microsoft/DirectX-Graphics-Samples/blob/389246/MiniEngine/Core/PipelineState.cpp#L125
size_t Graphics::PipelineStateManager::GetPipelineHash() {
    size_t HashCode = HashState(&impl->m_currentPSODesc);
    HashCode = HashState(impl->m_inputElementDescs.data(), impl->m_currentPSODesc.InputLayout.NumElements, HashCode);
    return HashCode;
}

void PipelineStateManager::Prepare() {
    impl->m_lastPSOHash = 0;
}

void PipelineStateManager::ApplyCurrentPipelineState() {
    SetDeviceParameters();
    size_t HashCode = GetPipelineHash();

    if (HashCode != impl->m_lastPSOHash) {
        ID3D12PipelineState* ps = nullptr;
        auto iter = impl->m_psoHashMap.find(HashCode);
        if (iter != impl->m_psoHashMap.end())
            ps = iter->second.Get();
        else {
            DX::ThrowIfFailed(impl->m_deviceRes->GetD3DDevice()->CreateGraphicsPipelineState(&impl->m_currentPSODesc, IID_GRAPHICS_PPV_ARGS(&ps)));
            impl->m_psoHashMap[HashCode].Attach(ps);
        }

        impl->m_deviceRes->GetCommandContext()->SetPipelineState(ps);
        impl->m_lastPSOHash = HashCode;
    }
}
