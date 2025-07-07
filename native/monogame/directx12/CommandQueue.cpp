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

void CommandQueue::Create(ID3D12Device* device) {
    D3D12_COMMAND_QUEUE_DESC desc = {};
    desc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
    desc.Type = m_type;
    ThrowIfFailed(device->CreateCommandQueue(&desc, IID_GRAPHICS_PPV_ARGS(m_queue.ReleaseAndGetAddressOf())));
    m_queue->SetName(m_name.c_str());

    ThrowIfFailed(device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_GRAPHICS_PPV_ARGS(m_fence.ReleaseAndGetAddressOf())));
    m_fence->Signal(m_lastCompletedFenceValue);
    m_fence->SetName((m_name + L" Fence").c_str());

    m_fenceEvent.Attach(CreateEventEx(nullptr, nullptr, 0, EVENT_MODIFY_STATE | SYNCHRONIZE));
    if (!m_fenceEvent.IsValid())
        throw std::system_error(std::error_code(static_cast<int>(GetLastError()), std::system_category()), "CreateEventEx");
}

uint64_t CommandQueue::ExecuteCommandList(ID3D12CommandList* commandList) {
    // Send the command list off to the GPU for processing.
    ThrowIfFailed(((ID3D12GraphicsCommandList*)commandList)->Close());
    m_queue->ExecuteCommandLists(1, &commandList);

    return SignalFence();
}

uint64_t CommandQueue::SignalFence() {
    std::lock_guard<std::mutex> lock(m_fenceMutex);

    m_queue->Signal(m_fence.Get(), m_nextFenceValue);

    return m_nextFenceValue++;
}


uint64_t CommandQueue::PollCurrentFenceValue() {
    m_lastCompletedFenceValue = std::max(m_lastCompletedFenceValue, m_fence->GetCompletedValue());
    return m_lastCompletedFenceValue;
}

bool CommandQueue::IsFenceComplete(uint64_t fenceValue) {
    if (fenceValue > m_lastCompletedFenceValue)
        PollCurrentFenceValue();

    return fenceValue <= m_lastCompletedFenceValue;
}

void CommandQueue::InsertWait(uint64_t fenceValue) {
    m_queue->Wait(m_fence.Get(), fenceValue);
}

void CommandQueue::InsertWaitForQueueFence(CommandQueue* otherQueue, uint64_t fenceValue) {
    m_queue->Wait(otherQueue->GetFence(), fenceValue);
}

void CommandQueue::InsertWaitForQueue(CommandQueue* otherQueue) {
    m_queue->Wait(otherQueue->GetFence(), otherQueue->GetNextFenceValue() - 1); // Wait for the last requested fence
}

void CommandQueue::WaitForFenceCPUBlocking(uint64_t fenceValue) {
    if (IsFenceComplete(fenceValue))
        return;

    std::lock_guard<std::mutex> lock(m_fenceMutex);

    m_fence->SetEventOnCompletion(fenceValue, m_fenceEvent.Get());
    WaitForSingleObjectEx(m_fenceEvent.Get(), INFINITE, FALSE);
    m_lastCompletedFenceValue = fenceValue;
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
    CommandList* ctx = nullptr;

    if (m_contextsRepo.empty()) {
        ctx = new CommandList(this);
        m_contexts.emplace_back(ctx);
        ctx->m_allocator = NewAllocator(m_queue.PollCurrentFenceValue());
        ThrowIfFailed(m_device->CreateCommandList(0, m_type, ctx->m_allocator, nullptr, IID_GRAPHICS_PPV_ARGS(ctx->m_list.ReleaseAndGetAddressOf())));
        ctx->Get()->SetName((m_name + L" CommandList").c_str());
    } else {
        ctx = m_contextsRepo.front();
        ctx->m_allocator = NewAllocator(m_queue.PollCurrentFenceValue());
        ctx->Get()->Reset(ctx->m_allocator, nullptr);
        m_contextsRepo.pop();
    }

    return ctx;
}

uint64_t CommandListPool::CloseList(CommandList* ctx, bool blocking) {
    uint64_t fenceValue = m_queue.ExecuteCommandList(ctx->m_list.Get());
    FreeAllocator(fenceValue, ctx->m_allocator);
    ctx->m_allocator = nullptr;

    if (blocking)
        m_queue.WaitForFenceCPUBlocking(fenceValue);

    m_contextsRepo.push(ctx);

    return fenceValue;
}

ID3D12CommandAllocator* CommandListPool::NewAllocator(uint64_t fenceValue) {
    ID3D12CommandAllocator* res = nullptr;

    if (!m_allocatorsRepo.empty()) {
        std::pair<uint64_t, ID3D12CommandAllocator*>& AllocatorPair = m_allocatorsRepo.front();

        if (AllocatorPair.first <= fenceValue) {
            res = AllocatorPair.second;
            ThrowIfFailed(res->Reset());
            m_allocatorsRepo.pop();
        }
    }

    if (!res) {
        ComPtr<ID3D12CommandAllocator> cmdAlloc;
        ThrowIfFailed(m_device->CreateCommandAllocator(m_type, IID_GRAPHICS_PPV_ARGS(cmdAlloc.ReleaseAndGetAddressOf())));
        cmdAlloc->SetName((m_name + L" Allocator Pool #" + std::to_wstring(m_allocators.size())).c_str());
        m_allocators.emplace_back(std::move(cmdAlloc));
        res = m_allocators.back().Get();
    }

    return res;
}

void CommandListPool::FreeAllocator(uint64_t fenceValue, ID3D12CommandAllocator* alloc) {
    m_allocatorsRepo.push(std::pair<uint64_t, ID3D12CommandAllocator*>(fenceValue, alloc));
}

