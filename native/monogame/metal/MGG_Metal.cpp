// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGG.h"

#include "mg_common.h"

#if defined(MG_SDL2)
#include <SDL_vulkan.h>
#include <SDL_syswm.h>
#include <SDL_events.h>
#endif

const int MAX_TEXTURE_SLOTS = 16;

MGG_GraphicsSystem* MGG_GraphicsSystem_Create()
{
    return nullptr;
}

void MGG_GraphicsSystem_Destroy(MGG_GraphicsSystem* system)
{
	assert(system != nullptr);

	delete system;
}

MGG_GraphicsAdapter* MGG_GraphicsAdapter_Get(MGG_GraphicsSystem* system, mgint index)
{
	return nullptr;
}

void MGG_GraphicsAdapter_GetInfo(MGG_GraphicsAdapter* adapter, MGG_GraphicsAdaptor_Info& info)
{
    assert(adapter != nullptr);
}

MGG_GraphicsDevice* MGG_GraphicsDevice_Create(MGG_GraphicsSystem* system, MGG_GraphicsAdapter* adapter)
{
	assert(system != nullptr);
	assert(adapter != nullptr);

    return nullptr;
}

void MGG_GraphicsDevice_Destroy(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, MGG_GraphicsDevice_Caps& caps)
{
	assert(device != nullptr);

	// TODO: Get actual stats from the device!

	caps.MaxTextureSlots = 16;
	caps.MaxVertexBufferSlots = 8;
	caps.MaxVertexTextureSlots = 8;

	// Vulkan shader profile from pipeline.
	caps.ShaderProfile = 80;
}

void MGG_GraphicsDevice_ResizeSwapchain(
	MGG_GraphicsDevice* device,
	void* nativeWindowHandle,
	mgint width,
	mgint height,
	MGSurfaceFormat color,
	MGDepthFormat depth,
	mgint multiSampleCount,
	mgint syncInterval)
{
	assert(device);
}

mgint MGG_GraphicsDevice_BeginFrame(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);

	// We do nothing here... everything is handled in Present().
	return 0;
}

void MGG_GraphicsDevice_Clear(MGG_GraphicsDevice* device, MGClearOptions options, Vector4& color, mgfloat depth, mgint stencil)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_Present(MGG_GraphicsDevice* device, mgint currentFrame, mgint syncInterval)
{
	assert(device != nullptr);
	assert(syncInterval >= 0);
	assert(currentFrame >= 0);
}

void MGG_GraphicsDevice_SetBlendState(MGG_GraphicsDevice* device, MGG_BlendState* state, mgfloat factorR, mgfloat factorG, mgfloat factorB, mgfloat factorA)
{
	assert(device != nullptr);
	assert(state != nullptr);
}

void MGG_GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);
}

void MGG_GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);
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
}

void MGG_GraphicsDevice_SetScissorRectangle(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_SetRenderTargets(MGG_GraphicsDevice* device, MGG_Texture** targets, mgint* arraySlices, mgint count)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Buffer* buffer)
{
	assert(device != nullptr);
	assert(buffer != nullptr);
}

void MGG_GraphicsDevice_SetTexture(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Texture* texture)
{
	assert(device != nullptr);
	assert(slot >= 0);
	assert(slot < MAX_TEXTURE_SLOTS);
}

void MGG_GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_SamplerState* state)
{
	assert(device != nullptr);
	assert(slot >= 0);
	assert(slot < MAX_TEXTURE_SLOTS);
}

void MGG_GraphicsDevice_SetIndexBuffer(MGG_GraphicsDevice* device, MGIndexElementSize size, MGG_Buffer* buffer)
{
	assert(device != nullptr);
	assert(buffer != nullptr);
}

void MGG_GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, mgint slot, MGG_Buffer* buffer, mgint vertexOffset)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	assert(slot >= 0 && slot < 8);
}

void MGG_GraphicsDevice_SetShader(MGG_GraphicsDevice* device, MGShaderStage stage, MGG_Shader* shader)
{
	assert(device != nullptr);
	assert(shader != nullptr);
}

void MGG_GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
	assert(device != nullptr);
	assert(layout != nullptr);
}

void MGG_GraphicsDevice_Draw(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint vertexStart, mgint vertexCount)
{
	assert(device != nullptr);
	assert(vertexStart >= 0);
	
	if (vertexCount <= 0)
		return;
}

void MGG_GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart)
{
	assert(device != nullptr);
	assert(primitiveCount >= 0);
	assert(indexStart >= 0);
	assert(vertexStart >= 0);

	if (primitiveCount <= 0)
		return;
}

void MGG_GraphicsDevice_DrawIndexedInstanced(
	MGG_GraphicsDevice* device,
	MGPrimitiveType primitiveType,
	mgint primitiveCount,
	mgint indexStart,
	mgint vertexStart,
	mgint instanceCount)
{
	assert(device != nullptr);
	assert(primitiveCount >= 0);
	assert(indexStart >= 0);
	assert(vertexStart >= 0);
	assert(instanceCount > 0);

	if (primitiveCount <= 0)
		return;
}

void MGG_GraphicsDevice_ResolveRenderTargets(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_GetBackBufferData(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, void* data, mgint count, mgint dataBytes)
{
	assert(device != nullptr);
	assert(data != nullptr);
	assert(count > 0);
	assert(dataBytes > 0);
}

MGG_BlendState* MGG_BlendState_Create(MGG_GraphicsDevice* device, MGG_BlendState_Info* infos)
{
	assert(device != nullptr);
	assert(infos != nullptr);

    return nullptr;
}

void MGG_BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;
}

MGG_DepthStencilState* MGG_DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

    return nullptr;
}

void MGG_DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;
}

MGG_RasterizerState* MGG_RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

    return nullptr;
}

void MGG_RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;
}

MGG_SamplerState* MGG_SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

    return nullptr;
}

void MGG_SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;
}

MGG_Buffer* MGG_Buffer_Create(MGG_GraphicsDevice* device, MGBufferType type, mgbool dynamic, mgint sizeInBytes)
{
    return nullptr;
}

void MGG_Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	if (!buffer)
		return;
}

void MGG_Buffer_SetData(MGG_GraphicsDevice* device, MGG_Buffer*& buffer, mgint offset, mgbyte* data, mgint elementCount, mgint vertexStride, mgint elementSizeInBytes, mgbool discard)
{
	assert(device != nullptr);
	assert(buffer != nullptr);
	assert(data != nullptr);
	assert(offset >= 0);
	assert(elementCount > 0);
	assert(vertexStride > 0);
	assert(elementSizeInBytes > 0);
}

void MGG_Buffer_GetData(MGG_GraphicsDevice* device, MGG_Buffer* buffer, mgint offset, mgbyte* data, mgint dataCount, mgint dataBytes, mgint dataStride)
{
    assert(device != nullptr);
    assert(buffer != nullptr);
    assert(data != nullptr);
    assert(dataCount > 0);
    assert(dataBytes > 0);
    assert(dataStride > 0);
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

    return nullptr;
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

    return nullptr;
}

void MGG_Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	if (!texture)
		return;
}

void MGG_Texture_SetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
	assert(device != nullptr);
	assert(texture != nullptr);
}

void MGG_Texture_GetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
	assert(device != nullptr);
	assert(texture != nullptr);
}

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
	assert(streamCount >= 0);
	assert(strides != nullptr);
	assert(elements != nullptr);
	assert(elementCount >= 0);

    return nullptr;
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

    return nullptr;
}

void MGG_Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader)
{
	assert(device != nullptr);
	assert(shader != nullptr);

	if (!shader)
		return;
}

MGG_OcclusionQuery* MGG_OcclusionQuery_Create(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);

    return nullptr;
}

void MGG_OcclusionQuery_Destroy(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
	assert(device != nullptr);
	assert(query != nullptr);

	if (!query)
		return;
}

void MGG_OcclusionQuery_Begin(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
	assert(device != nullptr);
	assert(query != nullptr);
}

void MGG_OcclusionQuery_End(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
	assert(device != nullptr);
	assert(query != nullptr);
}

mgbyte MGG_OcclusionQuery_GetResult(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query, mgint& pixelCount)
{
	assert(device != nullptr);
	assert(query != nullptr);
    
    return false;
}