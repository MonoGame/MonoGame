// MonoGame - Copyright (C) MonoGame Foundation, Inc
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

    /// <summary>
    /// Sets the vertex buffer data, uses a Span including only relevant data to be copied rather than the full source array,
    /// and the first index in the buffer to start copying to. Assumes the full Span will be copied with no padding between elements.
    /// </summary>
    /// <typeparam name="T">Type of elements in the data Span.</typeparam>
    /// <param name="destinationStartIndex">The first index in the destination buffer you want to copy data to</param>
    /// <param name="data">Data array to be passed to the shader as a Span.</param>
    /// elementCount will be inferred to be the number of elements in <paramref name="data"/>
    /// since the Span should only contain the relevant data to be copied.
    /// <remarks>
    /// If <c>T</c> is <see cref="VertexPositionTexture"/>, and you want to only update the first 10 elements of your array of
    /// <see cref="VertexPositionTexture"/>s, you would generate a Span containing those elements and pass it in
    /// <code>
    /// Span&lt;VertexPositionTexture&gt; vptSpan = new Span&lt;VertexPositionTexture&gt;(vptArray, 0, 10);
    /// vertexBuffer.SetData(0, vptSpan);
    /// </code>
    /// 
    /// If you wanted to update the next 10 elements (indicies 10-19) in the source array, you would simply update the start index
    /// <code>
    /// Span&lt;VertexPositionTexture&gt; vptSpan = new Span&lt;VertexPositionTexture&gt;(vptArray, 10, 10);
    /// vertexBuffer.SetData(10, vptSpan);
    /// </code>
    /// <para>
    /// Since a Span is a wrapper around a contiguous region of arbitrary memory, this is intended for cases with a 
    /// vertexStride of <c>sizeof(T)</c>, as you need to generate a contiguous array of only relevant elements to populate
    /// the Span, and the extra allocation and pre-processing to generate the Span partial objects will likely outweigh 
    /// any benefits of passing a Span instead of a copy of the source data array.
    /// </para>
    /// </remarks>
    public void SetData<T>(int destinationStartIndex, Span<T> data) where T : struct
    {
        SetDataInternal<T>(destinationStartIndex, data, data.Length, SetDataOptions.None);
    }

    /// <summary>
    /// Sets the vertex buffer data. This is the same as calling <see cref="SetData{T}(int, Span{T})"/>
    /// with <c>destinationStartIndex</c> equal to <c>0</c>
    /// </summary>
    /// <typeparam name="T">Type of elements in the data array.</typeparam>
    /// <param name="data">Data Span to be passed to the shader.</param>
    /// <inheritdoc cref="SetData{T}(int, Span{T})" path="/remarks"/>
    public void SetData<T>(Span<T> data) where T : struct
    {
        var elementSizeInBytes = ReflectionHelpers.FastSizeOf<T>();
        SetDataInternal<T>(0, data, data.Length, SetDataOptions.None);
    }

    /// <summary/>
    protected void SetDataInternal<T>(int destinationStartIndex, Span<T> data, int elementCount, SetDataOptions options) where T : struct
    {
        if (data == null)
            throw new ArgumentNullException("data");

        var elementSizeInBytes = ReflectionHelpers.FastSizeOf<T>();
        var offsetInBytes = destinationStartIndex * elementSizeInBytes;
        var bufferSize = VertexCount * VertexDeclaration.VertexStride;

        if (elementCount > data.Length || elementCount <= 0)
            throw new ArgumentOutOfRangeException("data", "The array specified in the data parameter is not the correct size for the amount of data requested.");
        if (offsetInBytes + elementCount * VertexDeclaration.VertexStride > bufferSize)
            throw new ArgumentOutOfRangeException("The provided offset and data Span must total to a larger number of bytes than the vertex buffer");

        PlatformSetData<T>(offsetInBytes, data, elementCount, VertexDeclaration.VertexStride, options, bufferSize, elementSizeInBytes);
    }

    private unsafe void PlatformConstruct()
    {
        Handle = MGG.Buffer_Create(GraphicsDevice.Handle, BufferType.Vertex, _isDynamic, VertexCount * VertexDeclaration.VertexStride);
    }

    private unsafe void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
    {
        var elementSizeInBytes = ReflectionHelpers.FastSizeOf<T>();
        var startBytes = startIndex * elementSizeInBytes;
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var dataPtr = (nint)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        MGG.Buffer_GetData(GraphicsDevice.Handle, Handle, offsetInBytes, (byte*)dataPtr, elementCount, elementSizeInBytes, vertexStride);

        dataHandle.Free();
    }

    private unsafe void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
    {
        var startBytes = startIndex * elementSizeInBytes;
        var dataBytes = elementCount * elementSizeInBytes;
        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var dataPtr = (nint)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

        // TODO: We need to figure out the correct behavior 
        // for SetDataOptions.None on a dynamic buffer.
        //
        // For now we always discard as it is a pretty safe default.
        //
        var discard = _isDynamic && options != SetDataOptions.NoOverwrite;

        MGG.Buffer_SetData(GraphicsDevice.Handle, ref Handle, offsetInBytes, (byte*)dataPtr, elementCount, vertexStride, elementSizeInBytes, discard);

        dataHandle.Free();
    }

    private unsafe void PlatformSetData<T>(int offsetInBytes, Span<T> data, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
    {
        var dataBytes = elementCount * elementSizeInBytes;

        // TODO: We need to figure out the correct behavior 
        // for SetDataOptions.None on a dynamic buffer.
        //
        // For now we always discard as it is a pretty safe default.
        //
        fixed (void* ptr = &data[0])
        {
            var discard = _isDynamic && options != SetDataOptions.NoOverwrite;

            var dataPtr = (byte*)ptr;
            MGG.Buffer_SetData(GraphicsDevice.Handle, ref Handle, offsetInBytes, dataPtr, elementCount, vertexStride, elementSizeInBytes, discard);
        }
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
