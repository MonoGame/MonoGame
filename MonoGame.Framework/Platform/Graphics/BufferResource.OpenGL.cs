// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BufferResource
    {
        internal int buffer;
        internal int counterBuffer; // for structured buffers with append/consume and counter support

        private void PlatformConstruct()
        {
            Threading.BlockOnUIThread(GenerateIfRequired);
        }

        private void PlatformGraphicsDeviceResetting()
        {
            buffer = 0;
        }

        BufferTarget GetBufferTarget()
        {
            switch (BufferType)
            {
                case BufferType.StructuredBuffer:
                    return BufferTarget.ShaderStorageBuffer;
                case BufferType.VertexBuffer:
                case BufferType.IndexBuffer:
                    return BufferTarget.ArrayBuffer;
                case BufferType.IndirectDrawBuffer:
                    return BufferTarget.IndirectDrawBuffer;
                default:
                    throw new InvalidOperationException("Unknown BufferType");
            }
        }

        /// <summary>
        /// If the buffer does not exist, create it.
        /// </summary>
        void GenerateIfRequired()
        {
            if (buffer == 0 && ElementStride != 0)
            {
                GL.GenBuffers(1, out this.buffer);
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(GetBufferTarget(), this.buffer);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(GetBufferTarget(),
                              new IntPtr(ElementStride * ElementCount), IntPtr.Zero,
                              _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }

            if (counterBuffer == 0 && StructuredBufferType != StructuredBufferType.Basic)
            {
                GL.GenBuffers(1, out this.counterBuffer);
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, this.counterBuffer);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(BufferTarget.ShaderStorageBuffer, new IntPtr(4), IntPtr.Zero, BufferUsageHint.StreamDraw);
                GraphicsExtensions.CheckGLError();

                SetCounterBufferValue(0);
            }
        }

        internal void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
            where T : struct
        {
#if GLES
            // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
            // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
            throw new NotSupportedException("Vertex buffers are write-only on OpenGL ES platforms");
#else
            Threading.BlockOnUIThread(() => GetBufferData(offsetInBytes, data, startIndex, elementCount, vertexStride));
#endif
        }

#if !GLES

        private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int elementStride)
            where T : struct
        {
            GL.BindBuffer(GetBufferTarget(), buffer);
            GraphicsExtensions.CheckGLError();

            // Pointer to the start of data in the vertex buffer
            var ptr = GL.MapBuffer(GetBufferTarget(), BufferAccess.ReadOnly);
            GraphicsExtensions.CheckGLError();

            ptr = (IntPtr)(ptr.ToInt64() + offsetInBytes);

            if (typeof(T) == typeof(byte) && elementStride == 1)
            {
                // If data is already a byte[] and stride is 1 we can skip the temporary buffer
                var buffer = data as byte[];
                Marshal.Copy(ptr, buffer, startIndex * elementStride, elementCount * elementStride);
            }
            else
            {
                // Temporary buffer to store the copied section of data
                var tmp = new byte[elementCount * elementStride];
                // Copy from the vertex buffer to the temporary buffer
                Marshal.Copy(ptr, tmp, 0, tmp.Length);

                // Copy from the temporary buffer to the destination array
                var tmpHandle = GCHandle.Alloc(tmp, GCHandleType.Pinned);
                try
                {
                    var tmpPtr = tmpHandle.AddrOfPinnedObject();
                    for (var i = 0; i < elementCount; i++)
                    {
                        data[startIndex + i] = (T)Marshal.PtrToStructure(tmpPtr, typeof(T));
                        tmpPtr = (IntPtr)(tmpPtr.ToInt64() + elementStride);
                    }
                }
                finally
                {
                    tmpHandle.Free();
                }
            }

            GL.UnmapBuffer(GetBufferTarget());
            GraphicsExtensions.CheckGLError();
        }

#endif

        internal void PlatformSetData<T>(
            int offsetInBytes, T[] data, int startIndex, int elementCount, int elementStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
            where T : struct
        {
            Threading.BlockOnUIThread(SetDataState<T>.Action, new SetDataState<T>
            {
                buffer = this,
                offsetInBytes = offsetInBytes,
                data = data,
                startIndex = startIndex,
                elementCount = elementCount,
                elementStride = elementStride,
                options = options,
                bufferSize = bufferSize,
                elementSizeInBytes = elementSizeInBytes
            });
        }

        private void PlatformSetDataBody<T>(
            int offsetInBytes, T[] data, int startIndex, int elementCount, int elementStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
            where T : struct
        {
            GenerateIfRequired();

            GL.BindBuffer(GetBufferTarget(), buffer);
            GraphicsExtensions.CheckGLError();

            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(
                    GetBufferTarget(),
                    (IntPtr)bufferSize,
                    IntPtr.Zero,
                    _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }

            var elementSizeInByte = Marshal.SizeOf<T>();
            if (elementSizeInByte == elementStride || elementSizeInByte % elementStride == 0)
            {
                // there are no gaps so we can copy in one go
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInBytes);

                    GL.BufferSubData(GetBufferTarget(), (IntPtr)offsetInBytes, (IntPtr)(elementSizeInBytes * elementCount), dataPtr);
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
                        GL.BufferSubData(GetBufferTarget(), (IntPtr)dstOffset, (IntPtr)elementSizeInByte, dataPtr);
                        GraphicsExtensions.CheckGLError();

                        dstOffset += elementStride;
                        dataPtr = (IntPtr)(dataPtr.ToInt64() + elementSizeInByte);
                    }
                }
                finally
                {
                    dataHandle.Free();
                }
            }
        }

        private void SetCounterBufferValue(int val)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, counterBuffer);
            GraphicsExtensions.CheckGLError();

            unsafe
            {
                int* data = stackalloc int[1];
                data[0] = val;

                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, new IntPtr(4), (IntPtr)data);
                GraphicsExtensions.CheckGLError();
            }
        }

        internal override void PlatformApply(GraphicsDevice device, ShaderProgram program, ref ResourceBinding resourceBinding, bool writeAcess)
        {
            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, resourceBinding.bindingSlot, buffer);
            GraphicsExtensions.CheckGLError();
            
            if (counterBuffer > 0)
            {            
                if (CounterBufferResetValue != -1)
                    SetCounterBufferValue(CounterBufferResetValue);

                GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, resourceBinding.bindingSlotForCounter, counterBuffer);
                GraphicsExtensions.CheckGLError();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                    GraphicsDevice.DisposeBuffer(buffer);
            }
            base.Dispose(disposing);
        }

        struct SetDataState<T>
            where T : struct
        {
            public BufferResource buffer;
            public int offsetInBytes;
            public T[] data;
            public int startIndex;
            public int elementCount;
            public int elementStride;
            public SetDataOptions options;
            public int bufferSize;
            public int elementSizeInBytes;

            public static Action<SetDataState<T>> Action = (s) =>
            {
                s.buffer.PlatformSetDataBody(
                    s.offsetInBytes, s.data, s.startIndex, s.elementCount, s.elementStride, s.options, s.bufferSize, s.elementSizeInBytes);
            };
        }
    }
}
