// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGG.h"

#include "mg_common.h"

#include "directx12.h"
#include "CommandContext.h"
#include "DeviceResources.h"
#include "PipelineState.h"
#include "Texture.h"
#include "Sampler.h"

#ifdef _WIN32
#include <Windows.h>
#include "directx12.resources.h"
#endif

#if defined(MG_SDL2)
#include <SDL_video.h>
#include <SDL_syswm.h>
#endif

using namespace Graphics;
using namespace Microsoft::WRL;

typedef mguint FrameCounter;

const FrameCounter kFreeFrames = 2;

static void MGDX_DestroyFrameResources(MGG_GraphicsDevice* device, mgint currentFrame, mgbool free_all);

struct MGG_GraphicsAdapter
{
	Microsoft::WRL::ComPtr<IDXGIAdapter1> adapter;

	std::string description;

	DXGI_ADAPTER_DESC1 desc;

	MGG_DisplayMode current = { MGSurfaceFormat::Color, 0, 0 };

	std::vector<MGG_DisplayMode> modes;
};

const int MAX_TEXTURE_SLOTS = 16;

struct MGG_GraphicsDevice
{
	FrameCounter frame = 0;

	DeviceResources* resources = nullptr;
	CommandContext* context = nullptr;
	PipelineStateManager* pipelineManager = nullptr;

	Texture* depthTexture = nullptr;

	MGG_InputLayout* layout = nullptr;

	// TODO: This is bad... need to support multiple constant buffers!
	MGG_Buffer* uniforms[2] = { nullptr, nullptr };
	uint32_t uniformsDirty = 0xFFFFFFFF;

	uint64_t vertexBuffersDirty = 0xFFFFFFFF;
	MGG_Buffer* vertexBuffers[16] = { 0 };
	uint32_t vertexOffsets[16] = { 0 };

	MGG_Buffer* indexBuffer = nullptr;
	MGIndexElementSize indexBufferSize = MGIndexElementSize::SixteenBits;
	bool indexBufferDirty = false;

	MGG_Texture* textures[2][MAX_TEXTURE_SLOTS];
	bool texturesDirty = false;

	MGG_SamplerState* samplers[2][MAX_TEXTURE_SLOTS];
	bool samplersDirty = false;

	CD3DX12_VIEWPORT viewport;
	bool viewportDirty = false;

	CD3DX12_RECT scissor;
	bool scissorDirty = false;

	bool scissorTestEnable = false;

	std::queue<MGG_Buffer*> destroyBuffers;
	std::queue<MGG_Texture*> destroyTextures;

	std::vector<MGG_Buffer*> discarded;
	std::vector<MGG_Buffer*> pending;
	std::vector<MGG_Buffer*> free;
};

struct MGG_Buffer
{
	mgint frame;

	MGBufferType type;

	// heapType = BufferType::Static;
	size_t dataSize = 0;
	size_t actualSize = 0;
	D3D12_RESOURCE_STATES m_currentState; // for static buffers

	Microsoft::WRL::ComPtr<D3D12MA::Allocation> m_alloc;
	Microsoft::WRL::ComPtr<ID3D12Resource> m_res;

	inline D3D12_GPU_VIRTUAL_ADDRESS GpuAddress() { return m_res->GetGPUVirtualAddress(); }
};

struct MGG_Texture
{
	mgint frame;

	MGSurfaceFormat format;
	Texture* texture = nullptr;
};

struct MGG_InputLayout
{
	std::vector<uint32_t> streamStrides;
	std::vector<D3D12_INPUT_ELEMENT_DESC> elements;
};

struct MGG_Shader
{
	MGShaderStage stage;
	std::vector<uint8_t> bytecode;
};

struct MGG_BlendState
{
	D3D12_BLEND_DESC desc;
	bool blending = false;
};

struct MGG_DepthStencilState
{
	D3D12_DEPTH_STENCIL_DESC desc;
};

struct MGG_RasterizerState
{
	D3D12_RASTERIZER_DESC desc;
	mgbool scissorTestEnable = false;
};

struct MGG_SamplerState
{
	Sampler* sampler = nullptr;
};

struct MGG_OcclusionQuery
{
	uint64_t handle;

	Microsoft::WRL::ComPtr<ID3D12Resource> buffer;
	Microsoft::WRL::ComPtr<D3D12MA::Allocation> alloc;

	uint64_t fence;
};

struct MGG_GraphicsSystem
{
#if defined(_GAMING_XBOX)
	
#else
	Microsoft::WRL::ComPtr<IDXGIFactory6> dxgiFactory;
#endif

	std::vector<MGG_GraphicsAdapter*> adapters;
};

void MGG_EffectResource_GetBytecode(mgbyte* name, mgbyte*& bytecode, mgint& size)
{
	bytecode = nullptr;
	size = 0;

	// TODO: Move this to use the new MGFXC header generation.
	// 
	// Get the handle of this DLL.
	HMODULE module;
	::GetModuleHandleExA(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, (LPCSTR)&MGG_EffectResource_GetBytecode, &module);

	LPCWSTR id = L"";

	if (strcmp((const char*)name, "AlphaTestEffect") == 0)
		id = MAKEINTRESOURCEW(C_AlphaTestEffect);
	else if (strcmp((const char*)name, "BasicEffect") == 0)
		id = MAKEINTRESOURCEW(C_BasicEffect);
	else if (strcmp((const char*)name, "DualTextureEffect") == 0)
		id = MAKEINTRESOURCEW(C_DualTextureEffect);
	else if (strcmp((const char*)name, "EnvironmentMapEffect") == 0)
		id = MAKEINTRESOURCEW(C_EnvironmentMapEffect);
	else if (strcmp((const char*)name, "SkinnedEffect") == 0)
		id = MAKEINTRESOURCEW(C_SkinnedEffect);
	else if (strcmp((const char*)name, "SpriteEffect") == 0)
		id = MAKEINTRESOURCEW(C_SpriteEffect);

	auto handle = ::FindResourceW(module, id, L"BIN");
	if (handle == nullptr)
		return;

	size = ::SizeofResource(module, handle);
	if (size == 0)
		return;

	HGLOBAL global = ::LoadResource(module, handle);
	if (global == nullptr)
		return;

	bytecode = (mgbyte*)LockResource(global);
}

MGG_GraphicsSystem* MGG_GraphicsSystem_Create()
{
	auto system = new MGG_GraphicsSystem();

#if defined(_GAMING_XBOX)

	// One fake adapter for now.
	auto adapter = new MGG_GraphicsAdapter();
	system->adapters.push_back(adapter);

#else

	DWORD dxgiFactoryFlags = 0;

#if defined(_DEBUG)

	// Enable the debug layer (requires the Graphics Tools "optional feature").
	//
	// NOTE: Enabling the debug layer after device creation will invalidate the active device.
	Microsoft::WRL::ComPtr<ID3D12Debug> debugController;
	Microsoft::WRL::ComPtr<IDXGIInfoQueue> dxgiInfoQueue;
	{
		if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(debugController.GetAddressOf()))))
			debugController->EnableDebugLayer();
		else
			OutputDebugStringA("WARNING: Direct3D Debug Device is not available\n");

		if (SUCCEEDED(DXGIGetDebugInterface1(0, IID_PPV_ARGS(dxgiInfoQueue.GetAddressOf()))))
		{
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

	HRESULT hr;
	{
		Microsoft::WRL::ComPtr<IDXGIFactory2> dxgiFactory2;
		hr = CreateDXGIFactory2(dxgiFactoryFlags, IID_PPV_ARGS(dxgiFactory2.ReleaseAndGetAddressOf()));
		DX::ThrowIfFailed(hr);

		hr = dxgiFactory2->QueryInterface(IID_PPV_ARGS(system->dxgiFactory.ReleaseAndGetAddressOf()));
		DX::ThrowIfFailed(hr);
	}

	// Gather the physical devices.
	{
		Microsoft::WRL::ComPtr<IDXGIAdapter1> adapter1;
		for (UINT adapterIndex = 0;
			SUCCEEDED(system->dxgiFactory->EnumAdapterByGpuPreference(
				adapterIndex,
				DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE,
				IID_PPV_ARGS(adapter1.ReleaseAndGetAddressOf())));
			adapterIndex++)
		{
			DXGI_ADAPTER_DESC1 desc;
			if (FAILED(adapter1->GetDesc1(&desc)))
				continue;

			// Check to see if the adapter supports Direct3D 12, but don't create the actual device yet.
			if (FAILED(D3D12CreateDevice(adapter1.Get(), D3D_FEATURE_LEVEL_11_0, __uuidof(ID3D12Device), nullptr)))
				continue;

			// Keep it!
			auto adapter = new MGG_GraphicsAdapter();
			{
				int size_needed = WideCharToMultiByte(CP_UTF8, 0, desc.Description, -1, NULL, 0, NULL, NULL);
				adapter->description.resize(size_needed - 1, '\0');
				WideCharToMultiByte(CP_UTF8, 0, desc.Description, -1, &adapter->description[0], size_needed, NULL, NULL);
			}
			adapter->adapter = adapter1;
			adapter->desc = desc;
			system->adapters.push_back(adapter);
		}
	}

#endif

	return system;
}

void MGG_GraphicsSystem_Destroy(MGG_GraphicsSystem* system)
{
	assert(system != nullptr);

	// Delete all adapters.
	for (auto adapter : system->adapters)
		delete adapter;

	// Delete the system.
	delete system;
}

MGG_GraphicsAdapter* MGG_GraphicsAdapter_Get(MGG_GraphicsSystem* system, mgint index)
{
	if (index < 0 || index >= system->adapters.size())
		return nullptr;

	return system->adapters[index];
}

void MGG_GraphicsAdapter_GetInfo(MGG_GraphicsAdapter* adapter, MGG_GraphicsAdaptor_Info& info)
{
	assert(adapter != nullptr);

	info.DeviceName = (void*)adapter->description.data();
	info.Description = (void*)adapter->description.data();
	info.DeviceId = adapter->desc.DeviceId;
	info.VendorId = adapter->desc.VendorId;
	info.SubSystemId = adapter->desc.SubSysId;
	info.Revision = adapter->desc.Revision;
	info.MonitorHandle = 0;

	MGG_DisplayMode currentDisplayMode
	{
		MGSurfaceFormat::Color, 1, 1,
	};

	if (adapter->modes.size() == 0)
	{
#if defined(_GAMING_XBOX)

#else
		UINT i = 0;
		IDXGIOutput* pOutput;
		while (adapter->adapter->EnumOutputs(i, &pOutput) != DXGI_ERROR_NOT_FOUND)
		{
			DXGI_OUTPUT_DESC desc;
			pOutput->GetDesc(&desc);

			// The first output device is the primary monitor on the desktop.
			if (i == 0)
			{
				info.MonitorHandle = desc.Monitor;

				currentDisplayMode.width = desc.DesktopCoordinates.right - desc.DesktopCoordinates.left;
				currentDisplayMode.height = desc.DesktopCoordinates.bottom - desc.DesktopCoordinates.top;
				currentDisplayMode.format = MGSurfaceFormat::Color;
			}

			UINT num = 0;
			DXGI_FORMAT format = DXGI_FORMAT_R8G8B8A8_UNORM;
			UINT flags = DXGI_ENUM_MODES_INTERLACED;
			pOutput->GetDisplayModeList(format, flags, &num, 0);

			DXGI_MODE_DESC* pDescs = new DXGI_MODE_DESC[num];
			pOutput->GetDisplayModeList(format, flags, &num, pDescs);

			for (int j = 0; j < num; j++)
			{
				MGG_DisplayMode mode;
				mode.width = pDescs[j].Width;
				mode.height = pDescs[j].Height;
				mode.format = MGSurfaceFormat::Color;

				bool found = false;
				for (auto m : adapter->modes)
				{
					if (m.width == mode.width &&
						m.height == mode.height)
					{
						found = true;
						break;
					}
				}

				if (!found)
					adapter->modes.push_back(mode);
			}

			delete[] pDescs;

			pOutput->Release();
			++i;
		}
#endif
	}

	info.DisplayModeCount = adapter->modes.size();
	info.DisplayModes = adapter->modes.data();
	info.CurrentDisplayMode = currentDisplayMode;
}

MGG_GraphicsDevice* MGG_GraphicsDevice_Create(MGG_GraphicsSystem* system, MGG_GraphicsAdapter* adapter)
{
	assert(system != nullptr);
	assert(adapter != nullptr);

	auto device = new MGG_GraphicsDevice();

	// TODO: Don't initialize the back buffer here... just the device.
	device->resources = new DeviceResources(MGSurfaceFormat::Color, 2);
#if defined(_GAMING_XBOX)
	device->resources->CreateDeviceResources();
#else
	device->resources->CreateDeviceResources(system->dxgiFactory.Get(), adapter->adapter.Get());
#endif

	device->context = device->resources->GetCommandContext();
	device->pipelineManager = new PipelineStateManager(device->resources);

	return device;
}

void MGG_GraphicsDevice_Destroy(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);


	MGDX_DestroyFrameResources(device, 0, true);

	delete device;
}

void MGG_GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, MGG_GraphicsDevice_Caps& caps)
{
	assert(device != nullptr);

	// TODO: Get actual stats from the device!

	caps.MaxTextureSlots = MAX_TEXTURE_SLOTS;
	caps.MaxVertexBufferSlots = 16;
	caps.MaxVertexTextureSlots = 16;

	// The shader profile id from the pipeline.
#if defined(_GAMING_XBOX)	
	caps.ShaderProfile = 21;	// For Xbox One and Xbox Series X/S.
#else
	caps.ShaderProfile = 2;
#endif
}

void MGG_GraphicsDevice_ResizeSwapchain(
	MGG_GraphicsDevice* device,
	void* nativeWindowHandle,
	mgint width,
	mgint height,
	MGSurfaceFormat color,
	MGDepthFormat depth)
{
#if !defined(_GAMING_XBOX)

#if defined(MG_SDL2)
	auto sdl_window = (SDL_Window*)nativeWindowHandle;

	SDL_SysWMinfo windowInfo;
	SDL_VERSION(&windowInfo.version);
	SDL_GetWindowWMInfo(sdl_window, &windowInfo);
	device->resources->SetWindow(windowInfo.info.win.window);

#else
#error Not Implemented
#endif

	//resetCallback = OnDeviceLost;
	//DxDevice.SetDeviceResetCallback(resetCallback);
#endif

	int sampleCount = 1; // PresentationParameters.MultiSampleCount;
	//Vector4 clear = DiscardColor.ToVector4();
	device->resources->CreateWindowSizeDependentResources(width, height, 0, 0, 0, 0, sampleCount);

	if (device->depthTexture)
	{
		delete device->depthTexture;
		device->depthTexture = nullptr;
	}

	if (depth != MGDepthFormat::None)
	{
		device->depthTexture = new  Texture(width, height, depth);
		//if (sampleCount > 1)
			//DepthTexture.SetMSAA(sampleCount);
		device->depthTexture->Create(device->resources);
	}
}

mgint MGG_GraphicsDevice_BeginFrame(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);

#if defined(_GAMING_XBOX)
	device->resources->WaitForOrigin();
#endif

	auto frameIndex = device->resources->Prepare();

	device->pipelineManager->Prepare();
	device->indexBufferDirty = true;
	device->vertexBuffersDirty = 0xFFFFFFFF;
	memset(device->textures, 0, sizeof(device->textures));
	device->texturesDirty = true;
	memset(device->samplers, 0, sizeof(device->samplers));
	device->samplersDirty = true;
	device->viewportDirty = true;
	device->scissorDirty = true;

	return frameIndex;
}

void MGG_GraphicsDevice_Clear(MGG_GraphicsDevice* device, MGClearOptions options, Vector4& color, mgfloat depth, mgint stencil)
{
	assert(device != nullptr);

	// Make sure something was set to be cleared.
	if ((mgint)options == 0)
		return;

	device->context->Clear(options, color.X, color.Y, color.Z, color.W, depth, stencil);
}

static void MGDX_DestroyFrameResources(MGG_GraphicsDevice* device, mgint currentFrame, mgbool free_all)
{
	assert(device != nullptr);
	assert(currentFrame >= 0);

	// Delete resources that haven't been used in a few frames 
	{
		while (device->destroyBuffers.size() > 0)
		{
			auto buffer = device->destroyBuffers.front();
			auto diff = currentFrame - buffer->frame;
			if (!free_all && diff < kFreeFrames || (0xFFFF - diff) < kFreeFrames)
				break;

			device->destroyBuffers.pop();

			delete buffer;
		}

		while (device->destroyTextures.size() > 0)
		{
			auto texture = device->destroyTextures.front();
			auto diff = currentFrame - texture->frame;
			if (!free_all && diff < kFreeFrames || (0xFFFF - diff) < kFreeFrames)
				break;

			device->destroyTextures.pop();

			delete texture->texture;
			delete texture;
		}
	}
}

void MGG_GraphicsDevice_Present(MGG_GraphicsDevice* device, mgint currentFrame, mgint syncInterval)
{
	assert(device != nullptr);
	assert(syncInterval >= 0);
	assert(currentFrame >= 0);

#if !defined(_GAMING_XBOX)
	device->resources->Present(syncInterval, 0);
#else
	device->resources->PresentX();
#endif

	++device->frame;

	// Move the pending buffers to the free list 
	// for reuse on the next frame.
	device->free.insert(device->free.end(), device->pending.begin(), device->pending.end());
	device->pending.clear();

	// Buffers discarded this frame can be moved
	// into the pending list for a future frame.
	std::swap(device->pending, device->discarded);

	// Cleanup resources for the next frame.
	MGDX_DestroyFrameResources(device, device->frame, false);
}

void MGG_GraphicsDevice_SetBlendState(MGG_GraphicsDevice* device, MGG_BlendState* state, mgfloat factorR, mgfloat factorG, mgfloat factorB, mgfloat factorA)
{
	assert(device != nullptr);
	//assert(state != nullptr);

	auto& blendState = device->pipelineManager->impl->m_currentPSODesc.BlendState;
	blendState = state->desc;

	// Set the blend color factor if we're actually bending.
	if (state->blending)
	{
		auto cl = device->context->GetCommandList();

		FLOAT blendFactor[] = { factorR, factorG, factorB, factorA };
		cl->OMSetBlendFactor(blendFactor);
	}
}

void MGG_GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	auto& depthStencilState = device->pipelineManager->impl->m_currentPSODesc.DepthStencilState;
	depthStencilState = state->desc;
}

void MGG_GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
	assert(device != nullptr);
	//assert(state != nullptr);

	auto& rasterizerState = device->pipelineManager->impl->m_currentPSODesc.RasterizerState;
	rasterizerState = state->desc;
	device->scissorTestEnable = state->scissorTestEnable;
	device->scissorDirty = true;	
}

void MGG_GraphicsDevice_GetTitleSafeArea(mgint& x, mgint& y, mgint& width, mgint& height)
{
	// Nothing for PC here unless we want to support
	// things like Steam TV modes and we need platform
	// specific calls for that.
}

void MGG_GraphicsDevice_SetViewport(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, mgfloat minDepth, mgfloat maxDepth)
{
	assert(device != nullptr);

	device->viewport.TopLeftX = x;
	device->viewport.TopLeftY = y;
	device->viewport.Width = width;
	device->viewport.Height = height;
	device->viewport.MinDepth = minDepth;
	device->viewport.MaxDepth = maxDepth;
	device->viewportDirty = true;
}

void MGG_GraphicsDevice_SetScissorRectangle(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height)
{
	assert(device != nullptr);

	device->scissor.left = x;
	device->scissor.top = y;
	device->scissor.right = x + width;
	device->scissor.bottom = y + height;
	device->scissorDirty = true;
}

void MGG_GraphicsDevice_SetRenderTargets(MGG_GraphicsDevice* device, MGG_Texture** targets, mgint count)
{
	assert(device != nullptr);

	if (targets == nullptr || count == 0)
	{
		device->context->SetRenderTarget(nullptr, 0, device->depthTexture);
	}
	else
	{
		/*
		device->context->SetRenderTarget(
		DxCommandContext.SetRenderTarget(rtHandle.AddrOfPinnedObject(), (uint)_currentRenderTargetCount, (_currentRenderTargetBindings[0].RenderTarget as RenderTarget2D).DxDepthTexture);
		rtHandle.Free();
		var renderTarget = (IRenderTarget)_currentRenderTargetBindings[0].RenderTarget;
		*/
	}
}

void MGG_GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Buffer* buffer)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	// TODO: slot ??

	MGG_Buffer** uniforms = device->uniforms;

	if (uniforms[(int)stage] != buffer)
	{
		uniforms[(int)stage] = buffer;
		device->uniformsDirty |= 1 << (int)stage;
	}
	else
	{
		//if (buffer->dirty)
			device->uniformsDirty |= 1 << (int)stage;
	}
}

void MGG_GraphicsDevice_SetTexture(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Texture* texture)
{
	assert(device != nullptr);
	assert(slot >= 0);
	assert(slot < MAX_TEXTURE_SLOTS);

	device->textures[(int)stage][slot] = texture;
	device->texturesDirty = true;
}

void MGG_GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_SamplerState* state)
{
	assert(device != nullptr);
	assert(slot >= 0);
	assert(slot < MAX_TEXTURE_SLOTS);

	device->samplers[(int)stage][slot] = state;
	device->samplersDirty = true;
}

void MGG_GraphicsDevice_SetIndexBuffer(MGG_GraphicsDevice* device, MGIndexElementSize size, MGG_Buffer* buffer)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	device->indexBuffer = buffer;
	device->indexBufferSize = size;
	device->indexBufferDirty = true;
}

void MGG_GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, mgint slot, MGG_Buffer* buffer, mgint vertexOffset)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	device->vertexBuffers[slot] = buffer;
	device->vertexOffsets[slot] = vertexOffset;
	device->vertexBuffersDirty |= 1ul << slot;
}

void MGG_GraphicsDevice_SetShader(MGG_GraphicsDevice* device, MGShaderStage stage, MGG_Shader* shader)
{
	assert(device != nullptr);
	assert(shader != nullptr);
	assert(shader->stage == stage);

	if (stage == MGShaderStage::Vertex)
		device->pipelineManager->impl->m_currentPSODesc.VS = { shader->bytecode.data(), shader->bytecode.size() };
	else if (stage == MGShaderStage::Pixel)
		device->pipelineManager->impl->m_currentPSODesc.PS = { shader->bytecode.data(), shader->bytecode.size() };
}

void MGG_GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
	assert(device != nullptr);
	assert(layout != nullptr);

	device->layout = layout;
	device->vertexBuffersDirty = 0xFFFFFFFF;

	auto& elements = device->pipelineManager->impl->m_inputElementDescs;
	elements.clear();
	elements.insert(elements.end(), layout->elements.begin(), layout->elements.end());
	device->pipelineManager->impl->m_currentPSODesc.InputLayout = { elements.data(), static_cast<uint32_t>(elements.size()) };
}

void MGDX_ApplyState(MGG_GraphicsDevice* device)
{
	auto currentFrame = device->frame;

	auto cl = device->context->GetCommandList();
	auto heaps = device->context->m_heaps;

	device->pipelineManager->ApplyCurrentPipelineState();

	if (device->viewportDirty)
	{
		cl->RSSetViewports(1, &device->viewport);
		device->viewportDirty = false;
	}

	if (device->scissorDirty)
	{
		if (device->scissorTestEnable)
			cl->RSSetScissorRects(1, &device->scissor);
		else
		{
			// Full viewport.
			CD3DX12_RECT rect;
			rect.left = device->viewport.TopLeftX;
			rect.top = device->viewport.TopLeftY;
			rect.right = rect.left + device->viewport.Width;
			rect.bottom = rect.top + device->viewport.Height;
			cl->RSSetScissorRects(1, &rect);
		}

		device->scissorDirty = false;
	}

	if (device->indexBufferDirty)
	{
		D3D12_INDEX_BUFFER_VIEW ibv;
		ibv.BufferLocation = device->indexBuffer->GpuAddress();
		ibv.Format = device->indexBufferSize == MGIndexElementSize::ThirtyTwoBits ? DXGI_FORMAT_R32_UINT : DXGI_FORMAT_R16_UINT;
		ibv.SizeInBytes = device->indexBuffer->dataSize;

		cl->IASetIndexBuffer(&ibv);
		device->indexBuffer->frame = currentFrame;

		device->indexBufferDirty = false;
	}

	if (device->vertexBuffersDirty)
	{
		D3D12_VERTEX_BUFFER_VIEW vbv;

		for (int i = 0; i < 16; i++)
		{
			if ((device->vertexBuffersDirty & (1 << i)) == 0)
				continue;

			auto buffer = device->vertexBuffers[i];
			if (!buffer)
				continue;

			vbv.BufferLocation = buffer->GpuAddress() + device->vertexOffsets[i];
			vbv.StrideInBytes = device->layout->streamStrides[i];
			vbv.SizeInBytes = buffer->dataSize;

			cl->IASetVertexBuffers(i, 1, &vbv);

			buffer->frame = currentFrame;
		}

		device->vertexBuffersDirty = 0;
	}

	if (device->uniformsDirty)
	{
		for (int i = 0; i < 2; i++)
		{
			auto buffer = device->uniforms[i];
			if (!buffer)
				continue;

			cl->SetGraphicsRootConstantBufferView(i, buffer->GpuAddress());

			buffer->frame = currentFrame;
		}

		device->uniformsDirty = 0;
	}

	if (device->texturesDirty)
	{
		for (int s = 0; s < 2; s++)
		{			
			for (int i = 0; i < MAX_TEXTURE_SLOTS; i++)
			{
				auto tex = device->textures[s][i];
				if (tex == nullptr)
					continue;

				heaps->CopySRVUAVToShader(tex->texture->GetSRV(), i);
				tex->frame = currentFrame;
			}

			cl->SetGraphicsRootDescriptorTable(s == (int)MGShaderStage::Pixel ? 3 : 2, heaps->ApplySRVsToShader());
		}

		device->texturesDirty = false;
	}

	if (device->samplersDirty)
	{
		for (int s = 0; s < 2; s++)
		{
			for (int i = 0; i < MAX_TEXTURE_SLOTS; i++)
			{
				auto samp = device->samplers[s][i];
				if (samp == nullptr)
					continue;

				heaps->CopySamplerToShader(samp->sampler->impl->m_handle, i);
			}

			cl->SetGraphicsRootDescriptorTable(s == (int)MGShaderStage::Pixel ? 5 : 4, heaps->ApplySamplersToShader());
		}

		device->samplersDirty = false;
	}

}

static int MGDX_GetIndexCount(MGPrimitiveType primitiveType, mgint primitiveCount)
{
	switch (primitiveType)
	{
	case MGPrimitiveType::LineList:
		return primitiveCount * 2;
	case MGPrimitiveType::LineStrip:
		return primitiveCount + 1;
	case MGPrimitiveType::TriangleList:
		return primitiveCount * 3;
	case MGPrimitiveType::TriangleStrip:
		return primitiveCount + 2;
	default:
	case MGPrimitiveType::PointList:
		return primitiveCount;
	}
}

void MGG_GraphicsDevice_Draw(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint vertexStart, mgint vertexCount)
{
	assert(device != nullptr);
	assert(vertexStart >= 0);
	
	if (vertexCount <= 0)
		return;

	auto cl = device->context->GetCommandList();

	MGDX_ApplyState(device);

	cl->IASetPrimitiveTopology(PrimitiveTypeToD3D_PRIMITIVE_TOPOLOGY[(int)primitiveType]);

	cl->DrawInstanced(vertexCount, 1, vertexStart, 0);
}

void MGG_GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart)
{
	assert(device != nullptr);
	assert(primitiveCount >= 0);
	assert(indexStart >= 0);
	assert(vertexStart >= 0);

	if (primitiveCount <= 0)
		return;

	auto cl = device->context->GetCommandList();

	MGDX_ApplyState(device);

	cl->IASetPrimitiveTopology(PrimitiveTypeToD3D_PRIMITIVE_TOPOLOGY[(int)primitiveType]);

	auto indexCount = MGDX_GetIndexCount(primitiveType, primitiveCount);

	cl->DrawIndexedInstanced(indexCount, 1, indexStart, vertexStart, 0);
}

void MGG_GraphicsDevice_DrawIndexedInstanced(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart, mgint instanceCount)
{
	assert(device != nullptr);
	assert(primitiveCount >= 0);
	assert(indexStart >= 0);
	assert(vertexStart >= 0);
	assert(instanceCount >= 0);

	if (primitiveCount <= 0)
		return;
	if (instanceCount <= 0)
		return;

	auto cl = device->context->GetCommandList();

	MGDX_ApplyState(device);

	cl->IASetPrimitiveTopology(PrimitiveTypeToD3D_PRIMITIVE_TOPOLOGY[(int)primitiveType]);

	auto indexCount = MGDX_GetIndexCount(primitiveType, primitiveCount);

	cl->DrawIndexedInstanced(indexCount, instanceCount, indexStart, vertexStart, 0);

	MG_NOT_IMPLEMEMTED;
}


MGG_BlendState* MGG_BlendState_Create(MGG_GraphicsDevice* device, MGG_BlendState_Info* infos)
{
	assert(device != nullptr);
	assert(infos != nullptr);

	auto state = new MGG_BlendState();

	state->desc = CD3DX12_BLEND_DESC(D3D12_DEFAULT);

	// TODO?
	//state->desc.AlphaToCoverageEnable = false;
	//state->desc.IndependentBlendEnable = false;
	state->blending = false;

	for (int i = 0; i < 4; i++)
	{
		auto& bstate = state->desc.RenderTarget[i];

		bstate.LogicOpEnable = FALSE; // not available in MonoGame?
		bstate.LogicOp = D3D12_LOGIC_OP_NOOP; // applies to RGBA

		// We're blending if we're not in the opaque state.
		bstate.BlendEnable = !(	infos[i].colorSourceBlend == MGBlend::One &&
								infos[i].colorDestBlend == MGBlend::Zero &&
								infos[i].alphaSourceBlend == MGBlend::One &&
								infos[i].alphaDestBlend == MGBlend::Zero);
		state->blending |= bstate.BlendEnable;

		bstate.SrcBlend = BlendToD3D12_BLEND[(int)infos[i].colorSourceBlend];
		bstate.DestBlend = BlendToD3D12_BLEND[(int)infos[i].colorDestBlend];
		bstate.BlendOp = BlendFunctionToD3D12_BLEND_OP[(int)infos[i].colorBlendFunc];
		bstate.SrcBlendAlpha = BlendToD3D12_BLEND[(int)infos[i].alphaSourceBlend];
		bstate.DestBlendAlpha = BlendToD3D12_BLEND[(int)infos[i].alphaDestBlend];
		bstate.BlendOpAlpha = BlendFunctionToD3D12_BLEND_OP[(int)infos[i].alphaBlendFunc];
		bstate.RenderTargetWriteMask = (uint8_t)infos[i].colorWriteChannels;
	}

	return state;
}

void MGG_BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;

	delete state;
}

MGG_DepthStencilState* MGG_DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

	auto state = new MGG_DepthStencilState();

	state->desc.DepthEnable = info->depthBufferEnable;
	state->desc.DepthWriteMask = info->depthBufferWriteEnable ? D3D12_DEPTH_WRITE_MASK_ALL : D3D12_DEPTH_WRITE_MASK_ZERO;
	state->desc.DepthFunc = CompareFunctionToD3D12_COMPARISON_FUNC[(int)info->depthBufferFunction];
	state->desc.StencilEnable = info->stencilEnable;
	state->desc.StencilReadMask = info->stencilMask;
	state->desc.StencilWriteMask = info->stencilWriteMask;
	state->desc.FrontFace.StencilFunc = CompareFunctionToD3D12_COMPARISON_FUNC[(int)info->stencilFunction];
	state->desc.FrontFace.StencilPassOp = StencilOperationToD3D12_D3D12_STENCIL_OP[(int)info->stencilPass];
	state->desc.FrontFace.StencilFailOp = StencilOperationToD3D12_D3D12_STENCIL_OP[(int)info->stencilFail];
	state->desc.FrontFace.StencilDepthFailOp = StencilOperationToD3D12_D3D12_STENCIL_OP[(int)info->stencilDepthBufferFail];
	state->desc.BackFace.StencilFunc = CompareFunctionToD3D12_COMPARISON_FUNC[(int)info->stencilFunction];
	state->desc.BackFace.StencilPassOp = StencilOperationToD3D12_D3D12_STENCIL_OP[(int)info->stencilPass];
	state->desc.BackFace.StencilFailOp = StencilOperationToD3D12_D3D12_STENCIL_OP[(int)info->stencilFail];
	state->desc.BackFace.StencilDepthFailOp = StencilOperationToD3D12_D3D12_STENCIL_OP[(int)info->stencilDepthBufferFail];

	return state;
}

void MGG_DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;

	delete state;
}

MGG_RasterizerState* MGG_RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

	auto state = new MGG_RasterizerState();
	state->desc = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);

	switch (info->cullMode)
	{
	case MGCullMode::None:
		state->desc.CullMode = D3D12_CULL_MODE_NONE;
		break;
	case MGCullMode::CullClockwiseFace:
		state->desc.CullMode = D3D12_CULL_MODE_FRONT;
		break;
	case MGCullMode::CullCounterClockwiseFace:
		state->desc.CullMode = D3D12_CULL_MODE_BACK;
		break;
	}

	switch (info->fillMode)
	{
	case MGFillMode::Solid:
		state->desc.FillMode = D3D12_FILL_MODE_SOLID;
		break;
	case MGFillMode::WireFrame:
		state->desc.FillMode = D3D12_FILL_MODE_WIREFRAME;
		break;
	}

	state->desc.DepthBias = info->depthBias;
	state->desc.DepthClipEnable = info->depthClipEnable;
	state->desc.SlopeScaledDepthBias = info->slopeScaleDepthBias;
	state->desc.MultisampleEnable = info->multiSampleAntiAlias;
	state->scissorTestEnable = info->scissorTestEnable;

	return state;
}

void MGG_RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;

	delete state;
}

MGG_SamplerState* MGG_SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

	auto state = new MGG_SamplerState();

	state->sampler = new Graphics::Sampler(
		device->resources,
		info->Filter,
		info->AddressU,
		info->AddressV,
		info->AddressW);

	return state;
}

void MGG_SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;

	delete state->sampler;

	delete state;
}

static MGG_Buffer* MGDX_BufferDiscard(MGG_GraphicsDevice* device, MGG_Buffer* buffer)
{
	// Get the info we need to find/allocate a new buffer.
	auto dataSize = buffer->dataSize;
	auto type = buffer->type;

	// Add it to the discard list.
	device->discarded.push_back(buffer);

	// Search for the best fit from the free list.		
	MGG_Buffer* best = nullptr;
	for (int i=0; i < device->free.size(); i++)
	{
		auto curr = device->free[i];

		if (curr->type != type)
			continue;
		auto currSize = curr->actualSize;

		if (currSize < dataSize)
			continue;

		if (best == nullptr || best->actualSize > currSize)
		{
			best = curr;

			if (currSize == dataSize)
			{
				device->free[i] = device->free.back();
				device->free.pop_back();
				break;
			}
		}
	}

	// We didn't find a match, so allocate a new one.
	if (best == nullptr)
		best = MGG_Buffer_Create(device, type, dataSize);

	best->dataSize = dataSize;

	return best;
}


MGG_Buffer* MGG_Buffer_Create(MGG_GraphicsDevice* device, MGBufferType type, mgint sizeInBytes)
{
	auto buffer = new MGG_Buffer();

	buffer->type = type;

	//buffer->heapType = BufferType::Dynamic;
	buffer->actualSize = buffer->dataSize = sizeInBytes;

	auto resourceDesc = CD3DX12_RESOURCE_DESC::Buffer(buffer->dataSize, D3D12_RESOURCE_FLAG_NONE, D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT);
	D3D12MA::ALLOCATION_DESC allocDesc = {};

	// TODO: All heaps are dynamic at the moment
	// can we fix that later?
	buffer->m_currentState = D3D12_RESOURCE_STATE_GENERIC_READ;
	allocDesc.HeapType = D3D12_HEAP_TYPE_UPLOAD;
	/*
	switch (buffer->heapType)
	{
	case BufferType::Static:
		buffer->m_currentState = D3D12_RESOURCE_STATE_COPY_DEST;
		allocDesc.HeapType = D3D12_HEAP_TYPE_DEFAULT;
		break;
	case BufferType::Dynamic:
		buffer->m_currentState = D3D12_RESOURCE_STATE_GENERIC_READ;
		allocDesc.HeapType = D3D12_HEAP_TYPE_UPLOAD;
		break;
	case BufferType::Transient:
		buffer->m_currentState = D3D12_RESOURCE_STATE_GENERIC_READ;
		allocDesc.CustomPool = device->resources->GetTransientBufferPool();
		break;
	}
	*/

	HRESULT hr = device->resources->GetAllocator()->CreateResource(
		&allocDesc,
		&resourceDesc,
		buffer->m_currentState,
		nullptr,
		buffer->m_alloc.ReleaseAndGetAddressOf(),
		IID_GRAPHICS_PPV_ARGS(buffer->m_res.ReleaseAndGetAddressOf()));
	DX::ThrowIfFailed(hr);

	return buffer;
}

void MGG_Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	if (!buffer)
		return;

	// Queue the buffer for later destruction.
	device->destroyBuffers.push(buffer);
}

void MGG_Buffer_SetData(MGG_GraphicsDevice* device, MGG_Buffer*& buffer, mgint offset, mgbyte* data, mgint length, mgbool discard)
{
	assert(device != nullptr);
	assert(buffer != nullptr);
	assert(data != nullptr);

	// TODO: Force discard here if we find we're
	// copying over data still in use.  See NX.

	if (discard)
	{
		auto last = buffer;

		buffer = MGDX_BufferDiscard(device, buffer);

		// Fix any active mapping of the buffer that
		// was just discarded for another.

		switch (buffer->type)
		{
		case MGBufferType::Constant:
			for (int i = 0; i < (int)MGShaderStage::Count; i++)
			{
				if (device->uniforms[i] == last)
				{
					device->uniforms[i] = buffer;
					device->uniformsDirty |= 1 << (int)i;
				}
			}
			break;

		case MGBufferType::Vertex:
			for (int i = 0; i < 8; i++)
			{
				if (device->vertexBuffers[i] == last)
				{
					device->vertexBuffers[i] = buffer;
					device->vertexBuffersDirty |= 1ul << i;
				}
			}
			break;

		case MGBufferType::Index:
			if (device->indexBuffer == last)
			{
				device->indexBuffer = buffer;
				device->indexBufferDirty = true;
			}
			break;
		}
	}

	// Copy the data.
	UINT8* pVertexDataBegin;
	CD3DX12_RANGE readRange(0, 0);
	DX::ThrowIfFailed(buffer->m_res->Map(0, &readRange, reinterpret_cast<void**>(&pVertexDataBegin)));
	memcpy(pVertexDataBegin + offset, data, length);
	CD3DX12_RANGE writeRange(offset, offset + length);
	buffer->m_res->Unmap(0, &writeRange);
}

void MGG_Buffer_GetData(MGG_GraphicsDevice* device, MGG_Buffer* buffer, mgint offset, mgbyte* data, mgint dataCount, mgint dataBytes, mgint dataStride)
{
	assert(device != nullptr);
	assert(buffer != nullptr);
	assert(data != nullptr);

	ComPtr<ID3D12Resource> intermediateBuffer;
	ComPtr<D3D12MA::Allocation> intermediateAlloc;
	CD3DX12_RESOURCE_DESC resourceDesc = CD3DX12_RESOURCE_DESC::Buffer(dataStride * dataCount);
	D3D12MA::ALLOCATION_DESC allocDesc = { D3D12MA::ALLOCATION_FLAG_NONE, D3D12_HEAP_TYPE_READBACK };
	device->resources->GetAllocator()->CreateResource(
		&allocDesc, &resourceDesc,
		D3D12_RESOURCE_STATE_COPY_DEST, nullptr,
		intermediateAlloc.ReleaseAndGetAddressOf(),
		IID_GRAPHICS_PPV_ARGS(intermediateBuffer.ReleaseAndGetAddressOf()));

	auto cmd = device->resources->BeginCommandList();
	auto cmdList = cmd->Get();

	if (buffer->m_currentState != D3D12_RESOURCE_STATE_COPY_SOURCE) {
		const D3D12_RESOURCE_BARRIER toCopySourceBarrier = CD3DX12_RESOURCE_BARRIER::Transition(
			buffer->m_res.Get(), buffer->m_currentState, D3D12_RESOURCE_STATE_COPY_SOURCE
		);
		cmdList->ResourceBarrier(1, &toCopySourceBarrier);
	}

	cmdList->CopyBufferRegion(intermediateBuffer.Get(), 0, buffer->m_res.Get(), offset, dataStride * dataCount);

	if (buffer->m_currentState != D3D12_RESOURCE_STATE_COPY_SOURCE) {
		const D3D12_RESOURCE_BARRIER revertBarrier = CD3DX12_RESOURCE_BARRIER::Transition(
			buffer->m_res.Get(), D3D12_RESOURCE_STATE_COPY_SOURCE, buffer->m_currentState
		);
		cmdList->ResourceBarrier(1, &revertBarrier);
	}

	cmd->Close(true);

	UINT8* pSourceDataBegin;
	DX::ThrowIfFailed(intermediateBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pSourceDataBegin)));
	if (dataStride == dataStride)
		memcpy(data, pSourceDataBegin, dataStride * dataCount);
	else {
		for (auto i = 0; i < dataCount; i++)
			memcpy(data + (i * dataStride), (void*)(pSourceDataBegin + (i * dataStride)), dataStride);
	}
	CD3DX12_RANGE writeRange(0, 0); // We haven't write to the buffer
	intermediateBuffer->Unmap(0, &writeRange);
}

MGG_Texture* MGG_Texture_Create(
	MGG_GraphicsDevice* device,
	MGTextureType type,
	MGSurfaceFormat format,
	mgint width,
	mgint height,
	mgint depth,
	mgint mipmaps,
	mgint slices)
{
	assert(device != nullptr);

	assert(width > 0);
	assert(height > 0);
	assert(depth > 0);
	assert(mipmaps > 0);
	assert(slices > 0);
	assert(type != MGTextureType::Cube || (slices % 6) == 0);

	auto texture = new MGG_Texture();

	texture->format = format;
	texture->texture = new Texture(SurfaceType::Texture, TextureDimension::Texture2D, width, height, mipmaps, format);
	texture->texture->Create(device->resources);

	return texture;
}

MGG_Texture* MGG_RenderTarget_Create(
	MGG_GraphicsDevice* device,
	MGTextureType type,
	MGSurfaceFormat format,
	mgint width,
	mgint height,
	mgint depth,
	mgint mipmaps,
	mgint slices,
	MGDepthFormat depthFormat,
	mgint multiSampleCount,
	MGRenderTargetUsage usage)
{
	assert(device != nullptr);

	assert(width > 0);
	assert(height > 0);
	assert(depth > 0);
	assert(mipmaps > 0);
	assert(slices > 0);
	assert(type != MGTextureType::Cube || (slices % 6) == 0);

	auto texture = new MGG_Texture();
	texture->format = format;

	return texture;
}

void MGG_Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	if (!texture)
		return;

	// Queue the texture for later destruction.
	device->destroyTextures.push(texture);
}

static size_t GetFormatSize(MGSurfaceFormat format)
{
	switch (format)
	{
	case MGSurfaceFormat::Dxt1:
	case MGSurfaceFormat::Dxt1SRgb:
	case MGSurfaceFormat::Dxt1a:
	case MGSurfaceFormat::RgbPvrtc2Bpp:
	case MGSurfaceFormat::RgbaPvrtc2Bpp:
	case MGSurfaceFormat::RgbPvrtc4Bpp:
	case MGSurfaceFormat::RgbaPvrtc4Bpp:
	case MGSurfaceFormat::RgbEtc1:
	case MGSurfaceFormat::Rgb8Etc2:
	case MGSurfaceFormat::Srgb8Etc2:
	case MGSurfaceFormat::Rgb8A1Etc2:
	case MGSurfaceFormat::Srgb8A1Etc2:
		// One texel in DXT1, PVRTC (2bpp and 4bpp) and ETC1 is a minimum 4x4 block (8x4 for PVRTC 2bpp), which is 8 bytes
		return 8;
	case MGSurfaceFormat::Dxt3:
	case MGSurfaceFormat::Dxt3SRgb:
	case MGSurfaceFormat::Dxt5:
	case MGSurfaceFormat::Dxt5SRgb:
	case MGSurfaceFormat::RgbaAtcExplicitAlpha:
	case MGSurfaceFormat::RgbaAtcInterpolatedAlpha:
	case MGSurfaceFormat::Rgba8Etc2:
	case MGSurfaceFormat::SRgb8A8Etc2:
		// One texel in DXT3 and DXT5 is a minimum 4x4 block, which is 16 bytes
		return 16;
	case MGSurfaceFormat::Alpha8:
		return 1;
	case MGSurfaceFormat::Bgr565:
	case MGSurfaceFormat::Bgra4444:
	case MGSurfaceFormat::Bgra5551:
	case MGSurfaceFormat::HalfSingle:
	case MGSurfaceFormat::NormalizedByte2:
		return 2;
	case MGSurfaceFormat::Color:
	case MGSurfaceFormat::ColorSRgb:
	case MGSurfaceFormat::Single:
	case MGSurfaceFormat::Rg32:
	case MGSurfaceFormat::HalfVector2:
	case MGSurfaceFormat::NormalizedByte4:
	case MGSurfaceFormat::Rgba1010102:
	case MGSurfaceFormat::Bgra32:
	case MGSurfaceFormat::Bgra32SRgb:
	case MGSurfaceFormat::Bgr32:
	case MGSurfaceFormat::Bgr32SRgb:
		return 4;
	case MGSurfaceFormat::HalfVector4:
	case MGSurfaceFormat::Rgba64:
	case MGSurfaceFormat::Vector2:
		return 8;
	case MGSurfaceFormat::Vector4:
		return 16;
	default:
		assert(false);
		return 4;
	}
}

static size_t GetTexturePitch(MGSurfaceFormat format, int width)
{
	size_t pitch;

	size_t size = GetFormatSize(format);

	switch (format)
	{
	case MGSurfaceFormat::Dxt1 :
	case MGSurfaceFormat::Dxt1SRgb:
	case MGSurfaceFormat::Dxt1a:
	case MGSurfaceFormat::RgbPvrtc2Bpp:
	case MGSurfaceFormat::RgbaPvrtc2Bpp:
	case MGSurfaceFormat::RgbEtc1:
	case MGSurfaceFormat::Rgb8Etc2:
	case MGSurfaceFormat::Srgb8Etc2:
	case MGSurfaceFormat::Rgb8A1Etc2:
	case MGSurfaceFormat::Srgb8A1Etc2:
	case MGSurfaceFormat::Dxt3:
	case MGSurfaceFormat::Dxt3SRgb:
	case MGSurfaceFormat::Dxt5:
	case MGSurfaceFormat::Dxt5SRgb:
	case MGSurfaceFormat::RgbPvrtc4Bpp:
	case MGSurfaceFormat::RgbaPvrtc4Bpp:
		pitch = ((width + 3) / 4) * size;
		break;

	default:
		pitch = width * size;
		break;
	};

	return pitch;
}

void MGG_Texture_SetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	if (x == 0 && y == 0 && width == 0 && height == 0)
	{
		width = texture->texture->GetWidth();
		height = texture->texture->GetHeight();
	}

	//assert(level >= 0 && level < texture->info.mipLevels);
	//assert(slice >= 0 && slice < texture->info.arrayLayers);
	//assert(x >= 0 && x < texture->info.extent.width);
	//assert(y >= 0 && y < texture->info.extent.height);
	//assert(z >= 0 && z < texture->info.extent.depth);
	//assert(x + width <= texture->info.extent.width);
	//assert(y + height <= texture->info.extent.height);
	//assert(z + depth <= texture->info.extent.depth);

	assert(data != nullptr);
	assert(dataBytes > 0);

	uint32_t subres = (slice * texture->texture->GetMipLevels()) + level;
	size_t rowPitch = GetTexturePitch(texture->format, width);

	if (x == 0 && y == 0 && width == texture->texture->GetWidth() && height == texture->texture->GetHeight())
		texture->texture->SetData(device->resources, subres, data, dataBytes, rowPitch);
	else
		texture->texture->SetData(device->resources, subres, x, y, width, height, data, dataBytes, rowPitch);
}

void MGG_Texture_GetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	//assert(level >= 0 && level < texture->info.mipLevels);
	//assert(slice >= 0 && slice < texture->info.arrayLayers);
	//assert(x >= 0 && x < texture->info.extent.width);
	//assert(y >= 0 && y < texture->info.extent.height);
	//assert(z >= 0 && z < texture->info.extent.depth);
	//assert(x + width <= texture->info.extent.width);
	//assert(y + height <= texture->info.extent.height);
	//assert(z + depth <= texture->info.extent.depth);

	assert(data != nullptr);
	assert(dataBytes > 0);

}

static const LPCSTR MGVertexElementUsageToLPCSTR[] =
{
	"POSITION",
	"COLOR",
	"TEXCOORD",
	"NORMAL",
	"BINORMAL",
	"TANGENT",
	"BLENDINDICES",
	"BLENDWEIGHT",
	"SV_Depth",
	"FOG",
	"PSIZE",
	"SV_SampleIndex",
	"TESSFACTOR"
};

static const DXGI_FORMAT MGVertexElementFormatToDXGI_FORMAT[] =
{
	DXGI_FORMAT_R32_FLOAT,
	DXGI_FORMAT_R32G32_FLOAT,
	DXGI_FORMAT_R32G32B32_FLOAT,
	DXGI_FORMAT_R32G32B32A32_FLOAT,
	DXGI_FORMAT_R8G8B8A8_UNORM,
	DXGI_FORMAT_R8G8B8A8_UINT,
	DXGI_FORMAT_R16G16_SINT,
	DXGI_FORMAT_R16G16B16A16_SINT,
	DXGI_FORMAT_R16G16_SNORM,
	DXGI_FORMAT_R16G16B16A16_SNORM,
	DXGI_FORMAT_R16G16_FLOAT,
	DXGI_FORMAT_R16G16B16A16_FLOAT,
};

MGG_InputLayout* MGG_InputLayout_Create(
	MGG_GraphicsDevice* device,
	MGG_Shader* vertexShader,
	mgint* strides,
	mgint streamCount,
	MGG_InputElement* elements,
	mgint elementCount
	)
{
	assert(device != nullptr);
	assert(vertexShader != nullptr);
	assert(streamCount >= 0);
	assert(strides != nullptr);
	assert(elements != nullptr);
	assert(elementCount >= 0);

	auto layout = new MGG_InputLayout();

	for (int i=0; i < streamCount; i++)
		layout->streamStrides.push_back(strides[i]);

	for (int i=0; i < elementCount; i++)
	{
		D3D12_INPUT_ELEMENT_DESC elem;
		elem.SemanticName = MGVertexElementUsageToLPCSTR[(int)elements[i].SemanticUsage];
		elem.SemanticIndex = elements[i].SemanticIndex;
		elem.AlignedByteOffset = elements[i].AlignedByteOffset;
		elem.InputSlot = elements[i].VertexBufferSlot;
		elem.InputSlotClass = D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA; // TODO: instancing!
		elem.InstanceDataStepRate = elements[i].InstanceDataStepRate;
		elem.Format = MGVertexElementFormatToDXGI_FORMAT[(int)elements[i].Format];
		layout->elements.push_back(elem);
	}

	return layout;
}

void MGG_InputLayout_Destroy(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
	assert(device != nullptr);
	assert(layout != nullptr);

	if (layout == nullptr)
		return;

	delete layout;
}

MGG_Shader* MGG_Shader_Create(MGG_GraphicsDevice* device, MGShaderStage stage, mgbyte* bytecode, mgint sizeInBytes)
{
	assert(device != nullptr);
	assert(bytecode != nullptr);
	assert(sizeInBytes > 0);

	// TODO: Here we copy the shader bytecode, but
	// doesn't the C# side hold it too?  We need the
	// extra copy?  Or should the C# side be responsible
	// to keep it around?

	auto shader = new MGG_Shader();
	shader->stage = stage;
	shader->bytecode.resize(sizeInBytes);
	memcpy(shader->bytecode.data(), bytecode, sizeInBytes);

	return shader;
}

void MGG_Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader)
{
	assert(device != nullptr);
	assert(shader != nullptr);

	if (!shader)
		return;

	delete shader;
}

MGG_OcclusionQuery* MGG_OcclusionQuery_Create(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);

	auto query = new MGG_OcclusionQuery();

	query->handle = device->resources->GetGraphicsHeaps()->CreateQueryHandle();

	CD3DX12_RESOURCE_DESC resourceDesc = CD3DX12_RESOURCE_DESC::Buffer(8);
	D3D12MA::ALLOCATION_DESC allocDesc = { D3D12MA::ALLOCATION_FLAG_COMMITTED, D3D12_HEAP_TYPE_READBACK };

	device->resources->GetAllocator()->CreateResource(
		&allocDesc, &resourceDesc,
		D3D12_RESOURCE_STATE_COPY_DEST, nullptr,
		query->alloc.ReleaseAndGetAddressOf(), IID_GRAPHICS_PPV_ARGS(query->buffer.ReleaseAndGetAddressOf()));

	return query;
}

void MGG_OcclusionQuery_Destroy(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
	assert(device != nullptr);
	assert(query != nullptr);

	if (!query)
		return;

	delete query;
}

void MGG_OcclusionQuery_Begin(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
	assert(device != nullptr);
	assert(query != nullptr);

	auto cl = device->context->GetCommandList();

	cl->BeginQuery(
		device->resources->GetGraphicsHeaps()->GetQueryHeap(),
		D3D12_QUERY_TYPE_OCCLUSION,
		query->handle);
}

void MGG_OcclusionQuery_End(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
	assert(device != nullptr);
	assert(query != nullptr);

	auto cl = device->context->GetCommandList();
	auto heap = device->resources->GetGraphicsHeaps()->GetQueryHeap();

	cl->EndQuery(
		heap,
		D3D12_QUERY_TYPE_OCCLUSION,
		query->handle);

	cl->ResolveQueryData(
		heap,
		D3D12_QUERY_TYPE_OCCLUSION,
		query->handle, 1, query->buffer.Get(), 0);

	query->fence = device->resources->GetCommandQueue()->SignalFence();
}

mgbyte MGG_OcclusionQuery_GetResult(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query, mgint& pixelCount)
{
	assert(device != nullptr);
	assert(query != nullptr);

	auto cq = device->resources->GetCommandQueue();
	if (!cq->IsFenceComplete(query->fence))
		return false;

	D3D12_RANGE readbackBufferRange{ 0, 8 };
	void* pReadbackBufferData{};
	query->buffer->Map(0, &readbackBufferRange, &pReadbackBufferData);

	uint64_t value;
	memcpy(&value, pReadbackBufferData, 8);

	CD3DX12_RANGE writeRange(0, 0);
	query->buffer->Unmap(0, &writeRange);

	pixelCount = value;
	return true;
}
