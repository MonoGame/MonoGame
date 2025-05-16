// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "directx12.h"

#include "CommandContext.h"
#include "DeviceResources.h"
#include "GraphicsEnums.h"
#include "Texture.h"
#include "Sampler.h"
#include "Heaps.h"

#if defined(_GAMING_XBOX_SCARLETT)
#include "GenerateMips_Scarlett.h"
#elif defined(_GAMING_XBOX)
#include "GenerateMips_XboxOne.h"
#else
#include "GenerateMips_Desktop.h"
#endif

#include <api_enums.h>

using namespace Microsoft::WRL;
using namespace DirectX;
using namespace DX;
using namespace Graphics;

#pragma pack(push, 4)
struct MipGenerationConstantData {
    XMFLOAT2 InvOutTexelSize;
    uint32_t SrcMipIndex;
};
#pragma pack(pop)

CommandContext::CommandContext(DeviceResources* deviceResources) : m_deviceRes(deviceResources) {
    auto d3dDevice = deviceResources->GetD3DDevice();
    m_heaps = deviceResources->GetGraphicsHeaps();
    CreateDefaultRootSignature();
    CreateGenerateMipPipelineResources();

    D3D12MA::ALLOCATION_DESC cbUploadAllocDesc = { D3D12MA::ALLOCATION_FLAG_NONE, D3D12_HEAP_TYPE_UPLOAD };
    D3D12_RESOURCE_DESC cbUploadResourceDesc = CD3DX12_RESOURCE_DESC::Buffer(512 * D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT); // should be more than enough for CB
    for (size_t i = 0; i < MAX_BACK_BUFFER_COUNT; ++i) {
        deviceResources->GetAllocator()->CreateResource(
            &cbUploadAllocDesc,
            &cbUploadResourceDesc,
            D3D12_RESOURCE_STATE_GENERIC_READ,
            nullptr,
            &m_cbAlloc[i],
            IID_GRAPHICS_PPV_ARGS(m_cbRes[i].ReleaseAndGetAddressOf()));

        CD3DX12_RANGE readRange(0, 0); // We do not intend to read from this resource on the CPU.
        m_cbRes[i]->Map(0, &readRange, (void**)(&m_cbContent[i]));

        m_tempTextures[i] = {};
    }
}

CommandContext::~CommandContext() {
    for (size_t i = 0; i < MAX_BACK_BUFFER_COUNT; ++i) {
        for (auto t : m_tempTextures[i]) {
            t->FreeDescriptors(m_deviceRes);
            delete t;
        }
        m_tempTextures[i].clear();
    }
}

void CommandContext::Reset(unsigned int currentFrame) {
    m_backBufferIndex = currentFrame;

    for (auto t : m_tempTextures[m_backBufferIndex]) {
        t->FreeDescriptors(m_deviceRes);
        delete t;
    }
    m_tempTextures[m_backBufferIndex].clear();

    cmd = m_deviceRes->BeginCommandList();
    cmdList = cmd->Get();

    cmdList->SetGraphicsRootSignature(m_rootSig.Get());

    ID3D12DescriptorHeap* descriptorHeaps[] = {
        m_heaps->GetSRVShaderHeap(),
        m_heaps->GetSamplerShaderHeap()
    };
    cmdList->SetDescriptorHeaps(_countof(descriptorHeaps), descriptorHeaps);

    m_currentRT.clear();
    m_currentDepthStencil = nullptr;

    m_cbOffset = 0;
}

uint64_t CommandContext::Close() {
    uint64_t fence = cmd->Close();
    cmd = nullptr;
    cmdList = nullptr;
    return fence;
}

void CommandContext::SetRenderTarget(void* colorTargets, size_t numColorTargets, Texture* depthTarget) {
    // Only allow SetRenderTarget between Prepare and Present
    // This check should be replaced by an assert but Monogame's GraphicsDevice::Initialize() call ApplyRenderTargets(null)
    // This is here to avoid adding a platform specific define in the managed code and avoid crashing
    if (!cmdList) return;

    for (auto t : m_currentRT)
        t->TransitionBatched(D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);
    m_currentRT.clear();

    if (m_currentDepthStencil != depthTarget) {
        if (m_currentDepthStencil)
            m_currentDepthStencil->TransitionBatched(D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);
        if (depthTarget)
            depthTarget->TransitionBatched(D3D12_RESOURCE_STATE_DEPTH_WRITE);
    }

    if (colorTargets != nullptr) {
        D3D12_CPU_DESCRIPTOR_HANDLE* rtvs = new D3D12_CPU_DESCRIPTOR_HANDLE[numColorTargets];

        for (auto i = 0; i < numColorTargets; i++) {
            Texture* rt = ((Texture**)colorTargets)[i];
            rtvs[i] = rt->GetRTV();
            m_currentRT.push_back(rt);

            rt->TransitionBatched(D3D12_RESOURCE_STATE_RENDER_TARGET);
        }
        Texture::SendTransitionBatch(cmdList);

        if (depthTarget)
            SetRenderTargets(numColorTargets, rtvs, depthTarget->GetDSV());
        else
            SetRenderTargets(numColorTargets, rtvs);

        delete[] rtvs;
    }
    else {
        Texture::SendTransitionBatch(cmdList);
        Texture* displayTarget = m_deviceRes->GetMainTarget();
        m_currentRT.push_back(displayTarget);
        if (depthTarget)
            SetRenderTarget(displayTarget->GetRTV(), depthTarget->GetDSV());
        else
            SetRenderTarget(displayTarget->GetRTV());
    }
    m_currentDepthStencil = depthTarget;
}

void CommandContext::ResolveResource(Texture* source, Texture* dest) {
    CommandList* cmdResolve = cmd;
    if (!cmd) cmdResolve = m_deviceRes->BeginCommandList(); // we might want to Resolve outside of Draw (to read the back buffer for example), so we create a blocking command list for that purpose

    source->TransitionBatched(D3D12_RESOURCE_STATE_RESOLVE_SOURCE);
    dest->TransitionBatched(D3D12_RESOURCE_STATE_RESOLVE_DEST);
    Texture::SendTransitionBatch(cmdResolve->Get());

    DXGI_FORMAT resolveFormat = dest->GetFormat();
    if (resolveFormat == DXGI_FORMAT_D24_UNORM_S8_UINT) resolveFormat = DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
    cmdResolve->Get()->ResolveSubresource(dest->Get(), 0, source->Get(), 0, resolveFormat);

    if (!cmd) cmdResolve->Close(true);
}

void CommandContext::GenerateMipmap(Texture* source) {
    CommandList* cmdResolve = cmd;
    if (!cmd) cmdResolve = m_deviceRes->BeginCommandList(); // we might want to generate mipmap outside of Draw, so we create a blocking command list for that purpose
    auto cmdList = cmdResolve->Get();

    auto mipLevels = source->GetMipLevels();

    // By default textures do not have the D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS flags
    // We will copy the content of our texture to a temp UA resource in order to use RWTexture2D
    auto staging = new Texture(*source);
    staging->AllowUAV();
    staging->Create(m_deviceRes);

    // Copy the texture to the staging resource
    source->Transition(cmdList, D3D12_RESOURCE_STATE_COPY_SOURCE);
    const CD3DX12_TEXTURE_COPY_LOCATION src(source->Get(), 0);
    const CD3DX12_TEXTURE_COPY_LOCATION dst(staging->Get(), 0);
    cmdList->CopyTextureRegion(&dst, 0, 0, 0, &src, nullptr);

    staging->TransitionBatched(D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE);
    for (uint32_t mip = 1; mip < mipLevels; ++mip)
        staging->TransitionBatched(D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE, D3D12_RESOURCE_STATE_UNORDERED_ACCESS, mip);
    Texture::SendTransitionBatch(cmdList);

    m_deviceRes->GetGraphicsHeaps()->CopySRVUAVToShader(staging->GetSRV(), 0);
    for (uint16_t mip = 1; mip < mipLevels; ++mip)
        m_deviceRes->GetGraphicsHeaps()->CopySRVUAVToShader(staging->GetUAV(mip), mip);

    cmdList->SetComputeRootSignature(m_generateMipRootSig.Get());
    cmdList->SetPipelineState(m_generatePipelinePSO.Get());
    ID3D12DescriptorHeap* descriptorHeaps[] = { m_deviceRes->GetGraphicsHeaps()->GetSRVShaderHeap() };
    cmdList->SetDescriptorHeaps(_countof(descriptorHeaps), descriptorHeaps);

    // Compute the mip
    uint32_t mipWidth = static_cast<uint32_t>(source->GetWidth());
    uint32_t mipHeight = source->GetHeight();
    cmdList->SetComputeRootDescriptorTable(1, m_deviceRes->GetGraphicsHeaps()->GetGpuHandleAtSlot(0));
    for (uint32_t mip = 1; mip < mipLevels; ++mip) {
        mipWidth = std::max<uint32_t>(1, mipWidth >> 1);
        mipHeight = std::max<uint32_t>(1, mipHeight >> 1);

        // Set shader parameters
        MipGenerationConstantData constants;
        constants.SrcMipIndex = mip - 1;
        constants.InvOutTexelSize = XMFLOAT2(1 / float(mipWidth), 1 / float(mipHeight));
        cmdList->SetComputeRoot32BitConstants(0, static_cast<uint32_t>(sizeof(MipGenerationConstantData) / sizeof(uint32_t)), &constants, 0);
        cmdList->SetComputeRootDescriptorTable(2, m_deviceRes->GetGraphicsHeaps()->GetGpuHandleAtSlot(mip));

        cmdList->Dispatch((mipWidth + 8 - 1) / 8, (mipHeight + 8 - 1) / 8, 1);

        staging->TransitionBatched(D3D12_RESOURCE_STATE_UNORDERED_ACCESS, D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE, mip);
        Texture::s_batchedBarriers.push_back(CD3DX12_RESOURCE_BARRIER::UAV(staging->Get()));
        Texture::SendTransitionBatch(cmdList);
    }
    m_deviceRes->GetGraphicsHeaps()->ApplySRVsToShader();

    // Copy back the content of the staging resource to the texture
    staging->TransitionBatched(D3D12_RESOURCE_STATE_COPY_SOURCE);
    source->TransitionBatched(D3D12_RESOURCE_STATE_COPY_DEST);
    Texture::SendTransitionBatch(cmdList);
    cmdList->CopyResource(source->Get(), staging->Get());

    // We leave the resource in the pixel shader resource state as it is the most likely to be used
    source->TransitionBatched(D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);

    if (cmd) {
        // If we are in the main context we reset the descriptor heaps for normal rendering
        ID3D12DescriptorHeap* descriptorHeaps[] = {
            m_heaps->GetSRVShaderHeap(),
            m_heaps->GetSamplerShaderHeap()
        };
        cmdList->SetDescriptorHeaps(_countof(descriptorHeaps), descriptorHeaps);
        m_tempTextures[m_backBufferIndex].push_back(staging); // and keep the temp texture around for deferred deletion
    }
    else {
        cmdResolve->Close(true); // otherwise wait for the completion
        staging->FreeDescriptors(m_deviceRes);
        delete staging;
    }
}

void CommandContext::Clear(MGClearOptions options, float r, float g, float b, float a, float depth, int stencil) {
    const bool clearTarget = ((int)options & (int)MGClearOptions::Target) != 0;
    const bool clearDepth = ((int)options & (int)MGClearOptions::DepthBuffer) != 0;
    const bool clearStencil = ((int)options & (int)MGClearOptions::Stencil) != 0;

    if (m_currentDepthStencil && (clearDepth || clearStencil)) {
        auto const dsvDescriptor = m_currentDepthStencil->GetDSV();

        D3D12_CLEAR_FLAGS flags = (D3D12_CLEAR_FLAGS)0;
        if (clearDepth) flags |= D3D12_CLEAR_FLAG_DEPTH;
        if (clearStencil) flags |= D3D12_CLEAR_FLAG_STENCIL;
        cmdList->ClearDepthStencilView(dsvDescriptor, flags, depth, stencil, 0, nullptr);
    }

    if (clearTarget) {
        float color[] = { r, g, b, a };
        for (auto t : m_currentRT)
            cmdList->ClearRenderTargetView(t->GetRTV(), color, 0, nullptr);
    }
}

void CommandContext::SetPSODeviceParameters(D3D12_GRAPHICS_PIPELINE_STATE_DESC& psoDesc) {
    psoDesc.pRootSignature = m_rootSig.Get();
    psoDesc.SampleMask = UINT32_MAX;
    psoDesc.SampleDesc = m_currentRT[0]->GetSampleDesc();

    psoDesc.NumRenderTargets = m_currentRT.size();
    for (int i = 0; i < m_currentRT.size(); i++)
        psoDesc.RTVFormats[i] = m_currentRT[i]->GetFormat();
    psoDesc.DSVFormat = m_currentDepthStencil ? m_currentDepthStencil->GetFormat() : DXGI_FORMAT_UNKNOWN;
}

void CommandContext::SetPipelineState(ID3D12PipelineState* ps) {
    cmdList->SetPipelineState(ps);
}

void CommandContext::SetRenderTargets(UINT NumRTVs, const D3D12_CPU_DESCRIPTOR_HANDLE RTVs[]) {
    cmdList->OMSetRenderTargets(NumRTVs, RTVs, FALSE, nullptr);
}
void CommandContext::SetRenderTargets(UINT NumRTVs, const D3D12_CPU_DESCRIPTOR_HANDLE RTVs[], D3D12_CPU_DESCRIPTOR_HANDLE DSV) {
    cmdList->OMSetRenderTargets(NumRTVs, RTVs, FALSE, &DSV);
}

void CommandContext::CreateDefaultRootSignature() {
    D3D12_ROOT_SIGNATURE_FLAGS flags =
        D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT
        | D3D12_ROOT_SIGNATURE_FLAG_DENY_DOMAIN_SHADER_ROOT_ACCESS
        | D3D12_ROOT_SIGNATURE_FLAG_DENY_GEOMETRY_SHADER_ROOT_ACCESS
        | D3D12_ROOT_SIGNATURE_FLAG_DENY_HULL_SHADER_ROOT_ACCESS;

    CD3DX12_DESCRIPTOR_RANGE DescRange[4];
    DescRange[0].Init(D3D12_DESCRIPTOR_RANGE_TYPE_SRV, -1, 0); // t0-unbounded
    DescRange[1].Init(D3D12_DESCRIPTOR_RANGE_TYPE_SRV, -1, 0); // t0-unbounded
    DescRange[2].Init(D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER, -1, 0); // s0-unbounded
    DescRange[3].Init(D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER, -1, 0); // s0-unbounded

    CD3DX12_ROOT_PARAMETER RP[6];
    RP[0].InitAsConstantBufferView(0, 0, D3D12_SHADER_VISIBILITY_VERTEX); // b0
    RP[1].InitAsConstantBufferView(0, 0, D3D12_SHADER_VISIBILITY_PIXEL); // b0
    RP[2].InitAsDescriptorTable(1, &DescRange[0], D3D12_SHADER_VISIBILITY_VERTEX); // t0-unbounded
    RP[3].InitAsDescriptorTable(1, &DescRange[1], D3D12_SHADER_VISIBILITY_PIXEL); // t0-unbounded
    RP[4].InitAsDescriptorTable(1, &DescRange[2], D3D12_SHADER_VISIBILITY_VERTEX); // s0-unbounded
    RP[5].InitAsDescriptorTable(1, &DescRange[3], D3D12_SHADER_VISIBILITY_PIXEL); // s0-unbounded

    CD3DX12_VERSIONED_ROOT_SIGNATURE_DESC RootSig(6, RP, 0, nullptr, flags);
    ID3DBlob* pSerializedRootSig = nullptr;
    ID3DBlob* pErrorBlob = nullptr;
    D3D12SerializeVersionedRootSignature(&RootSig, &pSerializedRootSig, &pErrorBlob);

    m_deviceRes->GetD3DDevice()->CreateRootSignature(
        0, pSerializedRootSig->GetBufferPointer(), pSerializedRootSig->GetBufferSize(),
        IID_GRAPHICS_PPV_ARGS(m_rootSig.ReleaseAndGetAddressOf()));
}

void CommandContext::CreateGenerateMipPipelineResources() {
    {
        D3D12_ROOT_SIGNATURE_FLAGS rootSignatureFlags =
            D3D12_ROOT_SIGNATURE_FLAG_DENY_VERTEX_SHADER_ROOT_ACCESS
            | D3D12_ROOT_SIGNATURE_FLAG_DENY_DOMAIN_SHADER_ROOT_ACCESS
            | D3D12_ROOT_SIGNATURE_FLAG_DENY_GEOMETRY_SHADER_ROOT_ACCESS
            | D3D12_ROOT_SIGNATURE_FLAG_DENY_HULL_SHADER_ROOT_ACCESS
            | D3D12_ROOT_SIGNATURE_FLAG_DENY_PIXEL_SHADER_ROOT_ACCESS;

        const CD3DX12_STATIC_SAMPLER_DESC sampler(
            0,
            D3D12_FILTER_MIN_MAG_LINEAR_MIP_POINT,
            D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
            D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
            D3D12_TEXTURE_ADDRESS_MODE_CLAMP); // s0

        const CD3DX12_DESCRIPTOR_RANGE sourceDescriptorRange(D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1, 0); // t0
        const CD3DX12_DESCRIPTOR_RANGE targetDescriptorRange(D3D12_DESCRIPTOR_RANGE_TYPE_UAV, 1, 0); // u0

        CD3DX12_ROOT_PARAMETER rootParameters[3] = {};
        rootParameters[0].InitAsConstants(3, 0); // b0
        rootParameters[1].InitAsDescriptorTable(1, &sourceDescriptorRange); // t0
        rootParameters[2].InitAsDescriptorTable(1, &targetDescriptorRange); // u0

        CD3DX12_VERSIONED_ROOT_SIGNATURE_DESC RootSig(3, rootParameters, 1, &sampler, rootSignatureFlags);
        ID3DBlob* pSerializedRootSig = nullptr;
        ID3DBlob* pErrorBlob = nullptr;
        D3D12SerializeVersionedRootSignature(&RootSig, &pSerializedRootSig, &pErrorBlob);

        m_deviceRes->GetD3DDevice()->CreateRootSignature(
            0, pSerializedRootSig->GetBufferPointer(), pSerializedRootSig->GetBufferSize(),
            IID_GRAPHICS_PPV_ARGS(m_generateMipRootSig.ReleaseAndGetAddressOf()));
    }

    {
        D3D12_COMPUTE_PIPELINE_STATE_DESC psoDesc = {};
        psoDesc.CS.BytecodeLength = sizeof(GenerateMips_main);
        psoDesc.CS.pShaderBytecode = GenerateMips_main;
        psoDesc.pRootSignature = m_generateMipRootSig.Get();

        ThrowIfFailed(m_deviceRes->GetD3DDevice()->CreateComputePipelineState(&psoDesc, IID_GRAPHICS_PPV_ARGS(m_generatePipelinePSO.GetAddressOf())));
    }
}
