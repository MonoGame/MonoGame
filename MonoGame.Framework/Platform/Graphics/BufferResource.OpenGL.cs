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
        //internal uint vao;
        internal int buffer;

        private void PlatformConstruct()
        {
            Threading.BlockOnUIThread(GenerateIfRequired);
        }

        private void PlatformGraphicsDeviceResetting()
        {
            buffer = 0;
        }

        BufferTarget bufferTarget { get { return BufferOptions == Options.BufferVertex ? BufferTarget.ArrayBuffer : BufferTarget.ShaderStorageBuffer; } }

        /// <summary>
        /// If the buffer does not exist, create it.
        /// </summary>
        void GenerateIfRequired()
        {
            if (buffer == 0)
            {
                GL.GenBuffers(1, out this.buffer);
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(bufferTarget, this.buffer);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(bufferTarget,
                              new IntPtr(ElementStride * ElementCount), IntPtr.Zero,
                              (BufferOptions & Options.Dynamic) != 0 ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
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

        private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
            where T : struct
        {
            GL.BindBuffer(bufferTarget, buffer);
            GraphicsExtensions.CheckGLError();

            // Pointer to the start of data in the vertex buffer
            var ptr = GL.MapBuffer(bufferTarget, BufferAccess.ReadOnly);
            GraphicsExtensions.CheckGLError();

            ptr = (IntPtr)(ptr.ToInt64() + offsetInBytes);

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
                        data[startIndex + i] = (T)Marshal.PtrToStructure(tmpPtr, typeof(T));
                        tmpPtr = (IntPtr)(tmpPtr.ToInt64() + vertexStride);
                    }
                }
                finally
                {
                    tmpHandle.Free();
                }
            }

            GL.UnmapBuffer(bufferTarget);
            GraphicsExtensions.CheckGLError();
        }

#endif

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

            GL.BindBuffer(bufferTarget, buffer);
            GraphicsExtensions.CheckGLError();

            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(
                    bufferTarget,
                    (IntPtr)bufferSize,
                    IntPtr.Zero,
                    (BufferOptions & Options.Dynamic) != 0 ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
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

                    GL.BufferSubData(bufferTarget, (IntPtr)offsetInBytes, (IntPtr)(elementSizeInBytes * elementCount), dataPtr);
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
                        GL.BufferSubData(bufferTarget, (IntPtr)dstOffset, (IntPtr)elementSizeInByte, dataPtr);
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

        internal void PlatformApply(GraphicsDevice device, ShaderProgram program, string paramName, int bindingSlot, bool writeAcess)
        {
            //bindingSlot = paramName == "type_StructuredBuffer_Input" ? 2 : 3;
            int blockIndex = GL.GetProgramResourceIndex(program.Program, ProgramInterface.ShaderStorageBlock, paramName);
            GraphicsExtensions.CheckGLError();

            if (blockIndex < 0)
                throw new InvalidOperationException("The active shader effect does not contain a buffer named " + paramName);

            // not needed if hardcoded in shader:    layout (std430, binding=2) buffer shader_data
            // GL.ShaderStorageBlockBinding(program.Program, blockIndex, bindingSlot);
            // GraphicsExtensions.CheckGLError();

            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, bindingSlot, buffer);
            GraphicsExtensions.CheckGLError();
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
