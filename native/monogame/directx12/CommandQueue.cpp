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
    m_fence->Signal(0);
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
    uint64_t value = m_fence->GetCompletedValue();
    return value;
}

bool CommandQueue::IsFenceComplete(uint64_t fenceValue)
{
    std::lock_guard lock(m_fenceMutex);
    uint64_t completedFenceValue = m_fence->GetCompletedValue();
    return completedFenceValue >= fenceValue;
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

CommandListPool::~CommandListPool()
{
    for (auto iter = m_allContexts.begin(); iter != m_allContexts.end(); iter++)
        delete (*iter);
}

CommandList* CommandListPool::Begin()
{
    // Ensure m_fenceMutex is acquired before m_mutex.
    uint64_t currentFence = m_queue->PollCurrentFenceValue();

    CommandList* ctx = nullptr;

    std::lock_guard lock(m_mutex);

    auto iter = m_freeContexts.begin();
    for (; iter != m_freeContexts.end(); iter++)
    {
        if ((*iter)->m_fence > currentFence)
            continue;

        ctx = (*iter);
        m_freeContexts.erase(iter);
        ctx->m_allocator->Reset();
        ctx->m_list->Reset(ctx->m_allocator.Get(), nullptr);
        return ctx;
    }
        
    ctx = new CommandList(this);
    ThrowIfFailed(m_device->CreateCommandAllocator(m_queue->GetType(), IID_GRAPHICS_PPV_ARGS(ctx->m_allocator.GetAddressOf())));
    ThrowIfFailed(m_device->CreateCommandList(0, m_queue->GetType(), ctx->m_allocator.Get(), nullptr, IID_GRAPHICS_PPV_ARGS(ctx->m_list.ReleaseAndGetAddressOf())));
    ctx->m_list->SetName((m_name + L" CommandList").c_str());
    m_allContexts.push_back(ctx);

    return ctx;
}

uint64_t CommandListPool::CloseList(CommandList* ctx, bool blocking)
{
    uint64_t fenceValue = m_queue->ExecuteCommandList(ctx->m_list.Get());

    if (blocking)
        m_queue->WaitForFenceCPUBlocking(fenceValue);

    std::lock_guard lock(m_mutex);
    ctx->m_fence = fenceValue;
    m_freeContexts.push_back(ctx);

    return fenceValue;
}
    
