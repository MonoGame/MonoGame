// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.Interop;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework.Graphics;

public partial class Texture3D : Texture
{
    private unsafe void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
    {
        Handle = MGG.Texture_Create(GraphicsDevice.Handle, TextureType._3D, format, width, height, depth, _levelCount, 1);
    }

    private unsafe void PlatformSetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount)
    {
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInByte;
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        var width = right - left;
        var height = bottom - top;
        var depth = back - front;

        MGG.Texture_SetData(
            GraphicsDevice.Handle,
            Handle,
            level,
            0,
            left,
            top,
            front,
            width,
            height,
            depth,
            (byte*)dataPtr,
            elementSizeInByte * elementCount);

        dataHandle.Free();
    }

    private unsafe void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct
    {
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInByte;
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        var width = right - left;
        var height = bottom - top;
        var depth = back - front;

        MGG.Texture_GetData(
            GraphicsDevice.Handle,
            Handle,
            level,
            0,
            left,
            top,
            front,
            width,
            height,
            depth,
            (byte*)dataPtr,
            elementSizeInByte * elementCount);

        dataHandle.Free();
    }
}
