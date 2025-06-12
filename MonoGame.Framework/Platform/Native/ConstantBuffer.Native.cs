// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

internal partial class ConstantBuffer
{
    internal unsafe MGG_Buffer* Handle;

    private unsafe void PlatformInitialize()
    {
        Handle = MGG.Buffer_Create(GraphicsDevice.Handle, BufferType.Constant, _buffer.Length);
    }

    private void PlatformClear()
    {
        // TODO: What is this for?
        throw new NotImplementedException();
    }

    internal unsafe void PlatformApply(GraphicsDevice device, ShaderStage stage, int slot)
    {
        if (Handle == null)
            PlatformInitialize();

        if (_dirty)
        {
            fixed (byte* data = &_buffer[0])
                MGG.Buffer_SetData(GraphicsDevice.Handle, ref Handle, 0, data, _buffer.Length, true);
            _dirty = false;
        }

        MGG.GraphicsDevice_SetConstantBuffer(GraphicsDevice.Handle, stage, slot, Handle);
    }

    protected override unsafe void Dispose(bool disposing)
    {
        if (Handle != null)
        {
            MGG.Buffer_Destroy(GraphicsDevice.Handle, Handle);
            Handle = null;
        }

        base.Dispose(disposing);
    }
}
