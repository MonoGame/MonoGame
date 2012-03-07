﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#if IPHONE || ANDROID
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
#else
using BufferUsageHint = OpenTK.Graphics.ES20.BufferUsage;
#endif

#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class VertexBuffer : GraphicsResource
	{			
		//internal uint vao;
		internal uint vbo;
		
		public int VertexCount { get; private set; }
		public VertexDeclaration VertexDeclaration { get; private set; }
		public BufferUsage BufferUsage { get; private set; }
		
		protected VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool dynamic)
		{
			if (graphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
			this.graphicsDevice = graphicsDevice;
            this.VertexDeclaration = vertexDeclaration;
            this.VertexCount = vertexCount;
            this.BufferUsage = bufferUsage;

            Threading.Begin();
            try
            {
                //GLExt.Oes.GenVertexArrays(1, out this.vao);
                //GLExt.Oes.BindVertexArray(this.vao);
#if IPHONE || ANDROID
                GL.GenBuffers(1, ref this.vbo);
#else
			    GL.GenBuffers(1, out this.vbo);
#endif
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
                GL.BufferData(BufferTarget.ArrayBuffer,
                              new IntPtr(vertexDeclaration.VertexStride * vertexCount), IntPtr.Zero,
                              dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
            }
            finally
            {
                Threading.End();
            }
		}

        public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, false)
        {
        }
		
		public VertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, bufferUsage, false)
		{
        }

		internal void Apply()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
		}

        public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data is null");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("This VertexBuffer was created with a usage type of BufferUsage.WriteOnly. Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
            if ((vertexStride > (VertexCount * VertexDeclaration.VertexStride)) || (vertexStride < VertexDeclaration.VertexStride))
                throw new ArgumentOutOfRangeException("One of the following conditions is true:\nThe vertex stride is larger than the vertex buffer.\nThe vertex stride is too small for the type of data requested.");

            Threading.Begin();
            try
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                var elementSizeInByte = Marshal.SizeOf(typeof(T));
#if IPHONE || ANDROID
                // I think the access parameter takes zero for read only or read/write.
                // The glMapBufferOES extension spec and gl2ext.h both only mention GL_WRITE_ONLY
                IntPtr ptr = GL.Oes.MapBuffer(All.ArrayBuffer, (All)0);
#else
                IntPtr ptr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
#endif
                // Pointer to the start of data to read in the index buffer
                ptr = new IntPtr(ptr.ToInt64() + offsetInBytes);
                if (data is byte[])
                {
                    byte[] buffer = data as byte[];
                    // If data is already a byte[] we can skip the temporary buffer
                    // Copy from the vertex buffer to the destination array
                    Marshal.Copy(ptr, buffer, 0, buffer.Length);
                }
                else
                {
                    // Temporary buffer to store the copied section of data
                    byte[] buffer = new byte[elementCount * vertexStride];
                    // Copy from the vertex buffer to the temporary buffer
                    Marshal.Copy(ptr, buffer, 0, buffer.Length);
                    // Copy from the temporary buffer to the destination array
                    Buffer.BlockCopy(buffer, 0, data, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
                }
#if IPHONE || ANDROID
                GL.Oes.UnmapBuffer(All.ArrayBuffer);
#else
                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
#endif
            }
            finally
            {
                Threading.End();
            }
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            this.GetData<T>(0, data, startIndex, elementCount, elementSizeInByte);
        }

        public void GetData<T>(T[] data) where T : struct
        {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            this.GetData<T>(0, data, 0, data.Count(), elementSizeInByte);
        }
		
		public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data is null");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if ((vertexStride > (VertexCount * VertexDeclaration.VertexStride)) || (vertexStride < VertexDeclaration.VertexStride))
                throw new ArgumentOutOfRangeException("One of the following conditions is true:\nThe vertex stride is larger than the vertex buffer.\nThe vertex stride is too small for the type of data requested.");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var sizeInBytes = elementSizeInByte * elementCount;
            Threading.Begin();
            try
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferSubData<T>(BufferTarget.ArrayBuffer, new IntPtr(offsetInBytes), new IntPtr(sizeInBytes), data);
            }
            finally
            {
                Threading.End();
            }
		}
		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetData<T>(0, data, startIndex, elementCount, VertexDeclaration.VertexStride);
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
            SetData<T>(0, data, 0, data.Length, VertexDeclaration.VertexStride);
        }
		
		public override void Dispose()
		{
			GL.DeleteBuffers(1, ref vbo);
			base.Dispose();
		}
    }
}
