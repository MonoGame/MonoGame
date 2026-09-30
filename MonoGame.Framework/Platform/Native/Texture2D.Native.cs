// MonoGame - Copyright (C) MonoGame Foundation, Inc
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

    /// <summary>
    /// Copies an Span of data to the texture.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the Span.</typeparam>
    /// <param name="level">The mipmap level where the data will be placed.</param>
    /// <param name="arraySlice">Index of the texture we want to copy to inside the texture array</param>
    /// <param name="rect">
    /// The section of the texture where the data will be placed. null indicates the data will be copied over the
    /// entire texture.
    /// </param>
    /// <param name="data">
    /// The Span of data to copy.  If <paramref name="rect"/> is null, the number of elements in the Span must be
    /// equal to the size of the texture, which is <see cref="Width"/> x <see cref="Height"/>; otherwise, the number
    /// of elements in the Span should be equal to the size of the rectangle.
    /// </param>
    /// <exception cref="ArgumentException">
    /// One of the following conditions is true:
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             The <paramref name="level"/> parameter is larger than the number of levels in this texture.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             The <paramref name="arraySlice"/> parameter is greater than zero and the texture arrays are not
    ///             supported on the graphics device.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             The <paramref name="arraySlice"/> parameter is less than zero or is greater than or equal to the
    ///             internal array buffer of this texture.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             The <paramref name="rect"/> is outside the bounds of the texture.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             The <typeparamref name="T"/> type size is invalid for the format of this texture.
    ///         </description>
    ///     </item>
    /// </list>
    /// </exception>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> parameter is null.</exception>
    public void SetData<T>(int level, int arraySlice, Rectangle? rect, ReadOnlySpan<T> data) where T : struct
    {
        Rectangle checkedRect;
        ValidateParams(level, arraySlice, rect, data, data.Length, out checkedRect);
        PlatformSetData(level, arraySlice, checkedRect, data);
    }

    /// <summary>
    /// Copies an Span of data to the texture.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the Span.</typeparam>
    /// <param name="level">The mipmap level where the data will be placed.</param>
    /// <param name="rect">
    /// The section of the texture where the data will be placed. null indicates the data will be copied over the
    /// entire texture.
    /// </param>
    /// <param name="data">
    /// The Span of data to copy.  If <paramref name="rect"/> is null, the number of elements in the Span must be
    /// equal to the size of the texture, which is <see cref="Width"/> x <see cref="Height"/>; otherwise, the number
    /// of elements in the Span should be equal to the size of the rectangle.
    /// </param>
    /// <exception cref="ArgumentException">
    /// One of the following conditions is true:
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             The <paramref name="level"/> parameter is larger than the number of levels in this texture.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             The <paramref name="rect"/> is outside the bounds of the texture.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             The <typeparamref name="T"/> type size is invalid for the format of this texture.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             The <paramref name="data"/> Span parameter is too small.
    ///             length of the data Span.
    ///         </description>
    ///     </item>
    /// </list>
    /// </exception>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> parameter is null.</exception>
    public void SetData<T>(int level, Rectangle? rect, ReadOnlySpan<T> data) where T : struct
    {
        Rectangle checkedRect;
        ValidateParams(level, 0, rect, data, data.Length, out checkedRect);
        if (rect.HasValue)
            PlatformSetData(level, 0, checkedRect, data);
        else
            PlatformSetData(level, data);
    }

    /// <summary>
    /// Copies an Span of data to the texture.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the Span.</typeparam>
    /// <param name="data"> The Span of data to copy.</param>
    /// <exception cref="ArgumentException">
    /// One of the following conditions is true:
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             The <typeparamref name="T"/> type size is invalid for the format of this texture.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             The <paramref name="data"/> Span parameter is too small.
    ///             length of the data Span.
    ///         </description>
    ///     </item>
    /// </list>
    /// </exception>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> parameter is null.</exception>
    public void SetData<T>(ReadOnlySpan<T> data) where T : struct
    {
        Rectangle checkedRect;
        ValidateParams(0, 0, null, data, data.Length, out checkedRect);
        PlatformSetData(0, data);
    }

    private void ValidateParams<T>(int level, int arraySlice, Rectangle? rect, ReadOnlySpan<T> data,
            int elementCount, out Rectangle checkedRect) where T : struct
    {
        if (data == null)
            throw new ArgumentNullException("data");
        if (data.Length < elementCount)
            throw new ArgumentException("The data array is too small.");
        CommonValidations<T>(level, arraySlice, rect, elementCount, out checkedRect);
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
        var dataPtr = (nint)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

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
        var dataPtr = (nint)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

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

    private unsafe void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, ReadOnlySpan<T> data) where T : struct
    {
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var dataBytes = data.Length * elementSizeInByte;

        fixed (T* dataPtr = data)
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
                dataBytes);
        }
    }

    private unsafe void PlatformSetData<T>(int level, ReadOnlySpan<T> data) where T : struct
    {
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var dataBytes = data.Length * elementSizeInByte;

        fixed (T* dataPtr = data)
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
                dataBytes);
        }
    }

    private unsafe void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
    {
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var elementSizeInByte = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInByte;
        var dataPtr = (nint)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

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
        ProcessorType processor = 0;
        if (colorProcessor == DefaultColorProcessors.ZeroTransparentPixels)
        {
            colorProcessor = null;
            processor |= ProcessorType.ZeroTransparentPixels;
        }

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);
        else
            throw new ArgumentException("Stream must support seeking.", nameof(stream));

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
                processor,
                out width,
                out height,
                out rgba);

            if (rgba == null)
                throw new InvalidOperationException("Failed to read valid RGBA data from the stream, it may not be a valid image format or the data is corrupted.");
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

            MGI.FreeRGBA(rgba);

            return texture;
        }

        // Since color processor takes a byte[] we need to copy the
        // native memory to a managed array.
        //
        // Ideally we change this to use Span which avoids this.
        var bytes = new byte[rgbaBytes];
        Marshal.Copy((nint)rgba, bytes, 0, rgbaBytes);
        MGI.FreeRGBA(rgba);

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

            // 91% is sort of a magic number that makes our unit tests
            // pass (meaning resulting images are good quality) but the
            // compressed file size is a little larger.
            MGI.WriteJpg((byte*)ptr, data.Length, width, height, 91, out jpg, out jpgBytes);

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
                ProcessorType.ZeroTransparentPixels,
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

        MGI.FreeRGBA(rgba);
    }
}
