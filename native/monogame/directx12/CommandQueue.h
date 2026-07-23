// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include <mutex>
#include <queue>

namespace Graphics {

// Adapted from Microsoft's MiniEngine https://github.com/microsoft/DirectX-Graphics-Samples/blob/master/MiniEngine/Core/CommandListManager.h
class CommandQueue {
public:
    CommandQueue(D3D12_COMMAND_LIST_TYPE type, std::wstring debugName) : m_type(type), m_name(debugName) {};
    void Create(ID3D12Device* device);

    bool IsFenceComplete(uint64_t fenceValue);

    void WaitForFenceCPUBlocking(uint64_t fenceValue);
    void WaitForIdle();
    uint64_t SignalFence();

    ID3D12CommandQueue* Get() const { return m_queue.Get(); }
    uint64_t ExecuteCommandList(ID3D12CommandList* commandList);

#ifdef _GAMING_XBOX
    void PresentX(UINT planeCount, const D3D12XBOX_PRESENT_PLANE_PARAMETERS* pPlaneParameters, const D3D12XBOX_PRESENT_PARAMETERS* pPresentParameters);
    void SuspendX(UINT flags);
    void ResumeX();
#endif

    uint64_t PollCurrentFenceValue();
    D3D12_COMMAND_LIST_TYPE GetType() const { return m_type; }

private:
    Microsoft::WRL::ComPtr<ID3D12CommandQueue> m_queue;
    D3D12_COMMAND_LIST_TYPE m_type;
    std::wstring m_name;

    std::mutex m_fenceMutex;

    Microsoft::WRL::ComPtr<ID3D12Fence> m_fence;
    uint64_t m_nextFenceValue = 1;
};

// Simplification of https://github.com/microsoft/DirectX-Graphics-Samples/blob/master/MiniEngine/Core/CommandAllocatorPool.cpp
// With CommandAllocatorPool merged with CommandListManager
class CommandList;

class CommandListPool {
    ID3D12Device* m_device = nullptr;
    CommandQueue* m_queue = nullptr;

    std::mutex m_mutex;

    std::wstring m_name;

    std::vector<CommandList*> m_freeContexts;
    std::vector<CommandList*> m_allContexts;

public:
    CommandListPool(ID3D12Device* device, CommandQueue* queue)
        : m_device(device),  m_queue(queue)
    {
    }

    ~CommandListPool();

    CommandList* Begin();

private:

    uint64_t CloseList(CommandList* ctx, bool blocking);

    friend class CommandList;
};

class CommandList
{
private:

    friend class CommandListPool;

    CommandListPool* m_pool = nullptr;
    Microsoft::WRL::ComPtr<ID3D12CommandAllocator> m_allocator;
    uint64_t m_fence = 0;

    Microsoft::WRL::ComPtr<ID3D12GraphicsCommandList> m_list;

    CommandList(CommandListPool* pool) : m_pool(pool) {}

public:

    uint64_t Close(bool waitCPUBlocking = false) { return m_pool->CloseList(this, waitCPUBlocking); }

    ID3D12GraphicsCommandList* Get() const { return m_list.Get(); }
};

}
