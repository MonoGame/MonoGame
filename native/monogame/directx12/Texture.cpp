// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "directx12.h"

#include "DeviceResources.h"
#include "GraphicsEnums.h"
#include "Texture.h"

#include "api_enums.h"


using namespace DirectX;
using namespace DX;
using namespace Graphics;
using namespace Microsoft::WRL;

Texture::Texture(SurfaceType type, TextureDimension dimension, int width, int height, int mipLevels, MGSurfaceFormat format) {
    mipLevels = std::max(mipLevels, 1);

    impl = new InternalData();
    impl->m_type = type;
    impl->m_depthFormat = MGDepthFormat::None;
    impl->m_dimension = dimension;
    impl->m_levels = mipLevels;

    D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAG_NONE;
    switch (type) {
    case SurfaceType::Texture:
        impl->m_currentState = D3D12_RESOURCE_STATE_COPY_DEST;
        break;
    case SurfaceType::RenderTarget:
        impl->m_currentState = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE | D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;
        flags |= D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;
        break;
    case SurfaceType::SwapChainRenderTarget:
        impl->m_currentState = D3D12_RESOURCE_STATE_PRESENT;
        flags |= D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;
        break;
    }

    switch (dimension) {
    case TextureDimension::Texture2D:
        impl->m_desc = CD3DX12_RESOURCE_DESC::Tex2D(TextureFormatToDXGI_FORMAT(format), width, height, 1, mipLevels, 1, 0, flags);
        break;
    case TextureDimension::Texture3D:
        // todo
        break;
    case TextureDimension::TextureCube:
        impl->m_desc = CD3DX12_RESOURCE_DESC::Tex2D(TextureFormatToDXGI_FORMAT(format), width, height, 6, mipLevels, 1, 0, flags);
        break;
    }
}

Texture::Texture(int width, int height, MGDepthFormat format) {
    impl = new InternalData();
    impl->m_type = SurfaceType::RenderTarget;
    impl->m_depthFormat = format;
    impl->m_levels = 1;
    impl->m_dimension = TextureDimension::Texture2D;
    impl->m_currentState = D3D12_RESOURCE_STATE_DEPTH_WRITE;
    impl->m_desc = CD3DX12_RESOURCE_DESC::Tex2D(DepthFormatToDXGI_FORMAT[(int)format], width, height, 1, 1, 1, 0, D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL);
}

Texture::Texture(const Texture& other) {
    impl = new InternalData();
    impl->m_type = SurfaceType::Texture;
    impl->m_dimension = other.impl->m_dimension;
    impl->m_depthFormat = other.impl->m_depthFormat;
    impl->m_desc = other.impl->m_desc;
    impl->m_levels = other.impl->m_levels;
    impl->m_currentState = D3D12_RESOURCE_STATE_COPY_DEST;
}

#ifndef _GAMING_XBOX
Texture::Texture(DeviceResources* device, IDXGISwapChain3* swapchain, int bufferId) {
    impl = new InternalData();
    impl->m_type = SurfaceType::SwapChainRenderTarget;
    impl->m_dimension = TextureDimension::Texture2D;
    impl->m_currentState = D3D12_RESOURCE_STATE_PRESENT;
    impl->m_depthFormat = MGDepthFormat::None;
    impl->m_levels = 1;

    ThrowIfFailed(swapchain->GetBuffer(bufferId, IID_PPV_ARGS(impl->m_res.ReleaseAndGetAddressOf())));

    impl->m_desc = impl->m_res->GetDesc();
    D3D12_RENDER_TARGET_VIEW_DESC rtvDesc = {};
    rtvDesc.Format = impl->m_desc.Format;
    rtvDesc.ViewDimension = D3D12_RTV_DIMENSION_TEXTURE2D;

    impl->m_rtvHandle = device->GetGraphicsHeaps()->CreateRTVHandle(impl->m_res.Get(), rtvDesc);
}
#endif

Texture::~Texture() {
    delete impl;
}

void Texture::Create(DeviceResources* device, bool createViews) {
    CD3DX12_CLEAR_VALUE optimizedClearValue;
    optimizedClearValue.Format = impl->m_desc.Format;
    
    if (impl->m_depthFormat != MGDepthFormat::None) {
        optimizedClearValue.DepthStencil.Depth = 1.0f;
        optimizedClearValue.DepthStencil.Stencil = 0u;
    } else {
        memcpy(optimizedClearValue.Color, impl->m_clearColor, sizeof(FLOAT) * 4);
    }

    const CD3DX12_CLEAR_VALUE* pClearValue = nullptr;
    if (impl->m_type != SurfaceType::Texture) // only RT could be cleared
        pClearValue = &optimizedClearValue;

    D3D12_HEAP_FLAGS heapFlags = D3D12_HEAP_FLAG_NONE;
    // In this context SurfaceType::SwapChainRenderTarget mean the displayable RT used on Gaming.Xbox rendering
    // Not a resource managed by IDXGISwapChain3 (Desktop only, cf Texture(DeviceResources*, IDXGISwapChain3*, int))
    if (impl->m_type == SurfaceType::SwapChainRenderTarget)
        heapFlags |= D3D12_HEAP_FLAG_ALLOW_DISPLAY;

    bool isMSAA = CheckMSAA(device->GetD3DDevice());
    if (!isMSAA) impl->m_desc.SampleDesc.Count = 1;

    D3D12_RESOURCE_DESC resDesc = impl->m_desc;
    if (impl->m_allowUAV)
        resDesc.Format = ConvertSRVtoResourceFormat(impl->m_desc.Format);

    D3D12MA::ALLOCATION_DESC allocDesc = { D3D12MA::ALLOCATION_FLAG_COMMITTED, D3D12_HEAP_TYPE_DEFAULT, heapFlags };
    device->GetAllocator()->CreateResource(
        &allocDesc,
        &resDesc,
        impl->m_currentState,
        pClearValue,
        impl->m_alloc.ReleaseAndGetAddressOf(),
        IID_GRAPHICS_PPV_ARGS(impl->m_res.ReleaseAndGetAddressOf()));

    switch (impl->m_type) {
    case SurfaceType::Texture:
        impl->m_res->SetName(L"Unnamed Texture");
        break;
    case SurfaceType::RenderTarget:
        impl->m_res->SetName(L"Unnamed RenderTarget");
        break;
    case SurfaceType::SwapChainRenderTarget:
        impl->m_res->SetName(L"Unnamed SwapChainRenderTarget");
        break;
    }

    if (!createViews) return;

    if (impl->m_type != SurfaceType::SwapChainRenderTarget) { // We shouldn't bind a SRV for the display RT
        D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
        if (impl->m_depthFormat != MGDepthFormat::None)
            srvDesc.Format = DepthFormatToSRV[(int)impl->m_depthFormat];
        else
            srvDesc.Format = impl->m_desc.Format;
        srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
        switch (impl->m_dimension) {
        case TextureDimension::Texture2D:
            srvDesc.ViewDimension = isMSAA ? D3D12_SRV_DIMENSION_TEXTURE2DMS : D3D12_SRV_DIMENSION_TEXTURE2D;
            srvDesc.Texture2D.MostDetailedMip = 0;
            srvDesc.Texture2D.MipLevels = impl->m_levels;
            srvDesc.Texture2D.PlaneSlice = 0;
            break;
        case TextureDimension::Texture3D:
            srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE3D;
            break;
        case TextureDimension::TextureCube:
            srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURECUBE;
            srvDesc.TextureCube.MostDetailedMip = 0;
            srvDesc.TextureCube.MipLevels = impl->m_levels;
            break;
        }
        impl->m_srvHandle = device->GetGraphicsHeaps()->CreateSRVHandle(impl->m_res.Get(), srvDesc);
    }

    if (impl->m_allowUAV) {
        for (uint16_t mip = 0; mip < impl->m_levels; ++mip) {
            D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = {};
            uavDesc.Format = GetFormat();
            uavDesc.ViewDimension = D3D12_UAV_DIMENSION_TEXTURE2D;
            uavDesc.Texture2D.MipSlice = mip;
            impl->m_uavHandles.push_back(device->GetGraphicsHeaps()->CreateUAVHandle(impl->m_res.Get(), uavDesc));
        }
    }

    if (impl->m_type != SurfaceType::Texture) { // This is a RT so we need a RTV or a DSV as well
        if (impl->m_depthFormat != MGDepthFormat::None) {
            D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
            dsvDesc.Format = impl->m_desc.Format;
            dsvDesc.ViewDimension = isMSAA ? D3D12_DSV_DIMENSION_TEXTURE2DMS: D3D12_DSV_DIMENSION_TEXTURE2D;
            impl->m_dsvHandle = device->GetGraphicsHeaps()->CreateDSVHandle(impl->m_res.Get(), dsvDesc);
        } else {
            D3D12_RENDER_TARGET_VIEW_DESC rtvDesc = {};
            rtvDesc.Format = impl->m_desc.Format;
            rtvDesc.ViewDimension = isMSAA ? D3D12_RTV_DIMENSION_TEXTURE2DMS : D3D12_RTV_DIMENSION_TEXTURE2D;
            impl->m_rtvHandle = device->GetGraphicsHeaps()->CreateRTVHandle(impl->m_res.Get(), rtvDesc);
        }
    }
}

void Texture::FreeDescriptors(DeviceResources* device) {
    if (impl->m_srvHandle.ptr) device->GetGraphicsHeaps()->FreeSRVUAVHandle(impl->m_srvHandle);
    impl->m_srvHandle = {};

    for (auto hdl : impl->m_uavHandles) device->GetGraphicsHeaps()->FreeSRVUAVHandle(hdl);
    impl->m_uavHandles.clear();

    if (impl->m_rtvHandle.ptr) device->GetGraphicsHeaps()->FreeRTVHandle(impl->m_rtvHandle);
    impl->m_rtvHandle = {};

    if (impl->m_dsvHandle.ptr) device->GetGraphicsHeaps()->FreeDSVHandle(impl->m_dsvHandle);
    impl->m_dsvHandle = {};
}

void Texture::SetClearColor(float r, float g, float b, float a) {
    impl->m_clearColor[0] = r;
    impl->m_clearColor[1] = g;
    impl->m_clearColor[2] = b;
    impl->m_clearColor[3] = a;
}

void Texture::SetMSAA(int sampleCount) {
    impl->m_desc.SampleDesc.Count = sampleCount;
}

void Texture::SetData(DeviceResources* device, uint32_t subResId, uint8_t* data, size_t size, size_t rowPitch) {
    ComPtr<ID3D12Resource> uploadBuffer;
    ComPtr<D3D12MA::Allocation> uploadAlloc;
    const UINT64 uploadSize = GetRequiredIntermediateSize(impl->m_res.Get(), subResId, 1);
    CD3DX12_RESOURCE_DESC resourceDesc = CD3DX12_RESOURCE_DESC::Buffer(uploadSize);
    D3D12MA::ALLOCATION_DESC allocDesc = { D3D12MA::ALLOCATION_FLAG_COMMITTED, D3D12_HEAP_TYPE_UPLOAD };
    device->GetAllocator()->CreateResource(
        &allocDesc, &resourceDesc,
        D3D12_RESOURCE_STATE_GENERIC_READ, nullptr,
        uploadAlloc.ReleaseAndGetAddressOf(), IID_GRAPHICS_PPV_ARGS(uploadBuffer.ReleaseAndGetAddressOf()));

    auto cmd = device->BeginCommandList();
    auto cmdList = cmd->Get();

    if (impl->m_currentState != D3D12_RESOURCE_STATE_COPY_DEST) {
        const D3D12_RESOURCE_BARRIER copyDestBarrier = CD3DX12_RESOURCE_BARRIER::Transition(
            impl->m_res.Get(), impl->m_currentState, D3D12_RESOURCE_STATE_COPY_DEST
        );
        cmdList->ResourceBarrier(1, &copyDestBarrier);
    }

    D3D12_SUBRESOURCE_DATA initData = { data, rowPitch, 0 };
    UpdateSubresources(cmdList, impl->m_res.Get(), uploadBuffer.Get(), 0, subResId, 1, &initData);

    impl->m_currentState = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE | D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;
    const D3D12_RESOURCE_BARRIER destToShaderResBarrier = CD3DX12_RESOURCE_BARRIER::Transition(
        impl->m_res.Get(), D3D12_RESOURCE_STATE_COPY_DEST, impl->m_currentState
    );
    cmdList->ResourceBarrier(1, &destToShaderResBarrier);

    cmd->Close(true);
}

// Two intermediate textures are used in this version, this could probably be improved
void Graphics::Texture::SetData(DeviceResources* device, uint32_t subResId, uint32_t x, uint32_t y, uint32_t w, uint32_t h, uint8_t* data, size_t size, size_t rowPitch) {
    Texture* intermediateTexture = new Texture(SurfaceType::Texture, TextureDimension::Texture2D, w, h, 1, MGSurfaceFormat::Color);
    intermediateTexture->Create(device, false);
    intermediateTexture->SetData(device, 0, data, size, rowPitch);

    auto cmd = device->BeginCommandList();
    auto cmdList = cmd->Get();

    intermediateTexture->TransitionBatched(D3D12_RESOURCE_STATE_COPY_SOURCE);
    if (impl->m_currentState != D3D12_RESOURCE_STATE_COPY_DEST)
        s_batchedBarriers.push_back(CD3DX12_RESOURCE_BARRIER::Transition(
            impl->m_res.Get(), impl->m_currentState, D3D12_RESOURCE_STATE_COPY_DEST
        ));
    Texture::SendTransitionBatch(cmdList);

    const CD3DX12_TEXTURE_COPY_LOCATION src(intermediateTexture->impl->m_res.Get(), 0);
    const CD3DX12_TEXTURE_COPY_LOCATION dst(impl->m_res.Get(), subResId);
    cmdList->CopyTextureRegion(&dst, x, y, 0, &src, nullptr);

    impl->m_currentState = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE | D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;
    const D3D12_RESOURCE_BARRIER toShaderResBarrier = CD3DX12_RESOURCE_BARRIER::Transition(
        impl->m_res.Get(), D3D12_RESOURCE_STATE_COPY_DEST, impl->m_currentState
    );
    cmdList->ResourceBarrier(1, &toShaderResBarrier);

    cmd->Close(true);

    delete intermediateTexture;
}

#ifdef _GAMING_XBOX
#define TEXTURE_DATA_PITCH_ALIGNMENT D3D12XBOX_TEXTURE_DATA_PITCH_ALIGNMENT
#else
#define TEXTURE_DATA_PITCH_ALIGNMENT D3D12_TEXTURE_DATA_PITCH_ALIGNMENT
#endif

void Graphics::Texture::GetData(DeviceResources* device, uint32_t subResId, uint32_t x, uint32_t y, uint32_t w, uint32_t h, uint8_t* data, size_t srcStride, size_t destStride) {
    D3D12_RESOURCE_DESC copyDesc = CD3DX12_RESOURCE_DESC::Tex2D(impl->m_desc.Format, w, h);
    UINT64 readbackBufferSize = 0;
    UINT64 fpRowPitch = 0;
    device->GetD3DDevice()->GetCopyableFootprints(&copyDesc, 0, 1, 0, nullptr, nullptr, &fpRowPitch, &readbackBufferSize);
    const UINT64 dstRowPitch = (fpRowPitch + static_cast<uint64_t>(TEXTURE_DATA_PITCH_ALIGNMENT) - 1u) & ~(static_cast<uint64_t>(TEXTURE_DATA_PITCH_ALIGNMENT) - 1u);

    Microsoft::WRL::ComPtr<ID3D12Resource> readbackBuffer;
    Microsoft::WRL::ComPtr<D3D12MA::Allocation> readbackAlloc;
    D3D12MA::ALLOCATION_DESC allocDesc = { D3D12MA::ALLOCATION_FLAG_COMMITTED, D3D12_HEAP_TYPE_READBACK };
    auto readbackBufferDesc = CD3DX12_RESOURCE_DESC::Buffer(readbackBufferSize);
    device->GetAllocator()->CreateResource(
        &allocDesc, &readbackBufferDesc,
        D3D12_RESOURCE_STATE_COPY_DEST, nullptr,
        readbackAlloc.ReleaseAndGetAddressOf(),
        IID_GRAPHICS_PPV_ARGS(readbackBuffer.ReleaseAndGetAddressOf()));

    auto cmd = device->BeginCommandList();
    auto cmdList = cmd->Get();

    if (impl->m_currentState != D3D12_RESOURCE_STATE_COPY_SOURCE) {
        const D3D12_RESOURCE_BARRIER toCopySourceBarrier = CD3DX12_RESOURCE_BARRIER::Transition(
            impl->m_res.Get(), impl->m_currentState, D3D12_RESOURCE_STATE_COPY_SOURCE
        );
        cmdList->ResourceBarrier(1, &toCopySourceBarrier);
    }

    D3D12_PLACED_SUBRESOURCE_FOOTPRINT bufferFootprint = {};
    bufferFootprint.Footprint.Width = static_cast<UINT>(copyDesc.Width);
    bufferFootprint.Footprint.Height = copyDesc.Height;
    bufferFootprint.Footprint.Depth = 1;
    bufferFootprint.Footprint.RowPitch = static_cast<UINT>(dstRowPitch);
    bufferFootprint.Footprint.Format = copyDesc.Format;

    const CD3DX12_TEXTURE_COPY_LOCATION copyDest(readbackBuffer.Get(), bufferFootprint);
    const CD3DX12_TEXTURE_COPY_LOCATION copySrc(impl->m_res.Get(), subResId);

    CD3DX12_BOX sourceRegion(x, y, x + w, y + h);
    cmdList->CopyTextureRegion(&copyDest, 0, 0, 0, &copySrc, &sourceRegion);

    const D3D12_RESOURCE_BARRIER revertBarrier = CD3DX12_RESOURCE_BARRIER::Transition(
        impl->m_res.Get(), D3D12_RESOURCE_STATE_COPY_SOURCE, impl->m_currentState
    );
    cmdList->ResourceBarrier(1, &revertBarrier);

    cmd->Close(true);

    // Phew, the copy is done we can map the resource and read it now

    D3D12_RANGE readbackBufferRange{ 0, readbackBufferSize };
    void* pReadbackBufferData{};
    ThrowIfFailed(readbackBuffer->Map(0, &readbackBufferRange, &pReadbackBufferData));

    uint8_t* current = data;
    uint8_t* currentRead = reinterpret_cast<uint8_t*>(pReadbackBufferData);
    for (size_t line = 0; line < copyDesc.Height; line++) {
        if (srcStride == destStride)
            memcpy(current, currentRead, destStride * w);
        else {
            for (auto i = 0; i < w; i++)
                memcpy(current + (i * destStride), (void*)(currentRead + (i * srcStride)), destStride);
        }

        currentRead += dstRowPitch;
        current += destStride * w;
    }

    CD3DX12_RANGE writeRange(0, 0); // we didnt write anything
    readbackBuffer->Unmap(0, &writeRange);
}

bool Texture::CheckMSAA(ID3D12Device* d3dDevice) {
    if (impl->m_desc.SampleDesc.Count <= 1) return false;

    D3D12_FEATURE_DATA_MULTISAMPLE_QUALITY_LEVELS msaaCheck {
        impl->m_desc.Format,                            // _In_  DXGI_FORMAT Format;
        impl->m_desc.SampleDesc.Count,                  // _In_  uint32_t SampleCount;
        D3D12_MULTISAMPLE_QUALITY_LEVELS_FLAG_NONE,     // _In_  D3D12_MULTISAMPLE_QUALITY_LEVELS_FLAG Flags;
        0u,												// _Out_  uint32_t NumQualityLevels;
    };
    HRESULT result = d3dDevice->CheckFeatureSupport(D3D12_FEATURE_MULTISAMPLE_QUALITY_LEVELS, &msaaCheck, sizeof(D3D12_FEATURE_DATA_MULTISAMPLE_QUALITY_LEVELS));
    return msaaCheck.NumQualityLevels > 0;
}

void Texture::AllowUAV() {
    impl->m_allowUAV = true;
    impl->m_desc.Flags |= D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
}

std::vector<D3D12_RESOURCE_BARRIER> Texture::s_batchedBarriers = {};

void Texture::TransitionBatched(D3D12_RESOURCE_STATES newState) {
    if (impl->m_currentState == newState)
        return;

    // We don't want the display texture to be a shader resource
    // It allow use to treat the display RT like any other RT in CommandContext and simplify the code
    if (impl->m_type == SurfaceType::SwapChainRenderTarget && newState == D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE)
        return;

    s_batchedBarriers.push_back(CD3DX12_RESOURCE_BARRIER::Transition(
        impl->m_res.Get(), impl->m_currentState, newState
    ));
    impl->m_currentState = newState;
}

void Texture::TransitionBatched(D3D12_RESOURCE_STATES oldState, D3D12_RESOURCE_STATES newState, UINT subresource) {
    s_batchedBarriers.push_back(CD3DX12_RESOURCE_BARRIER::Transition(
        impl->m_res.Get(), oldState, newState, subresource
    ));
}

void Texture::SendTransitionBatch(ID3D12GraphicsCommandList* commandList) {
    if (s_batchedBarriers.empty())
        return;
    commandList->ResourceBarrier(s_batchedBarriers.size(), s_batchedBarriers.data());
    s_batchedBarriers.clear();
}

void Texture::Transition(ID3D12GraphicsCommandList* commandList, D3D12_RESOURCE_STATES newState) {
    TransitionBatched(newState);
    SendTransitionBatch(commandList);
}
