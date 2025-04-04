// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.Interop;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework.Graphics;

public partial class VertexBuffer
{
    internal unsafe MGG_Buffer* Handle;

    private unsafe void PlatformConstruct()
    {
        Handle = MGG.Buffer_Create(GraphicsDevice.Handle, BufferType.Vertex, VertexCount * VertexDeclaration.VertexStride);
    }

    private unsafe void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
    {
        var elementSizeInBytes = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInBytes;
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        MGG.Buffer_GetData(GraphicsDevice.Handle, Handle, offsetInBytes, (byte*)dataPtr, elementCount, elementSizeInBytes, vertexStride);

        dataHandle.Free();
    }

    private unsafe void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
    {
        var startBytes = startIndex * elementSizeInBytes;
        var dataBytes = elementCount * elementSizeInBytes;
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        // TODO: We need to figure out the correct behavior 
        // for SetDataOptions.None on a dynamic buffer.
        //
        // For now we always discard as it is a pretty safe default.
        //
        var discard = _isDynamic && options != SetDataOptions.NoOverwrite;

        MGG.Buffer_SetData(GraphicsDevice.Handle, ref Handle, offsetInBytes, (byte*)dataPtr, dataBytes, discard);

        dataHandle.Free();
    }

    private unsafe void PlatformGraphicsDeviceResetting()
    {
        if (Handle != null)
        {
            MGG.Buffer_Destroy(GraphicsDevice.Handle, Handle);
            Handle = null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        PlatformGraphicsDeviceResetting();

        base.Dispose(disposing);
    }
}
