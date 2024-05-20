// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "csharp_MGG.h"

#include "stl_common.h"


struct MGG_Frame
{
	MGG_Texture* backbuffer = nullptr;
};

struct MGG_GraphicsDevice
{
	const static mgint FrameCount = 2;

	mgint frame = 0;

	mgint viewport[4] = { 0 };
	float viewportDepth[2] = { 0.0f };

	mgint scissor[4] = { 0 };

	MGG_Frame frames[FrameCount];

	MGG_BlendState* blendState = nullptr;
	MGG_DepthStencilState* depthStencilStateState = nullptr;
	MGG_RasterizerState* rasterizerState = nullptr;

	MGG_InputLayout* layout = nullptr;

	MGG_Shader* shader[(mgint)MGShaderStage::Count] = { 0 };

	std::vector<MGG_Buffer*> buffers;
	std::vector<MGG_Texture*> textures;
	std::vector<MGG_Shader*> shaders;
	std::vector<MGG_InputLayout*> layouts;

	std::vector<MGG_BlendState*> blendStates;
	std::vector<MGG_DepthStencilState*> depthStencilStates;
	std::vector<MGG_RasterizerState*> rasterizerStates;
	std::vector<MGG_SamplerState*> samplerStates;
};


struct MGG_Buffer
{
	mgint length;
};

struct MGG_Texture
{
	MGTextureType type;
	MGSurfaceFormat format;
	mgint width;
	mgint height;
	mgint depth;
	mgint mipmaps;
	mgint slices;

	mgbool isRenderTarget = false;
	MGDepthFormat depthFormat = MGDepthFormat::None;
	mgint multiSampleCount = 0;
	MGRenderTargetUsage usage = MGRenderTargetUsage::PlatformContents;
};

struct MGG_InputLayout
{
	MGG_Shader* vertexShader;
};

struct MGG_Shader
{
	MGShaderStage stage;
	mgbyte* bytecode;
	mgint bytecodeSize;
};

struct MGG_BlendState
{
	MGG_BlendState_Info info[4];
};

struct MGG_DepthStencilState
{
	MGG_DepthStencilState_Info info;
};

struct MGG_RasterizerState
{
	MGG_RasterizerState_Info info;
};

struct MGG_SamplerState
{
	MGG_SamplerState_Info info;
};


MGG_GraphicsDevice* MGG_GraphicsDevice_Create()
{
	auto device = new MGG_GraphicsDevice();
	return device;
}

void MGG_GraphicsDevice_Destroy(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);
	delete device;
}

void MGG_GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, MGG_GraphicsDevice_Caps* caps)
{
	assert(device != nullptr);
	assert(caps != nullptr);

	caps->MaxTextureSlots = 16;
	caps->MaxVertexBufferSlots = 8;
	caps->MaxVertexTextureSlots = 8;

	// The dummy device uses Windows DX shaders.
	caps->ShaderProfile = 20;
}

void MGG_GraphicsDevice_ResetBackbuffer(MGG_GraphicsDevice* device, mgint width, mgint height, MGSurfaceFormat color, MGDepthFormat depth)
{
	assert(device != nullptr);
}

mgint MGG_GraphicsDevice_BeginFrame(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);
	return device->frame % device->FrameCount;
}

void MGG_GraphicsDevice_Present(MGG_GraphicsDevice* device, mgint currentFrame, mgint syncInterval)
{
	assert(device != nullptr);
	assert(syncInterval >= 0);
	assert(currentFrame >= 0);
	assert((device->frame % 2) == currentFrame);

	++device->frame;
}

void MGG_GraphicsDevice_SetBlendState(MGG_GraphicsDevice* device, MGG_BlendState* stage, mgfloat factorR, mgfloat factorG, mgfloat factorB, mgfloat factorA)
{
	assert(device != nullptr);
	assert(stage != nullptr);
}

void MGG_GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* stage)
{
	assert(device != nullptr);
	assert(stage != nullptr);

}

void MGG_GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* stage)
{
	assert(device != nullptr);
	assert(stage != nullptr);

}

void MGG_GraphicsDevice_BindConstantBuffer(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Buffer* buffer)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_GetTitleSafeArea(mgint* x, mgint* y, mgint* width, mgint* height)
{
}

void MGG_GraphicsDevice_SetViewport(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, mgfloat minDepth, mgfloat maxDepth)
{
	assert(device != nullptr);

	device->viewport[0] = x;
	device->viewport[1] = y;
	device->viewport[2] = width;
	device->viewport[3] = height;

	device->viewportDepth[0] = minDepth;
	device->viewportDepth[0] = maxDepth;
}

void MGG_GraphicsDevice_SetScissorRectangle(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height)
{
	assert(device != nullptr);

	device->scissor[0] = x;
	device->scissor[1] = y;
	device->scissor[2] = width;
	device->scissor[3] = height;
}

void MGG_GraphicsDevice_SetRenderTargets(MGG_GraphicsDevice* device, MGG_Texture** targets, mgint count)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Buffer* buffer)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_SetTexture(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Texture* texture)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_SamplerState* state)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_SetIndexBuffer(MGG_GraphicsDevice* device, MGIndexElementSize size, MGG_Buffer* buffer)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, mgint slot, MGG_Buffer* buffer, mgint strideInBytes, mgint vertexOffset)
{
	assert(device != nullptr);
}

void MGG_GraphicsDevice_SetShader(MGG_GraphicsDevice* device, MGShaderStage stage, MGG_Shader* shader)
{
	assert(device != nullptr);
	assert(shader != nullptr);
	assert(shader->stage == stage);

	device->shader[(mgint)stage] = shader;
}

void MGG_GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
	assert(device != nullptr);
	assert(layout != nullptr);

	device->layout = layout;
}

void MGG_GraphicsDevice_PrepareState(MGG_GraphicsDevice* device)
{
	assert(device->shader[(mgint)MGShaderStage::Vertex] != nullptr);
	assert(device->shader[(mgint)MGShaderStage::Pixel] != nullptr);
	assert(device->layout != nullptr);
	assert(device->blendState != nullptr);
	assert(device->depthStencilStateState != nullptr);
	assert(device->rasterizerState != nullptr);
}

void MGG_GraphicsDevice_Draw(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint vertexStart, mgint vertexCount)
{
	assert(device != nullptr);
	assert(vertexStart >= 0);
	
	if (vertexCount <= 0)
		return;

	MGG_GraphicsDevice_PrepareState(device);
}

void MGG_GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart)
{
	assert(device != nullptr);
	assert(primitiveCount >= 0);
	assert(indexStart >= 0);
	assert(vertexStart >= 0);

	if (primitiveCount <= 0)
		return;

	MGG_GraphicsDevice_PrepareState(device);
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

	MGG_GraphicsDevice_PrepareState(device);
}

MGG_BlendState* MGG_BlendState_Create(MGG_GraphicsDevice* device, MGG_BlendState_Info* infos)
{
	assert(device != nullptr);
	assert(infos != nullptr);

	auto state = new MGG_BlendState();
	memcpy(state->info, infos, sizeof(MGG_BlendState_Info) * 4);

	device->blendStates.push_back(state);

	return state;
}

void MGG_BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	remove_by_value(device->blendStates, state);

	delete state;
}

MGG_DepthStencilState* MGG_DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

	auto state = new MGG_DepthStencilState();
	state->info = *info;

	device->depthStencilStates.push_back(state);

	return state;
}

void MGG_DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	remove_by_value(device->depthStencilStates, state);

	delete state;
}

MGG_RasterizerState* MGG_RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

	auto state = new MGG_RasterizerState();
	state->info = *info;

	device->rasterizerStates.push_back(state);

	return state;
}

void MGG_RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	remove_by_value(device->rasterizerStates, state);
	delete state;
}

MGG_SamplerState* MGG_SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

	auto state = new MGG_SamplerState();
	state->info = *info;

	device->samplerStates.push_back(state);

	return state;
}

void MGG_SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	remove_by_value(device->samplerStates, state);
	delete state;
}

MGG_Buffer* MGG_Buffer_Create(MGG_GraphicsDevice* device, mgint length)
{
	assert(device != nullptr);

	auto buffer = new MGG_Buffer();
	buffer->length = length;

	device->buffers.push_back(buffer);

	return buffer;
}

void MGG_Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer)
{
	assert(device != nullptr);

	remove_by_value(device->buffers, buffer);
	delete buffer;
}

void MGG_Buffer_SetData(MGG_GraphicsDevice* device, MGG_Buffer*& buffer, mgint offset, mgbyte* data, mgint length, mgbool discard)
{
	assert(device != nullptr);
	assert(buffer != nullptr);
	assert(data != nullptr);

	assert(offset > 0 && offset < buffer->length);
	assert(offset + length <= buffer->length);
}

void MGG_Buffer_GetData(MGG_GraphicsDevice* device, MGG_Buffer* buffer, mgint offset, mgbyte* data, mgint dataCount, mgint dataBytes, mgint dataStride)
{
	assert(device != nullptr);
	assert(buffer != nullptr);
	assert(data != nullptr);

	assert(offset > 0 && offset < buffer->length);

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

	auto texture = new MGG_Texture();
	texture->type = type;
	texture->format = format;
	texture->width = width;
	texture->height = height;
	texture->depth = depth;
	texture->mipmaps = mipmaps;
	texture->slices = slices;

	device->textures.push_back(texture);

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
	texture->isRenderTarget = true;
	texture->type = type;
	texture->format = format;
	texture->width = width;
	texture->height = height;
	texture->depth = depth;
	texture->mipmaps = mipmaps;
	texture->slices = slices;

	texture->isRenderTarget = true;
	texture->depthFormat = depthFormat;
	texture->multiSampleCount = multiSampleCount;
	texture->usage = usage;

	device->textures.push_back(texture);

	return texture;
}

void MGG_Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	remove_by_value(device->textures, texture);
	delete texture;
}

void MGG_Texture_SetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	assert(level >= 0 && level < texture->mipmaps);
	assert(slice >= 0 && slice < texture->slices);
	assert(x >= 0 && x < texture->width);
	assert(y >= 0 && y < texture->height);
	assert(z >= 0 && z < texture->depth);
	assert(x + width <= texture->width);
	assert(y + height <= texture->height);
	assert(z + depth <= texture->depth);

	assert(data != nullptr);
	assert(dataBytes > 0);

}

void MGG_Texture_GetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	assert(level >= 0 && level < texture->mipmaps);
	assert(slice >= 0 && slice < texture->slices);
	assert(x >= 0 && x < texture->width);
	assert(y >= 0 && y < texture->height);
	assert(z >= 0 && z < texture->depth);
	assert(x + width <= texture->width);
	assert(y + height <= texture->height);
	assert(z + depth <= texture->depth);

	assert(data != nullptr);
	assert(dataBytes > 0);

}

MGG_InputLayout* MGG_InputLayout_Create(MGG_GraphicsDevice* device, MGG_Shader* vertexShader, MGG_InputElement* elements, mgint count)
{
	assert(device != nullptr);
	assert(elements != nullptr);
	assert(vertexShader != nullptr);
	assert(count >= 0);

	auto layout = new MGG_InputLayout();
	layout->vertexShader = vertexShader;

	device->layouts.push_back(layout);

	return layout;
}

void MGG_InputLayout_Destroy(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
	assert(device != nullptr);
	assert(layout != nullptr);

	remove_by_value(device->layouts, layout);
	delete layout;
}

MGG_Shader* MGG_Shader_Create(MGG_GraphicsDevice* device, MGShaderStage stage, mgbyte* bytecode, mgint sizeInBytes)
{
	assert(device != nullptr);
	assert(bytecode != nullptr);
	assert(sizeInBytes > 0);

	auto shader = new MGG_Shader();
	shader->stage = stage;
	shader->bytecode = bytecode;
	shader->bytecodeSize = sizeInBytes;

	remove_by_value(device->shaders, shader);

	return shader;
}

void MGG_Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader)
{
	assert(device != nullptr);
	assert(shader != nullptr);

	remove_by_value(device->shaders, shader);
	delete shader;
}
