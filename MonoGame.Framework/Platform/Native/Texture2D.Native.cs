// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Interop;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework.Graphics;

public partial class Texture2D : Texture
{
    /// <summary>
    /// Wrap a texture handle without owning the native texture.
    /// </summary>
    internal unsafe Texture2D(GraphicsDevice graphicsDevice, MGG_Texture* handle, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, int arraySize)
    {
        this.GraphicsDevice = graphicsDevice;
        this.Handle = handle;
        this.Owned = false;
        this.width = width;
        this.height = height;
        this.TexelWidth = 1f / (float)width;
        this.TexelHeight = 1f / (float)height;
        this._format = format;
        this._levelCount = mipmap ? CalculateMipLevels(width, height) : 1;
        this.ArraySize = arraySize;
    }

    private unsafe void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
    {
        // Ignore creation calls for RenderTargets and Swapchains.
        if (type != SurfaceType.Texture)
            return;

        Handle = MGG.Texture_Create(GraphicsDevice.Handle, TextureType._2D, format, width, height, 1, _levelCount, ArraySize);
    }

    private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
    {
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInByte;
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        unsafe
        {
            MGG.Texture_SetData(
                GraphicsDevice.Handle,
                Handle,
                level,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                (byte*)dataPtr,
                elementSizeInByte * elementCount);
        }

        dataHandle.Free();
    }

    private void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
    {
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInByte;
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        unsafe
        {
            MGG.Texture_SetData(
                GraphicsDevice.Handle,
                Handle,
                level,
                arraySlice,
                rect.X,
                rect.Y,
                0,
                rect.Width,
                rect.Height,
                1,
                (byte*)dataPtr,
                elementSizeInByte * elementCount);
        }

        dataHandle.Free();
    }

    private unsafe void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
    {
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInByte;
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        MGG.Texture_GetData(
            GraphicsDevice.Handle,
            Handle,
            level,
            arraySlice,
            rect.X,
            rect.Y,
            0,
            rect.Width,
            rect.Height,
            1,
            (byte*)dataPtr,
            elementSizeInByte * elementCount);

        dataHandle.Free();
    }

    private static unsafe Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream, Action<byte[]> colorProcessor)
    {
        // HACK: Clear the default zero action as we do this natively.
        if (colorProcessor == DefaultColorProcessors.ZeroTransparentPixels)
            colorProcessor = null;

        // Simply read it all into memory as it will be fast
        // for most cases and simplifies the native API.

        var dataLength = (int)stream.Length;
        var streamTemp = new byte[dataLength];
        stream.Read(streamTemp, 0, dataLength);

        var handle = GCHandle.Alloc(streamTemp, GCHandleType.Pinned);

        byte* rgba;
        int width, height;

        try
        {
            MGI.ReadRGBA(
                (byte*)handle.AddrOfPinnedObject(),
                dataLength,
                colorProcessor == null ? true : false,
                out width,
                out height,
                out rgba);

            if (rgba == null)
                return null;
        }
        finally
        {
            handle.Free();
        }

        var texture = new Texture2D(graphicsDevice, width, height);
        var rgbaBytes = (width * height) * 4;

        if (colorProcessor == null)
        {
            // Without a color processor take the fast path.

            MGG.Texture_SetData(
                graphicsDevice.Handle,
                texture.Handle,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                rgba,
                rgbaBytes);

            Marshal.FreeHGlobal((nint)rgba);

            return texture;
        }

        // Since color processor takes a byte[] we need to copy the
        // native memory to a managed array.
        //
        // Ideally we change this to use Span which avoids this.
        var bytes = new byte[rgbaBytes];
        Marshal.Copy((nint)rgba, bytes, 0, rgbaBytes);
        Marshal.FreeHGlobal((nint)rgba);

        // Do the processing.
        colorProcessor(bytes);

        texture.SetData(bytes);
        return texture;
    }

    private unsafe void PlatformSaveAsJpeg(Stream stream, int width, int height)
    {
        Color[] data = GetColorData();

        fixed (Color* ptr = &data[0])
        {
            byte* jpg;
            int jpgBytes;

            MGI.WriteJpg((byte*)ptr, data.Length, width, height, 90, out jpg, out jpgBytes);

            stream.Write(new ReadOnlySpan<byte>(jpg, jpgBytes));
        }
    }

    private unsafe void PlatformSaveAsPng(Stream stream, int width, int height)
    {
        Color[] data = GetColorData();

        fixed (Color* ptr = &data[0])
        {
            byte* png;
            int pngBytes;

            MGI.WritePng((byte*)ptr, data.Length, width, height, out png, out pngBytes);

            stream.Write(new ReadOnlySpan<byte>(png, pngBytes));
        }
    }

    private unsafe void PlatformReload(Stream stream)
    {
        var dataLength = (int)stream.Length;
        var streamTemp = new byte[dataLength];
        stream.Read(streamTemp, 0, dataLength);

        var handle = GCHandle.Alloc(streamTemp, GCHandleType.Pinned);

        byte* rgba;
        int width, height;

        try
        {
            MGI.ReadRGBA(
                (byte*)handle.AddrOfPinnedObject(),
                dataLength,
                true,
                out width,
                out height,
                out rgba);

            if (rgba == null)
                return;
        }
        finally
        {
            handle.Free();
        }

        MGG.Texture_SetData(
            GraphicsDevice.Handle,
            Handle,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            rgba,
            width * height);

        Marshal.FreeHGlobal((nint)rgba);
    }
}
