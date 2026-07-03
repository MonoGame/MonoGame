// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "directx12.h"

#include "CommandQueue.h"
#include <mutex>

using namespace DirectX;
using namespace DX;
using Microsoft::WRL::ComPtr;
using namespace Graphics;

void CommandQueue::Create(ID3D12Device* device)
{
    D3D12_COMMAND_QUEUE_DESC desc = {};
    desc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
    desc.Type = m_type;
    ThrowIfFailed(device->CreateCommandQueue(&desc, IID_GRAPHICS_PPV_ARGS(m_queue.ReleaseAndGetAddressOf())));
    m_queue->SetName(m_name.c_str());

    ThrowIfFailed(device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_GRAPHICS_PPV_ARGS(m_fence.ReleaseAndGetAddressOf())));
    m_fence->Signal(m_lastCompletedFenceValue);
    m_fence->SetName((m_name + L" Fence").c_str());
}

uint64_t CommandQueue::ExecuteCommandList(ID3D12CommandList* commandList)
{
    // Send the command list off to the GPU for processing.
    ThrowIfFailed(((ID3D12GraphicsCommandList*)commandList)->Close());

    // Make sure the fence covers ExecuteCommandLists call.
    std::lock_guard lock(m_fenceMutex);
    m_queue->ExecuteCommandLists(1, &commandList);

    m_queue->Signal(m_fence.Get(), m_nextFenceValue);

    return m_nextFenceValue++;
}

uint64_t CommandQueue::SignalFence()
{
    std::lock_guard lock(m_fenceMutex);

    m_queue->Signal(m_fence.Get(), m_nextFenceValue);

    return m_nextFenceValue++;
}

uint64_t CommandQueue::PollCurrentFenceValue()
{
    std::lock_guard lock(m_fenceMutex);
    m_lastCompletedFenceValue = std::max(m_lastCompletedFenceValue, m_fence->GetCompletedValue());
    return m_lastCompletedFenceValue;
}

bool CommandQueue::IsFenceComplete(uint64_t fenceValue)
{
    std::lock_guard lock(m_fenceMutex);

    if (fenceValue > m_lastCompletedFenceValue)
        m_lastCompletedFenceValue = std::max(m_lastCompletedFenceValue, m_fence->GetCompletedValue());

    return fenceValue <= m_lastCompletedFenceValue;
}

void CommandQueue::WaitForFenceCPUBlocking(uint64_t fenceValue)
{
    if (IsFenceComplete(fenceValue))
        return;

    HANDLE evt = CreateEventEx(nullptr, nullptr, 0, EVENT_MODIFY_STATE | SYNCHRONIZE);
    HRESULT hr = m_fence->SetEventOnCompletion(fenceValue, evt);
    ThrowIfFailed(hr);

    // 5 seconds should be more than enough.
    // https://learn.microsoft.com/en-us/windows-hardware/drivers/display/timeout-detection-and-recovery
    // "The default timeout period in Windows is two seconds."
    // "If the GPU can't complete or preempt the current task within the TDR timeout period, the OS diagnoses that the GPU is frozen."
    DWORD waitResult = WaitForSingleObjectEx(evt, 5000, FALSE);

    CloseHandle(evt);
    if (waitResult == WAIT_TIMEOUT)
    {
        ThrowIfFailed(DXGI_ERROR_DEVICE_HUNG);
    }
    else if (waitResult == WAIT_FAILED)
    {
        DWORD win32Error = GetLastError();
        ThrowIfFailed(HRESULT_FROM_WIN32(win32Error));
    }

    std::lock_guard lock(m_fenceMutex);
    m_lastCompletedFenceValue = fenceValue;
}

void CommandQueue::WaitForIdle()
{
    uint64_t lastFenceValue;
    {
        std::lock_guard lock(m_fenceMutex);
        lastFenceValue = m_nextFenceValue - 1;
    }

    WaitForFenceCPUBlocking(lastFenceValue);
}


#ifdef _GAMING_XBOX
void CommandQueue::PresentX(UINT planeCount, const D3D12XBOX_PRESENT_PLANE_PARAMETERS* pPlaneParameters, const D3D12XBOX_PRESENT_PARAMETERS* pPresentParameters) {
    ThrowIfFailed(m_queue->PresentX(planeCount, pPlaneParameters, pPresentParameters));
}

void CommandQueue::SuspendX(UINT flags) {
    ThrowIfFailed(m_queue->SuspendX(flags));
}

void CommandQueue::ResumeX() {
    ThrowIfFailed(m_queue->ResumeX());
}
#endif

CommandList* CommandListPool::Begin() {
    // Ensure m_fenceMutex is acquired before m_mutex.
    uint64_t currentFence = m_queue->PollCurrentFenceValue();

    CommandList* ctx = nullptr;

    std::lock_guard lock(m_mutex);

    if (m_contextsRepo.empty()) {
        ctx = new CommandList(this);
        m_contexts.emplace_back(ctx);
        ctx->m_allocator = NewAllocator(currentFence);
        ThrowIfFailed(m_device->CreateCommandList(0, m_queue->GetType(), ctx->m_allocator, nullptr, IID_GRAPHICS_PPV_ARGS(ctx->m_list.ReleaseAndGetAddressOf())));
        ctx->m_list->SetName((m_name + L" CommandList").c_str());
    } else {
        ctx = m_contextsRepo.front();
        ctx->m_allocator = NewAllocator(currentFence);
        ctx->m_list->Reset(ctx->m_allocator, nullptr);
        m_contextsRepo.pop();
    }

    return ctx;
}

uint64_t CommandListPool::CloseList(CommandList* ctx, bool blocking)
{
    // Ensure m_fenceMutex is acquired before m_mutex.
    uint64_t fenceValue = m_queue->ExecuteCommandList(ctx->m_list.Get());

    if (blocking)
        m_queue->WaitForFenceCPUBlocking(fenceValue);

    std::lock_guard lock(m_mutex);
    m_allocatorsRepo.push(std::pair<uint64_t, ID3D12CommandAllocator*>(fenceValue, ctx->m_allocator));
    ctx->m_allocator = nullptr;
    m_contextsRepo.push(ctx);

    return fenceValue;
}

ID3D12CommandAllocator* CommandListPool::NewAllocator(uint64_t fenceValue)
{
    ID3D12CommandAllocator* res = nullptr;

    if (!m_allocatorsRepo.empty())
    {
        std::pair<uint64_t, ID3D12CommandAllocator*>& AllocatorPair = m_allocatorsRepo.front();

        if (AllocatorPair.first <= fenceValue)
        {
            res = AllocatorPair.second;
            ThrowIfFailed(res->Reset());
            m_allocatorsRepo.pop();
        }
    }

    if (!res)
    {
        ComPtr<ID3D12CommandAllocator> cmdAlloc;
        ThrowIfFailed(m_device->CreateCommandAllocator(m_queue->GetType(), IID_GRAPHICS_PPV_ARGS(cmdAlloc.ReleaseAndGetAddressOf())));
        cmdAlloc->SetName((m_name + L" Allocator Pool #" + std::to_wstring(m_allocators.size())).c_str());
        m_allocators.emplace_back(std::move(cmdAlloc));
        res = m_allocators.back().Get();
    }

    return res;
}

