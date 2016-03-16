// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC && PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#elif DESKTOPGL || (MONOMAC && !PLATFORM_MACOS_LEGACY)
using OpenTK.Graphics.OpenGL;
#endif
#if GLES
using OpenTK.Graphics.ES20;
using BufferUsageHint = OpenTK.Graphics.ES20.BufferUsage;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer
    {
		//internal uint vao;
		internal uint vbo;

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

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
#if GLES
            // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
            // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
            throw new NotSupportedException("Vertex buffers are write-only on OpenGL ES platforms");
#endif
#if !GLES
            if (Threading.IsOnUIThread())
            {
                GetBufferData(offsetInBytes, data, startIndex, elementCount, vertexStride);
            }
            else
            {
                Threading.BlockOnUIThread (() => GetBufferData(offsetInBytes, data, startIndex, elementCount, vertexStride));
            }
#endif
        }

#if !GLES

        private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            GL.BindBuffer (BufferTarget.ArrayBuffer, vbo);
            GraphicsExtensions.CheckGLError();
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            IntPtr ptr = GL.MapBuffer (BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
            GraphicsExtensions.CheckGLError();
            // Pointer to the start of data to read in the index buffer
            ptr = new IntPtr (ptr.ToInt64 () + offsetInBytes);
			if (typeof(T) == typeof(byte)) {
                byte[] buffer = data as byte[];
                // If data is already a byte[] we can skip the temporary buffer
                // Copy from the vertex buffer to the destination array
                Marshal.Copy (ptr, buffer, 0, buffer.Length);
            } else {
                // Temporary buffer to store the copied section of data
                byte[] buffer = new byte[elementCount * vertexStride - offsetInBytes];
                // Copy from the vertex buffer to the temporary buffer
                Marshal.Copy(ptr, buffer, 0, buffer.Length);
                
                var dataHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject ().ToInt64 () + startIndex * elementSizeInByte);
                
                // Copy from the temporary buffer to the destination array
                
                int dataSize = Marshal.SizeOf(typeof(T));
                if (dataSize == vertexStride)
                    Marshal.Copy(buffer, 0, dataPtr, buffer.Length);
                else
                {
                    // If the user is asking for a specific element within the vertex buffer, copy them one by one...
                    for (int i = 0; i < elementCount; i++)
                    {
                        Marshal.Copy(buffer, i * vertexStride, dataPtr, dataSize);
                        dataPtr = (IntPtr)(dataPtr.ToInt64() + dataSize);
                    }
                }
                
                dataHandle.Free ();
                
                //Buffer.BlockCopy(buffer, 0, data, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
            }
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            }
        
#endif

        private void PlatformSetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes) where T : struct
        {
            if (Threading.IsOnUIThread())
            {
                SetBufferData(bufferSize, elementSizeInBytes, offsetInBytes, data, startIndex, elementCount, vertexStride, options);
            }
            else
            {
                Threading.BlockOnUIThread(() => SetBufferData(bufferSize, elementSizeInBytes, offsetInBytes, data, startIndex, elementCount, vertexStride, options));
            }
        }

        private void SetBufferData<T>(int bufferSize, int elementSizeInBytes, int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            GenerateIfRequired();
            
            var sizeInBytes = elementSizeInBytes * elementCount;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GraphicsExtensions.CheckGLError();
            
            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(  BufferTarget.ArrayBuffer,
                              (IntPtr)bufferSize, 
                              IntPtr.Zero,
                              _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }

            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInBytes);

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
            GraphicsExtensions.CheckGLError();

            dataHandle.Free();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Threading.BlockOnUIThread(() =>
                {
                    GL.DeleteBuffers(1, ref vbo);
                    GraphicsExtensions.CheckGLError();
                });
            }

            base.Dispose(disposing);
        }
    }
}
