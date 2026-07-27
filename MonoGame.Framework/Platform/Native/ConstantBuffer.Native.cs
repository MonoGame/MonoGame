// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Framework.Utilities;
using MonoGame.Interop;


namespace Microsoft.Xna.Framework.Graphics;

internal partial class ConstantBuffer
{
    internal unsafe MGG_Buffer* Handle;

    private unsafe void PlatformInitialize()
    {
        Handle = MGG.Buffer_Create(GraphicsDevice.Handle, BufferType.Constant, true, _buffer.Length);
    }

    private unsafe void PlatformClear()
    {
        /*
         * Need to drop the OpenGL buffer here on reset so it gets
         * recreated and uploaded again the next time it is used.
         * Chris <aristurtledev>
         */
        if (PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL)
        {
            if (Handle != null)
            {
                MGG.Buffer_Destroy(GraphicsDevice.Handle, Handle);
                Handle = null;
            }

            _dirty = true;
        }
    }

    internal unsafe void PlatformApply(GraphicsDevice device, ShaderStage stage, int slot)
    {
        if (Handle == null)
            PlatformInitialize();

        if (_dirty)
        {
            fixed (byte* data = &_buffer[0])
                MGG.Buffer_SetData(GraphicsDevice.Handle, ref Handle, 0, data, 1, _buffer.Length, _buffer.Length, true);
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
