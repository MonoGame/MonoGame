// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.Interop;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework.Graphics;

public partial class TextureCube
{
    private unsafe void PlatformConstruct(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
    {
        if (renderTarget)
            return;

        Handle = MGG.Texture_Create(GraphicsDevice.Handle, TextureType.Cube, format, size, size, 1, _levelCount, 6);
    }

    private unsafe void PlatformSetData<T>(CubeMapFace face, int level, Rectangle rect, T[] data, int startIndex, int elementCount)
    {
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInByte;
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;

        MGG.Texture_SetData(
            GraphicsDevice.Handle,
            Handle,
            level,
            0,
            rect.Left,
            rect.Top,
            (int)face,
            width,
            height,
            1,
            (byte*)dataPtr,
            elementSizeInByte * elementCount);

        dataHandle.Free();
    }

    private unsafe void PlatformGetData<T>(CubeMapFace face, int level, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
    {
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInByte;
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;

        MGG.Texture_GetData(
            GraphicsDevice.Handle,
            Handle,
            level,
            0,
            rect.Left,
            rect.Top,
            (int)face,
            width,
            height,
            1,
            (byte*)dataPtr,
            elementSizeInByte * elementCount);

        dataHandle.Free();
    }
}
