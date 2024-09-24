// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using MonoGame.Interop;
using MonoGame.Framework.Utilities;


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

    internal static int ShaderProfile
    {
        get; private set;
    }

    private unsafe void PlatformSetup()
    {
        // Creates the device, but no swap chain yet.
        Handle = MGG.GraphicsDevice_Create(NativeGamePlatform.GraphicsSystem, Adapter.Handle);

        // Get the device caps.
        MGG_GraphicsDevice_Caps caps;
        MGG.GraphicsDevice_GetCaps(Handle, out caps);

        MaxTextureSlots = caps.MaxTextureSlots;
        MaxVertexTextureSlots = caps.MaxVertexTextureSlots;
        _maxVertexBufferSlots = caps.MaxVertexBufferSlots;
        ShaderProfile = caps.ShaderProfile;
        UseHalfPixelOffset = false;
    }

    private unsafe void PlatformInitialize()
    {
        MGG.GraphicsDevice_ResizeSwapchain(
                Handle,
                PresentationParameters.DeviceWindowHandle,
                PresentationParameters.BackBufferWidth,
                PresentationParameters.BackBufferHeight,
                PresentationParameters.BackBufferFormat,
                PresentationParameters.DepthStencilFormat);

        // Setup the default texture.
        DefaultTexture = new Texture2D(this, 2, 2);
        DefaultTexture.SetData(new[] { Color.Black, Color.Black, Color.Black, Color.Black });
    }

    private unsafe void OnPresentationChanged()
    {
        // Finish any frame that is currently rendering.
        if (_currentFrame > -1)
        {
            var syncInterval = PresentationParameters.PresentationInterval.GetSyncInterval();
            MGG.GraphicsDevice_Present(Handle, _currentFrame, syncInterval);
        }

        // Now resize the back buffer.
        MGG.GraphicsDevice_ResizeSwapchain(
            Handle,
            PresentationParameters.DeviceWindowHandle,
            PresentationParameters.BackBufferWidth,
            PresentationParameters.BackBufferHeight,
            PresentationParameters.BackBufferFormat,
            PresentationParameters.DepthStencilFormat);

        _viewport = new Viewport(
            0,
            0,
            PresentationParameters.BackBufferWidth,
            PresentationParameters.BackBufferHeight,
            _viewport.MinDepth,
            _viewport.MaxDepth);

        // Begin a new frame it if was previously rendering.
        if (_currentFrame > -1)
        {
            _currentFrame = -1;
            BeginFrame();
        }
    }

    private unsafe void BeginFrame()
    {
        if (_currentFrame > -1)
            return;

        // Start the command buffer now.
        _currentFrame = MGG.GraphicsDevice_BeginFrame(Handle);

        // We must reapply all the state on a new command buffer.
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

        var syncInterval = PresentationParameters.PresentationInterval.GetSyncInterval();
        MGG.GraphicsDevice_Present(Handle, _currentFrame, syncInterval);
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

        MGG.GraphicsDevice_SetRenderTargets(Handle, null, 0);
    }

    private void PlatformResolveRenderTargets()
    {
        // Resolving MSAA render targets should be done here.
    }

    private unsafe IRenderTarget PlatformApplyRenderTargets()
    {
        BeginFrame();

        Array.Clear(_curRenderTargets, 0, 4);

        RenderTarget2D first = null;

        for (var i = 0; i < _currentRenderTargetCount; i++)
        {
            var binding = _currentRenderTargetBindings[i];
            var target = binding.RenderTarget as RenderTarget2D;
            _curRenderTargets[i] = target.Handle;
            if (i == 0)
                first = target;
        }

        fixed (MGG_Texture** targets = _curRenderTargets)
            MGG.GraphicsDevice_SetRenderTargets(Handle, targets, _currentRenderTargetCount);
        
        return first;
    }

    private void PlatformBeginApplyState()
    {
        BeginFrame();
    }

    private void PlatformApplyBlend()
    {
        if (_blendStateDirty)
        {
            _actualBlendState.PlatformApplyState(this);
            _blendStateDirty = false;
        }

        if (_blendFactorDirty)
        {
            // TODO?
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

        // If we're not applying shaders then early out now.
        if (!applyShaders)
            return;

        if (_vertexShader == null)
            throw new InvalidOperationException("A vertex shader must be set!");
        if (_pixelShader == null)
            throw new InvalidOperationException("A pixel shader must be set!");

        if (_indexBufferDirty)
        {
            if (_indexBuffer != null)
                MGG.GraphicsDevice_SetIndexBuffer(Handle, _indexBuffer.IndexElementSize, _indexBuffer.Handle);
            _indexBufferDirty = false;
        }

        if (_vertexBuffersDirty && _vertexBuffers.Count > 0)
        {
            var numBuffers = _vertexBuffers.Count;
            for (var slot = 0; slot < numBuffers; slot++)
            {
                var vertexBufferBinding = _vertexBuffers.Get(slot);
                var buffer = vertexBufferBinding.VertexBuffer;

                MGG.GraphicsDevice_SetVertexBuffer(Handle, slot, buffer.Handle, vertexBufferBinding.VertexOffset);
            }
        }

        if (_vertexShaderDirty)
        {
            MGG.GraphicsDevice_SetShader(Handle, ShaderStage.Vertex, _vertexShader.Handle);
            unchecked { _graphicsMetrics._vertexShaderCount++; }
        }

        if (_vertexShaderDirty || _vertexBuffersDirty)
        {
            var layout = _vertexShader.GetOrCreateLayout(_vertexBuffers);
            MGG.GraphicsDevice_SetInputLayout(Handle, layout);
            _vertexShaderDirty = _vertexBuffersDirty = false;
        }

        if (_pixelShaderDirty)
        {
            MGG.GraphicsDevice_SetShader(Handle, ShaderStage.Pixel, _pixelShader.Handle);
            _pixelShaderDirty = false;
            unchecked { _graphicsMetrics._pixelShaderCount++; }
        }

        _vertexConstantBuffers.SetConstantBuffers(this);
        _pixelConstantBuffers.SetConstantBuffers(this);

        VertexTextures.SetTextures(this);
        VertexSamplerStates.PlatformSetSamplers(this);

        Textures.SetTextures(this);
        SamplerStates.PlatformSetSamplers(this);
    }

    private int SetUserVertexBuffer<T>(T[] vertexData, int vertexOffset, int vertexCount, VertexDeclaration vertexDecl)
        where T : struct
    {
        DynamicVertexBuffer buffer;

        if (!_userVertexBuffers.TryGetValue(vertexDecl.GetHashCode(), out buffer) || buffer.VertexCount < vertexCount)
        {
            // Dispose the previous buffer if we have one.
            if (buffer != null)
                buffer.Dispose();

            buffer = new DynamicVertexBuffer(this, vertexDecl, Math.Max(vertexCount, 2000), BufferUsage.WriteOnly);
            _userVertexBuffers[vertexDecl.GetHashCode()] = buffer;
        }

        var startVertex = buffer.UserOffset;

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

        var indexSize = ReflectionHelpers.FastSizeOf<T>();
        var indexElementSize = indexSize == 2 ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits;

        var requiredIndexCount = Math.Max(indexCount, 6000);
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

        var startIndex = buffer.UserOffset;

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

        MGG.GraphicsDevice_DrawIndexed(Handle, primitiveType, primitiveCount, startIndex, baseVertex);
    }

    private unsafe  void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
    {
        var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, vertexCount, vertexDeclaration);
        ApplyState(true);

        MGG.GraphicsDevice_Draw(Handle, primitiveType, startVertex, vertexCount);
    }

    private unsafe void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
    {
        ApplyState(true);

        MGG.GraphicsDevice_Draw(Handle, primitiveType, vertexStart, vertexCount);
    }

    private unsafe void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
    {
        var indexCount = GetElementCountArray(primitiveType, primitiveCount);
        var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
        var startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);
        ApplyState(true);

        MGG.GraphicsDevice_DrawIndexed(Handle, primitiveType, primitiveCount, startIndex, startVertex);
    }

    private unsafe void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
    {
        var indexCount = GetElementCountArray(primitiveType, primitiveCount);
        var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
        var startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);
        ApplyState(true);

        MGG.GraphicsDevice_DrawIndexed(Handle, primitiveType, primitiveCount, startIndex, startVertex);
    }

    private unsafe void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
    {
        ApplyState(true);

        MGG.GraphicsDevice_DrawIndexedInstanced(Handle, primitiveType, primitiveCount, startIndex, baseVertex, instanceCount);
    }

    private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
    {
        throw new NotImplementedException();
    }

    private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
    {
        MGG.GraphicsDevice_GetTitleSafeArea(ref x, ref y, ref width, ref height);

        return new Rectangle(x, y, width, height);
    }
}
