// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {
        internal int ibo;

        private void PlatformConstruct(IndexElementSize indexElementSize, int indexCount)
        {
            Threading.BlockOnUIThread(GenerateIfRequired);
        }

        private void PlatformGraphicsDeviceResetting()
        {
            ibo = 0;
        }

        /// <summary>
        /// If the IBO does not exist, create it.
        /// </summary>
        void GenerateIfRequired()
        {
            if (ibo == 0)
            {
                var sizeInBytes = IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

                GL.GenBuffers(1, out ibo);
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                              (IntPtr)sizeInBytes, IntPtr.Zero, _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
            // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
            if (GraphicsDevice != null && !GraphicsDevice.GraphicsCapabilities.SupportsMapBuffer)
                throw new NotSupportedException("IndexBuffer.GetData is not supported on OpenGL ES versions below 3.0. Index buffers are write-only on those OpenGL ES platforms");

            if (Threading.IsOnUIThread())
            {
                GetBufferData(offsetInBytes, data, startIndex, elementCount);
            }
            else
            {
                Threading.BlockOnUIThread(() => GetBufferData(offsetInBytes, data, startIndex, elementCount));
            }
        }

        private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GraphicsExtensions.CheckGLError();

            var elementSizeInByte = Marshal.SizeOf<T>();
            var dataSize = elementCount * elementSizeInByte;
            IntPtr ptr;

#if GLES
            if (GraphicsDevice != null && !GraphicsDevice.GraphicsCapabilities.SupportsMapBuffer)
                throw new NotSupportedException("IndexBuffer.GetBufferData is not supported on OpenGL ES versions below 3.0.");

            ptr = GL.MapBufferRange(
                BufferTarget.ElementArrayBuffer,
                (IntPtr)offsetInBytes,
                (IntPtr)dataSize,
                (int)GL.MapBufferAccessMask.MapReadBit);
#else
            // Desktop OpenGL uses glMapBuffer and adjusts the pointer
            ptr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.ReadOnly);
            ptr = new IntPtr(ptr.ToInt64() + offsetInBytes);
#endif

            GraphicsExtensions.CheckGLError();

            if (ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to map index buffer for reading");
            }

            try
            {
                if (typeof(T) == typeof(byte))
                {
                    byte[] buffer = data as byte[];
                    // If data is already a byte[] we can skip the temporary buffer
                    // Copy from the index buffer to the destination array
                    Marshal.Copy(ptr, buffer, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
                }
                else
                {
                    // Temporary buffer to store the copied section of data
                    byte[] buffer = new byte[elementCount * elementSizeInByte];
                    // Copy from the index buffer to the temporary buffer
                    Marshal.Copy(ptr, buffer, 0, buffer.Length);
                    // Copy from the temporary buffer to the destination array
                    Buffer.BlockCopy(buffer, 0, data, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
                }
            }
            finally
            {
                GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
            where T : struct
        {
            Threading.BlockOnUIThread(SetDataState<T>.Action, new SetDataState<T>
            {
                buffer = this,
                offsetInBytes = offsetInBytes,
                data = data,
                startIndex = startIndex,
                elementCount = elementCount,
                options = options
            });
        }

        private void PlatformSetDataBody<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            GenerateIfRequired();

            var elementSizeInByte = Marshal.SizeOf<T>();
            var sizeInBytes = elementSizeInByte * elementCount;
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
                var bufferSize = IndexCount * (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GraphicsExtensions.CheckGLError();

                if (options == SetDataOptions.Discard)
                {
                    // By assigning NULL data to the buffer this gives a hint
                    // to the device to discard the previous content.
                    GL.BufferData(
                        BufferTarget.ElementArrayBuffer,
                        (IntPtr)bufferSize,
                        IntPtr.Zero,
                        _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                    GraphicsExtensions.CheckGLError();
                }

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
                GraphicsExtensions.CheckGLError();
            }
            finally
            {
                dataHandle.Free();
            }
        }

        /// <summary/>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                    GraphicsDevice.DisposeBuffer(ibo);
            }
            base.Dispose(disposing);
        }

        struct SetDataState<T>
            where T : struct
        {
            public IndexBuffer buffer;
            public int offsetInBytes;
            public T[] data;
            public int startIndex;
            public int elementCount;
            public SetDataOptions options;

            public static Action<SetDataState<T>> Action =
                (s) => s.buffer.PlatformSetDataBody(s.offsetInBytes, s.data, s.startIndex, s.elementCount, s.options);
        }
    }
}
