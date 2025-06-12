// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "directx12.h"

#include "Heaps.h"
#include "DeviceResources.h"
#include "GraphicsEnums.h"

using namespace DirectX;
using namespace DX;
using namespace Graphics;

NonShaderVisibleDescHeap::NonShaderVisibleDescHeap(ID3D12Device* device, D3D12_DESCRIPTOR_HEAP_TYPE type, D3D12_DESCRIPTOR_HEAP_FLAGS flags, uint32_t size)
    : m_device(device), m_type(type), m_size(size) {
    m_increment = device->GetDescriptorHandleIncrementSize(type);

    D3D12_DESCRIPTOR_HEAP_DESC desc = { type, static_cast<UINT>(size), flags, 0 };
    device->CreateDescriptorHeap(&desc, IID_GRAPHICS_PPV_ARGS(m_heap.ReleaseAndGetAddressOf()));

    m_heapStartCPU = m_heap->GetCPUDescriptorHandleForHeapStart();
}

D3D12_CPU_DESCRIPTOR_HANDLE NonShaderVisibleDescHeap::GetCpuHandle(size_t index) const {
    if (index >= m_size)
        throw std::out_of_range("No more room in the NonShaderVisibleDescHeap");

    D3D12_CPU_DESCRIPTOR_HANDLE handle;
    handle.ptr = static_cast<SIZE_T>(m_heapStartCPU.ptr + UINT64(index) * UINT64(m_increment));
    return handle;
}

D3D12_CPU_DESCRIPTOR_HANDLE NonShaderVisibleDescHeap::AllocCpuHandle() {
    if(m_freeHandle.empty())
        return GetCpuHandle(m_firstFreeAlloc++);
    else {
        auto handle = GetCpuHandle(m_freeHandle.front());
        m_freeHandle.pop();
        return handle;
    }
}

void NonShaderVisibleDescHeap::FreeCpuHandle(D3D12_CPU_DESCRIPTOR_HANDLE handle) {
    SIZE_T index = (handle.ptr - m_heapStartCPU.ptr) / SIZE_T(m_increment);
    m_freeHandle.push(index);
}

Graphics::ShaderVisibleDescHeap::ShaderVisibleDescHeap(ID3D12Device* device, D3D12_DESCRIPTOR_HEAP_TYPE type, size_t size)
    : NonShaderVisibleDescHeap(device, type, D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE, size) {
    m_heapStartGPU = m_heap->GetGPUDescriptorHandleForHeapStart();
}

void ShaderVisibleDescHeap::WriteDescriptor(ID3D12Device* device, uint32_t offset, const D3D12_CPU_DESCRIPTOR_HANDLE desc) {
    if (offset >= m_size)
        throw std::out_of_range("Offset outside of the ShaderVisibleDescHeap");

    device->CopyDescriptorsSimple(1, GetCpuHandle(offset), desc, m_type);
}

D3D12_GPU_DESCRIPTOR_HANDLE ShaderVisibleDescHeap::GetGpuHandle(size_t index) const {
    if (index >= m_size)
        throw std::out_of_range("No more room in the ShaderVisibleDescHeap");

    D3D12_GPU_DESCRIPTOR_HANDLE handle;
    handle.ptr = static_cast<SIZE_T>(m_heapStartGPU.ptr + UINT64(index) * UINT64(m_increment));
    return handle;
}

Heaps::Heaps(ID3D12Device* device, int backBufferCount) {
    m_device = device;

    m_srvHeap = std::make_unique<NonShaderVisibleDescHeap>(device, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, 1 << 16);
    m_samplerHeap = std::make_unique<NonShaderVisibleDescHeap>(device, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER, 1024);
    m_rtvHeap = std::make_unique<NonShaderVisibleDescHeap>(device, D3D12_DESCRIPTOR_HEAP_TYPE_RTV, 1024);
    m_dsvHeap = std::make_unique<NonShaderVisibleDescHeap>(device, D3D12_DESCRIPTOR_HEAP_TYPE_DSV, 1024);

    for (UINT n = 0; n < backBufferCount; n++)
        m_srvShaderHeap[n] = std::make_unique<ShaderVisibleDescHeap>(device, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, 1 << 16);
    for (UINT n = 0; n < backBufferCount; n++)
        m_samplerShaderHeap[n] = std::make_unique<ShaderVisibleDescHeap>(device, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER, 1024);

    D3D12_QUERY_HEAP_DESC queryHeapDesc = { D3D12_QUERY_HEAP_TYPE_OCCLUSION, 1024 };
    ThrowIfFailed(m_device->CreateQueryHeap(&queryHeapDesc, IID_GRAPHICS_PPV_ARGS(m_queryHeap.ReleaseAndGetAddressOf())));
}

Heaps::~Heaps() {}

D3D12_CPU_DESCRIPTOR_HANDLE Heaps::CreateSRVHandle(ID3D12Resource* res, D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc) {
    D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = m_srvHeap->AllocCpuHandle();
    m_device->CreateShaderResourceView(res, &srvDesc, cpuHandle);
    return cpuHandle;
}

D3D12_CPU_DESCRIPTOR_HANDLE Heaps::CreateUAVHandle(ID3D12Resource* res, D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc) {
    D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = m_srvHeap->AllocCpuHandle();
    m_device->CreateUnorderedAccessView(res, nullptr, &uavDesc, cpuHandle);
    return cpuHandle;
}

void Heaps::CopySRVUAVToShader(const D3D12_CPU_DESCRIPTOR_HANDLE srv, uint32_t slot) {
    m_srvShaderHeap[m_idx]->WriteDescriptor(m_device, m_srvShaderHeapPos + slot, srv);
    m_srvSlotUsed = std::max(m_srvSlotUsed, slot + 1);
}

D3D12_GPU_DESCRIPTOR_HANDLE Heaps::GetGpuHandleAtSlot(uint32_t slot) {
    return m_srvShaderHeap[m_idx]->GetGpuHandle(m_srvShaderHeapPos + slot);
}

D3D12_GPU_DESCRIPTOR_HANDLE Heaps::ApplySRVsToShader() {
    D3D12_GPU_DESCRIPTOR_HANDLE res = m_srvShaderHeap[m_idx]->GetGpuHandle(m_srvShaderHeapPos);
    m_srvShaderHeapPos += m_srvSlotUsed;
    m_srvSlotUsed = 0;
    return res;
}

void Heaps::CopySamplerToShader(const D3D12_CPU_DESCRIPTOR_HANDLE srv, uint32_t slot) {
    m_samplerShaderHeap[m_idx]->WriteDescriptor(m_device, m_samplerShaderHeapPos + slot, srv);
    m_samplerSlotUsed = std::max(m_samplerSlotUsed, slot + 1);
}

D3D12_GPU_DESCRIPTOR_HANDLE Heaps::ApplySamplersToShader() {
    D3D12_GPU_DESCRIPTOR_HANDLE res = m_samplerShaderHeap[m_idx]->GetGpuHandle(m_samplerShaderHeapPos);
    m_samplerShaderHeapPos += m_samplerSlotUsed;
    m_samplerSlotUsed = 0;
    return res;
}

D3D12_CPU_DESCRIPTOR_HANDLE Heaps::CreateRTVHandle(ID3D12Resource* res, D3D12_RENDER_TARGET_VIEW_DESC rtvDesc) {
    D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = m_rtvHeap->AllocCpuHandle();
    m_device->CreateRenderTargetView(res, &rtvDesc, cpuHandle);
    return cpuHandle;
}

D3D12_CPU_DESCRIPTOR_HANDLE Heaps::CreateDSVHandle(ID3D12Resource* res, D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc) {
    D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = m_dsvHeap->AllocCpuHandle();
    m_device->CreateDepthStencilView(res, &dsvDesc, cpuHandle);
    return cpuHandle;
}
