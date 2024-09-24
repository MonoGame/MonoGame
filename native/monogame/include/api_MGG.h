// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
                
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes

#pragma once

#include "api_common.h"
#include "api_enums.h"
#include "api_structs.h"


struct MGG_GraphicsSystem;
struct MGG_GraphicsAdapter;
struct MGG_GraphicsDevice;
struct MGG_BlendState;
struct MGG_DepthStencilState;
struct MGG_RasterizerState;
struct MGG_Buffer;
struct MGG_Texture;
struct MGG_SamplerState;
struct MGG_Shader;
struct MGG_InputLayout;
struct MGG_OcclusionQuery;

MG_EXPORT void MGG_EffectResource_GetBytecode(const char* name, mgbyte*& bytecode, mgint& size);
MG_EXPORT MGG_GraphicsSystem* MGG_GraphicsSystem_Create();
MG_EXPORT void MGG_GraphicsSystem_Destroy(MGG_GraphicsSystem* system);
MG_EXPORT MGG_GraphicsAdapter* MGG_GraphicsAdapter_Get(MGG_GraphicsSystem* system, mgint index);
MG_EXPORT void MGG_GraphicsAdapter_GetInfo(MGG_GraphicsAdapter* adapter, MGG_GraphicsAdaptor_Info& info);
MG_EXPORT MGG_GraphicsDevice* MGG_GraphicsDevice_Create(MGG_GraphicsSystem* system, MGG_GraphicsAdapter* adapter);
MG_EXPORT void MGG_GraphicsDevice_Destroy(MGG_GraphicsDevice* device);
MG_EXPORT void MGG_GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, MGG_GraphicsDevice_Caps& caps);
MG_EXPORT void MGG_GraphicsDevice_ResizeSwapchain(MGG_GraphicsDevice* device, void* nativeWindowHandle, mgint width, mgint height, MGSurfaceFormat color, MGDepthFormat depth);
MG_EXPORT mgint MGG_GraphicsDevice_BeginFrame(MGG_GraphicsDevice* device);
MG_EXPORT void MGG_GraphicsDevice_Clear(MGG_GraphicsDevice* device, MGClearOptions options, Vector4& color, mgfloat depth, mgint stencil);
MG_EXPORT void MGG_GraphicsDevice_Present(MGG_GraphicsDevice* device, mgint currentFrame, mgint syncInterval);
MG_EXPORT void MGG_GraphicsDevice_SetBlendState(MGG_GraphicsDevice* device, MGG_BlendState* state, mgfloat factorR, mgfloat factorG, mgfloat factorB, mgfloat factorA);
MG_EXPORT void MGG_GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state);
MG_EXPORT void MGG_GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state);
MG_EXPORT void MGG_GraphicsDevice_GetTitleSafeArea(mgint& x, mgint& y, mgint& width, mgint& height);
MG_EXPORT void MGG_GraphicsDevice_SetViewport(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, mgfloat minDepth, mgfloat maxDepth);
MG_EXPORT void MGG_GraphicsDevice_SetScissorRectangle(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height);
MG_EXPORT void MGG_GraphicsDevice_SetRenderTargets(MGG_GraphicsDevice* device, MGG_Texture** targets, mgint count);
MG_EXPORT void MGG_GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Buffer* buffer);
MG_EXPORT void MGG_GraphicsDevice_SetTexture(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Texture* texture);
MG_EXPORT void MGG_GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_SamplerState* state);
MG_EXPORT void MGG_GraphicsDevice_SetIndexBuffer(MGG_GraphicsDevice* device, MGIndexElementSize size, MGG_Buffer* buffer);
MG_EXPORT void MGG_GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, mgint slot, MGG_Buffer* buffer, mgint vertexOffset);
MG_EXPORT void MGG_GraphicsDevice_SetShader(MGG_GraphicsDevice* device, MGShaderStage stage, MGG_Shader* shader);
MG_EXPORT void MGG_GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout);
MG_EXPORT void MGG_GraphicsDevice_Draw(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint vertexStart, mgint vertexCount);
MG_EXPORT void MGG_GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart);
MG_EXPORT void MGG_GraphicsDevice_DrawIndexedInstanced(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart, mgint instanceCount);
MG_EXPORT MGG_BlendState* MGG_BlendState_Create(MGG_GraphicsDevice* device, MGG_BlendState_Info* infos);
MG_EXPORT void MGG_BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state);
MG_EXPORT MGG_DepthStencilState* MGG_DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info);
MG_EXPORT void MGG_DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state);
MG_EXPORT MGG_RasterizerState* MGG_RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info);
MG_EXPORT void MGG_RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state);
MG_EXPORT MGG_SamplerState* MGG_SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info);
MG_EXPORT void MGG_SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state);
MG_EXPORT MGG_Buffer* MGG_Buffer_Create(MGG_GraphicsDevice* device, MGBufferType type, mgint sizeInBytes);
MG_EXPORT void MGG_Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer);
MG_EXPORT void MGG_Buffer_SetData(MGG_GraphicsDevice* device, MGG_Buffer*& buffer, mgint offset, mgbyte* data, mgint length, mgbool discard);
MG_EXPORT void MGG_Buffer_GetData(MGG_GraphicsDevice* device, MGG_Buffer* buffer, mgint offset, mgbyte* data, mgint dataCount, mgint dataBytes, mgint dataStride);
MG_EXPORT MGG_Texture* MGG_Texture_Create(MGG_GraphicsDevice* device, MGTextureType type, MGSurfaceFormat format, mgint width, mgint height, mgint depth, mgint mipmaps, mgint slices);
MG_EXPORT MGG_Texture* MGG_RenderTarget_Create(MGG_GraphicsDevice* device, MGTextureType type, MGSurfaceFormat format, mgint width, mgint height, mgint depth, mgint mipmaps, mgint slices, MGDepthFormat depthFormat, mgint multiSampleCount, MGRenderTargetUsage usage);
MG_EXPORT void MGG_Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture);
MG_EXPORT void MGG_Texture_SetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes);
MG_EXPORT void MGG_Texture_GetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes);
MG_EXPORT MGG_InputLayout* MGG_InputLayout_Create(MGG_GraphicsDevice* device, mgint* strides, mgint streamCount, MGG_InputElement* elements, mgint elementCount);
MG_EXPORT void MGG_InputLayout_Destroy(MGG_GraphicsDevice* device, MGG_InputLayout* layout);
MG_EXPORT MGG_Shader* MGG_Shader_Create(MGG_GraphicsDevice* device, MGShaderStage stage, mgbyte* bytecode, mgint sizeInBytes);
MG_EXPORT void MGG_Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader);
MG_EXPORT MGG_OcclusionQuery* MGG_OcclusionQuery_Create(MGG_GraphicsDevice* device);
MG_EXPORT void MGG_OcclusionQuery_Destroy(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query);
MG_EXPORT void MGG_OcclusionQuery_Begin(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query);
MG_EXPORT void MGG_OcclusionQuery_End(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query);
MG_EXPORT mgbool MGG_OcclusionQuery_GetResult(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query, mgint& pixelCount);
