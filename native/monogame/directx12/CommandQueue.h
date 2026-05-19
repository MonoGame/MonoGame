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
    void InsertWait(uint64_t fenceValue);
    void InsertWaitForQueueFence(CommandQueue* otherQueue, uint64_t fenceValue);
    void InsertWaitForQueue(CommandQueue* otherQueue);

    void WaitForFenceCPUBlocking(uint64_t fenceValue);
    void WaitForIdle() { WaitForFenceCPUBlocking(m_nextFenceValue - 1); }
    uint64_t SignalFence();

    ID3D12CommandQueue* Get() { return m_queue.Get(); }
    uint64_t ExecuteCommandList(ID3D12CommandList* commandList);
#ifdef _GAMING_XBOX
    void PresentX(UINT planeCount, const D3D12XBOX_PRESENT_PLANE_PARAMETERS* pPlaneParameters, const D3D12XBOX_PRESENT_PARAMETERS* pPresentParameters);
    void SuspendX(UINT flags);
    void ResumeX();
#endif

    uint64_t PollCurrentFenceValue();
    uint64_t GetLastCompletedFence() { return m_lastCompletedFenceValue; }
    uint64_t GetNextFenceValue() { return m_nextFenceValue; }
    ID3D12Fence* GetFence() { return m_fence.Get(); }
private:
    Microsoft::WRL::ComPtr<ID3D12CommandQueue> m_queue;
    D3D12_COMMAND_LIST_TYPE  m_type;
    std::wstring m_name;

    std::mutex m_fenceMutex;
    std::mutex m_eventMutex;

    Microsoft::WRL::ComPtr<ID3D12Fence> m_fence;
    uint64_t m_lastCompletedFenceValue = 0;
    uint64_t m_nextFenceValue = m_lastCompletedFenceValue + 1;
    Microsoft::WRL::Wrappers::Event m_fenceEvent;
};

// Simplification of https://github.com/microsoft/DirectX-Graphics-Samples/blob/master/MiniEngine/Core/CommandAllocatorPool.cpp
// With CommandAllocatorPool merged with CommandListManager
struct CommandList;

class CommandListPool {
    ID3D12Device* m_device = nullptr;

    D3D12_COMMAND_LIST_TYPE  m_type;
    std::wstring m_name;
    
    std::vector<std::unique_ptr<CommandList>> m_contexts;
    std::vector<Microsoft::WRL::ComPtr<ID3D12CommandAllocator>> m_allocators;

    // free pool
    std::queue<CommandList*> m_contextsRepo;
    std::queue<std::pair<uint64_t, ID3D12CommandAllocator*>> m_allocatorsRepo;
public:
    CommandQueue m_queue;

public:
    CommandListPool(ID3D12Device* device, D3D12_COMMAND_LIST_TYPE type, std::wstring debugName) : m_device(device), m_type(type), m_queue(type, debugName + L" Queue") {
        m_queue.Create(device);
    }

    CommandList* Begin();

    CommandQueue* GetCommandQueue() { return &m_queue; }
private:
    ID3D12CommandAllocator* NewAllocator(uint64_t fenceValue);
    void FreeAllocator(uint64_t fenceValue, ID3D12CommandAllocator* alloc);
    uint64_t CloseList(CommandList* ctx, bool blocking);

    friend struct CommandList;
};

struct CommandList {
    CommandListPool* m_pool = nullptr;
    ID3D12CommandAllocator* m_allocator = nullptr;

    Microsoft::WRL::ComPtr<ID3D12GraphicsCommandList> m_list;

    CommandList(CommandListPool* pool) : m_pool(pool) {}

    uint64_t Close(bool waitCPUBlocking = false) { return m_pool->CloseList(this, waitCPUBlocking); }

    ID3D12GraphicsCommandList* Get() { return m_list.Get(); }
    CommandQueue* GetCommandQueue() { return m_pool->GetCommandQueue(); }
};
}
