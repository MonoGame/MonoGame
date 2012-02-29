﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS
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

            Threading.BlockOnUIThread(() =>
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
                                  (IntPtr)(vertexDeclaration.VertexStride * vertexCount), IntPtr.Zero,
                                  dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                });
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
			throw new NotSupportedException();
        }
		
		public void GetData<T>(T[] data, int startIndex, int elementCount)
		{
			throw new NotSupportedException();
		}
		
		public void GetData<T>(T[] data)
		{
			throw new NotSupportedException();
		}
		
		public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
			if (data == null) throw new ArgumentNullException("data");

            Threading.BlockOnUIThread(() =>
                {
                    var elementSizeInByte = Marshal.SizeOf(typeof(T));
                    var sizeInBytes = elementSizeInByte * elementCount;
                    var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);

                    dataHandle.Free();
                });
		}
		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
			if (data == null) throw new ArgumentNullException("data");

            Threading.BlockOnUIThread(() =>
                {
                    var elementSizeInByte = Marshal.SizeOf(typeof(T));
                    var sizeInBytes = elementSizeInByte * elementCount;
                    var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)sizeInBytes, dataPtr);

                    dataHandle.Free();
                });
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
			if (data == null) throw new ArgumentNullException("data");

            Threading.BlockOnUIThread(() =>
                {
                    var elementSizeInByte = Marshal.SizeOf(typeof(T));
                    var sizeInBytes = elementSizeInByte * data.Length;
                    var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    var dataPtr = dataHandle.AddrOfPinnedObject();

                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)sizeInBytes, data);

                    dataHandle.Free();
                });
        }
		
		public override void Dispose ()
		{
			GL.DeleteBuffers(1, ref vbo);
			base.Dispose();
		}
    }
}
