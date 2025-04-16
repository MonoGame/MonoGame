// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "GraphicsEnums.h"
#include <queue>

namespace Graphics {
    class NonShaderVisibleDescHeap {
    protected:
        ID3D12Device* m_device;

        Microsoft::WRL::ComPtr<ID3D12DescriptorHeap> m_heap;
        size_t m_size;
        D3D12_DESCRIPTOR_HEAP_TYPE m_type;
        uint32_t m_increment;
        D3D12_CPU_DESCRIPTOR_HANDLE m_heapStartCPU;

        uint64_t m_firstFreeAlloc = 0;
        std::queue<uint64_t> m_freeHandle;

        NonShaderVisibleDescHeap(ID3D12Device* device, D3D12_DESCRIPTOR_HEAP_TYPE type, D3D12_DESCRIPTOR_HEAP_FLAGS flags, uint32_t size);
    public:
        NonShaderVisibleDescHeap(ID3D12Device* device, D3D12_DESCRIPTOR_HEAP_TYPE type, uint32_t size)
            : NonShaderVisibleDescHeap(device, type, D3D12_DESCRIPTOR_HEAP_FLAG_NONE, size) {}

        D3D12_CPU_DESCRIPTOR_HANDLE GetCpuHandle(size_t index) const;
        D3D12_CPU_DESCRIPTOR_HANDLE AllocCpuHandle();
        void FreeCpuHandle(D3D12_CPU_DESCRIPTOR_HANDLE handle);

        size_t Size() const noexcept { return m_size; }
        ID3D12DescriptorHeap* Heap() const noexcept { return m_heap.Get(); }
    };

    class ShaderVisibleDescHeap : public NonShaderVisibleDescHeap {
        D3D12_GPU_DESCRIPTOR_HANDLE m_heapStartGPU;
    public:
        ShaderVisibleDescHeap(ID3D12Device* device, D3D12_DESCRIPTOR_HEAP_TYPE type, size_t size);

        D3D12_GPU_DESCRIPTOR_HANDLE GetGpuHandle(size_t index) const;
        void WriteDescriptor(ID3D12Device* device, uint32_t offset, const D3D12_CPU_DESCRIPTOR_HANDLE desc);
    };

    class Heaps {
        ID3D12Device* m_device;
        unsigned int m_idx = 0;

        // Non-shader visible heaps
        std::unique_ptr<NonShaderVisibleDescHeap> m_srvHeap;
        std::unique_ptr<NonShaderVisibleDescHeap> m_samplerHeap;
        std::unique_ptr<NonShaderVisibleDescHeap> m_rtvHeap;
        std::unique_ptr<NonShaderVisibleDescHeap> m_dsvHeap;

        // Query heap
        Microsoft::WRL::ComPtr<ID3D12QueryHeap> m_queryHeap;
        size_t m_queryId = 0;

        // Shader visible heaps
        std::unique_ptr<ShaderVisibleDescHeap> m_srvShaderHeap[MAX_BACK_BUFFER_COUNT];
        size_t m_srvShaderHeapPos = 0;
        uint32_t m_srvSlotUsed = 0;
        std::unique_ptr<ShaderVisibleDescHeap> m_samplerShaderHeap[MAX_BACK_BUFFER_COUNT];
        size_t m_samplerShaderHeapPos = 0;
        uint32_t m_samplerSlotUsed = 0;
    public:
        Heaps(ID3D12Device* device, int backBufferCount);
        ~Heaps();

        void Prepare(unsigned int frame) {
            m_srvShaderHeapPos = 0;
            m_samplerShaderHeapPos = 0;
            m_idx = frame % MAX_BACK_BUFFER_COUNT;
        }

        D3D12_CPU_DESCRIPTOR_HANDLE CreateSRVHandle(ID3D12Resource* res, D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc);
        D3D12_CPU_DESCRIPTOR_HANDLE CreateUAVHandle(ID3D12Resource* res, D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc);
        void FreeSRVUAVHandle(D3D12_CPU_DESCRIPTOR_HANDLE handle) { m_srvHeap->FreeCpuHandle(handle); }
        void CopySRVUAVToShader(const D3D12_CPU_DESCRIPTOR_HANDLE srv, uint32_t slot);
        D3D12_GPU_DESCRIPTOR_HANDLE GetGpuHandleAtSlot(uint32_t slot);
        D3D12_GPU_DESCRIPTOR_HANDLE ApplySRVsToShader();
        ID3D12DescriptorHeap* GetSRVShaderHeap() { return m_srvShaderHeap[m_idx]->Heap(); }

        D3D12_CPU_DESCRIPTOR_HANDLE CreateSamplerHandle() { return m_samplerHeap->AllocCpuHandle(); }
        void CopySamplerToShader(const D3D12_CPU_DESCRIPTOR_HANDLE srv, uint32_t slot);
        D3D12_GPU_DESCRIPTOR_HANDLE ApplySamplersToShader();
        ID3D12DescriptorHeap* GetSamplerShaderHeap() { return m_samplerShaderHeap[m_idx]->Heap(); }

        D3D12_CPU_DESCRIPTOR_HANDLE CreateRTVHandle(ID3D12Resource* res, D3D12_RENDER_TARGET_VIEW_DESC rtvDesc);
        void FreeRTVHandle(D3D12_CPU_DESCRIPTOR_HANDLE handle) { m_rtvHeap->FreeCpuHandle(handle); }
        D3D12_CPU_DESCRIPTOR_HANDLE CreateDSVHandle(ID3D12Resource* res, D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc);
        void FreeDSVHandle(D3D12_CPU_DESCRIPTOR_HANDLE handle) { m_dsvHeap->FreeCpuHandle(handle); }

        ID3D12QueryHeap* GetQueryHeap() { return m_queryHeap.Get(); }
        size_t CreateQueryHandle() { return m_queryId++; }
    };
}
