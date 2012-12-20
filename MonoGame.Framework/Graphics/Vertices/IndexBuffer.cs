using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif PSM
using Sce.PlayStation.Core.Graphics;
using PssVertexBuffer = Sce.PlayStation.Core.Graphics.VertexBuffer;
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
#elif PSM
        internal ushort[] _buffer;
#else
		internal uint ibo;	
#endif

        public BufferUsage BufferUsage { get; private set; }
        public int IndexCount { get; private set; }
        public IndexElementSize IndexElementSize { get; private set; }

   		protected IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage, bool dynamic)
            : this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage, dynamic)
        {
        }

		protected IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool dynamic)
        {
			if (graphicsDevice == null)
            {
                throw new ArgumentNullException("GraphicsDevice is null");
            }
			this.GraphicsDevice = graphicsDevice;
			this.IndexElementSize = indexElementSize;	
            this.IndexCount = indexCount;
            this.BufferUsage = usage;
			
			var sizeInBytes = indexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            _isDynamic = dynamic;
            
#if DIRECTX

            // TODO: To use true Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            var accessflags = SharpDX.Direct3D11.CpuAccessFlags.None;
            var resUsage = SharpDX.Direct3D11.ResourceUsage.Default;

            if (dynamic)
            {
                accessflags |= SharpDX.Direct3D11.CpuAccessFlags.Write;
                resUsage = SharpDX.Direct3D11.ResourceUsage.Dynamic;
            }

            _buffer = new SharpDX.Direct3D11.Buffer(    graphicsDevice._d3dDevice,
                                                        sizeInBytes,
                                                        resUsage,
                                                        SharpDX.Direct3D11.BindFlags.IndexBuffer,
                                                        accessflags,
                                                        SharpDX.Direct3D11.ResourceOptionFlags.None,
                                                        0  // StructureSizeInBytes
                                                        );
#elif PSM
            if (indexElementSize != IndexElementSize.SixteenBits)
                throw new NotImplementedException("PSS Currently only supports ushort (SixteenBits) index elements");
            _buffer = new ushort[indexCount];
#else
            Threading.BlockOnUIThread(() => GenerateIfRequired());
#endif
		}
		
		public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, indexElementSize, indexCount, bufferUsage, false)
		{
		}

		public IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
			this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage, false)
		{
		}

        /// <summary>
        /// Gets the relevant IndexElementSize enum value for the given type.
        /// </summary>
        /// <param name="type">The type to use for the index buffer</param>
        /// <returns>The IndexElementSize enum value that matches the type</returns>
        static IndexElementSize SizeForType(GraphicsDevice graphicsDevice, Type type)
        {
            switch (Marshal.SizeOf(type))
            {
                case 2:
                    return IndexElementSize.SixteenBits;
                case 4:
                    if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
                        throw new NotSupportedException("The profile does not support an elementSize of IndexElementSize.ThirtyTwoBits; use IndexElementSize.SixteenBits or a type that has a size of two bytes.");
                    return IndexElementSize.ThirtyTwoBits;
                default:
                    throw new ArgumentOutOfRangeException("Index buffers can only be created for types that are sixteen or thirty two bits in length");
            }
        }

        /// <summary>
        /// The GraphicsDevice is resetting, so GPU resources must be recreated.
        /// </summary>
        internal protected override void GraphicsDeviceResetting()
        {
#if OPENGL
            ibo = 0;
#endif
        }

#if OPENGL
        /// <summary>
        /// If the IBO does not exist, create it.
        /// </summary>
        void GenerateIfRequired()
        {
            if (ibo == 0)
            {
                var sizeInBytes = IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

#if IOS || ANDROID
                GL.GenBuffers(1, ref ibo);
#else
                GL.GenBuffers(1, out ibo);
#endif
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                              (IntPtr)sizeInBytes, IntPtr.Zero, _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }
        }
#endif

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
#elif PSM
            throw new NotImplementedException();
#else        
            Threading.BlockOnUIThread(() =>
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, ibo);
                GraphicsExtensions.CheckGLError();
                var elementSizeInByte = Marshal.SizeOf(typeof(T));
#if IOS || ANDROID
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
#if IOS || ANDROID
                GL.Oes.UnmapBuffer(All.ArrayBuffer);
#else
                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
#endif
                GraphicsExtensions.CheckGLError();
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
            SetDataInternal<T>(0, data, startIndex, elementCount, SetDataOptions.Discard);
        }
        		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetDataInternal<T>(0, data, startIndex, elementCount, SetDataOptions.Discard);
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
            SetDataInternal<T>(0, data, 0, data.Length, SetDataOptions.Discard);
        }

        protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data is null");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");

#if DIRECTX

            if (_isDynamic)
            {
                // We assume discard by default.
                var mode = SharpDX.Direct3D11.MapMode.WriteDiscard;
                if ((options & SetDataOptions.NoOverwrite) == SetDataOptions.NoOverwrite)
                    mode = SharpDX.Direct3D11.MapMode.WriteNoOverwrite;

                SharpDX.DataStream stream;
                var d3dContext = GraphicsDevice._d3dContext;
                lock (d3dContext)
                {
                    d3dContext.MapSubresource(
                        _buffer,
                        mode,
                        SharpDX.Direct3D11.MapFlags.None,
                        out stream);

                    stream.Position = offsetInBytes;
                    stream.WriteRange(data, startIndex, elementCount);

                    d3dContext.UnmapSubresource(_buffer, 0);
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
                var d3dContext = GraphicsDevice._d3dContext;
                lock (d3dContext)
                    d3dContext.UpdateSubresource(box, _buffer, 0, region);

                dataHandle.Free();
            }

#elif PSM
            if (typeof(T) == typeof(ushort))
            {
                Array.Copy(data, offsetInBytes / sizeof(ushort), _buffer, startIndex, elementCount);
            }
            else
            {
                throw new NotImplementedException("PSS Currently only supports ushort (SixteenBits) index elements");
                //Something like as follows probably works if you really need this, but really just make a ushort array!
                /*
                int indexOffset = offsetInBytes / sizeof(T);
                for (int i = 0; i < elementCount; i++)
                    _buffer[i + startIndex] = (ushort)(object)data[i + indexOffset];
                */
            }
#else
            Threading.BlockOnUIThread(() =>
            {
                GenerateIfRequired();

                var elementSizeInByte = Marshal.SizeOf(typeof(T));
                var sizeInBytes = elementSizeInByte * elementCount;
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
                var bufferSize = IndexCount * (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GraphicsExtensions.CheckGLError();

                if (options == SetDataOptions.Discard)
                {
                    // By assigning NULL data to the buffer this gives a hint
                    // to the device to discard the previous content.
                    GL.BufferData(  BufferTarget.ElementArrayBuffer,
                                    (IntPtr)bufferSize,
                                    IntPtr.Zero,
                                    _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                    GraphicsExtensions.CheckGLError();
                }

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
                GraphicsExtensions.CheckGLError();

                dataHandle.Free();
            });
#endif
        }

		protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
#if DIRECTX
                if (disposing)
                {
                    if (_buffer != null)
                    {
                        _buffer.Dispose();
                        _buffer = null;
                    }
                }
#elif PSM
                //Do nothing
                _buffer = null;
#else
                GraphicsDevice.AddDisposeAction(() =>
                    {
                        GL.DeleteBuffers(1, ref ibo);
                        GraphicsExtensions.CheckGLError();
                    });
#endif
            }
            base.Dispose(disposing);
		}
	}
}
