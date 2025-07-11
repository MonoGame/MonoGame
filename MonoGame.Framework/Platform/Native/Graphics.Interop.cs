// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;


namespace MonoGame.Interop;

internal struct PtrTo<T>
{
    public unsafe T* Ptr;
}

[MGHandle] internal readonly struct MGG_GraphicsSystem { }
[MGHandle] internal readonly struct MGG_GraphicsAdapter { }
[MGHandle] internal readonly struct MGG_GraphicsDevice { }
[MGHandle] internal readonly struct MGG_BlendState { }
[MGHandle] internal readonly struct MGG_DepthStencilState { }
[MGHandle] internal readonly struct MGG_RasterizerState { }
[MGHandle] internal readonly struct MGG_Buffer { }
[MGHandle] internal readonly struct MGG_Texture { }
[MGHandle] internal readonly struct MGG_SamplerState { }
[MGHandle] internal readonly struct MGG_Shader { }
[MGHandle] internal readonly struct MGG_InputLayout { }
[MGHandle] internal readonly struct MGG_OcclusionQuery { }


[StructLayout(LayoutKind.Sequential)]
internal struct MGG_DisplayMode
{
    public SurfaceFormat format;
    public int width;
    public int height;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGG_GraphicsAdaptor_Info
{
    public nint DeviceName;
    public nint Description;
    public int DeviceId;
    public int Revision;
    public int VendorId;
    public int SubSystemId;
    public nint MonitorHandle;

    public unsafe MGG_DisplayMode* DisplayModes;
    public int DisplayModeCount;

    public MGG_DisplayMode CurrentDisplayMode;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGG_GraphicsDevice_Caps
{
    public int MaxTextureSlots;
    public int MaxVertexTextureSlots;
    public int MaxVertexBufferSlots;
    public int ShaderProfile;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGG_BlendState_Info
{
    public Blend colorSourceBlend;
    public Blend colorDestBlend;
    public BlendFunction colorBlendFunc;
    public Blend alphaSourceBlend;
    public Blend alphaDestBlend;
    public BlendFunction alphaBlendFunc;
    public ColorWriteChannels colorWriteChannels;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGG_DepthStencilState_Info
{
    public bool depthBufferEnable;
    public bool depthBufferWriteEnable;
    public CompareFunction depthBufferFunction;
    public int referenceStencil;
    public bool stencilEnable;
    public int stencilMask;
    public int stencilWriteMask;
    public CompareFunction stencilFunction;
    public StencilOperation stencilDepthBufferFail;
    public StencilOperation stencilFail;
    public StencilOperation stencilPass;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGG_RasterizerState_Info
{
    public FillMode fillMode;
    public CullMode cullMode;
    public bool scissorTestEnable;
    public bool depthClipEnable;
    public float depthBias;
    public float slopeScaleDepthBias;
    public bool multiSampleAntiAlias;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGG_SamplerState_Info
{
    public TextureAddressMode AddressU;
    public TextureAddressMode AddressV;
    public TextureAddressMode AddressW;
    public uint BorderColor;
    public TextureFilter Filter;
    public TextureFilterMode FilterMode;
    public int MaximumAnisotropy;
    public int MaxMipLevel;
    public float MipMapLevelOfDetailBias;
    public CompareFunction ComparisonFunction;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGG_InputElement
{
    public VertexElementUsage SemanticUsage;
    public uint SemanticIndex;
    public uint VertexBufferSlot;
    public VertexElementFormat Format;
    public uint AlignedByteOffset;
    public uint InstanceDataStepRate;
}

internal enum TextureType
{
    _2D,
    _3D,
    Cube,
}

internal enum BufferType
{
    Index,
    Vertex,
    Constant,
}


internal static unsafe partial class MGG
{
    #region Effect Resources

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_EffectResource_GetBytecode", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern void EffectResource_GetBytecode([MarshalAs(UnmanagedType.LPUTF8Str)] string name, out byte* bytecode, out int size);

    #endregion

    #region Graphics System

    
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsSystem_Create", ExactSpelling = true)]
    public static extern MGG_GraphicsSystem* GraphicsSystem_Create();

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsSystem_Destroy", ExactSpelling = true)]
    public static extern void GraphicsSystem_Destroy(MGG_GraphicsSystem* system);

    #endregion

    #region Graphics Adapter

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsAdapter_Get", ExactSpelling = true)]
    public static extern MGG_GraphicsAdapter* GraphicsAdapter_Get(MGG_GraphicsSystem* system, int index);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsAdapter_GetInfo", ExactSpelling = true)]
    public static extern void GraphicsAdapter_GetInfo(MGG_GraphicsAdapter* adapter, out MGG_GraphicsAdaptor_Info info);

    #endregion

    #region Graphics Device

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Create", ExactSpelling = true)]
    public static extern MGG_GraphicsDevice* GraphicsDevice_Create(MGG_GraphicsSystem* system, MGG_GraphicsAdapter* adapter);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Destroy", ExactSpelling = true)]
    public static extern void GraphicsDevice_Destroy(MGG_GraphicsDevice* device);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_GetCaps", ExactSpelling = true)]
    public static extern void GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, out MGG_GraphicsDevice_Caps caps);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_ResizeSwapchain", ExactSpelling = true)]
    public static extern void GraphicsDevice_ResizeSwapchain(
        MGG_GraphicsDevice* device,
        nint nativeWindowHandle,
        int width,
        int height,
        SurfaceFormat color,
        DepthFormat depth);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_BeginFrame", ExactSpelling = true)]
    public static extern int GraphicsDevice_BeginFrame(MGG_GraphicsDevice* device);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Clear", ExactSpelling = true)]
    public static extern void GraphicsDevice_Clear(MGG_GraphicsDevice* device, ClearOptions options, ref Microsoft.Xna.Framework.Vector4 color, float depth, int stencil);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Present", ExactSpelling = true)]
    public static extern void GraphicsDevice_Present(MGG_GraphicsDevice* device, int currentFrame, int syncInterval);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetBlendState", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetBlendState(MGG_GraphicsDevice* device, MGG_BlendState* state, float factorR, float factorG, float factorB, float factorA);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetDepthStencilState", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetRasterizerState", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_GetTitleSafeArea", ExactSpelling = true)]
    public static extern void GraphicsDevice_GetTitleSafeArea(
        ref int x,
        ref int y,
        ref int width,
        ref int height);
    
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetViewport", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetViewport(
        MGG_GraphicsDevice* device,
        int x,
        int y,
        int width,
        int height,
        float minDepth,
        float maxDepth);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetScissorRectangle", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetScissorRectangle(
        MGG_GraphicsDevice* device,
        int x,
        int y,
        int width,
        int height);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetRenderTargets", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetRenderTargets(MGG_GraphicsDevice* device, MGG_Texture** targets, int count);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetConstantBuffer", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, ShaderStage stage, int slot, MGG_Buffer* buffer);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetTexture", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetTexture(MGG_GraphicsDevice* device, ShaderStage stage, int slot, MGG_Texture* texture);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetSamplerState", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, ShaderStage stage, int slot, MGG_SamplerState* state);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetIndexBuffer", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetIndexBuffer(MGG_GraphicsDevice* device, IndexElementSize size, MGG_Buffer* buffer);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetVertexBuffer", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, int slot, MGG_Buffer* buffer, int vertexOffset);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetShader", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetShader(MGG_GraphicsDevice* device, ShaderStage stage, MGG_Shader* shader);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetInputLayout", ExactSpelling = true)]
    public static extern void GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Draw", ExactSpelling = true)]
    public static extern void GraphicsDevice_Draw(MGG_GraphicsDevice* device, PrimitiveType primitiveType, int vertexStart, int vertexCount);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_DrawIndexed", ExactSpelling = true)]
    public static extern void GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, PrimitiveType primitiveType, int primitiveCount, int indexStart, int vertexStart);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_DrawIndexedInstanced", ExactSpelling = true)]
    public static extern void GraphicsDevice_DrawIndexedInstanced(MGG_GraphicsDevice* device, PrimitiveType primitiveType, int primitiveCount, int indexStart, int vertexStart, int instanceCount);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_GetBackBufferData", ExactSpelling = true)]
    public static extern void GraphicsDevice_GetBackBufferData(
        MGG_GraphicsDevice* device,
        int x,
        int y,
        int width,
        int height,
        IntPtr data,
        int count,
        int dataBytes);

    #endregion

    #region State

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_BlendState_Create", ExactSpelling = true)]
    public static extern MGG_BlendState* BlendState_Create(MGG_GraphicsDevice* device, MGG_BlendState_Info* infos);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_BlendState_Destroy", ExactSpelling = true)]
    public static extern void BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_DepthStencilState_Create", ExactSpelling = true)]
    public static extern MGG_DepthStencilState* DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_DepthStencilState_Destroy", ExactSpelling = true)]
    public static extern void DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_RasterizerState_Create", ExactSpelling = true)]
    public static extern MGG_RasterizerState* RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_RasterizerState_Destroy", ExactSpelling = true)]
    public static extern void RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_SamplerState_Create", ExactSpelling = true)]
    public static extern MGG_SamplerState* SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_SamplerState_Destroy", ExactSpelling = true)]
    public static extern void SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state);

    #endregion

    #region Buffer

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Buffer_Create", ExactSpelling = true)]
    public static extern MGG_Buffer* Buffer_Create(MGG_GraphicsDevice* device, BufferType type, int sizeInBytes);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Buffer_Destroy", ExactSpelling = true)]
    public static extern void Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Buffer_SetData", ExactSpelling = true)]
    public static extern void Buffer_SetData(
        MGG_GraphicsDevice* device,
        ref MGG_Buffer* buffer,
        int offset,
        byte* data,
        int length,       
        bool discard);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Buffer_GetData", ExactSpelling = true)]
    public static extern void Buffer_GetData(
        MGG_GraphicsDevice* device,
        MGG_Buffer* buffer,
        int offset,
        byte* data,
        int dataCount,
        int dataBytes,
        int dataStride);

    #endregion

    #region Texture

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Texture_Create", ExactSpelling = true)]
    public static extern MGG_Texture* Texture_Create(MGG_GraphicsDevice* device, TextureType type, SurfaceFormat format, int width, int height, int depth, int mipmaps, int slices);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_RenderTarget_Create", ExactSpelling = true)]
    public static extern MGG_Texture* RenderTarget_Create(
        MGG_GraphicsDevice* device,
        TextureType type,
        SurfaceFormat format,
        int width,
        int height,
        int depth,
        int mipmaps,
        int slices,
        DepthFormat depthFormat,
        int multiSampleCount,
        RenderTargetUsage usage);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Texture_Destroy", ExactSpelling = true)]
    public static extern void Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Texture_SetData", ExactSpelling = true)]
    public static extern void Texture_SetData(
        MGG_GraphicsDevice* device,
        MGG_Texture* texture,
        int level,
        int slice,
        int x,
        int y,
        int z,
        int width,
        int height,
        int depth,
        byte* data,
        int dataBytes);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Texture_GetData", ExactSpelling = true)]
    public static extern void Texture_GetData(
        MGG_GraphicsDevice* device,
        MGG_Texture* texture,
        int level,
        int slice,
        int x,
        int y,
        int z,
        int width,
        int height,
        int depth,
        byte* data,
        int dataBytes);

    #endregion

    #region Input Layout

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_InputLayout_Create", ExactSpelling = true)]
    public static extern MGG_InputLayout* InputLayout_Create(
        MGG_GraphicsDevice* device,
        MGG_Shader* vertexShader,
        int* strides,
        int streamCount,
        MGG_InputElement* elements,
        int elementCount
        );

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_InputLayout_Destroy", ExactSpelling = true)]
    public static extern void InputLayout_Destroy(MGG_GraphicsDevice* device, MGG_InputLayout* layout);

    #endregion

    #region Shader

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Shader_Create", ExactSpelling = true)]
    public static extern MGG_Shader* Shader_Create(MGG_GraphicsDevice* device, ShaderStage stage, byte* bytecode, int sizeInBytes);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Shader_Destroy", ExactSpelling = true)]
    public static extern void Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader);

    #endregion

    #region Occlusion Query

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_Create", ExactSpelling = true)]
    public static extern MGG_OcclusionQuery* OcclusionQuery_Create(MGG_GraphicsDevice* device);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_Destroy", ExactSpelling = true)]
    public static extern void OcclusionQuery_Destroy(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_Begin", ExactSpelling = true)]
    public static extern void OcclusionQuery_Begin(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_End", ExactSpelling = true)]
    public static extern void OcclusionQuery_End(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_GetResult", ExactSpelling = true)]
    public static extern byte OcclusionQuery_GetResult(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query, out int pixelCount);

    #endregion
}
