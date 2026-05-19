// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "GraphicsEnums.h"


namespace Graphics {
class DeviceResources;

class Texture {
public:
    Texture(SurfaceType type, TextureDimension dimension, int width, int height, int mipLevels, MGSurfaceFormat format);
    Texture(int width, int height, MGDepthFormat format);
    Texture(const Texture& other);
#ifndef _GAMING_XBOX
    Texture(DeviceResources* device, IDXGISwapChain3* swapchain, int bufferId);
#endif
    ~Texture();

    // CppSharp doesn't seem to recognize default parameters (even with HandleDefaultParamValuesPass...), this add an overload to bypass that
    inline void Create(DeviceResources* device) { Create(device, true); }
    void Create(DeviceResources* device, bool createViews);
    void FreeDescriptors(DeviceResources* device);

    void SetClearColor(float r, float g, float b, float a);
    void SetMSAA(int sampleCount);

    void SetData(DeviceResources* device, uint32_t subResId, uint8_t* data, size_t size, size_t rowPitch);
    void SetData(DeviceResources* device, uint32_t subResId, uint32_t x, uint32_t y, uint32_t w, uint32_t h, uint8_t* data, size_t size, size_t rowPitch);
    void GetData(DeviceResources* device, uint32_t subResId, uint32_t x, uint32_t y, uint32_t w, uint32_t h, uint8_t* data, size_t srcStride, size_t destStride);

    static std::vector<D3D12_RESOURCE_BARRIER> s_batchedBarriers;

    // store a transition barrier but don't send it immediately, allow sending multiple barrier at once easily
    void TransitionBatched(D3D12_RESOURCE_STATES newState);
    // be careful, this function doesn't store the current state (since it could be different for each subresource and will be a pain to track)
    // you should probably stick to TransitionBatched(newState) unless you know what you're doing
    void TransitionBatched(D3D12_RESOURCE_STATES oldState, D3D12_RESOURCE_STATES newState, UINT subresource);
    // send all the barriers waiting
    static void SendTransitionBatch(ID3D12GraphicsCommandList* commandList);

    // shorthand for TransitionBatched() + SendTransitionBatch(), used when you are sure you want to add only a single barrier
    void Transition(ID3D12GraphicsCommandList* commandList, D3D12_RESOURCE_STATES newState);

    bool CheckMSAA(ID3D12Device* device);
    void AllowUAV();

    const UINT16 GetMipLevels() const { return impl->m_desc.MipLevels; }
    const UINT64 GetWidth() const { return impl->m_desc.Width; }
    const UINT GetHeight() const { return impl->m_desc.Height; }
    const DXGI_SAMPLE_DESC& GetSampleDesc() const { return impl->m_desc.SampleDesc; }
    const DXGI_FORMAT& GetFormat() const { return impl->m_desc.Format; }

    const D3D12_CPU_DESCRIPTOR_HANDLE& GetSRV() const { return impl->m_srvHandle; }
    const D3D12_CPU_DESCRIPTOR_HANDLE& GetUAV(uint32_t mip) const { return impl->m_uavHandles[mip]; }
    const D3D12_CPU_DESCRIPTOR_HANDLE& GetRTV() const { return impl->m_rtvHandle; }
    const D3D12_CPU_DESCRIPTOR_HANDLE& GetDSV() const { return impl->m_dsvHandle; }

    ID3D12Resource* Get() const { return impl->m_res.Get(); }
    ID3D12Resource** GetAddressOf() const { return impl->m_res.GetAddressOf(); }

    struct InternalData {
        SurfaceType m_type;
        TextureDimension m_dimension;
        MGDepthFormat m_depthFormat;

        D3D12_RESOURCE_DESC m_desc;
        D3D12_RESOURCE_STATES m_currentState;
        int m_levels;
        FLOAT m_clearColor[4] = { 0.0f, 0.0f, 0.0f, 1.0f };
        bool m_allowUAV = false;

        Microsoft::WRL::ComPtr<D3D12MA::Allocation> m_alloc;
        Microsoft::WRL::ComPtr<ID3D12Resource> m_res;

        D3D12_CPU_DESCRIPTOR_HANDLE m_srvHandle = {};
        std::vector<D3D12_CPU_DESCRIPTOR_HANDLE> m_uavHandles;
        D3D12_CPU_DESCRIPTOR_HANDLE m_rtvHandle = {};
        D3D12_CPU_DESCRIPTOR_HANDLE m_dsvHandle = {};
    };
private:
    InternalData* impl;
};

}
