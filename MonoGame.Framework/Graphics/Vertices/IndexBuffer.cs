using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif WINRT
#else
using OpenTK.Graphics.ES20;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{


    public class IndexBuffer : GraphicsResource
    {
		public BufferUsage BufferUsage { get; private set; }
		public int IndexCount { get; private set; }
		public IndexElementSize IndexElementSize { get; private set; }
		
		internal uint ibo;	
		
		protected IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage, bool dynamic)
        {
			if (graphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
			this.graphicsDevice = graphicsDevice;
			this.IndexElementSize = indexElementSize;	
            this.IndexCount = indexCount;
            this.BufferUsage = bufferUsage;
			
			var sizeInBytes = indexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

#if WINRT
#else
            Threading.Begin();
            try
            {
#if IPHONE || ANDROID
                GL.GenBuffers(1, ref ibo);
#else
                GL.GenBuffers(1, out ibo);
#endif
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                              (IntPtr)sizeInBytes, IntPtr.Zero, dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
            }
            finally
            {
                Threading.End();
            }
#endif
		}
		
		public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, indexElementSize, indexCount, bufferUsage, false)
		{
		}

		public IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
			this(graphicsDevice,
			     (indexType == typeof(short) || indexType == typeof(ushort)) ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits,
			     indexCount, usage)
		{
		}
		
		internal void Apply()
		{
#if WINRT
#else
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
#endif
        }

        public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data is null");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("This IndexBuffer was created with a usage type of BufferUsage.WriteOnly. Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");

#if WINRT
#else        
            Threading.Begin();
            try
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, ibo);
                var elementSizeInByte = Marshal.SizeOf(typeof(T));
#if IPHONE || ANDROID
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
                    // Copy from the index buffer to the destination array
                    Marshal.Copy(ptr, buffer, 0, buffer.Length);
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
#endif
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            this.GetData<T>(0, data, startIndex, elementCount);
        }

        public void GetData<T>(T[] data) where T : struct
        {
            this.GetData<T>(0, data, 0, data.Length);
        }
		
		public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
			if (data == null) 
                throw new ArgumentNullException("data");

#if WINRT
#else
            Threading.Begin();
            try
            {
                var elementSizeInByte = Marshal.SizeOf(typeof(T));
                var sizeInBytes = elementSizeInByte * elementCount;
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);

                dataHandle.Free();
            }
            finally
            {
                Threading.End();
            }
#endif
		}
		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetData<T>(0, data, startIndex, elementCount);
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
            SetData<T>(0, data, 0, data.Length);
        }
		
		public override void Dispose()
		{
#if WINRT
#else
			GL.DeleteBuffers(1, ref ibo);
#endif
            base.Dispose();
		}
	}
}
