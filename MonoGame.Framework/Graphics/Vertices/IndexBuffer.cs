using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif PSS
using Sce.Pss.Core.Graphics;
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
		
#if WINRT
        internal SharpDX.Direct3D11.Buffer _buffer;
#else
		internal uint ibo;	
#endif
	
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
            // TODO: To use Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            _buffer = new SharpDX.Direct3D11.Buffer(    graphicsDevice._d3dDevice,
                                                        sizeInBytes,
                                                        dynamic ? SharpDX.Direct3D11.ResourceUsage.Dynamic : SharpDX.Direct3D11.ResourceUsage.Default,
                                                        SharpDX.Direct3D11.BindFlags.IndexBuffer,
                                                        0, // CpuAccessFlags
                                                        0, // OptionFlags                                                          
                                                        0  // StructureSizeInBytes
                                                        );
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
            //using(var stream = new SharpDX.DataStream(sizeInBytes, false, true))
            {
                var elementSizeInBytes = IndexElementSize == Graphics.IndexElementSize.SixteenBits ? 2 : 4;
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var startBytes = startIndex * elementSizeInBytes;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                //stream.WriteRange(data, 0, elementCount);
                //var box = new SharpDX.DataBox(stream.DataPointer, elementSizeInByte, 0);
                var box = new SharpDX.DataBox(dataPtr, elementSizeInBytes, 0);

                var region = new SharpDX.Direct3D11.ResourceRegion();
                region.Top = 0;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = 1;
                region.Left = offsetInBytes / elementSizeInBytes;
                region.Right = elementCount;

                // TODO: We need to deal with threaded contexts here!
                graphicsDevice._d3dContext.UpdateSubresource(box, _buffer, 0, region);

                dataHandle.Free();
            }
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
            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }
#else
			GL.DeleteBuffers(1, ref ibo);
#endif
            base.Dispose();
		}
	}
}
