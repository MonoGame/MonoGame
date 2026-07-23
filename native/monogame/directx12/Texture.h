// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "GraphicsEnums.h"


namespace Graphics {
class DeviceResources;

class Texture {
public:
    Texture(SurfaceType type, TextureDimension dimension, int width, int height, int depth, int mipLevels, MGSurfaceFormat format);
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
    void SetData(DeviceResources* device, uint32_t subResId, uint32_t x, uint32_t y, uint32_t z, uint32_t w, uint32_t h, uint32_t d, uint8_t* data, size_t size, size_t rowPitch);
    void GetData(DeviceResources* device, uint32_t subResId, uint32_t x, uint32_t y, uint32_t z, uint32_t w, uint32_t h, uint32_t d, uint8_t* data, size_t dataSize);


    // store a transition barrier but don't send it immediately, allow sending multiple barrier at once easily
    void TransitionBatched(std::vector<D3D12_RESOURCE_BARRIER>& batch, D3D12_RESOURCE_STATES newState);
    // be careful, this function doesn't store the current state (since it could be different for each subresource and will be a pain to track)
    // you should probably stick to TransitionBatched(newState) unless you know what you're doing
    void TransitionBatched(std::vector<D3D12_RESOURCE_BARRIER>& batch, D3D12_RESOURCE_STATES oldState, D3D12_RESOURCE_STATES newState, UINT subresource);
    // send all the barriers waiting
    static void SendTransitionBatch(std::vector<D3D12_RESOURCE_BARRIER>& batch, ID3D12GraphicsCommandList* commandList);

    // shorthand for TransitionBatched() + SendTransitionBatch(), used when you are sure you want to add only a single barrier
    void Transition(std::vector<D3D12_RESOURCE_BARRIER>& batch, ID3D12GraphicsCommandList* commandList, D3D12_RESOURCE_STATES newState);

    bool CheckMSAA(ID3D12Device* device);
    void AllowUAV();

    UINT GetBlockSize() const
    {
        switch (impl->m_desc.Format)
        {
        case DXGI_FORMAT_BC1_TYPELESS:
        case DXGI_FORMAT_BC1_UNORM:
        case DXGI_FORMAT_BC1_UNORM_SRGB:
        case DXGI_FORMAT_BC2_TYPELESS:
        case DXGI_FORMAT_BC2_UNORM:
        case DXGI_FORMAT_BC2_UNORM_SRGB:
        case DXGI_FORMAT_BC3_TYPELESS:
        case DXGI_FORMAT_BC3_UNORM:
        case DXGI_FORMAT_BC3_UNORM_SRGB:
        case DXGI_FORMAT_BC4_TYPELESS:
        case DXGI_FORMAT_BC4_UNORM:
        case DXGI_FORMAT_BC4_SNORM:
        case DXGI_FORMAT_BC5_TYPELESS:
        case DXGI_FORMAT_BC5_UNORM:
        case DXGI_FORMAT_BC5_SNORM:
        case DXGI_FORMAT_BC6H_TYPELESS:
        case DXGI_FORMAT_BC6H_UF16:
        case DXGI_FORMAT_BC6H_SF16:
        case DXGI_FORMAT_BC7_TYPELESS:
        case DXGI_FORMAT_BC7_UNORM:
        case DXGI_FORMAT_BC7_UNORM_SRGB:
            return 4;
        }

        return 1;
    }

    UINT GetPixelSize(UINT level, UINT value) const
    {
        value = value >> level;
        if (value == 0)
            value = 1;
        UINT blockSize = GetBlockSize() - 1;
        value = (value + blockSize) & ~blockSize;
        return value;
    }

    const UINT16 GetMipLevels() const { return impl->m_desc.MipLevels; }
    const UINT GetWidth(UINT level = 0) const { return GetPixelSize(level, impl->m_desc.Width); }
    const UINT GetHeight(UINT level = 0) const { return GetPixelSize(level, impl->m_desc.Height); }
    const UINT GetDepthOrArraySize(UINT level = 0) const
    {
        return impl->m_dimension == TextureDimension::Texture2D ?
            impl->m_desc.DepthOrArraySize :
            GetPixelSize(level, impl->m_desc.DepthOrArraySize);
    }

    const DXGI_SAMPLE_DESC& GetSampleDesc() const { return impl->m_desc.SampleDesc; }
    const DXGI_FORMAT& GetFormat() const { return impl->m_desc.Format; }
    const bool IsRenderTarget() const { return impl->m_type == SurfaceType::RenderTarget; }

    const D3D12_CPU_DESCRIPTOR_HANDLE& GetSRV() const { return impl->m_srvHandle; }
    const D3D12_CPU_DESCRIPTOR_HANDLE& GetUAV(uint32_t mip) const { return impl->m_uavHandles[mip]; }
    const D3D12_CPU_DESCRIPTOR_HANDLE& GetRTV(size_t depthOrArray) const { return impl->m_rtvHandles[depthOrArray]; }
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
        std::vector<D3D12_CPU_DESCRIPTOR_HANDLE> m_rtvHandles;
        D3D12_CPU_DESCRIPTOR_HANDLE m_dsvHandle = {};
    };
private:
    InternalData* impl;
};

}
