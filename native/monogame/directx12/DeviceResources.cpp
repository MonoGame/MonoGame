// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "directx12.h"

#include "DeviceResources.h"
#include "GraphicsEnums.h"
#include "Texture.h"
#include "Sampler.h"
#include "PipelineState.h"
#include "CommandContext.h"

using namespace DirectX;
using namespace DX;
using namespace Graphics;

struct DeviceResources::Impl {
private:
    Microsoft::WRL::ComPtr<ID3D12Device> m_d3dDevice;
#if defined(_GAMING_XBOX)
    D3D12XBOX_FRAME_PIPELINE_TOKEN m_framePipelineToken = {};
#else
    Microsoft::WRL::ComPtr<IDXGIFactory6> m_dxgiFactory;
    Microsoft::WRL::ComPtr<IDXGISwapChain3> m_swapChain;

    DeviceResources::DeviceResetCallbackFunc m_lostCbk = nullptr;

    HWND m_window;
#endif

    uint32_t m_backBufferIndex = 0;
    MGSurfaceFormat m_backBufferFormat;
    uint32_t m_backBufferCount;
    Texture* m_displayTargets[MAX_BACK_BUFFER_COUNT];
    unsigned long m_fenceValues[MAX_BACK_BUFFER_COUNT];

    Texture* m_msaaTargets[MAX_BACK_BUFFER_COUNT];
    bool m_msaaEnabled = false;

    std::unique_ptr<CommandListPool> m_commandListPool;
    std::unique_ptr<CommandContext> m_commandContext;
    std::unique_ptr<Heaps> m_heaps;
    Microsoft::WRL::ComPtr<D3D12MA::Allocator> m_allocator;
    Microsoft::WRL::ComPtr<D3D12MA::Pool> m_transientBufferPool;

public:
    Impl(MGSurfaceFormat backBufferFormat, unsigned int backBufferCount = 2) noexcept(false) {
        if (backBufferCount < 2 || backBufferCount > MAX_BACK_BUFFER_COUNT)
            throw std::out_of_range("invalid backBufferCount");

        memset(&m_fenceValues, 0, sizeof(m_fenceValues));
        memset(&m_displayTargets, 0, sizeof(m_displayTargets));
        memset(&m_msaaTargets, 0, sizeof(m_msaaTargets));
        m_backBufferFormat = backBufferFormat;
        m_backBufferCount = backBufferCount;
    }

    ~Impl() {
        // Ensure that the GPU is no longer referencing resources that are about to be destroyed.
        WaitForGpu();

#if defined(_GAMING_XBOX)
        // Ensure we present a blank screen before cleaning up resources.
        m_commandListPool->GetCommandQueue()->PresentX(0, nullptr, nullptr);
#endif

        for (UINT n = 0; n < m_backBufferCount; n++) {
            delete m_displayTargets[n];
            if (m_msaaEnabled)
                delete m_msaaTargets[n];
        }
        m_commandContext.reset();
    }

#if defined(_GAMING_XBOX)
    void CreateDeviceResources(DeviceResources* device)
#else
    void CreateDeviceResources(DeviceResources* device, IDXGIFactory6* factory, IDXGIAdapter1* adapter)        
#endif
    {
#if defined(_GAMING_XBOX)
        D3D12XBOX_CREATE_DEVICE_PARAMETERS params = {};
        params.Version = D3D12_SDK_VERSION;

#if defined(_DEBUG)
        params.ProcessDebugFlags = D3D12_PROCESS_DEBUG_FLAG_DEBUG_LAYER_ENABLED; // Enable the debug layer.
#elif defined(PROFILE)
        params.ProcessDebugFlags = D3D12XBOX_PROCESS_DEBUG_FLAG_INSTRUMENTED; // Enable the instrumented driver.
#endif

        params.GraphicsCommandQueueRingSizeBytes = static_cast<UINT>(D3D12XBOX_DEFAULT_SIZE_BYTES);
        params.DisableGeometryShaderAllocations = TRUE;
        params.DisableTessellationShaderAllocations = TRUE;
#if defined(_GAMING_XBOX_SCARLETT)
        params.DisableDXR = TRUE;
        params.CreateDeviceFlags = D3D12XBOX_CREATE_DEVICE_FLAG_NONE;
#endif

        HRESULT hr = D3D12XboxCreateDevice(
            nullptr,
            &params,
            IID_GRAPHICS_PPV_ARGS(m_d3dDevice.ReleaseAndGetAddressOf()));
        ThrowIfFailed(hr);

#else

        DWORD dxgiFactoryFlags = 0;

#if defined(_DEBUG)
        // Enable the debug layer (requires the Graphics Tools "optional feature").
        //
        // NOTE: Enabling the debug layer after device creation will invalidate the active device.
        Microsoft::WRL::ComPtr<ID3D12Debug> debugController;
        Microsoft::WRL::ComPtr<IDXGIInfoQueue> dxgiInfoQueue;
        {
            if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(debugController.GetAddressOf())))) {
               debugController->EnableDebugLayer();
            } else {
                OutputDebugStringA("WARNING: Direct3D Debug Device is not available\n");
            }

            if (SUCCEEDED(DXGIGetDebugInterface1(0, IID_PPV_ARGS(dxgiInfoQueue.GetAddressOf())))) {
                dxgiFactoryFlags = DXGI_CREATE_FACTORY_DEBUG;

                dxgiInfoQueue->SetBreakOnSeverity(DXGI_DEBUG_ALL, DXGI_INFO_QUEUE_MESSAGE_SEVERITY_ERROR, true);
                dxgiInfoQueue->SetBreakOnSeverity(DXGI_DEBUG_ALL, DXGI_INFO_QUEUE_MESSAGE_SEVERITY_CORRUPTION, true);

                DXGI_INFO_QUEUE_MESSAGE_ID hide[] =
                {
                    80 // IDXGISwapChain::GetContainingOutput: The swapchain's adapter does not control the output on which the swapchain's window resides. 
                };
                DXGI_INFO_QUEUE_FILTER filter = {};
                filter.DenyList.NumIDs = static_cast<UINT>(std::size(hide));
                filter.DenyList.pIDList = hide;
                dxgiInfoQueue->AddStorageFilterEntries(DXGI_DEBUG_DXGI, &filter);
            }
        }
#endif

        m_dxgiFactory = factory;

        // Create the DX12 API device object.
        HRESULT hr;
        hr = D3D12CreateDevice(
            adapter,
            D3D_FEATURE_LEVEL_11_0,
            IID_PPV_ARGS(m_d3dDevice.ReleaseAndGetAddressOf())
        );
        ThrowIfFailed(hr);

#ifndef NDEBUG
        // Configure debug device (if active).
        Microsoft::WRL::ComPtr<ID3D12InfoQueue> d3dInfoQueue;
        if (SUCCEEDED(m_d3dDevice.As(&d3dInfoQueue))) {
#ifdef _DEBUG
            d3dInfoQueue->SetBreakOnSeverity(D3D12_MESSAGE_SEVERITY_CORRUPTION, true);
            d3dInfoQueue->SetBreakOnSeverity(D3D12_MESSAGE_SEVERITY_ERROR, true);
#endif
            D3D12_MESSAGE_ID hide[] =
            {
                D3D12_MESSAGE_ID_MAP_INVALID_NULLRANGE,
                D3D12_MESSAGE_ID_UNMAP_INVALID_NULLRANGE,
                // Workarounds for debug layer issues on hybrid-graphics systems
                D3D12_MESSAGE_ID_EXECUTECOMMANDLISTS_WRONGSWAPCHAINBUFFERREFERENCE,
                D3D12_MESSAGE_ID_RESOURCE_BARRIER_MISMATCHING_COMMAND_LIST_TYPE,
            };
            D3D12_INFO_QUEUE_FILTER filter = {};
            filter.DenyList.NumIDs = static_cast<UINT>(std::size(hide));
            filter.DenyList.pIDList = hide;
            d3dInfoQueue->AddStorageFilterEntries(&filter);
        }
#endif
#endif
        m_d3dDevice->SetName(L"DeviceResources");

#if defined(_GAMING_XBOX)
        RegisterFrameEvents();
#endif
        m_commandListPool = std::make_unique<CommandListPool>(m_d3dDevice.Get(), D3D12_COMMAND_LIST_TYPE_DIRECT, L"Graphics List");
        m_heaps = std::make_unique<Heaps>(m_d3dDevice.Get(), m_backBufferCount);

        {
            D3D12MA::ALLOCATOR_DESC desc = {};
            desc.Flags = D3D12MA::ALLOCATOR_FLAG_DEFAULT_POOLS_NOT_ZEROED;
            desc.pDevice = m_d3dDevice.Get();
#if !defined(_GAMING_XBOX)
            desc.pAdapter = adapter;
#else
            Microsoft::WRL::ComPtr<IDXGIDevice1> dxgiDevice;
            Microsoft::WRL::ComPtr<IDXGIAdapter> dxgiAdapter;
            auto hr = m_d3dDevice.As(&dxgiDevice);
            hr = dxgiDevice->GetAdapter(dxgiAdapter.GetAddressOf());
            desc.pAdapter = dxgiAdapter.Get();
#endif
            D3D12MA::CreateAllocator(&desc, &m_allocator);

            D3D12MA::POOL_DESC poolDesc = {};
            poolDesc.HeapProperties.Type = D3D12_HEAP_TYPE_UPLOAD; // We use an UPLOAD heap for temporary VBs/IBs (best for CPU-write-once, GPU-read-once data cf https://learn.microsoft.com/en-us/windows/win32/api/d3d12/ne-d3d12-d3d12_heap_type#constants)
            poolDesc.Flags = D3D12MA::POOL_FLAG_ALGORITHM_LINEAR;
            poolDesc.HeapFlags = D3D12_HEAP_FLAG_ALLOW_ONLY_BUFFERS;
            poolDesc.BlockSize = MAX_BACK_BUFFER_COUNT * MAX_BUFFER_PER_FRAME * D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT; // Alignment of buffers is always 64KB
            poolDesc.MinBlockCount = poolDesc.MaxBlockCount = 1;
            poolDesc.MaxBlockCount = 1;
            m_allocator->CreatePool(&poolDesc, &m_transientBufferPool);
        }

        m_commandContext = std::make_unique<CommandContext>(device);
    }

    // TODO: all that should probably be moved to the MG backend
    void CreateWindowSizeDependentResources(DeviceResources* device, unsigned int width, unsigned int height, float r, float g, float b, float a, int msaaCount) {
        WaitForGpu();

#if defined(_GAMING_XBOX)
        m_commandListPool->GetCommandQueue()->PresentX(0, nullptr, nullptr); // present a blank screen before cleaning up resources
#endif

        // Release resources that are tied to the swap chain and update fence values.
        for (UINT n = 0; n < m_backBufferCount; n++) {
            delete m_displayTargets[n];
            m_fenceValues[n] = m_fenceValues[m_backBufferIndex];
            if (m_msaaEnabled)
                delete m_msaaTargets[n];
        }
        m_msaaEnabled = false;

#if defined(_GAMING_XBOX)
        for (UINT n = 0; n < m_backBufferCount; n++) {
            m_displayTargets[n] = new Texture(SurfaceType::SwapChainRenderTarget, TextureDimension::Texture2D, width, height, 1, m_backBufferFormat);
            m_displayTargets[n]->SetClearColor(r, g, b, a);
            m_displayTargets[n]->Create(device);
        }

        m_backBufferIndex = 0;
#else
        const DXGI_FORMAT backBufferFormat = TextureFormatToDXGI_FORMAT(m_backBufferFormat);

        // If the swap chain already exists, resize it, otherwise create one.
        if (m_swapChain) {
            bool lost = HandleLost(m_swapChain->ResizeBuffers(m_backBufferCount, width, height, backBufferFormat, DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH));
            if (lost) return;
        } else {
            // Create a descriptor for the swap chain.
            DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
            swapChainDesc.Width = width;
            swapChainDesc.Height = height;
            swapChainDesc.Format = backBufferFormat;
            swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
            swapChainDesc.BufferCount = m_backBufferCount;
            swapChainDesc.SampleDesc.Count = 1;
            swapChainDesc.SampleDesc.Quality = 0;
            swapChainDesc.Scaling = DXGI_SCALING_STRETCH;
            swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
            swapChainDesc.AlphaMode = DXGI_ALPHA_MODE_IGNORE;
            swapChainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;

            DXGI_SWAP_CHAIN_FULLSCREEN_DESC fsSwapChainDesc = {};
            fsSwapChainDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;
            fsSwapChainDesc.Windowed = TRUE;

            // Create a swap chain for the window.
            Microsoft::WRL::ComPtr<IDXGISwapChain1> swapChain;
            ThrowIfFailed(m_dxgiFactory->CreateSwapChainForHwnd(
                m_commandListPool->GetCommandQueue()->Get(),
                m_window,
                &swapChainDesc,
                &fsSwapChainDesc,
                nullptr,
                swapChain.GetAddressOf()
            ));

            ThrowIfFailed(swapChain.As(&m_swapChain));

            // This class does not support exclusive full-screen mode and prevents DXGI from responding to the ALT+ENTER shortcut
            ThrowIfFailed(m_dxgiFactory->MakeWindowAssociation(m_window, DXGI_MWA_NO_WINDOW_CHANGES | DXGI_MWA_NO_ALT_ENTER | DXGI_MWA_NO_PRINT_SCREEN));
        }

        // Obtain the back buffers for this window which will be the final render targets
        // and create render target views for each of them.
        for (UINT n = 0; n < m_backBufferCount; n++)
            m_displayTargets[n] = new Texture(device, m_swapChain.Get(), n);

        if (msaaCount > 1) {
            m_msaaEnabled = true;
            for (UINT n = 0; n < m_backBufferCount; n++) {
                m_msaaTargets[n] = new Texture(SurfaceType::RenderTarget, TextureDimension::Texture2D, width, height, 1, m_backBufferFormat);
                m_msaaTargets[n]->SetClearColor(r, g, b, a);
                m_msaaTargets[n]->SetMSAA(msaaCount);
                if (!m_msaaTargets[n]->CheckMSAA(m_d3dDevice.Get())) {
                    delete m_msaaTargets[n];
                    m_msaaEnabled = false;
                    break;
                }
                m_msaaTargets[n]->Create(device);
            }
        }

        m_backBufferIndex = m_swapChain->GetCurrentBackBufferIndex();
#endif
    }

    uint32_t Prepare() {
        m_commandListPool->GetCommandQueue()->WaitForFenceCPUBlocking(m_fenceValues[m_backBufferIndex]); // wait if the m_backBufferCount-th previous frame is still in flight

        m_heaps->Prepare(m_backBufferIndex);
        m_commandContext->Reset(m_backBufferIndex);

        GetMainTarget()->Transition(m_commandContext->GetCommandList(), D3D12_RESOURCE_STATE_RENDER_TARGET);

        return m_backBufferIndex;
    }

    void WaitForGpu() noexcept {
        m_commandListPool->GetCommandQueue()->SignalFence();
        m_commandListPool->GetCommandQueue()->WaitForIdle();
    }

    // Code common between Present and PresentX
    void BeforePresent() {
        if (m_msaaEnabled)
            m_commandContext->ResolveResource(GetMainTarget(), GetDisplayTarget());
        GetDisplayTarget()->Transition(m_commandContext->GetCommandList(), D3D12_RESOURCE_STATE_PRESENT);

        // Send the command list and store the fence value for us to wait on it later
        m_fenceValues[m_backBufferIndex] = m_commandContext->Close();
    }

#if defined(_GAMING_XBOX)
    void PresentX() {
        BeforePresent();

        D3D12XBOX_PRESENT_PLANE_PARAMETERS planeParameters = {};
        planeParameters.Token = m_framePipelineToken;
        planeParameters.ResourceCount = 1;
        planeParameters.ppResources = GetDisplayTarget()->GetAddressOf();

        m_commandListPool->GetCommandQueue()->PresentX(1, &planeParameters, nullptr);

        m_backBufferIndex = (m_backBufferIndex + 1) % m_backBufferCount;
    }
#else
    void Present(UINT sync, UINT flags) {
        BeforePresent();

        HandleLost(m_swapChain->Present(sync, flags));

        m_fenceValues[m_backBufferIndex] = m_commandListPool->GetCommandQueue()->SignalFence();

        m_backBufferIndex = m_swapChain->GetCurrentBackBufferIndex();
    }

    void SetWindow(HWND window) noexcept {
        m_window = window;
    }

    bool HandleLost(HRESULT hr) {
        // If the device was reset we must completely reinitialize the renderer.
        if (hr != DXGI_ERROR_DEVICE_REMOVED && hr != DXGI_ERROR_DEVICE_RESET) {
            ThrowIfFailed(hr);
            return false;
        }

        m_lostCbk(); // let the C# dictate the order of reset
        return true;
    }

    void Reset() {
        for (UINT n = 0; n < m_backBufferCount; n++) {
            delete m_displayTargets[n];
            if (m_msaaEnabled)
                delete m_msaaTargets[n];
        }
        memset(&m_fenceValues, 0, sizeof(m_fenceValues));
        memset(&m_displayTargets, 0, sizeof(m_displayTargets));
        memset(&m_msaaTargets, 0, sizeof(m_msaaTargets));

        m_commandContext.reset();
        m_commandListPool.reset();
        m_heaps.reset();
        m_transientBufferPool.Reset();
        m_swapChain.Reset();
        m_d3dDevice.Reset();
        m_dxgiFactory.Reset();
        m_allocator.Reset();
    }
#endif

#if defined(_GAMING_XBOX)
    void Suspend() {
        m_commandListPool->GetCommandQueue()->SuspendX(0);
    }

    void Resume() {
        m_commandListPool->GetCommandQueue()->ResumeX();

        RegisterFrameEvents();
    }

    void WaitForOrigin() {
        // Wait until frame start is signaled
        m_framePipelineToken = D3D12XBOX_FRAME_PIPELINE_TOKEN_NULL;
        ThrowIfFailed(m_d3dDevice->WaitFrameEventX(
            D3D12XBOX_FRAME_EVENT_ORIGIN,
            INFINITE,
            nullptr,
            D3D12XBOX_WAIT_FRAME_EVENT_FLAG_NONE,
            &m_framePipelineToken));
    }
#endif

private:
    Texture* GetMainTarget() const noexcept { return m_msaaEnabled ? m_msaaTargets[m_backBufferIndex] : m_displayTargets[m_backBufferIndex]; }
    Texture* GetDisplayTarget() const noexcept { return m_displayTargets[m_backBufferIndex]; }

#if defined(_GAMING_XBOX)
    void RegisterFrameEvents() {
        Microsoft::WRL::ComPtr<IDXGIDevice1> dxgiDevice;
        auto hr = m_d3dDevice.As(&dxgiDevice);
        assert(SUCCEEDED(hr));

        // Identify the physical adapter (GPU or card) this device is running on.
        Microsoft::WRL::ComPtr<IDXGIAdapter> dxgiAdapter;
        hr = dxgiDevice->GetAdapter(dxgiAdapter.GetAddressOf());
        assert(SUCCEEDED(hr));

        // Retrieve the outputs for the adapter.
        Microsoft::WRL::ComPtr<IDXGIOutput> dxgiOutput;
        hr = dxgiAdapter->EnumOutputs(0, dxgiOutput.GetAddressOf());
        assert(SUCCEEDED(hr));

        // Set frame interval and register for frame events
        ThrowIfFailed(m_d3dDevice->SetFrameIntervalX(
            dxgiOutput.Get(),
            D3D12XBOX_FRAME_INTERVAL_60_HZ,
            m_backBufferCount - 1u /* Allow n-1 frames of latency */,
            D3D12XBOX_FRAME_INTERVAL_FLAG_NONE));

        ThrowIfFailed(m_d3dDevice->ScheduleFrameEventX(
            D3D12XBOX_FRAME_EVENT_ORIGIN,
            0U,
            nullptr,
            D3D12XBOX_SCHEDULE_FRAME_EVENT_FLAG_NONE));
    }
#else
    // This method acquires the first available hardware adapter that supports Direct3D 12.
    // If no such adapter can be found, try WARP. Otherwise throw an exception.
    void GetAdapter(IDXGIAdapter1** ppAdapter) {
        *ppAdapter = nullptr;

        Microsoft::WRL::ComPtr<IDXGIAdapter1> adapter;
        for (UINT adapterIndex = 0;
             SUCCEEDED(m_dxgiFactory->EnumAdapterByGpuPreference(
                 adapterIndex,
                 DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE,
                 IID_PPV_ARGS(adapter.ReleaseAndGetAddressOf())));
             adapterIndex++) {
            DXGI_ADAPTER_DESC1 desc;
            ThrowIfFailed(adapter->GetDesc1(&desc));

            if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE) {
                // Don't select the Basic Render Driver adapter.
                continue;
            }

            // Check to see if the adapter supports Direct3D 12, but don't create the actual device yet.
            if (SUCCEEDED(D3D12CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_11_0, __uuidof(ID3D12Device), nullptr))) {
#ifdef _DEBUG
                wchar_t buff[256] = {};
                swprintf_s(buff, L"Direct3D Adapter (%u): VID:%04X, PID:%04X - %ls\n", adapterIndex, desc.VendorId, desc.DeviceId, desc.Description);
                OutputDebugStringW(buff);
#endif
                break;
            }
        }

#if !defined(NDEBUG)
        if (!adapter) {
            // Try WARP12 instead
            if (FAILED(m_dxgiFactory->EnumWarpAdapter(IID_PPV_ARGS(adapter.ReleaseAndGetAddressOf())))) {
                throw std::runtime_error("WARP12 not available. Enable the 'Graphics Tools' optional feature");
            }

            OutputDebugStringA("Direct3D Adapter - WARP12\n");
        }
#endif

        if (!adapter) {
            throw std::runtime_error("No Direct3D 12 device found");
        }

        *ppAdapter = adapter.Detach();
    }
#endif // _GAMING_XBOX

    friend class DeviceResources;
};

// Constructor for DeviceResources.
DeviceResources::DeviceResources(
    MGSurfaceFormat backBufferFormat,
    uint32_t backBufferCount) {
    pImpl = new Impl(backBufferFormat, backBufferCount);
}

DeviceResources::~DeviceResources() {
    delete pImpl;
}

#if defined(_GAMING_XBOX)
void DeviceResources::CreateDeviceResources()
{
    pImpl->CreateDeviceResources(this);
}
#else
void DeviceResources::CreateDeviceResources(IDXGIFactory6* factory, IDXGIAdapter1* adapter)
{
    pImpl->CreateDeviceResources(this, factory, adapter);
}
#endif

// These resources need to be recreated every time the window size is changed.
void DeviceResources::CreateWindowSizeDependentResources(int width, int height, float r, float g, float b, float a, int msaaCount) {
    pImpl->CreateWindowSizeDependentResources(this, width, height, r, g, b, a, msaaCount);
}

// Prepare the command list and render target for rendering.
uint32_t DeviceResources::Prepare() {
    return pImpl->Prepare();
}

void DeviceResources::WaitForGpu() {
    pImpl->WaitForGpu();
}

#if defined(_GAMING_XBOX)
// Present the contents of the swap chain to the screen.
void DeviceResources::PresentX() {
    pImpl->PresentX();
}
// Handle GPU suspend/resume
void DeviceResources::Suspend() {
    pImpl->Suspend();
}

void DeviceResources::Resume() {
    pImpl->Resume();
}

// For PresentX rendering, we should wait for the origin event just before processing input.
void DeviceResources::WaitForOrigin() {
    pImpl->WaitForOrigin();
}
#else
void DeviceResources::Present(int sync, int flags) {
    pImpl->Present(sync, flags);
}

void DeviceResources::SetWindow(void* hwnd) {
    pImpl->SetWindow((HWND)hwnd);
}

void DeviceResources::SetDeviceResetCallback(DeviceResetCallbackFunc callback) {
    pImpl->m_lostCbk = callback;
}

void DeviceResources::Reset() {
    pImpl->Reset();
}
#endif


void DeviceResources::GetBackBufferData(uint32_t x, uint32_t y, uint32_t w, uint32_t h, uint8_t* data, size_t stride) {
    if (pImpl->m_msaaEnabled)
        pImpl->m_commandContext->ResolveResource(pImpl->GetMainTarget(), pImpl->GetDisplayTarget());
    pImpl->GetDisplayTarget()->GetData(this, 0, x, y, w, h, data, stride, stride);
}

uint64_t DeviceResources::CreateQueryHandle() {
    return pImpl->m_heaps->CreateQueryHandle();
}

CommandContext* Graphics::DeviceResources::GetCommandContext() const {
    return pImpl->m_commandContext.get();
}

ID3D12Device* Graphics::DeviceResources::GetD3DDevice() const {
    return pImpl->m_d3dDevice.Get();
}

unsigned int Graphics::DeviceResources::GetBackBufferCount() const {
    return pImpl->m_backBufferCount;
}

Graphics::CommandList* DeviceResources::BeginCommandList() const {
    return pImpl->m_commandListPool->Begin();
}

CommandQueue* DeviceResources::GetCommandQueue() const {
    return pImpl->m_commandListPool->GetCommandQueue();
}

Heaps* Graphics::DeviceResources::GetGraphicsHeaps() const {
    return pImpl->m_heaps.get();
}

D3D12MA::Allocator* Graphics::DeviceResources::GetAllocator() const {
    return pImpl->m_allocator.Get();
}

D3D12MA::Pool* Graphics::DeviceResources::GetTransientBufferPool() const {
    return pImpl->m_transientBufferPool.Get();
}

Texture* Graphics::DeviceResources::GetMainTarget() const noexcept {
    return pImpl->GetMainTarget();
}
