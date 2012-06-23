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
using PssVertexBuffer = Sce.Pss.Core.Graphics.VertexBuffer;
#elif GLES
using OpenTK.Graphics.ES20;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{


    public class IndexBuffer : GraphicsResource
    {
        private bool _isDynamic;

#if DIRECTX
        internal SharpDX.Direct3D11.Buffer _buffer;
#elif PSS
        internal PssVertexBuffer _buffer;
#else
		internal uint ibo;	
#endif

        public BufferUsage BufferUsage { get; private set; }
        public int IndexCount { get; private set; }
        public IndexElementSize IndexElementSize { get; private set; }

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

            _isDynamic = dynamic;
            
#if DIRECTX

            // TODO: To use true Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            SharpDX.Direct3D11.CpuAccessFlags accessflags = SharpDX.Direct3D11.CpuAccessFlags.None;
            SharpDX.Direct3D11.ResourceUsage usage = SharpDX.Direct3D11.ResourceUsage.Default;

            if (dynamic)
            {
                accessflags |= SharpDX.Direct3D11.CpuAccessFlags.Write;
                usage = SharpDX.Direct3D11.ResourceUsage.Dynamic;
            }

            if (bufferUsage != Graphics.BufferUsage.WriteOnly)
                accessflags |= SharpDX.Direct3D11.CpuAccessFlags.Read;

            _buffer = new SharpDX.Direct3D11.Buffer(    graphicsDevice._d3dDevice,
                                                        sizeInBytes,
                                                        usage,
                                                        SharpDX.Direct3D11.BindFlags.IndexBuffer,
                                                        accessflags,
                                                        SharpDX.Direct3D11.ResourceOptionFlags.None,
                                                        0  // StructureSizeInBytes
                                                        );
#elif PSS
            _buffer = new PssVertexBuffer(0, indexCount);
#else
            Threading.BlockOnUIThread(() =>
            {
#if IPHONE || ANDROID
                GL.GenBuffers(1, ref ibo);
#else
                GL.GenBuffers(1, out ibo);
#endif
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                              (IntPtr)sizeInBytes, IntPtr.Zero, dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
            });
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

#if DIRECTX
            throw new NotImplementedException();
#elif PSS
            throw new NotImplementedException();
#else        
            Threading.BlockOnUIThread(() =>
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
            });
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
            SetData<T>(0, data, startIndex, elementCount, SetDataOptions.Discard);
        }
        		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetData<T>(0, data, startIndex, elementCount, SetDataOptions.Discard);
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
            SetData<T>(0, data, 0, data.Length, SetDataOptions.Discard);
        }

        protected void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");

#if DIRECTX

            if (_isDynamic)
            {
                // We assume discard by default.
                var mode = SharpDX.Direct3D11.MapMode.WriteDiscard;
                if ((options & SetDataOptions.NoOverwrite) == SetDataOptions.NoOverwrite)
                    mode = SharpDX.Direct3D11.MapMode.WriteNoOverwrite;

                SharpDX.DataStream stream;
                lock (graphicsDevice._d3dContext)
                {
                    graphicsDevice._d3dContext.MapSubresource(
                        _buffer,
                        mode,
                        SharpDX.Direct3D11.MapFlags.None,
                        out stream);

                    stream.Position = offsetInBytes;
                    stream.WriteRange(data, startIndex, elementCount);

                    graphicsDevice._d3dContext.UnmapSubresource(_buffer, 0);
                }
            }
            else
            {
                var elementSizeInBytes = Marshal.SizeOf(typeof(T));
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var startBytes = startIndex * elementSizeInBytes;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                var box = new SharpDX.DataBox(dataPtr, 1, 0);

                var region = new SharpDX.Direct3D11.ResourceRegion();
                region.Top = 0;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = 1;
                region.Left = offsetInBytes;
                region.Right = offsetInBytes + (elementCount * elementSizeInBytes);

                // TODO: We need to deal with threaded contexts here!
                lock (graphicsDevice._d3dContext)
                    graphicsDevice._d3dContext.UpdateSubresource(box, _buffer, 0, region);

                dataHandle.Free();
            }

#elif PSS
            if (typeof(T) == typeof(ushort))
            {
#warning This is a terrible way to achieve what I want to do
                _buffer.SetIndices((ushort[])(object)data, 0, 0, elementCount); 
            }
            else
            {
                ushort[] clone = new ushort[data.Length];
                for (int i = 0; i < data.Length; i++)
#warning This is a terrible way to achieve what I want to do
                    clone[i] = (ushort)(object)data[i];
                _buffer.SetIndices(clone, 0, 0, elementCount);
            }
#else
            Threading.BlockOnUIThread(() =>
            {
                var elementSizeInByte = Marshal.SizeOf(typeof(T));
                var sizeInBytes = elementSizeInByte * elementCount;
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);

                dataHandle.Free();
            });
#endif
        }

		public override void Dispose()
        {
#if DIRECTX || PSS

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
