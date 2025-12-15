// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer
    {
        //internal uint vao;
        internal int vbo;

        private void PlatformConstruct()
        {
            Threading.BlockOnUIThread(GenerateIfRequired);
        }

        private void PlatformGraphicsDeviceResetting()
        {
            vbo = 0;
        }

        /// <summary>
        /// If the VBO does not exist, create it.
        /// </summary>
        void GenerateIfRequired()
        {
            if (vbo == 0)
            {
                //GLExt.Oes.GenVertexArrays(1, out this.vao);
                //GLExt.Oes.BindVertexArray(this.vao);
                GL.GenBuffers(1, out this.vbo);
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(BufferTarget.ArrayBuffer,
                              new IntPtr(VertexDeclaration.VertexStride * VertexCount), IntPtr.Zero,
                              _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGetData<
           [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T
        >(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
            where T : struct
        {
            // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
            // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
            if (GraphicsDevice != null && !GraphicsDevice.GraphicsCapabilities.SupportsMapBuffer)
                throw new NotSupportedException("VertexBuffer.GetData is not supported on OpenGL ES versions below 3.0. Vertex buffers are write-only on those OpenGL ES platforms");

            Threading.BlockOnUIThread(() => GetBufferData(offsetInBytes, data, startIndex, elementCount, vertexStride));
        }

        private void GetBufferData<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T
        >(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
            where T : struct
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GraphicsExtensions.CheckGLError();

            var dataSize = elementCount * vertexStride;
            IntPtr ptr;

#if GLES
            if (GraphicsDevice != null && !GraphicsDevice.GraphicsCapabilities.SupportsMapBuffer)
                throw new NotSupportedException("VertexBuffer.GetBufferData is not supported on OpenGL ES versions below 3.0.");

            ptr = GL.MapBufferRange(
                BufferTarget.ArrayBuffer,
                (IntPtr)offsetInBytes,
                (IntPtr)dataSize,
                (int)GL.MapBufferAccessMask.MapReadBit);
#else
            // Desktop OpenGL uses glMapBuffer and adjusts the pointer
            ptr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
            ptr = (IntPtr)(ptr.ToInt64() + offsetInBytes);
#endif

            GraphicsExtensions.CheckGLError();

            if (ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to map vertex buffer for reading");
            }

            try
            {
                if (typeof(T) == typeof(byte) && vertexStride == 1)
                {
                    // If data is already a byte[] and stride is 1 we can skip the temporary buffer
                    var buffer = data as byte[];
                    Marshal.Copy(ptr, buffer, startIndex * vertexStride, elementCount * vertexStride);
                }
                else
                {
                    // Temporary buffer to store the copied section of data
                    var tmp = new byte[elementCount * vertexStride];
                    // Copy from the vertex buffer to the temporary buffer
                    Marshal.Copy(ptr, tmp, 0, tmp.Length);

                    // Copy from the temporary buffer to the destination array
                    var tmpHandle = GCHandle.Alloc(tmp, GCHandleType.Pinned);
                    try
                    {
                        var tmpPtr = tmpHandle.AddrOfPinnedObject();
                        for (var i = 0; i < elementCount; i++)
                        {
                            data[startIndex + i] = Marshal.PtrToStructure<T>(tmpPtr);
                            tmpPtr = (IntPtr)(tmpPtr.ToInt64() + vertexStride);
                        }
                    }
                    finally
                    {
                        tmpHandle.Free();
                    }
                }
            }
            finally
            {
                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformSetData<T>(
            int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
            where T : struct
        {
            Threading.BlockOnUIThread(SetDataState<T>.Action, new SetDataState<T>
            {
                buffer = this,
                offsetInBytes = offsetInBytes,
                data = data,
                startIndex = startIndex,
                elementCount = elementCount,
                vertexStride = vertexStride,
                options = options,
                bufferSize = bufferSize,
                elementSizeInBytes = elementSizeInBytes
            });
        }

        private void PlatformSetDataBody<T>(
            int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
            where T : struct
        {
            GenerateIfRequired();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GraphicsExtensions.CheckGLError();

            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(
                    BufferTarget.ArrayBuffer,
                    (IntPtr)bufferSize,
                    IntPtr.Zero,
                    _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }

            var elementSizeInByte = Marshal.SizeOf<T>();
            if (elementSizeInByte == vertexStride || elementSizeInByte % vertexStride == 0)
            {
                // there are no gaps so we can copy in one go
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInBytes);

                    GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)(elementSizeInBytes * elementCount), dataPtr);
                    GraphicsExtensions.CheckGLError();
                }
                finally
                {
                    dataHandle.Free();
                }
            }
            else
            {
                // else we must copy each element separately
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    int dstOffset = offsetInBytes;
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                    for (int i = 0; i < elementCount; i++)
                    {
                        GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)dstOffset, (IntPtr)elementSizeInByte, dataPtr);
                        GraphicsExtensions.CheckGLError();

                        dstOffset += vertexStride;
                        dataPtr = (IntPtr)(dataPtr.ToInt64() + elementSizeInByte);
                    }
                }
                finally
                {
                    dataHandle.Free();
                }
            }

        }

        /// <summary/>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                    GraphicsDevice.DisposeBuffer(vbo);
            }
            base.Dispose(disposing);
        }

        struct SetDataState<T>
            where T : struct
        {
            public VertexBuffer buffer;
            public int offsetInBytes;
            public T[] data;
            public int startIndex;
            public int elementCount;
            public int vertexStride;
            public SetDataOptions options;
            public int bufferSize;
            public int elementSizeInBytes;

            public static Action<SetDataState<T>> Action = (s) =>
            {
                s.buffer.PlatformSetDataBody(
                    s.offsetInBytes, s.data, s.startIndex, s.elementCount, s.vertexStride, s.options, s.bufferSize, s.elementSizeInBytes);
            };
        }
    }
}
