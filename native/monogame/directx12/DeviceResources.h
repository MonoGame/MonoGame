// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "GraphicsEnums.h"
#include "CommandQueue.h"
#include "Heaps.h"

enum class MGSurfaceFormat;

// Used by the ring buffer for temporary VBs/IBs :
// A frame can spike temporarily and have more buffers than MAX_BUFFER_PER_FRAME
// but if there is constantly more than MAX_BUFFER_PER_FRAME * MAX_BACK_BUFFER_COUNT it will result in a crash.
// In that case you can increase this number or you should probably reduce the number of drawcall in your application
// Also note that this value is used internally to allocate some memory, larger buffers (> 64Kb) will "count as" multiple small buffer (buffers are 64Kb aligned)
#define MAX_BUFFER_PER_FRAME 8 * 1024

namespace Graphics {
class CommandContext;
class Texture;

// Controls all the DirectX device resources.
class DeviceResources {
    struct Impl;
    Impl* pImpl;
public:
    DeviceResources(MGSurfaceFormat backBufferFormat, unsigned int backBufferCount = 2);
    ~DeviceResources();

    DeviceResources(DeviceResources&&) = default;
    DeviceResources& operator=(DeviceResources&&) = default;

    DeviceResources(DeviceResources const&) = delete;
    DeviceResources& operator=(DeviceResources const&) = delete;

#if defined(_GAMING_XBOX)
    void CreateDeviceResources();
#else
    void CreateDeviceResources(IDXGIFactory6* factory, IDXGIAdapter1* adapter);
#endif

    void CreateWindowSizeDependentResources(int width, int height, float r, float g, float b, float a, int msaaCount);
    uint32_t Prepare();
    void WaitForGpu();

#ifdef _GAMING_XBOX
    void PresentX();
    void Suspend();
    void Resume();
    void WaitForOrigin();
#else
    void Present(int sync, int flags);
    void SetWindow(void* hwnd);
    void Reset();
    typedef void (*DeviceResetCallbackFunc)();
    void SetDeviceResetCallback(DeviceResetCallbackFunc callback);
#endif

    void GetBackBufferData(uint32_t x, uint32_t y, uint32_t w, uint32_t h, uint8_t* data, size_t stride);

    uint64_t CreateQueryHandle();

    CommandContext* GetCommandContext() const;

    ID3D12Device* GetD3DDevice() const;
    unsigned int GetBackBufferCount() const;
    CommandList* BeginCommandList() const;
    CommandQueue* GetCommandQueue() const;
    Heaps* GetGraphicsHeaps() const;
    D3D12MA::Allocator* GetAllocator() const;
    D3D12MA::Pool* GetTransientBufferPool() const;
    Texture* GetMainTarget() const noexcept;
};

}
