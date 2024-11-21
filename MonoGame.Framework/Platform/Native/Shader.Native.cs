// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

partial class Shader
{
    internal unsafe MGG_Shader* Handle;

    private byte[] _bytecode;
    private readonly Dictionary<VertexInputLayout, PtrTo<MGG_InputLayout>> _cache = new Dictionary<VertexInputLayout, PtrTo<MGG_InputLayout>>();

    private static int PlatformProfile()
    {
        return GraphicsDevice.ShaderProfile;
    }

    private unsafe void PlatformConstruct(ShaderStage stage, byte[] shaderBytecode)
    {
        fixed (byte* bytecode = shaderBytecode)
            Handle = MGG.Shader_Create(GraphicsDevice.Handle, stage, bytecode, shaderBytecode.Length);
    }

    internal unsafe MGG_InputLayout* GetOrCreateLayout(VertexBufferBindings vertexBuffers)
    {
        // First check the cache.
        PtrTo<MGG_InputLayout> inputLayout;
        if (_cache.TryGetValue(vertexBuffers, out inputLayout))
            return inputLayout.Ptr;

        // Generate and cache a new layout based on the current
        // vb bindings and this vertex shader.

        var vertexInputLayout = vertexBuffers.ToImmutable();
        vertexInputLayout.GenerateInputElements(Attributes, out var inputElements, out var streamStrides);

        inputLayout.Ptr = MGG.InputLayout_Create(
            GraphicsDevice.Handle,
            streamStrides,
            streamStrides.Length,
            inputElements,
            inputElements.Length);

        _cache.Add(vertexInputLayout, inputLayout);

        return inputLayout.Ptr;
    }

    private unsafe void PlatformGraphicsDeviceResetting()
    {
        foreach (var pair in _cache)
            MGG.InputLayout_Destroy(GraphicsDevice.Handle, pair.Value.Ptr);
        _cache.Clear();

        if (Handle != null)
        {
            MGG.Shader_Destroy(GraphicsDevice.Handle, Handle);
            Handle = null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        PlatformGraphicsDeviceResetting();

        base.Dispose(disposing);
    }
}
