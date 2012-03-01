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
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            IntPtr ptr = new IntPtr();
            //ptr = GL.Arb.MapBuffer(BufferTargetArb.ArrayBuffer, ArbVertexBufferObject.ReadOnlyArb | ArbVertexBufferObject.ArrayBufferArb);
            //ptr = OpenTK.Graphics.ES20.GL.Oes.MapBuffer(OpenTK.Graphics.ES20.All.ArrayBuffer, (OpenTK.Graphics.ES20.All)0);
            ptr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
            //ErrorCode e = GL.GetError();

            ///its dark magic time
            if (ptr != null && ptr.ToInt32() != 0)
            {
                ptr = ptr + offsetInBytes + startIndex * vertexStride;

                byte[] bytes = new byte[vertexStride * data.Count()];
                Marshal.Copy(ptr, bytes, 0, vertexStride * data.Count());

                byte[] by = new byte[elementSizeInByte];

                GL.UnmapBuffer( BufferTarget.ArrayBuffer);
                Marshal.Release(ptr);

                IntPtr buffer = Marshal.AllocHGlobal(elementSizeInByte);

                for (int j = 0; j < elementCount; j++)
                {
                    Array.ConstrainedCopy(bytes, j * vertexStride, by, 0, elementSizeInByte);

                    Marshal.Copy(by, 0, buffer, elementSizeInByte);
                    object retobj = Marshal.PtrToStructure(buffer, typeof(T));
                    data[j] = (T)retobj;
                }

                Marshal.Release(buffer);
            }
            else
            {
                throw new Exception("Could not decode the Vertex Buffer");
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
		
		public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
			if (data == null) throw new ArgumentNullException("data");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
			var sizeInBytes = elementSizeInByte * elementCount;
			var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
		
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);

			dataHandle.Free();
		}
		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
			if (data == null) throw new ArgumentNullException("data");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
			var sizeInBytes = elementSizeInByte * elementCount;
			var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
		
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)sizeInBytes, dataPtr);
			
			dataHandle.Free();
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
			if (data == null) throw new ArgumentNullException("data");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
			var sizeInBytes = elementSizeInByte * data.Length;
			var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var dataPtr = dataHandle.AddrOfPinnedObject();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)sizeInBytes, data);
			
			dataHandle.Free();
        }
		
		public override void Dispose ()
		{
			GL.DeleteBuffers(1, ref vbo);
			base.Dispose();
		}
    }
}
