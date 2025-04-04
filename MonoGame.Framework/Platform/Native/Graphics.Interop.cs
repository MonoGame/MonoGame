// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
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

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_EffectResource_GetBytecode", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void EffectResource_GetBytecode(string name, out byte* bytecode, out int size);

    #endregion

    #region Graphics System

    
    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsSystem_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_GraphicsSystem* GraphicsSystem_Create();

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsSystem_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsSystem_Destroy(MGG_GraphicsSystem* system);

    #endregion

    #region Graphics Adapter

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsAdapter_Get", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_GraphicsAdapter* GraphicsAdapter_Get(MGG_GraphicsSystem* system, int index);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsAdapter_GetInfo", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsAdapter_GetInfo(MGG_GraphicsAdapter* adapter, out MGG_GraphicsAdaptor_Info info);

    #endregion

    #region Graphics Device

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_GraphicsDevice* GraphicsDevice_Create(MGG_GraphicsSystem* system, MGG_GraphicsAdapter* adapter);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_Destroy(MGG_GraphicsDevice* device);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_GetCaps", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, out MGG_GraphicsDevice_Caps caps);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_ResizeSwapchain", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_ResizeSwapchain(
        MGG_GraphicsDevice* device,
        nint nativeWindowHandle,
        int width,
        int height,
        SurfaceFormat color,
        DepthFormat depth);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_BeginFrame", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int GraphicsDevice_BeginFrame(MGG_GraphicsDevice* device);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Clear", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_Clear(MGG_GraphicsDevice* device, ClearOptions options, ref Microsoft.Xna.Framework.Vector4 color, float depth, int stencil);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Present", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_Present(MGG_GraphicsDevice* device, int currentFrame, int syncInterval);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetBlendState", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetBlendState(MGG_GraphicsDevice* device, MGG_BlendState* state, float factorR, float factorG, float factorB, float factorA);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetDepthStencilState", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetRasterizerState", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_GetTitleSafeArea", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_GetTitleSafeArea(
        ref int x,
        ref int y,
        ref int width,
        ref int height);
    
    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetViewport", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetViewport(
        MGG_GraphicsDevice* device,
        int x,
        int y,
        int width,
        int height,
        float minDepth,
        float maxDepth);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetScissorRectangle", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetScissorRectangle(
        MGG_GraphicsDevice* device,
        int x,
        int y,
        int width,
        int height);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetRenderTargets", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetRenderTargets(MGG_GraphicsDevice* device, MGG_Texture** targets, int count);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetConstantBuffer", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, ShaderStage stage, int slot, MGG_Buffer* buffer);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetTexture", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetTexture(MGG_GraphicsDevice* device, ShaderStage stage, int slot, MGG_Texture* texture);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetSamplerState", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, ShaderStage stage, int slot, MGG_SamplerState* state);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetIndexBuffer", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetIndexBuffer(MGG_GraphicsDevice* device, IndexElementSize size, MGG_Buffer* buffer);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetVertexBuffer", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, int slot, MGG_Buffer* buffer, int vertexOffset);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetShader", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetShader(MGG_GraphicsDevice* device, ShaderStage stage, MGG_Shader* shader);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_SetInputLayout", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_Draw", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_Draw(MGG_GraphicsDevice* device, PrimitiveType primitiveType, int vertexStart, int vertexCount);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_DrawIndexed", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, PrimitiveType primitiveType, int primitiveCount, int indexStart, int vertexStart);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_GraphicsDevice_DrawIndexedInstanced", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GraphicsDevice_DrawIndexedInstanced(MGG_GraphicsDevice* device, PrimitiveType primitiveType, int primitiveCount, int indexStart, int vertexStart, int instanceCount);

    #endregion

    #region State

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_BlendState_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_BlendState* BlendState_Create(MGG_GraphicsDevice* device, MGG_BlendState_Info* infos);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_BlendState_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_DepthStencilState_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_DepthStencilState* DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_DepthStencilState_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_RasterizerState_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_RasterizerState* RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_RasterizerState_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_SamplerState_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_SamplerState* SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_SamplerState_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state);

    #endregion

    #region Buffer

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Buffer_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_Buffer* Buffer_Create(MGG_GraphicsDevice* device, BufferType type, int sizeInBytes);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Buffer_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Buffer_SetData", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Buffer_SetData(
        MGG_GraphicsDevice* device,
        ref MGG_Buffer* buffer,
        int offset,
        byte* data,
        int length,
        [MarshalAs(UnmanagedType.U1)]
        bool discard);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Buffer_GetData", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Buffer_GetData(
        MGG_GraphicsDevice* device,
        MGG_Buffer* buffer,
        int offset,
        byte* data,
        int dataCount,
        int dataBytes,
        int dataStride);

    #endregion

    #region Texture

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Texture_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_Texture* Texture_Create(MGG_GraphicsDevice* device, TextureType type, SurfaceFormat format, int width, int height, int depth, int mipmaps, int slices);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_RenderTarget_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_Texture* RenderTarget_Create(
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

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Texture_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Texture_SetData", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Texture_SetData(
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

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Texture_GetData", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Texture_GetData(
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

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_InputLayout_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_InputLayout* InputLayout_Create(
        MGG_GraphicsDevice* device,
        int[] strides,
        int streamCount,
        MGG_InputElement[] elements,
        int elementCount
        );

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_InputLayout_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void InputLayout_Destroy(MGG_GraphicsDevice* device, MGG_InputLayout* layout);

    #endregion

    #region Shader

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Shader_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_Shader* Shader_Create(MGG_GraphicsDevice* device, ShaderStage stage, byte* bytecode, int sizeInBytes);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_Shader_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader);

    #endregion

    #region Occlusion Query

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGG_OcclusionQuery* OcclusionQuery_Create(MGG_GraphicsDevice* device);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void OcclusionQuery_Destroy(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_Begin", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void OcclusionQuery_Begin(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_End", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void OcclusionQuery_End(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGG_OcclusionQuery_GetResult", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool OcclusionQuery_GetResult(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query, out int pixelCount);

    #endregion
}
