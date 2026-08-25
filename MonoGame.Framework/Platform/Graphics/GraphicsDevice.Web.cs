// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

public partial class GraphicsDevice
{
    internal unsafe MGG_GraphicsDevice* Handle;

    internal Texture2D DefaultTexture;

    private int _currentFrame = -1;

    private readonly Dictionary<int, DynamicVertexBuffer> _userVertexBuffers = new Dictionary<int, DynamicVertexBuffer>();
    private DynamicIndexBuffer _userIndexBuffer16;
    private DynamicIndexBuffer _userIndexBuffer32;

    private unsafe readonly MGG_Texture*[] _curRenderTargets = new MGG_Texture*[4];
    private readonly int[] _currentRenderTargetArraySlices = new int[4];

    internal static int ShaderProfile
    {
        get; private set;
    }

    internal int MaxTextureAnisotropy { get; private set; }
    internal int MaxMultiSampleCount { get; private set; }
    internal bool SupportsNonPowerOfTwo { get; private set; }
    internal bool SupportsTextureFilterAnisotropic { get; private set; }
    internal bool SupportsDepth24 { get; private set; }
    internal bool SupportsPackedDepthStencil { get; private set; }
    internal bool SupportsDepthNonLinear { get; private set; }
    internal bool SupportsTextureMaxLevel { get; private set; }
    internal bool SupportsDxt1 { get; private set; }
    internal bool SupportsS3tc { get; private set; }
    internal bool SupportsSRgb { get; private set; }
    internal bool SupportsDepthClamp { get; private set; }
    internal bool SupportsTextureArrays { get; private set; }
    internal bool SupportsVertexTextures { get; private set; }
    internal bool SupportsFloatTextures { get; private set; }
    internal bool SupportsHalfFloatTextures { get; private set; }
    internal bool SupportsNormalized { get; private set; }
    internal bool SupportsInstancing { get; private set; }
    internal bool SupportsBaseIndexInstancing { get; private set; }
    internal bool SupportsSeparateBlendStates { get; private set; }

    private unsafe void RefreshCapabilities()
    {
        MGG_GraphicsDevice_Caps caps;
        MGG.GraphicsDevice_GetCaps(Handle, out caps);

        MaxTextureSlots = caps.MaxTextureSlots;
        MaxVertexTextureSlots = caps.MaxVertexTextureSlots;
        _maxVertexBufferSlots = caps.MaxVertexBufferSlots;
        ShaderProfile = caps.ShaderProfile;
        MaxTextureAnisotropy = caps.MaxTextureAnisotropy;
        MaxMultiSampleCount = caps.MaxMultiSampleCount;
        SupportsNonPowerOfTwo = caps.SupportsNonPowerOfTwo;
        SupportsTextureFilterAnisotropic = caps.SupportsTextureFilterAnisotropic;
        SupportsDepth24 = caps.SupportsDepth24;
        SupportsPackedDepthStencil = caps.SupportsPackedDepthStencil;
        SupportsDepthNonLinear = caps.SupportsDepthNonLinear;
        SupportsTextureMaxLevel = caps.SupportsTextureMaxLevel;
        SupportsDxt1 = caps.SupportsDxt1;
        SupportsS3tc = caps.SupportsS3tc;
        SupportsSRgb = caps.SupportsSRgb;
        SupportsDepthClamp = caps.SupportsDepthClamp;
        SupportsTextureArrays = caps.SupportsTextureArrays;
        SupportsVertexTextures = caps.SupportsVertexTextures;
        SupportsFloatTextures = caps.SupportsFloatTextures;
        SupportsHalfFloatTextures = caps.SupportsHalfFloatTextures;
        SupportsNormalized = caps.SupportsNormalized;
        SupportsInstancing = caps.SupportsInstancing;
        SupportsBaseIndexInstancing = caps.SupportsBaseIndexInstancing;
        SupportsSeparateBlendStates = caps.SupportsSeparateBlendStates;
    }

    private unsafe void PlatformSetup()
    {
        Handle = MGG.GraphicsDevice_Create(WebGamePlatform.GraphicsSystem, Adapter.Handle);

        RefreshCapabilities();
        UseHalfPixelOffset = false;
    }

    private unsafe void PlatformInitialize()
    {
        int requestedMultiSampleCount = NormalizeMultiSampleCount(PresentationParameters.MultiSampleCount, MaxMultiSampleCount);

        MGG.GraphicsDevice_ResizeSwapchain(
                Handle,
                PresentationParameters.DeviceWindowHandle,
                PresentationParameters.BackBufferWidth,
                PresentationParameters.BackBufferHeight,
                PresentationParameters.BackBufferFormat,
                PresentationParameters.DepthStencilFormat,
                requestedMultiSampleCount,
                PresentationParameters.PresentationInterval.GetSyncInterval());

        RefreshCapabilities();
        UpdateBackBufferMultiSampleCount(requestedMultiSampleCount);
        GraphicsCapabilities.Initialize(this);

        DefaultTexture = new Texture2D(this, 2, 2);
        DefaultTexture.SetData(new[] { Color.Black, Color.Black, Color.Black, Color.Black });
    }

    internal int PlatformGetMaxMultiSampleCount(SurfaceFormat format)
    {
        return MaxMultiSampleCount;
    }

    private unsafe void OnPresentationChanged()
    {
        int requestedMultiSampleCount = NormalizeMultiSampleCount(PresentationParameters.MultiSampleCount, MaxMultiSampleCount);

        if (_currentFrame > -1)
        {
            PresentInterval syncInterval = PresentationParameters.PresentationInterval;
            MGG.GraphicsDevice_Present(Handle, _currentFrame, syncInterval.GetSyncInterval());
        }

        if (PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL)
        {
            WebGameWindow window = WebGameWindow.Instance;
            if (window != null)
                window.ApplyPendingNativeWindowChanges(PresentationParameters);
        }

        MGG.GraphicsDevice_ResizeSwapchain(
            Handle,
            PresentationParameters.DeviceWindowHandle,
            PresentationParameters.BackBufferWidth,
            PresentationParameters.BackBufferHeight,
            PresentationParameters.BackBufferFormat,
            PresentationParameters.DepthStencilFormat,
            requestedMultiSampleCount,
            PresentationParameters.PresentationInterval.GetSyncInterval());

        if (PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL)
        {
            WebGameWindow window = WebGameWindow.Instance;
            if (window != null)
                window.FinalizePendingNativeWindowChanges();
        }

        RefreshCapabilities();
        UpdateBackBufferMultiSampleCount(requestedMultiSampleCount);
        GraphicsCapabilities.Initialize(this);

        _viewport = new Viewport(
            0,
            0,
            PresentationParameters.BackBufferWidth,
            PresentationParameters.BackBufferHeight,
            _viewport.MinDepth,
            _viewport.MaxDepth);

        _scissorRectangle = new Rectangle(
            0,
            0,
            PresentationParameters.BackBufferWidth,
            PresentationParameters.BackBufferHeight);

        if (_currentFrame > -1)
        {
            _currentFrame = -1;
            BeginFrame();
        }
    }

    private unsafe void UpdateBackBufferMultiSampleCount(int requestedMultiSampleCount)
    {
        int actualMultiSampleCount = MGG.GraphicsDevice_GetBackBufferMultiSampleCount(Handle);

        if (PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL
            && actualMultiSampleCount == 0
            && requestedMultiSampleCount > 0)
        {
            PresentationParameters.MultiSampleCount = requestedMultiSampleCount;
            return;
        }

        PresentationParameters.MultiSampleCount = actualMultiSampleCount;
    }

    private unsafe void BeginFrame()
    {
        if (_currentFrame > -1)
            return;

        _currentFrame = MGG.GraphicsDevice_BeginFrame(Handle);

        _scissorRectangleDirty = true;
        _blendFactorDirty = true;
        _blendStateDirty = true;
        _pixelShaderDirty = true;
        _vertexShaderDirty = true;
        _depthStencilStateDirty = true;
        _indexBufferDirty = true;
        _rasterizerStateDirty = true;
        _vertexBuffersDirty = true;
        Textures.Dirty();
        SamplerStates.Dirty();

        MGG.GraphicsDevice_SetViewport(
            Handle,
            _viewport.X,
            _viewport.Y,
            _viewport.Width,
            _viewport.Height,
            _viewport.MinDepth,
            _viewport.MaxDepth);

        PlatformApplyDefaultRenderTarget();
    }

    private unsafe void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
    {
        BeginFrame();

        PlatformBeginApplyState();
        MGG.GraphicsDevice_Clear(Handle, options, ref color, depth, stencil);
    }

    private unsafe void PlatformDispose()
    {
        foreach (DynamicVertexBuffer vb in _userVertexBuffers.Values)
            vb.Dispose();
        _userVertexBuffers.Clear();

        if (_userIndexBuffer16 != null)
        {
            _userIndexBuffer16.Dispose();
            _userIndexBuffer16 = null;
        }

        if (_userIndexBuffer32 != null)
        {
            _userIndexBuffer32.Dispose();
            _userIndexBuffer32 = null;
        }

        DefaultTexture.Dispose();

        if (Handle != null)
        {
            MGG.GraphicsDevice_Destroy(Handle);
            Handle = null;
        }
    }

    private unsafe void PlatformPresent()
    {
        if (_currentFrame < 0)
            return;

        PresentInterval syncInterval = PresentationParameters.PresentationInterval;
        MGG.GraphicsDevice_Present(Handle, _currentFrame, syncInterval.GetSyncInterval());
        _currentFrame = -1;
    }

    private unsafe void PlatformSetViewport(ref Viewport viewport)
    {
        BeginFrame();

        MGG.GraphicsDevice_SetViewport(
            Handle,
            viewport.X,
            viewport.Y,
            viewport.Width,
            viewport.Height,
            viewport.MinDepth,
            viewport.MaxDepth);
    }

    private unsafe void PlatformApplyDefaultRenderTarget()
    {
        BeginFrame();

        _viewport = new Viewport(
            0,
            0,
            PresentationParameters.BackBufferWidth,
            PresentationParameters.BackBufferHeight,
            _viewport.MinDepth,
            _viewport.MaxDepth);

        _scissorRectangle = new Rectangle(
            0,
            0,
            PresentationParameters.BackBufferWidth,
            PresentationParameters.BackBufferHeight);

        if (PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL)
        {
            _rasterizerStateDirty = true;
            Textures.Dirty();
        }

        MGG.GraphicsDevice_SetRenderTargets(Handle, null, null, 0);
    }

    private unsafe void PlatformResolveRenderTargets()
    {
        MGG.GraphicsDevice_ResolveRenderTargets(Handle);
    }

    private unsafe IRenderTarget PlatformApplyRenderTargets()
    {
        BeginFrame();

        if (PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL)
        {
            _rasterizerStateDirty = true;
            Textures.Dirty();
        }

        Array.Clear(_curRenderTargets, 0, 4);

        IRenderTarget first = null;

        for (int i = 0; i < _currentRenderTargetCount; i++)
        {
            RenderTargetBinding binding = _currentRenderTargetBindings[i];
            Texture target = binding.RenderTarget;
            _curRenderTargets[i] = target.Handle;
            _currentRenderTargetArraySlices[i] = binding.ArraySlice;
            if (i == 0)
                first = target as IRenderTarget;
        }

        fixed (MGG_Texture** targets = _curRenderTargets)
        fixed (int* arraySlices = _currentRenderTargetArraySlices)
            MGG.GraphicsDevice_SetRenderTargets(Handle, targets, arraySlices, _currentRenderTargetCount);

        return first;
    }

    private void PlatformBeginApplyState()
    {
        BeginFrame();
    }

    private void PlatformApplyBlend()
    {
        if (_blendStateDirty || _blendFactorDirty)
        {
            _actualBlendState.PlatformApplyState(this);
            _blendStateDirty = false;
            _blendFactorDirty = false;
        }
    }

    private unsafe void PlatformApplyState(bool applyShaders)
    {
        if (_scissorRectangleDirty)
        {
            MGG.GraphicsDevice_SetScissorRectangle(
                Handle,
                _scissorRectangle.X,
                _scissorRectangle.Y,
                _scissorRectangle.Width,
                _scissorRectangle.Height);

            _scissorRectangleDirty = false;
        }

        if (!applyShaders)
            return;

        if (_vertexShader == null)
            throw new InvalidOperationException("A vertex shader must be set!");
        if (_pixelShader == null)
            throw new InvalidOperationException("A pixel shader must be set!");

        bool layoutChanged = false;

        if (_vertexShaderDirty)
        {
            MGG.GraphicsDevice_SetShader(Handle, ShaderStage.Vertex, _vertexShader.Handle);
            _vertexBuffersDirty = true;
            unchecked { _graphicsMetrics._vertexShaderCount++; }
        }

        if (_pixelShaderDirty)
        {
            MGG.GraphicsDevice_SetShader(Handle, ShaderStage.Pixel, _pixelShader.Handle);
            unchecked { _graphicsMetrics._pixelShaderCount++; }
        }

        if (_indexBufferDirty)
        {
            if (_indexBuffer != null)
                MGG.GraphicsDevice_SetIndexBuffer(Handle, _indexBuffer.IndexElementSize, _indexBuffer.Handle);
        }

        if (layoutChanged || _vertexBuffersDirty)
        {
            MGG_InputLayout* layout = _vertexShader.GetOrCreateLayout(_vertexBuffers);
            MGG.GraphicsDevice_SetInputLayout(Handle, layout);
        }

        if (_vertexBuffersDirty)
        {
            for (int slot = 0; slot < _vertexBuffers.Count; slot++)
            {
                VertexBufferBinding vertexBufferBinding = _vertexBuffers.Get(slot);
                VertexBuffer buffer = vertexBufferBinding.VertexBuffer;

                MGG.GraphicsDevice_SetVertexBuffer(Handle, slot, buffer.Handle, vertexBufferBinding.VertexOffset);
            }
        }

        _vertexConstantBuffers.SetConstantBuffers(this);
        _pixelConstantBuffers.SetConstantBuffers(this);
        VertexTextures.SetTextures(this);
        VertexSamplerStates.PlatformSetSamplers(this);
        Textures.SetTextures(this);
        SamplerStates.PlatformSetSamplers(this);

        _indexBufferDirty = false;
        _vertexBuffersDirty = false;
        _vertexShaderDirty = false;
        _pixelShaderDirty = false;
    }

    private int SetUserVertexBuffer<T>(T[] vertexData, int vertexOffset, int vertexCount, VertexDeclaration vertexDecl)
        where T : struct
    {
        DynamicVertexBuffer buffer;

        if (!_userVertexBuffers.TryGetValue(vertexDecl.GetHashCode(), out buffer) || buffer.VertexCount < vertexCount)
        {
            if (buffer != null)
                buffer.Dispose();

            buffer = new DynamicVertexBuffer(this, vertexDecl, Math.Max(vertexCount, 2000), BufferUsage.WriteOnly);
            _userVertexBuffers[vertexDecl.GetHashCode()] = buffer;
        }

        int startVertex = buffer.UserOffset;

        if ((vertexCount + buffer.UserOffset) < buffer.VertexCount)
        {
            buffer.UserOffset += vertexCount;
            buffer.SetData(startVertex * vertexDecl.VertexStride, vertexData, vertexOffset, vertexCount, vertexDecl.VertexStride, SetDataOptions.NoOverwrite);
        }
        else
        {
            buffer.UserOffset = vertexCount;
            buffer.SetData(vertexData, vertexOffset, vertexCount, SetDataOptions.Discard);
            startVertex = 0;
        }

        SetVertexBuffer(buffer);

        return startVertex;
    }

    private int SetUserIndexBuffer<T>(T[] indexData, int indexOffset, int indexCount)
        where T : struct
    {
        DynamicIndexBuffer buffer;

        int indexSize = ReflectionHelpers.FastSizeOf<T>();
        IndexElementSize indexElementSize = indexSize == 2 ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits;

        int requiredIndexCount = Math.Max(indexCount, 6000);
        if (indexElementSize == IndexElementSize.SixteenBits)
        {
            if (_userIndexBuffer16 == null || _userIndexBuffer16.IndexCount < requiredIndexCount)
            {
                if (_userIndexBuffer16 != null)
                    _userIndexBuffer16.Dispose();

                _userIndexBuffer16 = new DynamicIndexBuffer(this, indexElementSize, requiredIndexCount, BufferUsage.WriteOnly);
            }

            buffer = _userIndexBuffer16;
        }
        else
        {
            if (_userIndexBuffer32 == null || _userIndexBuffer32.IndexCount < requiredIndexCount)
            {
                if (_userIndexBuffer32 != null)
                    _userIndexBuffer32.Dispose();

                _userIndexBuffer32 = new DynamicIndexBuffer(this, indexElementSize, requiredIndexCount, BufferUsage.WriteOnly);
            }

            buffer = _userIndexBuffer32;
        }

        int startIndex = buffer.UserOffset;

        if ((indexCount + buffer.UserOffset) < buffer.IndexCount)
        {
            buffer.UserOffset += indexCount;
            buffer.SetData(startIndex * indexSize, indexData, indexOffset, indexCount, SetDataOptions.NoOverwrite);
        }
        else
        {
            startIndex = 0;
            buffer.UserOffset = indexCount;
            buffer.SetData(indexData, indexOffset, indexCount, SetDataOptions.Discard);
        }

        Indices = buffer;

        return startIndex;
    }

    private unsafe void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
    {
        ApplyState(true);
        if (baseVertex < 0)
            baseVertex = 0;
        if (startIndex < 0)
            startIndex = 0;

        MGG.GraphicsDevice_DrawIndexed(Handle, primitiveType, primitiveCount, startIndex, baseVertex);
    }

    private unsafe void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount)
        where T : struct
    {
        int startVertex = SetUserVertexBuffer(vertexData, vertexOffset, vertexCount, vertexDeclaration);
        ApplyState(true);

        MGG.GraphicsDevice_Draw(Handle, primitiveType, startVertex, vertexCount);
    }

    private unsafe void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
    {
        ApplyState(true);
        if (vertexStart < 0)
            vertexStart = 0;

        MGG.GraphicsDevice_Draw(Handle, primitiveType, vertexStart, vertexCount);
    }

    private unsafe void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
        where T : struct
    {
        int indexCount = GetElementCountArray(primitiveType, primitiveCount);
        int startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
        int startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);
        ApplyState(true);

        MGG.GraphicsDevice_DrawIndexed(Handle, primitiveType, primitiveCount, startIndex, startVertex);
    }

    private unsafe void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
        where T : struct
    {
        int indexCount = GetElementCountArray(primitiveType, primitiveCount);
        int startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
        int startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);
        ApplyState(true);

        MGG.GraphicsDevice_DrawIndexed(Handle, primitiveType, primitiveCount, startIndex, startVertex);
    }

    private unsafe void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
    {
        ApplyState(true);

        MGG.GraphicsDevice_DrawIndexedInstanced(Handle, primitiveType, primitiveCount, startIndex, baseVertex, instanceCount);
    }

    private unsafe void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count)
        where T : struct
    {
        Rectangle rectangle = rect ?? new Rectangle(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
        int tSize = Marshal.SizeOf<T>();
        GCHandle dataHandle = default;
        try
        {
            dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pData = dataHandle.AddrOfPinnedObject();
            MGG.GraphicsDevice_GetBackBufferData(
                Handle,
                rectangle.X,
                rectangle.Y,
                rectangle.Width,
                rectangle.Height,
                pData + startIndex * tSize,
                count,
                tSize);
        }
        finally
        {
            if (dataHandle.IsAllocated)
                dataHandle.Free();
        }
    }

    private static unsafe Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
    {
        MGG.GraphicsDevice_GetTitleSafeArea(ref x, ref y, ref width, ref height);

        return new Rectangle(x, y, width, height);
    }
}
