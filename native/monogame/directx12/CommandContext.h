// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "GraphicsEnums.h"

#include "Heaps.h"
#include "CommandQueue.h"

enum class MGClearOptions;
enum class MGPrimitiveType;

namespace Graphics {
class Buffer;
class Texture;
class Sampler;
class PipelineStateManager;
class CommandContext;
class OcclusionQuery;
class DeviceResources;

class CommandContext {
public:
    DeviceResources* m_deviceRes;
    Heaps* m_heaps;

    CommandList* cmd = nullptr;
    ID3D12GraphicsCommandList* cmdList = nullptr;

    Microsoft::WRL::ComPtr<ID3D12RootSignature> m_rootSig;
    Microsoft::WRL::ComPtr<ID3D12RootSignature> m_generateMipRootSig;
    Microsoft::WRL::ComPtr<ID3D12PipelineState> m_generatePipelinePSO;
    ID3D12PipelineState* m_prevPSO = nullptr;
    ID3D12PipelineState* m_currentPSO = nullptr;

    size_t m_cbOffset = 0;
    Microsoft::WRL::ComPtr<D3D12MA::Allocation> m_cbAlloc[MAX_BACK_BUFFER_COUNT];
    Microsoft::WRL::ComPtr<ID3D12Resource> m_cbRes[MAX_BACK_BUFFER_COUNT];
    uint8_t* m_cbContent[MAX_BACK_BUFFER_COUNT];

    std::vector<Texture*> m_tempTextures[MAX_BACK_BUFFER_COUNT];

    std::vector<Texture*> m_currentRT;
    Texture* m_currentDepthStencil = nullptr;

    unsigned int m_backBufferIndex = 0;
public:
    CommandContext(DeviceResources* deviceResources); // CommandContext cannot be constructed from C#
    ~CommandContext();

    void Reset(unsigned int backBufferIndex);
    uint64_t Close();

    void SetRenderTarget(void* colorTargets, size_t numColorTargets, Texture* depthTarget);
    void ResolveResource(Texture* source, Texture* dest);
    void GenerateMipmap(Texture* source);
    void Clear(MGClearOptions options, float r, float g, float b, float a, float depth, int stencil);

    ID3D12GraphicsCommandList* GetCommandList() { return cmdList; }

    void SetPSODeviceParameters(D3D12_GRAPHICS_PIPELINE_STATE_DESC& desc);
    void SetPipelineState(ID3D12PipelineState* ps);

    void SetRenderTargets(UINT NumRTVs, const D3D12_CPU_DESCRIPTOR_HANDLE RTVs[]);
    void SetRenderTargets(UINT NumRTVs, const D3D12_CPU_DESCRIPTOR_HANDLE RTVs[], D3D12_CPU_DESCRIPTOR_HANDLE DSV);
    void SetRenderTarget(D3D12_CPU_DESCRIPTOR_HANDLE RTV) { SetRenderTargets(1, &RTV); }
    void SetRenderTarget(D3D12_CPU_DESCRIPTOR_HANDLE RTV, D3D12_CPU_DESCRIPTOR_HANDLE DSV) { SetRenderTargets(1, &RTV, DSV); }
    void SetDepthStencilTarget(D3D12_CPU_DESCRIPTOR_HANDLE DSV) { SetRenderTargets(0, nullptr, DSV); }

    void CreateDefaultRootSignature();
    void CreateGenerateMipPipelineResources();

};
}
